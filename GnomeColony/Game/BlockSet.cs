using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;
using Gem.Render;

namespace Game
{
    public class BlockSet
    {
        public BlockTemplateSet BlockTemplates = new BlockTemplateSet();
        public TileSheet BlockTiles;

        public BlockSet(EpisodeContentManager Content)
        {
            BlockTemplates.Add(BlockTypes.Scaffold, new BlockTemplate
            {
                Preview = TileNames.TaskMarkerBuild,
                Top = TileNames.TaskMarkerBuild,
                SideA = TileNames.TaskMarkerBuild,
                Bottom = TileNames.TaskMarkerBuild,
                Shape = BlockShape.Cube,
                Solid = false,
                ResourceHeightOffset = -0.5f
            });

            BlockTemplates.Add(BlockTypes.Grass, new BlockTemplate
                {
                    Preview = TileNames.BlockGrassTop,
                    Top = TileNames.BlockGrassTop,
                    SideA = TileNames.BlockGrassSide,
                    Bottom = TileNames.BlockDirt,
                    Shape = BlockShape.Cube,
                    ConstructionResources = new int[] {  BlockTypes.Dirt, BlockTypes.Dirt },
                    MineResources = new int[] { BlockTypes.Dirt, BlockTypes.Dirt, BlockTypes.Dirt },
                    Hanging = BlockTypes.HangingVines
                });

            BlockTemplates.Add(BlockTypes.Dirt, new BlockTemplate
            {
                Preview = TileNames.BlockDirt,
                Top = TileNames.BlockDirt,
                SideA = TileNames.BlockDirt,
                Bottom = TileNames.BlockDirt,
                Shape = BlockShape.Cube
            });

            BlockTemplates.Add(BlockTypes.HangingVines, new BlockTemplate
            {
                Preview = TileNames.BlockHangingRoots,
                Top = TileNames.Blank,
                SideA = TileNames.BlockHangingRoots,
                Bottom = TileNames.Blank,
                Shape = BlockShape.Cube
            });

            BlockTemplates.Add(BlockTypes.TestSlope, new BlockTemplate
            {
                Preview = TileNames.BlockGrassSlope,
                Top = TileNames.BlockGrassTop,
                SideA = TileNames.BlockGrassSlope,
                SideB = TileNames.BlockGrassSide,
                Bottom = TileNames.BlockDirt,
                Orientable = true,
                Shape = BlockShape.Slope,
                ConstructionResources = new int[] { BlockTypes.Dirt, BlockTypes.Dirt },
                MineResources = new int[] { BlockTypes.Dirt }
            });

            var tileTexture = Content.Load<Texture2D>("tiles");
            BlockTiles = new TileSheet(tileTexture, 16, 16);
        }
    }
}
