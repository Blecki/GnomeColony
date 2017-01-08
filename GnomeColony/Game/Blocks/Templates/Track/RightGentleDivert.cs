using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates.Track
{
    public class RightGentleDivert : BlockTemplate
    {
        public RightGentleDivert()
        {
            PreviewTiles = HelperExtensions.MakeList(
                new OrientedTile(161, Direction.East), new OrientedTile(162, Direction.West),
                new OrientedTile(162, Direction.East), new OrientedTile(161, Direction.West));
            PreviewDimensions = new Microsoft.Xna.Framework.Point(2, 2);
            Shape = BlockShape.Combined;
            ShowInEditor = true;
        }

        public override void Initialize(BlockSet BlockSet)
        {
            CompositeBlocks = new List<OrientedBlock>();

            CompositeBlocks.Add(new OrientedBlock
            {
                Template = BlockSet.Templates["MediumCurve1_0"],
                Offset = new Coordinate(0, 0, 0),
                Orientation = Direction.West
            });

            CompositeBlocks.Add(new OrientedBlock
            {
                Template = BlockSet.Templates["MediumCurve0_0"],
                Offset = new Coordinate(1, 0, 0),
                Orientation = Direction.East
            });

            CompositeBlocks.Add(new OrientedBlock
            {
                Template = BlockSet.Templates["MediumCurve0_0"],
                Offset = new Coordinate(0, 1, 0),
                Orientation = Direction.West
            });

            CompositeBlocks.Add(new OrientedBlock
            {
                Template = BlockSet.Templates["MediumCurve1_0"],
                Offset = new Coordinate(1, 1, 0),
                Orientation = Direction.East
            });

        }
    }
}
