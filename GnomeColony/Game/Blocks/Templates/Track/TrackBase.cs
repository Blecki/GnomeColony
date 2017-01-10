using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates.Track
{
    public enum TrackTiles
    {
        SwitchCurveLeft0 = 131,
        SwitchCurveLeft1 = 132,
        SwitchCurveRight0 = 133,
        SwitchCurveRight1 = 134,

        SwitchStraightLeft0 = 163,
        SwitchStraightLeft1 = 164,
        SwitchStraightRight0 = 165,
        SwitchStraightRight1 = 166,

        DualSmallCurve = 193,
    }

    public class TrackBase : BlockTemplate
    {
        public TrackBase()
        {
            Top = 128;
            Shape = BlockShape.Decal;
            Orientable = true;
            ShowInEditor = false;
            PlacementType = BlockPlacementType.Combine;
        }

        public override bool CanCompose(OrientedBlock Onto, Direction MyOrientation)
        {
            if (Onto.Template == null) return false;
            if (Onto.Template.Shape == BlockShape.HalfSlopeLow) return true;
            if (Onto.Template.Shape == BlockShape.HalfSlopeHigh) return true;
            if (Onto.Template.Shape == BlockShape.LowerSlab) return true;
            if (Onto.Template.Shape == BlockShape.UpperSlab) return true;
            if (Onto.Template.Shape == BlockShape.Slope) return true;
            if (Onto.Template.Shape == BlockShape.Cube) return true;

            var top = Onto.Template.GetTopOfComposite(Onto.Orientation);
            if (Object.ReferenceEquals(top.Template, this) && top.Orientation == MyOrientation)
                return true;

            if (Object.ReferenceEquals(this, Onto.Template) && Onto.Orientation == MyOrientation) 
                return true;
            return false;
        }

        public override OrientedBlock Compose(OrientedBlock With, Direction MyOrientation, BlockSet TemplateSet)
        {
            if (Object.ReferenceEquals(this, With.Template) && With.Orientation == MyOrientation)
            {
                return With;
            }

            var top = With.Template.GetTopOfComposite(With.Orientation);
            if (Object.ReferenceEquals(this, top.Template) && top.Orientation == MyOrientation)
                return With;

            if (With.Template.Shape == BlockShape.HalfSlopeLow ||
                With.Template.Shape == BlockShape.HalfSlopeHigh ||
                With.Template.Shape == BlockShape.LowerSlab ||
                With.Template.Shape == BlockShape.UpperSlab || 
                With.Template.Shape == BlockShape.Slope ||
                With.Template.Shape == BlockShape.Cube)
            {
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
                    Orientation = Direction.North
                };
            }

            return base.Compose(With, MyOrientation, TemplateSet);
        }
    }
}
