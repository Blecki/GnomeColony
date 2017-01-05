using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public enum BlockShape
    {
        Cube,
        Slope,
        LowerSlab,
        UpperSlab,
        HalfSlopeLow,
        HalfSlopeHigh,
        Decal,
        Combined
    }
    
    public class PhantomBlock
    {
        public BlockTemplate Block;
        public Coordinate Offset = new Coordinate(0, 0, 0);
        public CellLink.Directions Orientation = CellLink.Directions.North;
        public bool PlacementAllowed = false;
        public bool WillCombine = false;
        public Coordinate FinalPosition;
        public OrientedBlock TargetCell;
        public BlockTemplate FinalBlock;

        public PhantomBlock() { }

        public PhantomBlock(OrientedBlock Source)
        {
            this.Block = Source.Template;
            this.Offset = Source.Offset;
            this.Orientation = Source.Orientation;
        }

        public void Rotate(int Steps)
        {
            while (Steps > 0)
            {
                Offset = new Coordinate(Offset.Y, -Offset.X, Offset.Z);
                Steps -= 1;
                Orientation = CellLink.Rotate(Orientation);
            }
        }
    }

    public class BlockTemplate
    {
        public String Name;
        public int Preview = 1;
        public int Top = 1;
        public int NorthSide = -1;
        public int SouthSide = -1;
        public int EastSide = -1;
        public int WestSide = -1;
        public int Bottom = -1;
        public String Hanging = null;
        public BlockShape Shape;

        public bool Orientable = false;

        public List<OrientedBlock> CompositeBlocks;

        public bool ShowInEditor = true;

        public virtual bool CanComposite(OrientedBlock Onto, CellLink.Directions MyOrientation) 
        { 
            return false; 
        }

        public virtual OrientedBlock Compose(
            OrientedBlock With, 
            CellLink.Directions MyOrientation,
            BlockSet TemplateSet) 
        {
            return new OrientedBlock
                {
                    Template = this,
                    Orientation = MyOrientation
                };
        }

        public virtual void Initialize(BlockSet BlockSet) { }

        public OrientedBlock GetTopOfComposite()
        {
            if (Shape == BlockShape.Combined)
                return CompositeBlocks[CompositeBlocks.Count - 1];
            else
                return new OrientedBlock
                {
                    Template = this,
                    Offset = new Coordinate(0, 0, 0),
                    Orientation = CellLink.Directions.North
                };
        }

        public OrientedBlock SansTopOfComposite()
        {
            if (Shape == BlockShape.Combined)
            {
                if (CompositeBlocks.Count == 2)
                    return new OrientedBlock { Template = CompositeBlocks[0].Template, Orientation = CompositeBlocks[0].Orientation };
                else
                {
                    var r = new OrientedBlock
                    {
                        Template = new BlockTemplate
                            {
                                Shape = BlockShape.Combined
                            },
                        Orientation = CellLink.Directions.North
                    };
                    r.Template.CompositeBlocks.AddRange(CompositeBlocks.Take(CompositeBlocks.Count - 1));
                    return r;
                }
            }
            else
                return new OrientedBlock { Template = this, Orientation = CellLink.Directions.North };
        }

        public BlockTemplate ComposeWith(CellLink.Directions MyOrientation, OrientedBlock NewTop)
        {
            if (Shape == BlockShape.Combined)
            {
                var r = new BlockTemplate
                {
                    Shape = BlockShape.Combined
                };

                r.CompositeBlocks.AddRange(CompositeBlocks);
                foreach (var sub in r.CompositeBlocks)
                    sub.Orientation = CellLink.Add(MyOrientation, sub.Orientation);
                r.CompositeBlocks.Add(new OrientedBlock
                    {
                        Template = NewTop.Template,
                        Orientation = NewTop.Orientation
                    });
                return r;
            }
            else
            {
                return new BlockTemplate
                {
                    Shape = BlockShape.Combined,
                    CompositeBlocks = HelperExtensions.MakeList(
                        new OrientedBlock { Template = this, Orientation = MyOrientation },
                        new OrientedBlock { Template = NewTop.Template, Orientation = NewTop.Orientation }
                        )
                };
            }
        }
    }
}
