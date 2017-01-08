using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates
{
    public class Grass : BlockTemplate
    {
        public Grass()
        {
            PreviewTiles = HelperExtensions.MakeList(new OrientedTile(32, Direction.North));
            Top = 33;
            NorthSide = 32;
            Bottom = 34;
            Shape = BlockShape.Cube;
            Hanging = "HangingVines";
        }
    }
}
