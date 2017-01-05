using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates
{
    public class TrackBase : BlockTemplate
    {
        public TrackBase()
        {
            Preview = 0;
            Top = 128;
            Shape = BlockShape.Decal;
            Orientable = true;
            ShowInEditor = false;
        }

        public override bool CanComposite(OrientedBlock Onto, CellLink.Directions MyOrientation)
        {
            if (Onto.Template == null) return false;
            if (Onto.Template.Shape == BlockShape.HalfSlopeLow) return true;
            if (Onto.Template.Shape == BlockShape.HalfSlopeHigh) return true;
            if (Onto.Template.Shape == BlockShape.LowerSlab) return true;
            if (Onto.Template.Shape == BlockShape.Slope) return true;
            return false;
        }

        public override OrientedBlock Compose(OrientedBlock With, CellLink.Directions MyOrientation, BlockSet TemplateSet)
        {
            if (With.Template.Shape == BlockShape.HalfSlopeLow ||
                With.Template.Shape == BlockShape.HalfSlopeHigh ||
                With.Template.Shape == BlockShape.LowerSlab ||
                With.Template.Shape == BlockShape.Slope)
                return new OrientedBlock
                {
                    Template = new BlockTemplate
                    {
                        Shape = BlockShape.Combined,
                        CompositeBlocks = HelperExtensions.MakeList(
                            new OrientedBlock { Template = With.Template, Orientation = With.Orientation },
                            new OrientedBlock { Template = this, Orientation = MyOrientation }
                        )
                    },
                    Orientation = CellLink.Directions.North
                };

            return base.Compose(With, MyOrientation, TemplateSet);
        }
    }
}
