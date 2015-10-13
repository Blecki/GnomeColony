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

        public MoveAction(CellLink.Directions Direction, float TotalTime)
        {
            this.Direction = Direction;
            this.TotalTime = TotalTime;
        }

        public override bool Update(Game Game, Actor Actor, float ElapsedTime)
        {
            if (!Game.World.check(Actor.Location.X, Actor.Location.Y, Actor.Location.Z)) return true;

            var halfTime = TotalTime / 2.0f;

            this.ElapsedTime += ElapsedTime;

            var startCell = Game.World.CellAt(Actor.Location.X, Actor.Location.Y, Actor.Location.Z);
            var linkIndex = startCell.Links.FindIndex(l => l.Direction == Direction);
            if (linkIndex < 0 || linkIndex >= startCell.Links.Count) return true;
            var link = startCell.Links[linkIndex];
            if (link.Neighbor == null) return true;
            var endCell = link.Neighbor;

            if (this.ElapsedTime >= this.TotalTime)
            {
                Actor.Location = endCell.Location;
                Actor.PositionOffset = Vector3.Zero;
                Done = true;
                return true;
            }

            if (this.ElapsedTime < halfTime)
                Actor.PositionOffset = (link.EdgePoint - startCell.CenterPoint) * (this.ElapsedTime / halfTime);
            else
                Actor.PositionOffset = (link.EdgePoint - startCell.CenterPoint) +
                    ((endCell.CenterPoint - link.EdgePoint) * ((this.ElapsedTime - halfTime) / halfTime));

            return false;
        }

    }
}
