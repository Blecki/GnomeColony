using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Game.RenderModule
{
    public class RenderComponent : Component
    {
        public Gem.Render.SceneNode Renderable = null;
    }

    public class LightComponent : Component
    {}

    public class RenderModule
    {
        public WorldSceneNode WorldNode;
        public Dictionary<int, RenderComponent> Renderables = new Dictionary<int, RenderComponent>();
        public Dictionary<int, LightComponent> Lights = new Dictionary<int, LightComponent>();

        public void NewEntity(int ID, List<Component> Components)
        {
            foreach (var c in Components)
            {
                if (c is LightComponent) Lights.Upsert(ID, c as LightComponent);
                if (c is RenderComponent) Renderables.Upsert(ID, c as RenderComponent);
            }
        }

        public WorldSceneNode CreateSceneNode(Simulation Sim)
        {
            var worldSceneNode = new WorldSceneNode(Sim.World, new WorldSceneNodeProperties
            {
                HiliteTexture = TileNames.HoverHilite,
                BlockSet = Sim.Blocks
            });
            worldSceneNode.UpdateWorldTransform(Matrix.Identity);
            worldSceneNode.UpdateGeometry();
            return worldSceneNode;
        }
    }
}
