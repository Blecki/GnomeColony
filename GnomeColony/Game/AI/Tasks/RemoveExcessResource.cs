using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Tasks
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

        public override bool QueryValidLocation(Simulation Sim, Coordinate GnomeLocation)
        {
            return Adjacent(GnomeLocation, Location);
        }

        public override TaskStatus QueryStatus(Simulation Sim)
        {
            if (ParentTask != null)
            {
                if (Task.FindExcessResources(Sim.World.CellAt(Location), ParentTask).Count == 0)
                    return TaskStatus.Complete;
                else
                    return TaskStatus.NotComplete;
            }
            else
            {
                if (Sim.World.CellAt(Location).Resources.Count == 0)
                    return TaskStatus.Complete;
                else
                    return TaskStatus.NotComplete;
            }
        }

        public override Task Prerequisite(Simulation Sim, Gnome Gnome)
        {
            if (Gnome.CarryingResource)
                return new Deposit();
            return null;
        }

        public override void ExecuteTask(Simulation Sim, Gnome Gnome)
        {
            var cell = Sim.World.CellAt(Location);
            int resourceIndex = 0;

            if (ParentTask != null)
            {
                var excess = Task.FindExcessResources(cell, ParentTask);
                resourceIndex = cell.Resources.FindIndex(i => excess.Contains(i));
            }

            if (resourceIndex >= 0)
                Sim.AddWorldMutation(new WorldMutations.PickupResourceMutation(Location, cell.Resources[resourceIndex], Gnome));
        }
    }
}
