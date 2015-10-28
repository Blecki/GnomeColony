using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gnome
{
    public class MoveAction : ActorAction
    {
        public CellLink.Directions Direction;
        public float TotalTime;
        public float ElapsedTime = 0.0f;
        public bool Done = false;
        private Vector3[] SplinePoints = null;

        public MoveAction(CellLink.Directions Direction, float TotalTime)
        {
            this.Direction = Direction;
            this.TotalTime = TotalTime;
        }

        public override bool Update(Game Game, Actor Actor, float ElapsedTime)
        {
            if (!Game.World.Check(Actor.Location)) return true;

            this.ElapsedTime += ElapsedTime;

            if (SplinePoints == null)
            {
                SplinePoints = new Vector3[3];

                var startCell = Game.World.CellAt(Actor.Location);
                SplinePoints[0] = startCell.CenterPoint;

                var linkIndex = startCell.Links.FindIndex(l => l.Direction == Direction);
                
                if (linkIndex >= 0 && linkIndex < startCell.Links.Count)
                {
                    var link = startCell.Links[linkIndex];
                    SplinePoints[1] = link.EdgePoint;
                    if (link.Neighbor == null)
                        return true; // SplinePoints[2] = SplinePoints[0] + CellLink.DirectionOffset(Direction);
                    else
                    {
                        SplinePoints[2] = link.Neighbor.CenterPoint;

                        // Update cell's present actor. Update at begining of move for reasons.
                        startCell.PresentActor = null;
                        link.Neighbor.PresentActor = Actor;

                        Actor.Location = link.Neighbor.Location;
                    }
                }
                else
                {
                    return true;
                    // SplinePoints[2] = SplinePoints[0] + CellLink.DirectionOffset(Direction);
                    // SplinePoints[1] = (SplinePoints[0] + SplinePoints[2]) / 2.0f;
                }
            }

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
