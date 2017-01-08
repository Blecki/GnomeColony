using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates
{
    public class GrassHalfSlopeLow : BlockTemplate
    {
        public GrassHalfSlopeLow()
        {
            PreviewTiles = HelperExtensions.MakeList(new OrientedTile(38, Direction.North));
            Top = 33;
            NorthSide = 32;
            SouthSide = 40;
            EastSide = 38;
            WestSide = 38;
            Bottom = 34;
            Shape = BlockShape.HalfSlopeLow;
            Orientable = true;
            //Hanging = "HangingVines";
        }
    }
}
