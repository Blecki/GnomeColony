using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates.Track
{
    public class LeftGentleDivert : BlockTemplate
    {
        public LeftGentleDivert()
        {
            PreviewTiles = HelperExtensions.MakeList(
                new OrientedTile(129), new OrientedTile(130),
                new OrientedTile(130, Direction.South), new OrientedTile(129, Direction.South));
            PreviewDimensions = new Microsoft.Xna.Framework.Point(2, 2);
            Shape = BlockShape.Combined;
            ShowInEditor = true;
        }

        public override void Initialize(BlockSet BlockSet)
        {
            CompositeBlocks = new List<OrientedBlock>();

            CompositeBlocks.Add(new OrientedBlock
            {
                Template = BlockSet.Templates["MediumCurve1_1"],
                Offset = new Coordinate(0, 0, 0),
                Orientation = Direction.South
            });

            CompositeBlocks.Add(new OrientedBlock
            {
                Template = BlockSet.Templates["MediumCurve1_0"],
                Offset = new Coordinate(1, 0, 0),
                Orientation = Direction.North
            });

            CompositeBlocks.Add(new OrientedBlock
            {
                Template = BlockSet.Templates["MediumCurve1_0"],
                Offset = new Coordinate(0, 1, 0),
                Orientation = Direction.South
            });

            CompositeBlocks.Add(new OrientedBlock
            {
                Template = BlockSet.Templates["MediumCurve1_1"],
                Offset = new Coordinate(1, 1, 0)
            });

        }
    }
}
