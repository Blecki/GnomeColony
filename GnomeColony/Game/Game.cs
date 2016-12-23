using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;
using Gem.Render;

namespace Game
{
    public class Game : IScreen
    {
        public Gem.Input Input { get; set; }
        public Main Main { get; set; }

        protected EpisodeContentManager Content;
        public RenderContext RenderContext { get; private set; }
        public SceneNode HoverNode { get; private set; }

        public class RenderTree
        {
            public Gem.Render.ICamera Camera;
            public Gem.Render.BranchNode SceneGraph;
        }

        public List<RenderTree> RenderTrees = new List<RenderTree>();

        private List<InputState> InputStack = new List<InputState>();
        public float ElapsedSeconds { get; private set; }

        public void PushInputState(InputState NextState)
        {
            if (InputStack.Count > 0) InputStack.Last().Covered(this);
            InputStack.Add(NextState);
            NextState.EnterState(this);
        }

        public void PopInputState()
        {
            InputStack.Last().LeaveState(this);
            InputStack.RemoveAt(InputStack.Count - 1);
            if (InputStack.Count > 0) InputStack.Last().Exposed(this);
        }

        public Game()
        {
        }

        public void Begin()
        {
            Content = new EpisodeContentManager(Main.EpisodeContent.ServiceProvider, "Content");
            RenderContext = new RenderContext(Content.Load<Effect>("draw"), Main.GraphicsDevice);
        }

        public void End()
        {
        }

        public void Update(float elapsedSeconds)
        {
            this.ElapsedSeconds = elapsedSeconds;
                        
            HoverNode = null;
            var hoverItems = new List<HoverItem>();

            foreach (var renderTree in RenderTrees)
            {
                var mousePosition = Main.Input.QueryAxis("MAIN-AXIS");
                var pickRay = renderTree.Camera.GetPickRay(mousePosition);

                renderTree.SceneGraph.CalculateLocalMouse(pickRay, (node, distance) => hoverItems.Add(new HoverItem { Node = node, Distance = distance }));
            }

            if (hoverItems.Count > 0)
            {
                var nearestDistance = float.PositiveInfinity;
                foreach (var hoverItem in hoverItems)
                    if (hoverItem.Distance < nearestDistance) nearestDistance = hoverItem.Distance;
                HoverNode = hoverItems.First(item => item.Distance <= nearestDistance).Node;
            }

            if (InputStack.Count > 0) InputStack.Last().Update(this);
            foreach (var renderTree in RenderTrees)
                renderTree.SceneGraph.UpdateWorldTransform(Matrix.Identity);
        }

        private struct HoverItem
        {
            public SceneNode Node;
            public float Distance;
        }

        public void Draw(float elapsedSeconds)
        {
            var viewport = Main.GraphicsDevice.Viewport;

            foreach (var renderTree in RenderTrees)
            {
                RenderContext.Camera = renderTree.Camera;
                renderTree.SceneGraph.PreDraw(elapsedSeconds, RenderContext);
            }
                       
            Main.GraphicsDevice.SetRenderTarget(null);
            Main.GraphicsDevice.Viewport = viewport;
            Main.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            Main.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Main.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            Main.GraphicsDevice.Clear(ClearOptions.Target, Color.CornflowerBlue, 0xFFFFFF, 0);

            foreach (var renderTree in RenderTrees)
            {
                RenderContext.Camera = renderTree.Camera;
                RenderContext.Color = Vector3.One;
                RenderContext.Alpha = 1.0f;
                RenderContext.ClipAlpha = 0.2f;
                RenderContext.LightingEnabled = true;
                RenderContext.UVTransform = Matrix.Identity;
                RenderContext.World = Matrix.Identity;
                //RenderContext.SetLight(0, PlayerActor.Orientation.Position + new Vector3(0.0f, -0.2f, 2.0f), 10, new Vector3(1, 0, 0));
                RenderContext.SetLight(1, new Vector3(-6.5f, -6.5f, 6.5f), 20, new Vector3(1, 1, 1));
                RenderContext.SetLight(2, new Vector3(6.5f, 6.5f, 6.5f), 20, new Vector3(1, 1, 1));
                RenderContext.ActiveLightCount = 3;
                RenderContext.Texture = RenderContext.White;
                RenderContext.NormalMap = RenderContext.NeutralNormals;
                RenderContext.Ambient = new Vector4(0.2f, 0.2f, 0.4f, 0);
                RenderContext.ApplyChanges();

                renderTree.SceneGraph.Draw(RenderContext);

                RenderContext.LightingEnabled = true;
                RenderContext.Ambient = Vector4.Zero;
                RenderContext.World = Matrix.Identity;
                RenderContext.Texture = RenderContext.White;
            }
        }
    }
}
