using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public enum BlockType
    {
        Normal,
        Combined,
        Decal
    }

    public enum BlockShape
    {
        Cube,
        Slope,
        LowerSlab,
        UpperSlab,
        HalfSlopeLow,
        HalfSlopeHigh,
    }
    
    public class SubBlock
    {
        public BlockTemplate Block;
        public Coordinate Offset = new Coordinate(0, 0, 0);
        public CellLink.Directions Orientation = CellLink.Directions.North;
    }

    public class PhantomBlock
    {
        public BlockTemplate Block;
        public Coordinate Offset = new Coordinate(0, 0, 0);
        public CellLink.Directions Orientation = CellLink.Directions.North;
        public bool PlacementAllowed = false;
        public bool WillCombine = false;
        public Coordinate FinalPosition;
        public Cell TargetCell;
        public BlockTemplate FinalBlock;

        public PhantomBlock() { }

        public PhantomBlock(SubBlock Source)
        {
            this.Block = Source.Block;
            this.Offset = Source.Offset;
            this.Orientation = Source.Orientation;
        }

        public void Rotate(int Steps)
        {
            while (Steps > 0)
            {
                Offset = new Coordinate(-Offset.Y, Offset.X, Offset.Z);
                Steps -= 1;
            }

            Orientation = (CellLink.Directions)Steps;
        }
    }

    public class BlockTemplate
    {
        public BlockType Type = BlockType.Normal;
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

        public List<SubBlock> CompositeBlocks;

        public virtual bool CanComposite(BlockTemplate Onto) { return false; }
        public virtual BlockTemplate Compose(BlockTemplate With, BlockSet TemplateSet) { return this; }
        public virtual void Initialize(BlockSet BlockSet) { }
    }
}
