using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates
{
    public class Dirt : BlockTemplate
    {
        public Dirt()
        {
            PreviewTiles = HelperExtensions.MakeList(new OrientedTile(34, Direction.North));
            Top = 34;
            NorthSide = 34;
            Bottom = 34;
            Shape = BlockShape.Cube;
        }
    }
}
