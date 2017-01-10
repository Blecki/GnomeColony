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
    public class SimulationGame : Game, IScreen
    {
        float CameraDistance = -12;
        Vector3 CameraFocus = new Vector3(0.0f, 0.0f, 3.0f);
        public Simulation Sim { get; private set; }
        private float CameraYaw = 0.25f;
        private float CameraPitch = 0.0f;

        public Gum.Widget BlockChooser = null;
        private GuiTool SelectedTool;

        void IScreen.Begin()
        {
            base.Begin();

            var blocks = BlockSet.FromReflection();
            blocks.Tiles = new TileSheet(Main.Content.Load<Texture2D>("Content/gnome_colony_skin/tiles"), 16, 16);

            Sim = new Simulation(blocks);

            SceneGraph = new WorldSceneNode(Sim.World, new WorldSceneNodeProperties
            {
                HiliteTexture = 2,
                BlockSet = Sim.Blocks
            });
            SceneGraph.UpdateWorldTransform(Matrix.Identity);
            (SceneGraph as WorldSceneNode).UpdateGeometry();

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
            Input.BindKeyAction(Keys.Y, "WIREFRAME", KeyBindingType.Held, () => WorldSceneNode.WireFrameMode = true);
            
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
        
        void IScreen.Update(float elapsedSeconds)
        {
            base.Update(elapsedSeconds);

            WorldSceneNode.WireFrameMode = false;
            CameraDistance = -6.0f;

            Input.FireActions(GuiRoot, (msg, args) =>
                {
                    base.HandleInput(msg, args);

                    if (msg == Gum.InputEvents.MouseClick &&
                        HoverNode is WorldSceneNode &&
                        SelectedTool != null)
                    {
                        SelectedTool.Apply(Sim, HoverNode as WorldSceneNode);
                    }
                });

            if (CameraPitch < 0.5f) CameraPitch = 0.5f;
            if (CameraPitch > 1.5f) CameraPitch = 1.5f;

            Camera.Position =
                CameraFocus +
                Vector3.Transform(new Vector3(0, -CameraDistance, 0),
                    Matrix.CreateRotationX(CameraPitch) * Matrix.CreateRotationZ(CameraYaw));
            Camera.LookAt(CameraFocus, Vector3.UnitZ);

            if (HoverNode is WorldSceneNode)
            {
                if (SelectedTool != null)
                {
                    var hoverNormal = (HoverNode as WorldSceneNode).HoverNormal;
                    var hoverSide = GuiTool.HiliteFace.Sides;
                    if (hoverNormal.Z > 0)
                        hoverSide = GuiTool.HiliteFace.Top;

                    if ((SelectedTool.HiliteFaces & hoverSide) == hoverSide)
                    {
                        HoverNode.SetHover();
                        SelectedTool.Hover(Sim, HoverNode as WorldSceneNode);
                    }
                    else
                    {
                        SelectedTool.UnHover();
                    }
                }
            }
        }
    }
}
