using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Game.Game.Tiles
{ 
    public class FloorTile : Tile
    {
        public override void Create(Gem.PropertyBag Properties)
        {
            Properties.Upsert("render-mesh", Gem.Geo.Gen.TransformCopy(Gem.Geo.Gen.CreateTexturedFacetedCube(), Matrix.CreateTranslation(0, 0, 0.5f) * Matrix.CreateScale(1, 1, 0.25f)));
            Properties.Upsert("nav-mesh", Gem.Geo.Gen.TransformCopy(Gem.Geo.Gen.CreateQuad(), Matrix.CreateTranslation(0, 0, 0.25f)));

            base.Create(Properties);
        }
    }
}
