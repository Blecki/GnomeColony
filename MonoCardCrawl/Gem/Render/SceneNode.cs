using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Render
{
    public class SceneNode
    {
        public bool InteractWithMouse = true;

        protected Matrix WorldTransform;
        public Euler Orientation { get; set; }

        public virtual void UpdateWorldTransform(Matrix M)
        {
            WorldTransform = Orientation.Transform * M;
        }

        public virtual void PreDraw(float ElapsedSeconds, RenderContext Context) { }
        public virtual void Draw(RenderContext Context) { }
        public virtual void CalculateLocalMouse(Ray MouseRay, Action<SceneNode, float> HoverCallback) { }

        public virtual Action HoverAction { get; set; }
        public virtual Action ClickAction { get; set; }

        public virtual void SetHover() { }
        public virtual Action GetHoverAction() { return HoverAction; }
        public virtual Action GetClickAction() { return ClickAction; }

        public Ray GetLocalMouseRay(Ray MouseRay)
        {
            MouseRay.Direction = Vector3.Normalize(MouseRay.Direction);

            var inverseTransform = Matrix.Invert(WorldTransform);
            var localMouseSource = Vector3.Transform(MouseRay.Position, inverseTransform);

            var forwardPoint = MouseRay.Position + MouseRay.Direction;
            forwardPoint = Vector3.Transform(forwardPoint, inverseTransform);

            return new Ray(localMouseSource, forwardPoint - localMouseSource);
        }
    }
}
