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
        Slab
    }

    public class BlockShapeTemplate
    {
        public Gem.Geo.Mesh Mesh;
        public Gem.Geo.Mesh NavigationMesh;
    }

    public class BlockTemplate
    {
        public int ID;
        public int Preview = 1;
        public int Top = 1;
        public int SideA = 1;
        public int SideB = -1;
        public int Bottom = 1;
        public int Hanging = -1;
        public BlockShape Shape;
        public bool Solid = true;
        public float ResourceHeightOffset = 0.0f;
        public int[] ConstructionResources;
        public int[] MineResources;
        public bool Orientable = false;
    }

    public class BlockTemplateSet : Dictionary<int, BlockTemplate>
    {
        new public void Add(int ID, BlockTemplate Template)
        {
            Template.ID = ID;
            base.Add(ID, Template);
        }
    }

    public static class BlockTypes
    {
        public const int Scaffold = 0;
        public const int Grass = 1;
        public const int Dirt = 2;
        public const int TestSlope = 3;
        public const int HangingVines = 4;
    }
}
