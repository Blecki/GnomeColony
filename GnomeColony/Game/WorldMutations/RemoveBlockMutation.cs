using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.WorldMutations
{
    public class RemoveBlockMutation : WorldMutation
    {
        public Coordinate Location;
        public Gnome Gnome;

        public RemoveBlockMutation(Coordinate Location, Gnome Gnome)
        {
            this.MutationTimeFrame = MutationTimeFrame.BeforeUpdatingConnectivity;

            this.Location = Location;
            this.Gnome = Gnome;
        }

        public override void Apply(Game Game)
        {
            var cell = Game.World.CellAt(Location);

            if (cell.Block == null)
            {
                Result = MutationResult.Failure;
                return;
            }

            if (cell.PresentActor != null || (!Task.NoGnomesInArea(Game, Location)))
            {
                Result = MutationResult.Failure;
                return;
            }

            if (cell.Resources.Count != 0)
            {
                Result = MutationResult.Failure;
                return;
            }
            
            cell.Resources = new List<int>(cell.Block.MineResources);
            cell.Block = null;

            if (cell.Resources.Count > 0)
            {
                Gnome.CarriedResource = cell.Resources[0];
                cell.Resources.RemoveAt(0);
            }

            Game.SetUpdateFlag(Location);

            Result = MutationResult.Success;
        }
    }
}
