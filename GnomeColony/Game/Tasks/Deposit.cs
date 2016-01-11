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

        public override bool QueryValidLocation(Game Game, Coordinate GnomeLocation)
        {
            return EnumerateAdjacent(GnomeLocation).Count(c => Game.World.Check(c) && Game.World.CellAt(c).CanPlaceResource(AssignedGnome.CarriedResource)) > 0;
        }

        public override TaskStatus QueryStatus(Game Game)
        {
            if (AssignedGnome.CarriedResource != 0) return TaskStatus.NotComplete;
            return TaskStatus.Complete;
        }

        public override void ExecuteTask(Game Game, Gnome Gnome)
        {
            var dropLocation = EnumerateAdjacent(Gnome.Location).First(c => Game.World.Check(c) && Game.World.CellAt(c).CanPlaceResource(Gnome.CarriedResource));
            Game.AddWorldMutation(new WorldMutations.DropResourceMutation(dropLocation, Gnome.CarriedResource, Gnome));
        }
    }
}
