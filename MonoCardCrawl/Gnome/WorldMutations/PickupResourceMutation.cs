using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gnome.WorldMutations
{
    public class PickupResourceMutation : WorldMutation
    {
        public Coordinate Location;
        public int Resource;
        public Gnome Gnome;

        public PickupResourceMutation(Coordinate Location, int Resource, Gnome Gnome)
        {
            this.MutationTimeFrame = MutationTimeFrame.BeforeUpdatingConnectivity;

            this.Location = Location;
            this.Resource = Resource;
            this.Gnome = Gnome;
        }

        public override void Apply(Game Game)
        {
            var cell = Game.World.CellAt(Location);
            
            var resourceIndex = cell.Resources.FindIndex(i => i == Resource);
            if (resourceIndex < 0)
            {
                Result = MutationResult.Failure;
                return;
            }

            if (Gnome.CarriedResource != 0)
            {
                Result = MutationResult.Failure;
                return;
            }

            Gnome.CarriedResource = Resource;
            cell.Resources.RemoveAt(resourceIndex);
            Game.World.MarkDirtyBlock(Location);

            Result = MutationResult.Success;
        }
    }
}
