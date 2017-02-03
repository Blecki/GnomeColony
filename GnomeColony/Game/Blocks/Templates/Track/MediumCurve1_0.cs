using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates.Track
{
    public class MediumCurve1_0 : TrackBase
    {
        public MediumCurve1_0()
        {
            Top = 130;
            Shape = BlockShape.Decal;
            Orientable = true;
            ShowInEditor = false;
        }

        public override bool CanCompose(OrientedBlock Onto, Direction MyOrientation)
        {
            var top = Onto.GetTopOfComposite();
            if (Object.ReferenceEquals(top.Template, this) && top.Orientation ==
                Directions.Rotate(Directions.Rotate(MyOrientation)))
                return true;

            return base.CanCompose(Onto, MyOrientation);
        }

        public override OrientedBlock Compose(OrientedBlock With, Direction MyOrientation, BlockSet TemplateSet)
        {
            var top = With.GetTopOfComposite();
            if (Object.ReferenceEquals(top.Template, this) && top.Orientation ==
                Directions.Rotate(Directions.Rotate(MyOrientation)))
            {
                var sansTop = With.SansTopOfComposite();
                return sansTop.ComposeWith(new OrientedBlock
                    {
                        Template = TemplateSet.Templates["DualOuterMediumCurve"],
                        Orientation = MyOrientation
                    });
            }

            return base.Compose(With, MyOrientation, TemplateSet);
        }
    }
}
