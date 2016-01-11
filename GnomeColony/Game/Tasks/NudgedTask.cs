﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class NudgedTask : Task
    {
        public Gnome Nudger;
        private WorldMutations.ActorMoveMutation MoveMutation = null;
        public bool FailedToFindPath = false;

        private static Gem.Pathfinding<Cell> Pathfinding = new Gem.Pathfinding<Cell>(
           (cell) =>
           {
               return new List<Cell>(cell.Links.Select(c => c.Neighbor).Where(c => c.Navigatable));
           },
           (cell) =>
           {
               if (cell.PresentActor != null) return 4.0f;
               return 1.0f;
           });

        public NudgedTask(Gnome Nudger, Coordinate Location)
            : base(Location)
        {
            this.Nudger = Nudger;
        }

        public override TaskStatus QueryStatus(Game Game)
        {
            if (FailedToFindPath) return TaskStatus.Impossible;
            if (MoveMutation != null && !MoveMutation.Done) return TaskStatus.NotComplete;
            if (AssignedGnome.Location == Location) return TaskStatus.NotComplete;
            return TaskStatus.Complete;
        }

        public override bool QueryValidLocation(Game Game, Coordinate GnomeLocation)
        {
            return true;

            // Any spot that ISN'T the place we started is a good spot.
            //if (GnomeLocation == Location) return false;
            //if (GnomeLocation == Nudger.Location) return false; // Avoid nudging back
            //return true;
        }

        public override void ExecuteTask(Game Game, Gnome Gnome)
        {
            var cell = Game.World.CellAt(Location);
            if (cell.Links.Count == 0)
            {
                FailedToFindPath = true;
                return;
            }

            var retreatLinks = cell.Links.OrderBy(l => l.Neighbor.PresentActor == null ? 0 : 1);
            var link = retreatLinks.First();

            var blockingGnome = link.Neighbor.PresentActor as Gnome;
            if (blockingGnome != null)
                blockingGnome.PushTask(new NudgedTask(Gnome, link.Neighbor.Location));
            else
            {
                MoveMutation = new WorldMutations.ActorMoveMutation(AssignedGnome.Location, link.Neighbor, Gnome);
                Game.AddWorldMutation(MoveMutation);
            }
        }
    }
}
