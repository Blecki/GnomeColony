using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gnome
{
    public class ResourceRequirement
    {
        public BlockTemplate BlockType;
        public bool Filled;
    }

    public class Cell 
    {
        public BlockTemplate Block;
        public Task Task;
        public Coordinate Location;

        public bool Navigatable = false;
        public List<CellLink> Links = new List<CellLink>();
        public Vector3 CenterPoint;
        public Gem.Geo.Mesh NavigationMesh;

        public bool Storehouse = false;

        public ResourceRequirement Resource;

        public bool FullfillsResourceRequirement(BlockTemplate ResourceType)
        {
            if (Resource == null) return false;
            if (Resource.Filled) return false;
            return Object.ReferenceEquals(ResourceType, Resource.BlockType);
        }
    }
}
