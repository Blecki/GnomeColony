using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates
{
    public class Lamp : BlockTemplate
    {
        public Lamp()
        {
            PreviewTiles = HelperExtensions.MakeList(new OrientedTile(224, Direction.North));
            Top = 224;
            NorthSide = 224;
            Bottom = 224;
            Shape = BlockShape.Cube;
            Transparent = true;
            EmitsLight = true;
        }
    }
}
