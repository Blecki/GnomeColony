using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Tasks
{
    class FillResourceNeed : Task
    {
        private Task ParentTask;

        public FillResourceNeed(Task ParentTask) : base(ParentTask.Location)
        {
            this.ParentTask = ParentTask;
            MarkerTile = 0;
        }

        public override bool QueryValidLocation(Game Game, Coordinate GnomeLocation)
        {
            return Adjacent(GnomeLocation, Location);
        }

        public override TaskStatus QueryStatus(Game Game)
        {
            if (!Game.World.Check(Location)) return TaskStatus.Impossible;
            var cell = Game.World.CellAt(Location);
            var requiredResources = Task.FindUnfilledResourceRequirments(cell, ParentTask);
            if (requiredResources.Count == 0) return TaskStatus.Complete;
            else return TaskStatus.NotComplete;
        }

        public override Task Prerequisite(Game Game, Gnome Gnome)
        {
            var cell = Game.World.CellAt(Location);
            var requiredResources = Task.FindUnfilledResourceRequirments(cell, ParentTask);
            if (requiredResources.Count == 0) return null;

            if (Gnome.CarriedResource != 0 && !requiredResources.Contains(Gnome.CarriedResource))
                return new Deposit();

            if (Gnome.CarriedResource == 0)
                return new Acquire(requiredResources);

            // The gnome is carrying one of the required resources; let the normal mechanism move the gnome to
            // the task site to deposite it.
            return null;
        }

        public override void ExecuteTask(Game Game, Gnome Gnome)
        {
            Game.AddWorldMutation(new WorldMutations.DropResourceMutation(Location, Gnome.CarriedResource, Gnome));
        }

    }
}
