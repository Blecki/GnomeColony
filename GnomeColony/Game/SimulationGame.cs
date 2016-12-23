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
        private FreeCamera Camera;

        void IScreen.Begin()
        {
            base.Begin();
            Sim = new Simulation(Content, 1.0f);
            Camera = new Gem.Render.FreeCamera(new Vector3(0, 0, 0), Vector3.UnitY, Vector3.UnitZ, Main.GraphicsDevice.Viewport);

            RenderTrees.Add(new RenderTree
            {
                Camera = Camera,
                SceneGraph = Sim.CreateSceneNode()
            });

            Camera.Position = CameraFocus + new Vector3(0, -4, 3);

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
            Main.Input.AddBinding("PAN-UP", new KeyboardBinding(Keys.E, KeyBindingType.Held));
            Main.Input.AddBinding("PAN-DOWN", new KeyboardBinding(Keys.Q, KeyBindingType.Held));
            Main.Input.AddBinding("LEFT-CLICK", new MouseButtonBinding("LeftButton", KeyBindingType.Pressed));
            Main.Input.AddBinding("RIGHT-CLICK", new MouseButtonBinding("RightButton", KeyBindingType.Pressed));

            Main.Input.AddBinding("CAMERA-DISTANCE-TOGGLE", new KeyboardBinding(Keys.R, KeyBindingType.Held));
            Main.Input.AddBinding("SUPER-CAM-TOGGLE", new KeyboardBinding(Keys.T, KeyBindingType.Held));
            Main.Input.AddBinding("WIREFRAME-TOGGLE", new KeyboardBinding(Keys.Y, KeyBindingType.Held));

            Main.ScriptBuilder.DeriveScriptsFrom("Gnome.ScriptBase");

            var guiTools = new List<GuiTool>();
            guiTools.Add(new Creative.Build());
            guiTools.Add(new Creative.Mine());

            PushInputState(new CommandInput(Sim, guiTools));

            #endregion
        }

        void IScreen.End()
        {
        }

        void IScreen.Update(float elapsedSeconds)
        {
            base.Update(elapsedSeconds);

            if (Main.Input.Check("RIGHT")) CameraYaw += elapsedSeconds;
            if (Main.Input.Check("LEFT")) CameraYaw -= elapsedSeconds;
            if (Main.Input.Check("UP")) CameraPitch += elapsedSeconds;
            if (Main.Input.Check("DOWN")) CameraPitch -= elapsedSeconds;

            if (Main.Input.Check("PAN-FORWARD"))
                CameraFocus += Vector3.Normalize(new Vector3(Camera.GetEyeVector().X, Camera.GetEyeVector().Y, 0)) * elapsedSeconds * 10;
            if (Main.Input.Check("PAN-BACK"))
                CameraFocus -= Vector3.Normalize(new Vector3(Camera.GetEyeVector().X, Camera.GetEyeVector().Y, 0)) * elapsedSeconds * 10;
            if (Main.Input.Check("PAN-LEFT"))
                CameraFocus -= Vector3.Normalize(new Vector3(Camera.GetEyeVector().Y, -Camera.GetEyeVector().X, 0)) * elapsedSeconds * 10;
            if (Main.Input.Check("PAN-RIGHT"))
                CameraFocus += Vector3.Normalize(new Vector3(Camera.GetEyeVector().Y, -Camera.GetEyeVector().X, 0)) * elapsedSeconds * 10;
            if (Main.Input.Check("PAN-UP"))
                CameraFocus += Vector3.UnitZ * elapsedSeconds * 10;
            if (Main.Input.Check("PAN-DOWN"))
                CameraFocus -= Vector3.UnitZ * elapsedSeconds * 10;

            if (Main.Input.Check("SUPER-CAM-TOGGLE"))
                CameraDistance = -128.0f;
            else if (Main.Input.Check("CAMERA-DISTANCE-TOGGLE"))
                CameraDistance = -24.0f;
            else 
                CameraDistance = -6.0f;

            if (CameraPitch < 0.5f) CameraPitch = 0.5f;
            if (CameraPitch > 1.5f) CameraPitch = 1.5f;

            Camera.Position =
                CameraFocus +
                Vector3.Transform(new Vector3(0, -CameraDistance, 0),
                    Matrix.CreateRotationX(CameraPitch) * Matrix.CreateRotationZ(CameraYaw));
            Camera.LookAt(CameraFocus, Vector3.UnitZ);

            Sim.Update(this, elapsedSeconds);

            RenderModule.WorldSceneNode.WireFrameMode = Main.Input.Check("WIREFRAME-TOGGLE");
        }

    }
}
