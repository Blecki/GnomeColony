using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class OrientedBlock
    {
        public BlockTemplate Template;
        public Coordinate Offset = new Coordinate(0, 0, 0);
        public CellLink.Directions Orientation = CellLink.Directions.North;

        public OrientedBlock() { }
        public OrientedBlock(BlockTemplate Template, CellLink.Directions Orientation)
        {
            this.Template = Template;
            this.Orientation = Orientation;
        }
    }
}