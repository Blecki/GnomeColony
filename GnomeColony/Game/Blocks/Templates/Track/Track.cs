using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates
{
    public class Track : BlockTemplate
    {
        public Track()
        {
            Preview = 128;
            Top = 128;
            Shape = BlockShape.Decal;
            Orientable = true;
        }

        public override bool CanComposite(Generate.OrientatedBlock Onto, CellLink.Directions MyOrientation)
        {
            if (Onto.Block.Shape == BlockShape.HalfSlopeLow) return true;
            if (Onto.Block is Track)
            {
                if (Onto.Orientation != MyOrientation && CellLink.Rotate(CellLink.Rotate(Onto.Orientation)) != MyOrientation)
                    return true;
            }
            return false;
        }

        public override Generate.OrientatedBlock Compose(Generate.OrientatedBlock With, CellLink.Directions MyOrientation, BlockSet TemplateSet)
        {
            if (With.Block.Shape == BlockShape.HalfSlopeLow)
                return new Generate.OrientatedBlock
                {
                    Block = new BlockTemplate
                    {
                        Type = BlockType.Combined,
                        CompositeBlocks = HelperExtensions.MakeList(
                            new SubBlock { Block = With.Block, Orientation = With.Orientation },
                            new SubBlock { Block = this, Orientation = MyOrientation }
                        )
                    },
                    Orientation = CellLink.Directions.North
                };

            if (With.Block is Track)
                return new Generate.OrientatedBlock
                {
                    Block = TemplateSet.Templates["TrackSmallJunction"],
                    Orientation = CellLink.Directions.North
                };

            return base.Compose(With, MyOrientation, TemplateSet);
        }
    }
}
