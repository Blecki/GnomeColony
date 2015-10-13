using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Game
{
    public class TilePropertyBag : Gem.PropertyBag
    {
        public Texture2D Texture
        {
            get { return GetPropertyAs<Texture2D>("texture"); }
            set { Upsert("texture", value); }
        }

        public Texture2D NormalMap
        {
            get { return GetPropertyAs<Texture2D>("normal-map"); }
            set { Upsert("normal-map", value); }
        }

        public Gem.Geo.Mesh RenderMesh
        {
            get { return GetPropertyAs<Gem.Geo.Mesh>("render-mesh"); }
            set { Upsert("render-mesh", value); }
        }

        public Gem.Geo.Mesh NavigationMesh
        {
            get { return GetPropertyAs<Gem.Geo.Mesh>("nav-mesh"); }
            set { Upsert("nav-mesh", value); }
        }
    }

    public class Tile : SharpRuleEngine.RuleObject
    {
        public World World;

        public Gem.Geo.Mesh RenderMesh;
        public Gem.Geo.Mesh NavigationMesh;
        public Texture2D Texture;
        public Texture2D NormalMap;

        public override SharpRuleEngine.RuleEngine GlobalRules { get { return World.GlobalRules; } }

        public virtual bool Combinable(Tile Other) 
        {
            return Object.ReferenceEquals(this.Texture, Other.Texture);
        }

        public virtual void Create(Gem.PropertyBag Properties)
        {
            RenderMesh = Properties.GetPropertyAs<Gem.Geo.Mesh>("render-mesh");
            NavigationMesh = Properties.GetPropertyAs<Gem.Geo.Mesh>("nav-mesh", () => null);
            Texture = Properties.GetPropertyAs<Texture2D>("texture");
            NormalMap = Properties.GetPropertyAs<Texture2D>("normal-map", () => null);
        }
    }
}
