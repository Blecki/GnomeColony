using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Game.Actors.Actions
{
    class WalkPath : ActorAction
    {
        List<Vector3> PathPoints = new List<Vector3>();

        public WalkPath(List<CombatCell> Path)
        {
            // Convert path of cells to a path of points along the floor.
            for (int i = 0; i < Path.Count; ++i)
            {
                PathPoints.Add(Path[i].CenterPoint);
                if (i != Path.Count - 1)
                {
                    var stepDirection = CombatCell.DirectionFromAToB(Path[i].Coordinate, Path[i + 1].Coordinate);
                    var link = Path[i].Links.First(l => l.Direction == stepDirection);
                    PathPoints.Add(link.EdgePoint);
                }
            }
        }

        public override void Begin(World World, Actor Actor)
        {
            //TODO: states could be implemented entirely with rules.
            World.GlobalRules.ConsiderPerformRule("enter-run", Actor);
        }

        public override void End(World World, Actor Actor)
        {
            World.GlobalRules.ConsiderPerformRule("enter-idle", Actor);
        }

        public override void Update(World World, Actor Actor, float ElapsedTime)
        {
            var speed = 4.0f;

            if (PathPoints.Count == 0)
            {
                Actor.NextAction = new Idle();
                return;
            }

            var waypoint = PathPoints[0];

            var delta = waypoint - Actor.Orientation.Position;
            if (delta.Length() < (speed * ElapsedTime))
            {
                PathPoints.RemoveAt(0);
                Actor.Orientation.Position = waypoint;
            }
            else
            {
                delta.Normalize();
                delta *= (speed * ElapsedTime);
                Actor.Orientation.Position += delta;
            }

            Actor.MotionDelta = delta;
        }
    }
}
