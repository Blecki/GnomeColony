using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;
using Gem.Gui;

namespace Gem.Gui
{
    public class GuiSceneNode : Render.SceneNode
    {
        internal Render.OrthographicCamera uiCamera = null;
        public UIItem uiRoot = null;
        internal RenderTarget2D renderTarget = null;
        private Geo.Mesh Mesh;

        internal bool MouseHover = false;
        internal int LocalMouseX = 0;
        internal int LocalMouseY = 0;
        internal UIItem HoverItem = null;
        
        public Vector2 LayoutScaling = Vector2.One;

        public float DistanceBias = 0.0f;
        public bool RenderOnTop = false;
        public Gem.Render.ICamera OverrideCamera;

        GraphicsDevice Device;

        public GuiSceneNode(Geo.Mesh Mesh, GraphicsDevice device, int width, int height, Euler Euler = null)
        {
            this.Device = device;
            this.Mesh = Mesh;

            this.Orientation = Euler;
            if (this.Orientation == null) this.Orientation = new Euler();

            uiCamera = new Render.OrthographicCamera(new Viewport(0, 0, width, height));
            uiRoot = new UIItem(Shape.CreateQuad(0, 0, width, height), null);

            uiCamera.focus = new Vector2(width / 2, height / 2);

            renderTarget = new RenderTarget2D(device, uiCamera.Viewport.Width, uiCamera.Viewport.Height);
        }

        public void ClearUI() { uiRoot.children.Clear(); }

        private static float ScalarProjection(Vector3 A, Vector3 B)
        {
            return Vector3.Dot(A, B) / B.Length();
        }

        public override void CalculateLocalMouse(Ray MouseRay, Action<Gem.Render.SceneNode, float> HoverCallback)
        {
            if (HoverItem != null) HoverItem.Hover = false;
            
            var localMouse = GetLocalMouseRay(MouseRay);

            var intersection = Mesh.RayIntersection(localMouse);
            if (intersection.Intersects)
            {
                LocalMouseX = (int)System.Math.Round(intersection.UV.X * (uiCamera.Viewport.Width / LayoutScaling.X));
                LocalMouseY = (int)System.Math.Round(intersection.UV.Y * (uiCamera.Viewport.Height / LayoutScaling.Y));
                HoverItem = uiRoot.FindHoverItem(LocalMouseX, LocalMouseY);
                if (HoverItem != null) HoverCallback(this, intersection.Distance + DistanceBias);
            }
        }

        public override Action GetClickAction()
        {
            if (HoverItem != null)
                return HoverItem.GetSetting("click-action", null) as Action;
            else
                return base.GetClickAction();
        }

        public override Action GetHoverAction()
        {
            if (HoverItem != null)
            {
                HoverItem.Hover = true;
                return HoverItem.GetSetting("hover-action", null) as Action;
            }
            else
                return base.GetHoverAction();
        }
        
        public override void PreDraw(float ElapsedSeconds, Render.RenderContext Context)
        {
            Device.SetRenderTarget(renderTarget);
            Device.Clear(Color.Transparent);
            Context.Camera = uiCamera;
            Context.Color = Vector3.One;
            Context.Alpha = 1.0f;
            Context.LightingEnabled = false;
            Context.World = Matrix.CreateScale(LayoutScaling.X, LayoutScaling.Y, 1.0f);
            uiRoot.Render(Context);            
        }

        public override void Draw(Render.RenderContext Context)
        {
            Context.Color = Vector3.One;
            Context.Texture = renderTarget;
            Context.NormalMap = Context.NeutralNormals;
            Context.World = WorldTransform;
            Context.LightingEnabled = false;

            if (RenderOnTop)
                Context.Device.DepthStencilState = DepthStencilState.None;
            else
                Context.Device.DepthStencilState = DepthStencilState.Default;

            Context.ApplyChanges();
            Context.Draw(Mesh);

            Context.Device.DepthStencilState = DepthStencilState.Default;
        }
    }
}
