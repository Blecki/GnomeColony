using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gnome.Tasks
{
    class Mine : Task
    {
        WaitAction Progress = null;

        public Mine(Coordinate Location) : base(Location)
        {
            MarkerTile = TileNames.TaskMarkerMine;
            GnomeIcon = TileNames.TaskIconMine;
        }

        public override bool QueryValidLocation(Game Game, Coordinate GnomeLocation)
        {
            return Adjacent(GnomeLocation, Location);
        }

        public override TaskStatus QueryStatus(Game Game)
        {
            if (Game.World.CellAt(Location).Block == null)
            {
                if (Game.World.CellAt(Location).Resources.Count == 0) return TaskStatus.Complete;
                else return TaskStatus.NotComplete;
            }
            if (!NoGnomesInArea(Game, Location)) return TaskStatus.Impossible;
            
            return TaskStatus.NotComplete;
        }

        public override Task Prerequisite(Game Game, Gnome Gnome)
        {
            if (Gnome.CarriedResource != 0) return new Deposit();
            if (Game.World.CellAt(Location).Resources.Count != 0) return new RemoveExcessResource(this.Location);
            return null;
        }

        public override void ExecuteTask(Game Game, Gnome Gnome)
        {
            Gnome.FacingDirection = CellLink.DirectionFromAToB(Gnome.Location, Location);

            if (Progress == null)
            {
                Progress = new WaitAction(2.0f);
                Gnome.NextAction = Progress;
            }
            else if (Progress.Done)
            {
                Progress = null;

                var cell = Game.World.CellAt(Location);
                cell.Resources = new List<int>(cell.Block.MineResources);
                cell.Block = null;
                
                if (cell.Resources.Count > 0)
                {
                    Gnome.CarriedResource = cell.Resources[0];
                    cell.Resources.RemoveAt(0);
                }
                
                Game.SetUpdateFlag(Location);
            }
        }
    }
}
