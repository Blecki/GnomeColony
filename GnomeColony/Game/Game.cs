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

        EpisodeContentManager Content;
        float CameraDistance = -12;
        Vector3 CameraFocus = new Vector3(8.0f, 8.0f, 3.0f);
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
        public Simulation Sim { get; private set; }

        private float CameraYaw = 0.25f;
        private float CameraPitch = 0.0f;

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
            Sim = new Simulation(Content, 1.0f);

            RenderContext = new RenderContext(Content.Load<Effect>("draw"), Main.GraphicsDevice);

            RenderTrees.Add(new RenderTree
            {
                Camera = new Gem.Render.FreeCamera(new Vector3(0, 0, 0), Vector3.UnitY, Vector3.UnitZ, Main.GraphicsDevice.Viewport),
                SceneGraph = Sim.CreateSceneNode()
            });

            (RenderTrees[0].Camera as FreeCamera).Position = CameraFocus + new Vector3(0, -4, 3);

            #region Prepare Input

            Main.Input.ClearBindings();
            Main.Input.AddAxis("MAIN-AXIS", new MouseAxisBinding());
            Main.Input.AddBinding("RIGHT", new KeyboardBinding(Keys.Right, KeyBindingType.Held));
            Main.Input.AddBinding("LEFT", new KeyboardBinding(Keys.Left, KeyBindingType.Held));
            Main.Input.AddBinding("UP", new KeyboardBinding(Keys.Up, KeyBindingType.Held));
            Main.Input.AddBinding("DOWN", new KeyboardBinding(Keys.Down, KeyBindingType.Held));
            Main.Input.AddBinding("PAN-LEFT", new KeyboardBinding(Keys.A, KeyBindingType.Held));
            Main.Input.AddBinding("PAN-FORWARD", new KeyboardBinding(Keys.W, KeyBindingType.Held));
            Main.Input.AddBinding("PAN-RIGHT", new KeyboardBinding(Keys.D, KeyBindingType.Held));
            Main.Input.AddBinding("PAN-BACK", new KeyboardBinding(Keys.S, KeyBindingType.Held));
            Main.Input.AddBinding("LEFT-CLICK", new MouseButtonBinding("LeftButton", KeyBindingType.Pressed));
            Main.Input.AddBinding("RIGHT-CLICK", new MouseButtonBinding("RightButton", KeyBindingType.Pressed));

            Main.Input.AddBinding("CAMERA-DISTANCE-TOGGLE", new KeyboardBinding(Keys.R, KeyBindingType.Held));

            Main.ScriptBuilder.DeriveScriptsFrom("Gnome.ScriptBase");

            var guiTools = new List<GuiTool>();
            guiTools.Add(new GuiTools.Build());
            guiTools.Add(new GuiTools.Mine());
            guiTools.Add(new GuiTools.MarkStorehouse());

            PushInputState(new HoverTest(Sim.Blocks.BlockTemplates, Sim.Blocks.BlockTiles, guiTools));

            #endregion
        }

        public void End()
        {
        }

        public void Update(float elapsedSeconds)
        {
            this.ElapsedSeconds = elapsedSeconds;

            if (Main.Input.Check("RIGHT")) CameraYaw += elapsedSeconds;
            if (Main.Input.Check("LEFT")) CameraYaw -= elapsedSeconds;
            if (Main.Input.Check("UP")) CameraPitch += elapsedSeconds;
            if (Main.Input.Check("DOWN")) CameraPitch -= elapsedSeconds;

            if (Main.Input.Check("CAMERA-DISTANCE-TOGGLE"))
                CameraDistance = -24.0f;
            else CameraDistance = -14.0f;

            if (CameraPitch < 0.5f) CameraPitch = 0.5f;
            if (CameraPitch > 1.5f) CameraPitch = 1.5f;

            (RenderTrees[0].Camera as FreeCamera).Position =
                CameraFocus +
                Vector3.Transform(new Vector3(0, -CameraDistance, 0),
                    Matrix.CreateRotationX(CameraPitch) * Matrix.CreateRotationZ(CameraYaw));
            (RenderTrees[0].Camera as FreeCamera).LookAt(CameraFocus, Vector3.UnitZ);
            
            Sim.Update(this, elapsedSeconds);
                        
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
                RenderContext.ApplyChanges();

                renderTree.SceneGraph.Draw(RenderContext);
                RenderContext.LightingEnabled = true;


                RenderContext.World = Matrix.Identity;
                RenderContext.Texture = RenderContext.White;
            }
            
            //World.NavMesh.DebugRender(RenderContext);
            //if (HitFace != null) 
            //    World.NavMesh.DebugRenderFace(RenderContext, HitFace);
        }
    }
}
