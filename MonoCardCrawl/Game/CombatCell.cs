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
    public class CombatCell
    {

        public enum Direction
        {
            North,
            East,
            South,
            West
        }

        public struct Link
        {
            public Direction Direction;
            public CombatCell Neighbor;
            public Vector3 EdgePoint;
        }

        public static Vector3 DirectionOffset(Direction Direction)
        {
            switch (Direction)
            {
                case Direction.North: return new Vector3(0, -1, 0);
                case Direction.East: return new Vector3(1, 0, 0);
                case Direction.South: return new Vector3(0, 1, 0);
                case Direction.West: return new Vector3(-1, 0, 0);
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
        public static Direction DirectionFromAToB(Coordinate A, Coordinate B)
        {
            if (B.X > A.X) return Direction.East;
            if (B.X < A.X) return Direction.West;
            if (B.Y > A.Y) return Direction.South;
            if (B.Y < A.Y) return Direction.North;
            throw new InvalidOperationException();
        }

        public bool Visible = true;
        public Gem.Geo.Mesh Mesh;
        public Cell ParentCell;
        public int Texture;
        public Coordinate Coordinate;
        public Vector3 CenterPoint;
        public Actor AnchoredActor;
        public Action ClickAction;
        public Action HoverAction;

        public List<Link> Links;
        public Pathfinding<CombatCell>.PathNode PathNode;
    }
}
