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
        public Direction Orientation = Direction.North;

        public OrientedBlock() { }

        public OrientedBlock(BlockTemplate Template, Direction Orientation)
        {
            this.Template = Template;
            this.Orientation = Orientation;
        }

        public OrientedBlock(OrientedBlock Other)
        {
            CopyFrom(Other);
        }

        public void Rotate(int Steps)
        {
            while (Steps > 0)
            {
                Offset = new Coordinate(-Offset.Y, Offset.X, Offset.Z);
                Steps -= 1;
                if (Template.Orientable)
                    Orientation = Directions.Rotate(Orientation);
            }
        }

        internal void CopyFrom(OrientedBlock Other)
        {
            this.Template = Other.Template;
            this.Offset = Other.Offset;
            this.Orientation = Other.Orientation;
        }
    }
}