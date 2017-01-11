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

        public RenderContext RenderContext { get; private set; }
        protected Vector2 MousePosition;

        public Gem.Render.FreeCamera Camera;
        public WorldRenderer SceneGraph;

        public float ElapsedSeconds { get; private set; }

        public Simulation Sim { get; private set; }

        float CameraDistance = -12;
        Vector3 CameraFocus = new Vector3(0.0f, 0.0f, 3.0f);
        private float CameraYaw = 0.25f;
        private float CameraPitch = 0.0f;

        public Gum.Widget BlockChooser = null;
        private GuiTool SelectedTool;

        void IScreen.Begin()
        {
            RenderContext = new RenderContext(Main.Content.Load<Effect>("Content/draw"), Main.GraphicsDevice);
            Camera = new Gem.Render.FreeCamera(new Vector3(0, 0, 0), Vector3.UnitY, Vector3.UnitZ, Main.GraphicsDevice.Viewport);
            Input = new Input(Main.InputMapper);
            GuiRoot = new Gum.Root(new Point(640, 480), Main.GuiSkin);

            var blocks = BlockSet.FromReflection();
            blocks.Tiles = new TileSheet(Main.Content.Load<Texture2D>("Content/gnome_colony_skin/tiles"), 16, 16);

            Sim = new Simulation(blocks);

            SceneGraph = new WorldRenderer(Sim.World, blocks, 2);
            SceneGraph.UpdateGeometry();

            Camera.Position = CameraFocus + new Vector3(0, -4, 3);

            #region Prepare Input

            Input.BindKeyAction(Keys.Right, "RIGHT", KeyBindingType.Held, () => CameraYaw += ElapsedSeconds);
            Input.BindKeyAction(Keys.Left, "LEFT", KeyBindingType.Held, () => CameraYaw -= ElapsedSeconds);
            Input.BindKeyAction(Keys.Up, "UP", KeyBindingType.Held, () => CameraPitch += ElapsedSeconds);
            Input.BindKeyAction(Keys.Down, "DOWN", KeyBindingType.Held, () => CameraPitch -= ElapsedSeconds);

            Input.BindKeyAction(Keys.A, "PAN-LEFT", KeyBindingType.Held, () =>
                {
                    CameraFocus -= Vector3.Normalize(new Vector3(Camera.GetEyeVector().Y, -Camera.GetEyeVector().X, 0)) * ElapsedSeconds * 10.0f;
                });

            Input.BindKeyAction(Keys.D, "PAN-RIGHT", KeyBindingType.Held, () =>
            {
                CameraFocus += Vector3.Normalize(new Vector3(Camera.GetEyeVector().Y, -Camera.GetEyeVector().X, 0)) * ElapsedSeconds * 10.0f;
            });

            Input.BindKeyAction(Keys.W, "PAN-FORWARD", KeyBindingType.Held, () =>
            {
                CameraFocus += Vector3.Normalize(new Vector3(Camera.GetEyeVector().X, Camera.GetEyeVector().Y, 0)) * ElapsedSeconds * 10.9f;
            });

            Input.BindKeyAction(Keys.S, "PAN-BACK", KeyBindingType.Held, () =>
            {
                CameraFocus -= Vector3.Normalize(new Vector3(Camera.GetEyeVector().X, Camera.GetEyeVector().Y, 0)) * ElapsedSeconds * 10.9f;
            });

            Input.BindKeyAction(Keys.E, "PAN-UP", KeyBindingType.Held, () =>
            {
                CameraFocus += Vector3.UnitZ * ElapsedSeconds * 10.0f;
            });

            Input.BindKeyAction(Keys.Q, "PAN-DOWN", KeyBindingType.Held, () =>
            {
                CameraFocus -= Vector3.UnitZ * ElapsedSeconds * 10.0f;
            });

            Input.BindKeyAction(Keys.R, "CAMERA-DISTANCE-FAR", KeyBindingType.Held, () => CameraDistance = -24.0f);
            Input.BindKeyAction(Keys.T, "CAMERA-DISTANCE-SUPER", KeyBindingType.Held, () => CameraDistance = -128.0f);
            Input.BindKeyAction(Keys.Y, "WIREFRAME", KeyBindingType.Held, () => SceneGraph.WireFrameMode = true);
            
            Input.BindKeyAction(Keys.F, "ROTATE-BLOCK", KeyBindingType.Pressed);

            #endregion

            GuiRoot.RootItem.AddChild(
                new Gum.Widget
                {
                    Rect = new Rectangle(8, 8, 64, 64),
                    Background = new Gum.TileReference("tiles", 482),
                    OnClick = SetupBlockChooser
                });

            GuiRoot.RootItem.AddChild(
                new Gum.Widget
                {
                    Rect = new Rectangle(8, 76, 64, 64),
                    Background = new Gum.TileReference("tiles", 483),
                    OnClick = (sender, args) =>
                    {
                        if (BlockChooser != null)
                            GuiRoot.RootItem.RemoveChild(BlockChooser);
                        BlockChooser = null;
                        SelectTool(new Creative.Mine());
                    }
                });
        }

        private void SelectTool(GuiTool NewTool)
        {
            if (SelectedTool != null) SelectedTool.OnDeselected(this);
            SelectedTool = NewTool;
            if (SelectedTool != null) SelectedTool.OnSelected(this);
        }

        private void SetupBlockChooser(Gum.Widget _sender, Gum.InputEventArgs _args)
        {
            if (BlockChooser != null) return;

            BlockChooser = GuiRoot.RootItem.AddChild(new Gum.Widget
            {
                Rect = new Rectangle(96, 8, 512, 128),
                Border = "border-one"
            });

            var x = 96 + 8;
            foreach (var template in Sim.Blocks.Templates.Where(t => t.Value.ShowInEditor))
            {
                var lambdaTemplate = template;
                BlockChooser.AddChild(new BlockButton
                {
                    Rect = new Rectangle(x, 16, 32, 32),
                    Template = template.Value,
                    OnClick = (sender, args) =>
                        {
                            GuiRoot.RootItem.RemoveChild(BlockChooser);
                            BlockChooser = null;
                            SelectTool(new Creative.Build(lambdaTemplate.Value));
                        }
                });

                x += 32;
            }
        }

        void IScreen.End()
        {
        }
                
        void IScreen.Update(float ElapsedSeconds)
        {
            this.ElapsedSeconds = ElapsedSeconds;

            if (SelectedTool != null &&
                GuiRoot.RootItem.FindWidgetAt((int)MousePosition.X, (int)MousePosition.Y) == null)
            {
                var pickRay = Camera.GetPickRay(MousePosition);
                SceneGraph.CalculateLocalMouse(pickRay, SelectedTool.HiliteFaces);
            }

            SceneGraph.WireFrameMode = false;
            CameraDistance = -6.0f;

            Input.FireActions(GuiRoot, (msg, args) =>
            {
                if (msg == Gum.InputEvents.MouseClick || msg == Gum.InputEvents.MouseMove)
                {
                    MousePosition = new Vector2(args.X, args.Y);
                }

                if (msg == Gum.InputEvents.MouseClick &&
                    SceneGraph.MouseHover &&
                    SelectedTool != null)
                {
                    SelectedTool.Apply(Sim, SceneGraph);
                }
            });

            if (CameraPitch < 0.5f) CameraPitch = 0.5f;
            if (CameraPitch > 1.5f) CameraPitch = 1.5f;

            Camera.Position =
                CameraFocus +
                Vector3.Transform(new Vector3(0, -CameraDistance, 0),
                    Matrix.CreateRotationX(CameraPitch) * Matrix.CreateRotationZ(CameraYaw));
            Camera.LookAt(CameraFocus, Vector3.UnitZ);

            if (SelectedTool != null)
            {
                if (SceneGraph.MouseHover)
                    SelectedTool.Hover(Sim, SceneGraph);
                else
                    SelectedTool.UnHover();
            }           
        }

        public void Draw(float elapsedSeconds)
        {
            SceneGraph.UpdateGeometry();

            Main.GraphicsDevice.SetRenderTarget(null);
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
            RenderContext.ActiveLightCount = 0;
            RenderContext.Texture = RenderContext.White;
            RenderContext.NormalMap = RenderContext.NeutralNormals;
            RenderContext.Ambient = new Vector4(0.2f, 0.2f, 0.4f, 0);
            RenderContext.ApplyChanges();

            SceneGraph.Draw(RenderContext);

            GuiRoot.Draw();
        }

    }
}
