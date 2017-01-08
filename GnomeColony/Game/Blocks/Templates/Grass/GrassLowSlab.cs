using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates
{
    public class GrassLowSlab : BlockTemplate
    {
        public GrassLowSlab()
        {
            PreviewTiles = HelperExtensions.MakeList(new OrientedTile(40, Direction.North));
            Top = 33;
            NorthSide = 40;
            Bottom = 34;
            Shape = BlockShape.LowerSlab;
            //Hanging = "HangingVines";
        }
    }
}
