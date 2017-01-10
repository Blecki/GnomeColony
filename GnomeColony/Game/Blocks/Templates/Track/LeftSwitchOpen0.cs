using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates.Track
{
    public class LeftSwitchOpen0 : TrackBase
    {
        public LeftSwitchOpen0()
        {
            Top = 133;
            Shape = BlockShape.Decal;
            Orientable = true;
            ShowInEditor = false;
        }

        public override bool CanCompose(OrientedBlock Onto, Direction MyOrientation)
        {
            var top = Onto.Template.GetTopOfComposite(Onto.Orientation);

            if (top.Template is Track &&
                (top.Orientation == MyOrientation || Directions.Opposite(top.Orientation) == MyOrientation))
                return true;

            return base.CanCompose(Onto, MyOrientation);
        }

        public override OrientedBlock Compose(OrientedBlock With, Direction MyOrientation, BlockSet TemplateSet)
        {
            var top = With.Template.GetTopOfComposite(With.Orientation);
            if (top.Template is Track &&
                (top.Orientation == MyOrientation || Directions.Opposite(top.Orientation) == MyOrientation))
            {
                var sansTop = With.Template.SansTopOfComposite(With.Orientation);
                return new OrientedBlock
                {
                    Template = sansTop.Template.ComposeWith(sansTop.Orientation, new OrientedBlock
                    {
                        Template = this,
                        Orientation = MyOrientation
                    }),
                    Orientation = Direction.North
                };
            }

            return base.Compose(With, MyOrientation, TemplateSet);
        }
    }
}
