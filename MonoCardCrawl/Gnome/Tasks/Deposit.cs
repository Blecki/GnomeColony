using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gnome.Tasks
{
    class Deposit : Task
    {
        public Deposit() : base(new Coordinate(0,0,0))
        {
            MarkerTile = 2;
        }

        public override bool QueryValidLocation(Game Game, Coordinate GnomeLocation)
        {
            return EnumerateAdjacent(GnomeLocation).Count(c => Game.World.Check(c) && Game.World.CellAt(c).FullfillsResourceRequirement(AssignedGnome.CarriedResource)) > 0;
        }

        public override TaskStatus QueryStatus(Game Game)
        {
            if (AssignedGnome.CarriedResource != null) return TaskStatus.NotComplete;
            return TaskStatus.Complete;
        }

        public override void ExecuteTask(Game Game, Gnome Gnome)
        {
            var dropLocation = EnumerateAdjacent(Gnome.Location).First(c => Game.World.Check(c) && Game.World.CellAt(c).FullfillsResourceRequirement(Gnome.CarriedResource));
            var cell = Game.World.CellAt(dropLocation);
            cell.Resource.Filled = true;
            Gnome.CarriedResource = null;
            Game.World.MarkDirtyBlock(dropLocation);
        }
    }
}
