using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Game
{
    public class MoveAction : ActorAction
    {
        public bool Done = false;
        private Vector3[] SplinePoints = null;

        public MoveAction(Cell Start, CellLink.Directions Direction)
        {
            SplinePoints = new Vector3[3];

            var startCell = Start;
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

        public override bool Step(Actor Actor)
        {
            Done = true;
            return true;
        }

        public override void Update(Actor Actor, float StepPercentage)
        {
            if (!Done)
            {
                if (StepPercentage >= 1.0f)
                    Actor.PositionOffset = Vector3.Zero;
                else
                {
                    var realPosition = Vector3.Zero;
                    if (StepPercentage < 0.5f)
                        realPosition = SplinePoints[0] + ((SplinePoints[1] - SplinePoints[0]) * (StepPercentage / 0.5f));
                    else
                        realPosition = SplinePoints[1] + ((SplinePoints[2] - SplinePoints[1]) * ((StepPercentage - 0.5f) / 0.5f));

                    Actor.PositionOffset = realPosition - SplinePoints[2];
                }
            }
        }
    }
}
