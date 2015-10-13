using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gnome.Tasks
{
    class Acquire : Task
    {
        private BlockTemplate ResourceType;

        public Acquire(BlockTemplate ResourceType) : base(new Coordinate(0,0,0))
        {
            this.ResourceType = ResourceType;
            MarkerTile = 2;
        }

        public override bool QueryValidLocation(Game Game, Coordinate GnomeLocation)
        {
            foreach (var adjacentTile in EnumerateAdjacent(GnomeLocation))
            {
                if (Game.World.Check(adjacentTile))
                {
                    var cell = Game.World.CellAt(adjacentTile);
                    if (cell.Storehouse && cell.Resource.Filled && Object.ReferenceEquals(cell.Resource.BlockType, ResourceType))
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
            if (Object.ReferenceEquals(AssignedGnome.CarriedResource, ResourceType)) return TaskStatus.Complete;
            return TaskStatus.NotComplete;
        }

        public override Task Prerequisite(Game Game, Gnome Gnome)
        {
            if (Gnome.CarriedResource != null && !Object.ReferenceEquals(Gnome.CarriedResource, ResourceType))
                return new Deposit();
            return null;
        }

        public override void ExecuteTask(Game Game, Gnome Gnome)
        {
           var cell = Game.World.CellAt(Location);
            cell.Resource.Filled = false;
            Gnome.CarriedResource = ResourceType;
            Game.World.MarkDirtyBlock(Location);
        }
    }
}
