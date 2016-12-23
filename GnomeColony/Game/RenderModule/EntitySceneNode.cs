using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.RenderModule
{
    public class EntitySceneNode : Gem.Render.SceneNode
    {
        private RenderModule Module;

        public EntitySceneNode(RenderModule Module)
        {
            this.Module = Module;
        }

        public IEnumerable<Gem.Render.SceneNode> Nodes
        {
            get
            {
                foreach (var entity in Module.Renderables)
                    if (entity.Value.Renderable != null) yield return entity.Value.Renderable;
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
