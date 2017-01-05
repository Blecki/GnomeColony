using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates
{
    public class Track : TrackBase
    {
        public Track()
        {
            Preview = 128;
            Top = 128;
            Shape = BlockShape.Decal;
            Orientable = true;
            ShowInEditor = true;
        }

        public override bool CanComposite(OrientedBlock Onto, CellLink.Directions MyOrientation)
        {
            if (Onto.Template.GetTopOfComposite().Template is Track)
            {
                if (Onto.Orientation != MyOrientation && CellLink.Rotate(CellLink.Rotate(Onto.Orientation)) != MyOrientation)
                    return true;
            }


            return base.CanComposite(Onto, MyOrientation);
        }

        public override OrientedBlock Compose(OrientedBlock With, CellLink.Directions MyOrientation, BlockSet TemplateSet)
        {
            if (With.Template.GetTopOfComposite().Template is Track)
            {
                var sansTop = With.Template.SansTopOfComposite();
                With.Orientation = CellLink.Add(With.Orientation, sansTop.Orientation);
                return new OrientedBlock
                {
                    Template = sansTop.Template.ComposeWith(With.Orientation, new OrientedBlock
                    {
                        Template = TemplateSet.Templates["TrackSmallJunction"],
                        Orientation = CellLink.Directions.North
                    }),
                    Orientation = CellLink.Directions.North
                };
            }

            return base.Compose(With, MyOrientation, TemplateSet);
        }
    }
}
