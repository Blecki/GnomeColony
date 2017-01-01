using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Game
{
    public class Cell
    {
        public BlockTemplate Block;
        public BlockTemplate Decal;
        public CellLink.Directions BlockOrientation = CellLink.Directions.North;
        public Coordinate Location;
    }
}
