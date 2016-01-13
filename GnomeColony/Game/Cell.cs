﻿using System;
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
        public CellLink.Directions BlockOrientation = CellLink.Directions.North;
        public Task Task;
        public Coordinate Location;

        public bool Navigatable = false;
        public List<CellLink> Links = new List<CellLink>();
        public Vector3 CenterPoint;
        public Gem.Geo.Mesh NavigationMesh;

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

        public List<int> Resources = new List<int>();

        public bool CanPlaceResource(int ResourceType)
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
                return Block.Solid;
            }
        }        
    }
}