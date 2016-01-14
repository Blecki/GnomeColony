using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.WorldMutations
{
    public class PickupResourceMutation : WorldMutation
    {
        public Coordinate Location;
        public String Resource;
        public Gnome Gnome;

        public PickupResourceMutation(Coordinate Location, String Resource, Gnome Gnome)
        {
            this.MutationTimeFrame = MutationTimeFrame.BeforeUpdatingConnectivity;

            this.Location = Location;
            this.Resource = Resource;
            this.Gnome = Gnome;
        }

        public override void Apply(Simulation Sim)
        {
            var cell = Sim.World.CellAt(Location);
            
            var resourceIndex = cell.Resources.FindIndex(i => i == Resource);
            if (resourceIndex < 0)
            {
                Result = MutationResult.Failure;
                return;
            }

            if (Gnome.CarryingResource)
            {
                Result = MutationResult.Failure;
                return;
            }

            Gnome.CarriedResource = Resource;
            cell.Resources.RemoveAt(resourceIndex);
            Sim.World.MarkDirtyBlock(Location);

            Result = MutationResult.Success;
        }
    }
}
