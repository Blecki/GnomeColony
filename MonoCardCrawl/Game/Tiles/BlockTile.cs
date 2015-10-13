using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Game.Game.Tiles
{ 
    public class BlockTile : Tile
    {
        public override void Create(Gem.PropertyBag Properties)
        {
            Properties.Upsert("render-mesh", Gem.Geo.Gen.TransformCopy(Gem.Geo.Gen.CreateTexturedFacetedCube(), Matrix.CreateTranslation(0, 0, 0.5f)));
            Properties.Upsert("nav-mesh", Gem.Geo.Gen.TransformCopy(Gem.Geo.Gen.CreateQuad(), Matrix.CreateTranslation(0, 0, 1.0f)));

            base.Create(Properties);
        }
    }
}
