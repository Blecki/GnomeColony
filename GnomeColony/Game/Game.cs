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
        public Gem.Input Input { get; private set; }
        public Main Main { get; set; }
        public Gum.Root GuiRoot;

        protected EpisodeContentManager Content;
        public RenderContext RenderContext { get; private set; }
        public SceneNode HoverNode { get; private set; }
        private Vector2 MousePosition;

        public Gem.Render.FreeCamera Camera;
        public Gem.Render.SceneNode SceneGraph;
        
        public float ElapsedSeconds { get; private set; }

        public Game()
        {
        }

        public void Begin()
        {
            Content = new EpisodeContentManager(Main.EpisodeContent.ServiceProvider, "Content");
            RenderContext = new RenderContext(Content.Load<Effect>("draw"), Main.GraphicsDevice);
            Camera = new Gem.Render.FreeCamera(new Vector3(0, 0, 0), Vector3.UnitY, Vector3.UnitZ, Main.GraphicsDevice.Viewport);
            Input = new Input(Main.InputMapper);
            GuiRoot = new Gum.Root(new Point(640, 480), Main.GuiSkin);
        }

        public void End()
        {
        }

        public void HandleInput(Gum.InputEvents Event, Gum.InputEventArgs Args)
        {
            if (Event == Gum.InputEvents.MouseClick || Event == Gum.InputEvents.MouseMove)
            {
                MousePosition = new Vector2(Args.X, Args.Y);
            }
        }
        
        public void Update(float ElapsedSeconds)
        {
            HoverNode = null;

            if (GuiRoot.RootItem.FindWidgetAt((int)MousePosition.X, (int)MousePosition.Y) == null)
            {
                var hoverItems = new List<HoverItem>();
                var pickRay = Camera.GetPickRay(MousePosition);

                SceneGraph.CalculateLocalMouse(pickRay, (node, distance) => hoverItems.Add(new HoverItem { Node = node, Distance = distance }));

                if (hoverItems.Count > 0)
                {
                    var nearestDistance = float.PositiveInfinity;
                    foreach (var hoverItem in hoverItems)
                        if (hoverItem.Distance < nearestDistance) nearestDistance = hoverItem.Distance;
                    HoverNode = hoverItems.First(item => item.Distance <= nearestDistance).Node;
                }
            }         

            this.ElapsedSeconds = ElapsedSeconds;
            SceneGraph.UpdateWorldTransform(Matrix.Identity);
        }

        private struct HoverItem
        {
            public SceneNode Node;
            public float Distance;
        }

        public void Draw(float elapsedSeconds)
        {
            var viewport = Main.GraphicsDevice.Viewport;


            RenderContext.Camera = Camera;
            SceneGraph.PreDraw(elapsedSeconds, RenderContext);


            Main.GraphicsDevice.SetRenderTarget(null);
            Main.GraphicsDevice.Viewport = viewport;
            Main.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            Main.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Main.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            Main.GraphicsDevice.Clear(ClearOptions.Target, Color.CornflowerBlue, 0xFFFFFF, 0);

            RenderContext.Camera = Camera;
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

            SceneGraph.Draw(RenderContext);

            RenderContext.LightingEnabled = true;
            RenderContext.Ambient = Vector4.Zero;
            RenderContext.World = Matrix.Identity;
            RenderContext.Texture = RenderContext.White;

            GuiRoot.Draw();
        }
    }
}
