using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gnome
{
    public class ActorSceneNode : Gem.Render.SceneNode
    {
        private List<Actor> Source;

        public ActorSceneNode(List<Actor> Source)
        {
            this.Source = Source;
        }

        public IEnumerable<Gem.Render.SceneNode> Nodes
        {
            get
            {
                foreach (var actor in Source)
                {
                    if (actor.Renderable != null) yield return actor.Renderable;
                    //if (actor.PopupGui != null) yield return actor.PopupGui;
                }
            }
        }

        public override void UpdateWorldTransform(Microsoft.Xna.Framework.Matrix M)
        {
            foreach (var node in Nodes) node.UpdateWorldTransform(M);
        }

        public override void PreDraw(float ElapsedSeconds, Gem.Render.RenderContext Context)
        {
            foreach (var node in Nodes) node.PreDraw(ElapsedSeconds, Context);
        }

        public override void Draw(Gem.Render.RenderContext Context)
        {
            foreach (var node in Nodes) node.Draw(Context);
        }

        public override void CalculateLocalMouse(Microsoft.Xna.Framework.Ray MouseRay, Action<Gem.Render.SceneNode, float> HoverCallback)
        {
            foreach (var node in Nodes) node.CalculateLocalMouse(MouseRay, HoverCallback);
        }
    }
}
