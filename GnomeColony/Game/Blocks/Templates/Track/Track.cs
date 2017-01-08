﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates.Track
{
    public class Track : TrackBase
    {
        public Track()
        {
            PreviewTiles = HelperExtensions.MakeList(new OrientedTile(128, Direction.North));
            Top = 128;
            Shape = BlockShape.Decal;
            Orientable = true;
            ShowInEditor = true;
        }

        public override bool CanCompose(OrientedBlock Onto, Direction MyOrientation)
        {
            var top = Onto.Template.GetTopOfComposite(Onto.Orientation);
           
            if (top.Template is Track)
                return true;
    
            return base.CanCompose(Onto, MyOrientation);
        }

        public override OrientedBlock Compose(OrientedBlock With, Direction MyOrientation, BlockSet TemplateSet)
        {
            var top = With.Template.GetTopOfComposite(With.Orientation);

            if (top.Template is Track)
            {
                var sansTop = With.Template.SansTopOfComposite(With.Orientation);
                if (top.Orientation != MyOrientation && top.Orientation != Directions.Rotate(Directions.Rotate(MyOrientation)))
                    return new OrientedBlock
                    {
                        Template = sansTop.Template.ComposeWith(sansTop.Orientation, new OrientedBlock
                        {
                            Template = TemplateSet.Templates["Junction"],
                            Orientation = Direction.North
                        }),
                        Orientation = Direction.North
                    };
                else
                    return With;
            }

            return base.Compose(With, MyOrientation, TemplateSet);
        }
    }
}
