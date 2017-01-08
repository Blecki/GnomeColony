using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates
{
    public class GrassSlope : BlockTemplate
    {
        public GrassSlope()
        {
            PreviewTiles = HelperExtensions.MakeList(new OrientedTile(35, Direction.North));
            Top = 33;
            NorthSide = 32;
            SouthSide = 32;
            EastSide = 35;
            WestSide = 35;
            Bottom = 34;
            Shape = BlockShape.Slope;
            Orientable = true;
            //Hanging = "HangingVines";
        }
    }
}
