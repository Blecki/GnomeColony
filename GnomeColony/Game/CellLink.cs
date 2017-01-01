using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;

namespace Game
{
    public struct CellLink
    {
        public enum Directions
        {
            North,
            East,
            South,
            West
        }

        public static Directions DeriveDirectionFromNormal(Vector3 N)
        {
            if (N.X < 0) return Directions.West;
            if (N.X > 0) return Directions.East;
            if (N.Y < 0) return Directions.North;
            if (N.Y > 0) return Directions.South;
            return Directions.North;
        }
    }
}
