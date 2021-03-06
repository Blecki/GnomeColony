﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Tasks
{
    class Acquire : Task
    {
        private List<String> ResourceTypes;

        public Acquire(List<String> ResourceTypes) : base(new Coordinate(0,0,0))
        {
            this.ResourceTypes = ResourceTypes;
            MarkerTile = 2;
        }

        public override bool QueryValidLocation(Simulation Sim, Coordinate GnomeLocation)
        {
            foreach (var adjacentTile in EnumerateAdjacent(GnomeLocation))
            {
                if (Sim.World.Check(adjacentTile))
                {
                    var cell = Sim.World.CellAt(adjacentTile);
                    if (cell.HasFlag(CellFlags.Storehouse) && cell.Resources.Count(i => ResourceTypes.Contains(i)) > 0)
                    {
                        Location = adjacentTile;
                        return true;
                    }
                }
            }

            return false;
        }

        public override TaskStatus QueryStatus(Simulation Sim)
        {
            if (!AssignedGnome.CarryingResource && ResourceTypes.Contains(AssignedGnome.CarriedResource)) return TaskStatus.Complete;
            return TaskStatus.NotComplete;
        }

        public override Task Prerequisite(Simulation Sim)
        {
            if (AssignedGnome.CarryingResource && !ResourceTypes.Contains(AssignedGnome.CarriedResource))
                return new Deposit();
            return null;
        }

        public override void ExecuteTask(Simulation Sim)
        {
            var cell = Sim.World.CellAt(Location);
            var resourceIndex = cell.Resources.FindIndex(i => ResourceTypes.Contains(i));
            Sim.AddWorldMutation(new WorldMutations.PickupResourceMutation(Location, cell.Resources[resourceIndex], AssignedGnome));
        }
    }
}
