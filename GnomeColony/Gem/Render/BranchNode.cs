using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Render
{
    public class BranchNode : SceneNode
    {
        public BranchNode(Euler Orientation = null)
        {
            this.Orientation = Orientation;
            if (this.Orientation == null) this.Orientation = new Euler();
        }

        private List<SceneNode> children = new List<SceneNode>();

        public SceneNode Add(SceneNode child) { children.Add(child); return child; }
        public void Remove(SceneNode child) { children.Remove(child); }
        public IEnumerator<SceneNode> GetEnumerator() { return children.GetEnumerator(); }

        public override void UpdateWorldTransform(Matrix M)
        {
            base.UpdateWorldTransform(M);
            foreach (var child in this) child.UpdateWorldTransform(WorldTransform);
        }

        public override void PreDraw(float ElapsedSeconds, RenderContext Context)
        {
            foreach (var child in this) child.PreDraw(ElapsedSeconds, Context);
        }

        public override void Draw(RenderContext Context)
        {
            foreach (var child in this) child.Draw(Context);
        }

        public override void CalculateLocalMouse(Ray MouseRay, Action<SceneNode, float> HoverCallback)
        {
            foreach (var child in this) child.CalculateLocalMouse(MouseRay, HoverCallback);
        }

        public override Action HoverAction
        {
            get
            {
                return base.HoverAction;
            }
            set
            {
                foreach (var child in this) child.HoverAction = value;
                base.HoverAction = value;
            }
        }

        public override Action ClickAction
        {
            get
            {
                return base.ClickAction;
            }
            set
            {
                foreach (var child in this) child.ClickAction = value;
                base.ClickAction = value;
            }
        }
    }
}
