using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Game.Actors
{
    public class StaticActorPropertyBag : Gem.PropertyBag
    {
        public Texture2D Texture
        {
            get { return GetPropertyAs<Texture2D>("texture"); }
            set { Upsert("texture", value); }
        }

        public Gem.Geo.Mesh Mesh
        {
            get { return GetPropertyAs<Gem.Geo.Mesh>("mesh"); }
            set { Upsert("mesh", value); }
        }
    }

    public class StaticMeshActor : Actor
    {
        public override void Create(Gem.PropertyBag Properties)
        {
            base.Create(Properties);

            Renderable = new Gem.Render.MeshNode(
                Properties.GetPropertyAs<Gem.Geo.Mesh>("mesh"),
                Properties.GetPropertyAs<Texture2D>("texture"),
                null,
                Orientation);
        }
    }
}
