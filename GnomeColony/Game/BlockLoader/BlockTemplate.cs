using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public enum BlockShape
    {
        Cube,
        Slope,
        Stair,
        Slab,
        Surface,
        HalfSlopeLow,
        HalfSlopeHigh,
    }
    
    public enum BlockMaterialType
    {
        Solid,
        Liquid,
        Air
    }

    public enum BuildType
    {
        All,
        CreativeOnly,
        None
    }

    public class BlockTemplate
    {
        public String Name;
        [TileBlockPropertyConverter] public int Preview = 1;
        [TileBlockPropertyConverter] public int Top = 1;
        [TileBlockPropertyConverter] public int NorthSide = -1;
        [TileBlockPropertyConverter] public int SouthSide = -1;
        [TileBlockPropertyConverter] public int EastSide = -1;
        [TileBlockPropertyConverter] public int WestSide = -1;
        [TileBlockPropertyConverter] public int Bottom = -1;
        [BlockBlockPropertyConverter] public String Hanging = null;
        [EnumBlockPropertyConverter(typeof(BlockShape))] public BlockShape Shape;
        public float ResourceHeightOffset = 0.0f;
        [ListBlockPropertyConverter] public String[] ConstructionResources;
        [ListBlockPropertyConverter] public String[] MineResources;

        [BoolBlockPropertyConverter] public bool Orientable = false;
        [EnumBlockPropertyConverter(typeof(BlockMaterialType))] public BlockMaterialType MaterialType = BlockMaterialType.Solid;
        [EnumBlockPropertyConverter(typeof(BuildType))] public BuildType BuildType = BuildType.All;
    }
}
