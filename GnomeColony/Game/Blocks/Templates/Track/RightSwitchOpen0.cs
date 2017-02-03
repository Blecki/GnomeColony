using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates.Track
{
    public class RightSwitchOpen0 : TrackBase
    {
        public RightSwitchOpen0()
        {
            Top = 131;
            Shape = BlockShape.Decal;
            Orientable = true;
            ShowInEditor = false;
        }

        public override bool CanCompose(OrientedBlock Onto, Direction MyOrientation)
        {
            var top = Onto.GetTopOfComposite();

            if (top.Template is Track &&
                (top.Orientation == MyOrientation || Directions.Opposite(top.Orientation) == MyOrientation))
                return true;

            return base.CanCompose(Onto, MyOrientation);
        }

        public override OrientedBlock Compose(OrientedBlock With, Direction MyOrientation, BlockSet TemplateSet)
        {
            var top = With.GetTopOfComposite();
            if (top.Template is Track &&
                (top.Orientation == MyOrientation || Directions.Opposite(top.Orientation) == MyOrientation))
            {
                var sansTop = With.SansTopOfComposite();
                return sansTop.ComposeWith(new OrientedBlock
                    {
                        Template = this,
                        Orientation = MyOrientation
                    });
            }

            return base.Compose(With, MyOrientation, TemplateSet);
        }
    }
}
