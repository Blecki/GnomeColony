﻿using System;
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
        Stair,
        LowerSlab,
        UpperSlab,
        HalfSlopeLow,
        HalfSlopeHigh,
        Composite
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

        public List<BlockTemplate> CompositeBlocks;

        public virtual bool CanComposite(BlockTemplate Onto) { return false; }
        public virtual BlockTemplate Compose(BlockTemplate With, BlockSet TemplateSet) { return this; }
    }
}
