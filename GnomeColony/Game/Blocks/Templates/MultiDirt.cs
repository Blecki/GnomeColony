using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates
{
    public class MultiDirt : BlockTemplate
    {
        public MultiDirt()
        {
            Preview = 34;
            Type = BlockType.Combined;
        }

        public override void Initialize(BlockSet BlockSet)
        {
            CompositeBlocks = new List<SubBlock>();
            CompositeBlocks.Add(new SubBlock { Block = BlockSet.Templates["Dirt"], Offset = new Coordinate(1, 1, 0) });
            CompositeBlocks.Add(new SubBlock { Block = BlockSet.Templates["Dirt"], Offset = new Coordinate(-1, -1, 0) });
            CompositeBlocks.Add(new SubBlock { Block = BlockSet.Templates["Dirt"], Offset = new Coordinate(0, 0, 0) });
        }
    }
}
