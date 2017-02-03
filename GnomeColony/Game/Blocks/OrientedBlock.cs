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

        public OrientedBlock GetTopOfComposite()
        {
            if (Template.Shape == BlockShape.Combined)
            {
                if (Orientation != Direction.North) throw new InvalidOperationException();
                return Template.CompositeBlocks[Template.CompositeBlocks.Count - 1];
            }
            else
                return new OrientedBlock
                {
                    Template = Template,
                    Offset = new Coordinate(0, 0, 0),
                    Orientation = Orientation
                };
        }

        public OrientedBlock GetBottomOfComposite()
        {
            if (Template.Shape == BlockShape.Combined)
            {
                if (Orientation != Direction.North) throw new InvalidOperationException();
                return Template.CompositeBlocks[0];
            }
            else
                return new OrientedBlock
                {
                    Template = Template,
                    Offset = new Coordinate(0, 0, 0),
                    Orientation = Orientation
                };
        }

        public OrientedBlock SansTopOfComposite()
        {
            if (Template.Shape == BlockShape.Combined)
            {
                if (Orientation != Direction.North) throw new InvalidOperationException();

                if (Template.CompositeBlocks.Count == 2)
                    return new OrientedBlock 
                    { 
                        Template = Template.CompositeBlocks[0].Template,
                        Orientation = Template.CompositeBlocks[0].Orientation
                    };
                else
                    return new OrientedBlock
                    {
                        Template = new BlockTemplate
                            {
                                Shape = BlockShape.Combined,
                                CompositeBlocks = Template.CompositeBlocks.Take(Template.CompositeBlocks.Count - 1).ToList()
                            },
                        Orientation = Direction.North
                    };
            }
            else
                return new OrientedBlock { Template = Template, Orientation = Orientation };
        }

        public OrientedBlock ComposeWith(OrientedBlock NewTop)
        {
            if (Template.Shape == BlockShape.Combined)
            {
                if (Orientation != Direction.North) throw new InvalidOperationException();

                var r = new BlockTemplate
                {
                    Shape = BlockShape.Combined
                };

                r.CompositeBlocks.AddRange(Template.CompositeBlocks);
                r.CompositeBlocks.Add(new OrientedBlock
                    {
                        Template = NewTop.Template,
                        Orientation = NewTop.Orientation
                    });

                return new OrientedBlock
                {
                    Template = r,
                    Orientation = Direction.North
                };
            }
            else
            {
                return new OrientedBlock
                {
                    Template = new BlockTemplate
                    {
                        Shape = BlockShape.Combined,
                        CompositeBlocks = HelperExtensions.MakeList(
                            new OrientedBlock { Template = Template, Orientation = Orientation },
                            new OrientedBlock { Template = NewTop.Template, Orientation = NewTop.Orientation }
                            )
                    },
                    Orientation = Direction.North
                };
            }
        }
    }
}