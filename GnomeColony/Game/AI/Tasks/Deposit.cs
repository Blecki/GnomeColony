using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Tasks
{
    class Deposit : Task
    {
        public Deposit() : base(new Coordinate(0,0,0))
        {
            MarkerTile = 0;
        }

        private bool CanPlace(Cell C, String ResourceType)
        {
            if (C.HasFlag(CellFlags.Storehouse) && C.Resources.Count < 8) return true;
            else if (C.Task != null) return Task.FindUnfilledResourceRequirments(C, C.Task).Contains(ResourceType);
            return false;
        }

        public override bool QueryValidLocation(Simulation Sim, Coordinate GnomeLocation)
        {
            return EnumerateAdjacent(GnomeLocation).Count(c => Sim.World.Check(c) && CanPlace(Sim.World.CellAt(c), AssignedGnome.CarriedResource)) > 0;
        }

        public override TaskStatus QueryStatus(Simulation Sim)
        {
            if (AssignedGnome.CarryingResource) return TaskStatus.NotComplete;
            return TaskStatus.Complete;
        }

        public override void ExecuteTask(Simulation Sim, Gnome Gnome)
        {
            var dropLocation = EnumerateAdjacent(Gnome.Location).First(c => Sim.World.Check(c) && CanPlace(Sim.World.CellAt(c), Gnome.CarriedResource));
            Sim.AddWorldMutation(new WorldMutations.DropResourceMutation(dropLocation, Gnome.CarriedResource, Gnome));
        }
    }
}
