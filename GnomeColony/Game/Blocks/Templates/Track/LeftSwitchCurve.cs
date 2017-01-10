using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates.Track
{
    public class LeftSwitchCurve : BlockTemplate
    {
        // Todo: Can lay straight over straight; need to be able to lay curve over curve.

        public LeftSwitchCurve()
        {
            PreviewTiles = HelperExtensions.MakeList(
                new OrientedTile(162, Direction.South), new OrientedTile(130),
                new OrientedTile(131), new OrientedTile(132));
            PreviewDimensions = new Microsoft.Xna.Framework.Point(2, 2);
            Shape = BlockShape.Combined;
            ShowInEditor = true;
            PlacementType = BlockPlacementType.Combine;
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
                Template = BlockSet.Templates["LeftSwitchOpen0"],
                Offset = new Coordinate(0, 1, 0)
            });

            CompositeBlocks.Add(new OrientedBlock
            {
                Template = BlockSet.Templates["LeftSwitchOpen1"],
                Offset = new Coordinate(1, 1, 0)
            });

        }
    }
}
