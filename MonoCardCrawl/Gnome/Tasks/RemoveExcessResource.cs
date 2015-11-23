using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gnome.Tasks
{
    class RemoveExcessResource : Task
    {
        private Task ParentTask;

        public RemoveExcessResource(Coordinate Location) : base(Location)
        {
            ParentTask = null;
        }

        public RemoveExcessResource(Task ParentTask) : base(ParentTask.Location)
        {
            this.ParentTask = ParentTask;
        }

        public override bool QueryValidLocation(Game Game, Coordinate GnomeLocation)
        {
            return Adjacent(GnomeLocation, Location);
        }

        public override TaskStatus QueryStatus(Game Game)
        {
            if (ParentTask != null)
            {
                if (Task.FindExcessResources(Game.World.CellAt(Location), ParentTask).Count == 0)
                    return TaskStatus.Complete;
                else
                    return TaskStatus.NotComplete;
            }
            else
            {
                if (Game.World.CellAt(Location).Resources.Count == 0)
                    return TaskStatus.Complete;
                else
                    return TaskStatus.NotComplete;
            }
        }

        public override Task Prerequisite(Game Game, Gnome Gnome)
        {
            if (Gnome.CarriedResource != 0)
                return new Deposit();
            return null;
        }

        public override void ExecuteTask(Game Game, Gnome Gnome)
        {
            var cell = Game.World.CellAt(Location);
            int resourceIndex = 0;

            if (ParentTask != null)
            {
                var excess = Task.FindExcessResources(cell, ParentTask);
                resourceIndex = cell.Resources.FindIndex(i => excess.Contains(i));
            }

            if (resourceIndex >= 0)
                Game.AddWorldMutation(new WorldMutations.PickupResourceMutation(Location, cell.Resources[resourceIndex], Gnome));
        }
    }
}
