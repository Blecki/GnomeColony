using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;

namespace Gnome
{
    public struct CellLink
    {
        public Directions Direction;
        public Cell Neighbor;
        public Vector3 EdgePoint;
        public float LinkZOffset;

        public enum Directions
        {
            North,
            East,
            South,
            West
        }

        public static Vector3 DirectionOffset(Directions Direction)
        {
            switch (Direction)
            {
                case Directions.North: return new Vector3(0, -1, 0);
                case Directions.East: return new Vector3(1, 0, 0);
                case Directions.South: return new Vector3(0, 1, 0);
                case Directions.West: return new Vector3(-1, 0, 0);
                default: throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Assuming the two coordinates are adjacent, what direction would you go to
        /// move from A to B?
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Directions DirectionFromAToB(Coordinate A, Coordinate B)
        {
            if (B.X > A.X) return Directions.East;
            if (B.X < A.X) return Directions.West;
            if (B.Y > A.Y) return Directions.South;
            if (B.Y < A.Y) return Directions.North;
            return Directions.North;
            throw new InvalidOperationException();
        }
    }
}
