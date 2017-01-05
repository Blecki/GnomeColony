using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates
{
    public class GrassHighSlab : BlockTemplate
    {
        public GrassHighSlab()
        {
            Preview = 40;
            Top = 33;
            NorthSide = 40;
            Bottom = 34;
            Shape = BlockShape.UpperSlab;
            //Hanging = "HangingVines";
        }

        public override bool CanComposite(OrientedBlock Onto, CellLink.Directions MyOrientation)
        {
            if (Onto.Template.Shape == BlockShape.LowerSlab)
                return true;
            return false;
        }

        public override OrientedBlock Compose(OrientedBlock With, CellLink.Directions MyOrientation, BlockSet TemplateSet)
        {
            return new OrientedBlock
            {
                Template = With.Template.ComposeWith(With.Orientation, new OrientedBlock
                {
                    Template = this,
                    Orientation = MyOrientation
                }),
                Orientation = CellLink.Directions.North
            };
        }
    }
}
