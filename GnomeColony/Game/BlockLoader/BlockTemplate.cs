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
        Decal
    }

    public class BlockShapeTemplate
    {
        public Gem.Geo.Mesh Mesh;
        public Gem.Geo.Mesh NavigationMesh;
    }

    public class BlockTemplate
    {
        public String Name;
        [TileBlockPropertyConverter] public int Preview = 1;
        [TileBlockPropertyConverter] public int Top = 1;
        [TileBlockPropertyConverter] public int SideA = -1;
        [TileBlockPropertyConverter] public int SideB = -1;
        [TileBlockPropertyConverter] public int Bottom = -1;
        [BlockBlockPropertyConverter] public String Hanging = null;
        [EnumBlockPropertyConverter(typeof(BlockShape))] public BlockShape Shape;
        [BoolBlockPropertyConverter] public bool Solid = true;
        public float ResourceHeightOffset = 0.0f;
        [ListBlockPropertyConverter] public String[] ConstructionResources;
        [ListBlockPropertyConverter] public String[] MineResources;
        [BoolBlockPropertyConverter] public bool Orientable = false;
    }
}
