using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gnome.Tasks
{
    class FillResourceNeed : Task
    {
        private BlockTemplate ResourceType;

        public FillResourceNeed(BlockTemplate ResourceType, Coordinate Location) : base(Location)
        {
            this.ResourceType = ResourceType;
            MarkerTile = 2;
        }

        public override bool QueryValidLocation(Game Game, Coordinate GnomeLocation)
        {
            return Adjacent(GnomeLocation, Location);
        }

        public override TaskStatus QueryStatus(Game Game)
        {
            if (!Game.World.Check(Location)) return TaskStatus.Impossible;
            var cell = Game.World.CellAt(Location);
            if (cell.Resource == null) return TaskStatus.Impossible;
            if (cell.Resource.Filled) return TaskStatus.Complete;
            else return TaskStatus.NotComplete;
        }

        public override Task Prerequisite(Game Game, Gnome Gnome)
        {
            if (!Object.ReferenceEquals(Gnome.CarriedResource, ResourceType))
            {
                if (Gnome.CarriedResource == null)
                    return new Acquire(ResourceType);
                else
                    return new Deposit();
            }

            return null;
        }

        public override void ExecuteTask(Game Game, Gnome Gnome)
        {
            var cell = Game.World.CellAt(Location);
            cell.Resource.Filled = true;
            Gnome.CarriedResource = null;
            Game.World.MarkDirtyBlock(Location);
        }

        public override int GnomeIcon()
        {
            return 482;
        }
    }
}
