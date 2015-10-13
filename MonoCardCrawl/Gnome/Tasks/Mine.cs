using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gnome.Tasks
{
    class Mine : Task
    {
        WaitAction MiningProgress = null;

        public Mine(Coordinate Location) : base(Location)
        {
            MarkerTile = 2;
        }

        public override bool QueryValidLocation(Game Game, Coordinate GnomeLocation)
        {
            return Adjacent(GnomeLocation, Location);
        }

        public override TaskStatus QueryStatus(Game Game)
        {
            if (Game.World.CellAt(Location).Block == null) return TaskStatus.Complete;
            return TaskStatus.NotComplete;
        }

        public override Task Prerequisite(Game Game, Gnome Gnome)
        {
            if (Gnome.CarriedResource != null) return new Deposit();
            return null;
        }

        public override void ExecuteTask(Game Game, Gnome Gnome)
        {
            Gnome.FacingDirection = CellLink.DirectionFromAToB(Gnome.Location, Location);

            if (MiningProgress == null)
            {
                MiningProgress = new WaitAction(2.0f);
                Gnome.NextAction = MiningProgress;
            }
            else if (MiningProgress.Done)
            {
                MiningProgress = null;
                Gnome.CarriedResource = Game.World.CellAt(Location).Block;
                Game.World.CellAt(Location).Block = null;
                Game.SetUpdateFlag(Location);
            }
        }

        public override int GnomeIcon()
        {
            return 483;
        }
    }
}
