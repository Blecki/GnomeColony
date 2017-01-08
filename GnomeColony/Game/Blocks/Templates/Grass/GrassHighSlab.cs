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
            PreviewTiles = HelperExtensions.MakeList(new OrientedTile(40, Direction.North));
            Top = 33;
            NorthSide = 40;
            Bottom = 34;
            Shape = BlockShape.UpperSlab;
            //Hanging = "HangingVines";
            PlacementType = BlockPlacementType.Combine | BlockPlacementType.OrientToHoverFace;
        }

        public override bool CanCompose(OrientedBlock Onto, Direction MyOrientation)
        {
            if (Onto.Template.Shape == BlockShape.LowerSlab)
                return true;
            return false;
        }

        public override OrientedBlock Compose(OrientedBlock With, Direction MyOrientation, BlockSet TemplateSet)
        {
            return new OrientedBlock
            {
                Template = TemplateSet.Templates["Grass"],
                Orientation = Direction.North
            };
        }
    }
}
