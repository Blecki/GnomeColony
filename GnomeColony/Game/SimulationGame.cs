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
            Sim = new Simulation(Content);

            SceneGraph = Sim.RenderModule.CreateSceneNode(Sim);

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
            Input.BindKeyAction(Keys.Y, "WIREFRAME", KeyBindingType.Held, () => RenderModule.WorldSceneNode.WireFrameMode = true);
            
            Main.Input.BindKeyAction(Keys.F, "ROTATE-BLOCK", KeyBindingType.Pressed);

            #endregion

            Main.GuiRoot.RootItem.AddChild(
                new Gum.Widget
                {
                    Rect = new Rectangle(8, 8, 64, 64),
                    Background = new Gum.TileReference("tiles", TileNames.TaskIconBuild),
                    OnClick = SetupBlockChooser
                });

            Main.GuiRoot.RootItem.AddChild(
                new Gum.Widget
                {
                    Rect = new Rectangle(8, 76, 64, 64),
                    Background = new Gum.TileReference("tiles", TileNames.TaskIconMine),
                    OnClick = (sender, args) =>
                    {
                        if (BlockChooser != null)
                            Main.GuiRoot.RootItem.RemoveChild(BlockChooser);
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

            BlockChooser = Main.GuiRoot.RootItem.AddChild(new Gum.Widget
            {
                Rect = new Rectangle(96, 8, 512, 128),
                Border = "border-one"
            });

            var x = 96 + 8;
            foreach (var template in Sim.Blocks.Templates)
            {
                var lambdaTemplate = template;
                BlockChooser.AddChild(new Gum.Widget
                {
                    Rect = new Rectangle(x, 16, 32, 32),
                    Background = new Gum.TileReference("tiles", template.Value.Preview),
                    OnClick = (sender, args) =>
                        {
                            Main.GuiRoot.RootItem.RemoveChild(BlockChooser);
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

        void IScreen.HandleInput(Gum.InputEvents Event, Gum.InputEventArgs Args)
        {
            base.HandleInput(Event, Args);

            if (Event == Gum.InputEvents.MouseClick && 
                HoverNode is RenderModule.WorldSceneNode &&
                SelectedTool != null)
            {
                SelectedTool.Apply(Sim, HoverNode as RenderModule.WorldSceneNode);
            }
        }

        void IScreen.BeforeInput()
        {
            RenderModule.WorldSceneNode.WireFrameMode = false;
            CameraDistance = -6.0f;
        }

        void IScreen.Update(float elapsedSeconds)
        {
            base.Update(elapsedSeconds);

            if (CameraPitch < 0.5f) CameraPitch = 0.5f;
            if (CameraPitch > 1.5f) CameraPitch = 1.5f;

            Camera.Position =
                CameraFocus +
                Vector3.Transform(new Vector3(0, -CameraDistance, 0),
                    Matrix.CreateRotationX(CameraPitch) * Matrix.CreateRotationZ(CameraYaw));
            Camera.LookAt(CameraFocus, Vector3.UnitZ);

            if (HoverNode is RenderModule.WorldSceneNode)
            {
                if (SelectedTool != null)
                {
                    var hoverNormal = (HoverNode as RenderModule.WorldSceneNode).HoverNormal;
                    var hoverSide = GuiTool.HiliteFace.Sides;
                    if (hoverNormal.Z > 0)
                        hoverSide = GuiTool.HiliteFace.Top;

                    if ((SelectedTool.HiliteFaces & hoverSide) == hoverSide)
                    {
                        HoverNode.SetHover();
                        SelectedTool.Hover(Sim, HoverNode as RenderModule.WorldSceneNode);
                    }
                }
            }
        }
    }
}
