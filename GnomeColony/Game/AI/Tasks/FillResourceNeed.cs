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

        public override bool QueryValidLocation(Simulation Sim, Coordinate GnomeLocation)
        {
            return Adjacent(GnomeLocation, Location);
        }

        public override TaskStatus QueryStatus(Simulation Sim)
        {
            if (!Sim.World.Check(Location)) return TaskStatus.Impossible;
            var cell = Sim.World.CellAt(Location);
            var requiredResources = Task.FindUnfilledResourceRequirments(cell, ParentTask);
            if (requiredResources.Count == 0) return TaskStatus.Complete;
            else return TaskStatus.NotComplete;
        }

        public override Task Prerequisite(Simulation Sim)
        {
            var cell = Sim.World.CellAt(Location);
            var requiredResources = Task.FindUnfilledResourceRequirments(cell, ParentTask);
            if (requiredResources.Count == 0) return null;

            if (AssignedGnome.CarryingResource && !requiredResources.Contains(AssignedGnome.CarriedResource))
                return new Deposit();

            if (!AssignedGnome.CarryingResource)
                return new Acquire(requiredResources);

            // The gnome is carrying one of the required resources; let the normal mechanism move the gnome to
            // the task site to deposite it.
            return null;
        }

        public override void ExecuteTask(Simulation Sim)
        {
            Sim.AddWorldMutation(new WorldMutations.DropResourceMutation(Location, AssignedGnome.CarriedResource, AssignedGnome));
        }

    }
}
