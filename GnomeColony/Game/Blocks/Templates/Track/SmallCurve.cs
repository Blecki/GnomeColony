using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates.Track
{
    public class SmallCurve : TrackBase
    {
        public SmallCurve()
        {
            PreviewTiles = HelperExtensions.MakeList(new OrientedTile(160, Direction.North));
            Top = 160;
            Shape = BlockShape.Decal;
            Orientable = true;
            ShowInEditor = true;
        }

        public override bool CanCompose(OrientedBlock Onto, Direction MyOrientation)
        {
            var topBlock = Onto.Template.GetTopOfComposite(Onto.Orientation);
            if (topBlock.Template is MediumCurve1_0 &&
                Directions.Opposite(topBlock.Orientation) == MyOrientation)
                return true;

            return base.CanCompose(Onto, MyOrientation);
        }

        public override OrientedBlock Compose(OrientedBlock With, Direction MyOrientation, BlockSet TemplateSet)
        {
            if (With.Template.GetTopOfComposite(With.Orientation).Template is MediumCurve1_0)
            {
                var sansTop = With.Template.SansTopOfComposite(With.Orientation);
                return new OrientedBlock
                {
                    Template = sansTop.Template.ComposeWith(sansTop.Orientation, new OrientedBlock
                    {
                        Template = TemplateSet.Templates["DualSmallCurve0_1"],
                        Orientation = MyOrientation
                    }),
                    Orientation = Direction.North
                };
            }

            return base.Compose(With, MyOrientation, TemplateSet);
        }
    }
}
