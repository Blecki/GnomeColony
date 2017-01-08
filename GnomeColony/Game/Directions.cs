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
    public enum Direction
    {
        North,
        East,
        South,
        West
    }

    public static class Directions
    {
        public static Direction DeriveDirectionFromNormal(Vector3 N)
        {
            if (N.X < 0) return Direction.West;
            if (N.X > 0) return Direction.East;
            if (N.Y < 0) return Direction.North;
            if (N.Y > 0) return Direction.South;
            return Direction.North;
        }

        internal static Direction Rotate(Direction BaseOrientation)
        {
            switch (BaseOrientation)
            {
                case Direction.North:
                    return Direction.East;
                case Direction.East:
                    return Direction.South;
                case Direction.South:
                    return Direction.West;
                case Direction.West:
                    return Direction.North;
                default:
                    return BaseOrientation;
            }
        }

        internal static Direction Opposite(Direction BaseOrientation)
        {
            switch (BaseOrientation)
            {
                case Direction.North:
                    return Direction.South;
                case Direction.East:
                    return Direction.West;
                case Direction.South:
                    return Direction.North;
                case Direction.West:
                    return Direction.East;
                default:
                    return BaseOrientation;
            }
        }

        internal static Direction Add(Direction A, Direction B)
        {
            var r = (int)A + (int)B;
            return (Direction)(r % 4);
        }
    }
}
