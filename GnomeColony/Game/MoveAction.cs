using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Game
{
    public class MoveAction : ActorAction
    {
        public CellLink.Directions Direction;
        public float TotalTime;
        public float ElapsedTime = 0.0f;
        public bool Done = false;
        private Vector3[] SplinePoints = null;
        private Cell Start;

        public MoveAction(Cell Start, CellLink.Directions Direction, float TotalTime)
        {
            this.Direction = Direction;
            this.TotalTime = TotalTime;
            this.Start = Start;

            SetFlag(ActorActionFlags.Interuptible, false);
        }

        public override void Begin(Game Game, Actor Actor)
        {
            if (!Game.World.Check(Actor.Location))
                Done = true;
            else
            {
                SplinePoints = new Vector3[3];

                var startCell = Start; // Game.World.CellAt(Actor.Location);
                SplinePoints[0] = startCell.CenterPoint;

                var linkIndex = startCell.Links.FindIndex(l => l.Direction == Direction);

                if (linkIndex >= 0 && linkIndex < startCell.Links.Count)
                {
                    var link = startCell.Links[linkIndex];
                    SplinePoints[1] = link.EdgePoint;
                    if (link.Neighbor == null)
                        Done = true;
                    else
                        SplinePoints[2] = link.Neighbor.CenterPoint;
                }
                else
                    Done = true;
            }
        }

        public override bool Update(Game Game, Actor Actor, float ElapsedTime)
        {
            if (Done) return true;

            this.ElapsedTime += ElapsedTime;

            if (this.ElapsedTime >= this.TotalTime)
            {
                Actor.PositionOffset = Vector3.Zero;
                Done = true;
                return true;
            }

            var halfTime = TotalTime / 2.0f;

            var realPosition = Vector3.Zero;
            if (this.ElapsedTime < halfTime)
                realPosition = SplinePoints[0] + ((SplinePoints[1] - SplinePoints[0]) * (this.ElapsedTime / halfTime));
            else
                realPosition = SplinePoints[1] + ((SplinePoints[2] - SplinePoints[1]) * ((this.ElapsedTime - halfTime) / halfTime));

            Actor.PositionOffset = -(SplinePoints[2] - realPosition);

            return false;
        }

    }
}
