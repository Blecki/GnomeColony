using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Tasks
{
    class Acquire : Task
    {
        private List<int> ResourceTypes;

        public Acquire(List<int> ResourceTypes) : base(new Coordinate(0,0,0))
        {
            this.ResourceTypes = ResourceTypes;
            MarkerTile = 2;
        }

        public override bool QueryValidLocation(Game Game, Coordinate GnomeLocation)
        {
            foreach (var adjacentTile in EnumerateAdjacent(GnomeLocation))
            {
                if (Game.World.Check(adjacentTile))
                {
                    var cell = Game.World.CellAt(adjacentTile);
                    if (cell.HasFlag(CellFlags.Storehouse) && cell.Resources.Count(i => ResourceTypes.Contains(i)) > 0)
                    {
                        Location = adjacentTile;
                        return true;
                    }
                }
            }

            return false;
        }

        public override TaskStatus QueryStatus(Game Game)
        {
            if (AssignedGnome.CarriedResource != 0 && ResourceTypes.Contains(AssignedGnome.CarriedResource)) return TaskStatus.Complete;
            return TaskStatus.NotComplete;
        }

        public override Task Prerequisite(Game Game, Gnome Gnome)
        {
            if (Gnome.CarriedResource != 0 && !ResourceTypes.Contains(AssignedGnome.CarriedResource))
                return new Deposit();
            return null;
        }

        public override void ExecuteTask(Game Game, Gnome Gnome)
        {
            var cell = Game.World.CellAt(Location);
            var resourceIndex = cell.Resources.FindIndex(i => ResourceTypes.Contains(i));
            Game.AddWorldMutation(new WorldMutations.PickupResourceMutation(Location, cell.Resources[resourceIndex], Gnome));
        }
    }
}
