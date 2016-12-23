using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Game
{
    [Flags]
    public enum CellFlags
    {
        Storehouse = 1,
    }

    public class Cell 
    {
        public BlockTemplate Block;
        public BlockTemplate Decal;
        public CellLink.Directions BlockOrientation = CellLink.Directions.North;
        public Task Task;
        public Coordinate Location;

        public bool Navigatable = false;
        public List<CellLink> Links = new List<CellLink>();
        public Gem.Geo.Mesh NavigationMesh;

        public Vector3 CenterPoint
        {
            get
            {
                if (Block == null) return Location.AsVector3() + new Vector3(0.5f, 0.5f, 0.5f);
                else return Location.AsVector3() + Generate.GetCenterPoint(Block.Shape, (int)BlockOrientation) + new Vector3(0, 0, 0.5f);
            }
        }

        public CellFlags Flags = 0;

        public void SetFlag(CellFlags Flag, bool Value)
        {
            if (Value)
                Flags |= Flag;
            else
                Flags &= ~Flag;
        }

        public bool HasFlag(CellFlags Flag)
        {
            return Flags.HasFlag(Flag);
        }

        public Actor PresentActor; // Actors are assigned only to their base cell, even though they are generally two cells tall.

        public List<String> Resources = new List<String>();

        public bool CanPlaceResource(String ResourceType)
        {
            if (HasFlag(CellFlags.Storehouse) && Resources.Count < 8) return true;
            else if (Task != null) return Task.FindUnfilledResourceRequirments(this, Task).Contains(ResourceType);
            return false;
        }

        public bool IsSolid
        {
            get
            {
                if (Block == null) return false;
                return Block.MaterialType == BlockMaterialType.Solid;
            }
        }        
    }
}
