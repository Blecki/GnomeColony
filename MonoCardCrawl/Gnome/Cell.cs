using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gnome
{
    public class Cell 
    {
        public BlockTemplate Block;
        public Task Task;
        public Coordinate Location;

        public bool Navigatable = false;
        public List<CellLink> Links = new List<CellLink>();
        public Vector3 CenterPoint;
        public Gem.Geo.Mesh NavigationMesh;

        public Actor PresentActor; // Actors are assigned only to their base cell, even though they are generally two cells tall.

        public bool Storehouse = false;

        public List<int> Resources = new List<int>();

        public bool CanPlaceResource(int ResourceType)
        {
            if (Storehouse && Resources.Count < 8) return true;
            else if (Task != null) return global::Gnome.Task.FindUnfilledResourceRequirments(this, Task).Contains(ResourceType);
            return false;
        }

        public bool IsSolid
        {
            get
            {
                if (Block == null) return false;
                return Block.Solid;
            }
        }

        public bool CanWalk
        {
            get
            {
                if (!Navigatable) return false; // No nav mesh for this block.
                if (PresentActor != null) return false; // An actor is in the way.
                if (Resources.Count > 2) return false; // More than two resources on the tile? Can't walk.
                return true;
            }
        }
    }
}
