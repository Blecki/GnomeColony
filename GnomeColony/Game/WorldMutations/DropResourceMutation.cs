using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.WorldMutations
{
    public class DropResourceMutation : WorldMutation
    {
        public Coordinate Location;
        public String Resource;
        public Gnome Gnome;

        public DropResourceMutation(Coordinate Location, String Resource, Gnome Gnome)
        {
            this.MutationTimeFrame = MutationTimeFrame.BeforeUpdatingConnectivity;

            this.Location = Location;
            this.Resource = Resource;
            this.Gnome = Gnome;
        }

        public override void Apply(Simulation Sim)
        {
            var cell = Sim.World.CellAt(Location);

            if (!cell.CanPlaceResource(Resource))
            {
                Result = MutationResult.Failure;
                return;
            }

            if (Gnome.CarriedResource != Resource)
            {
                Result = MutationResult.Failure;
                return;
            }

            cell.Resources.Add(Resource);
            Gnome.CarriedResource = null;
            Sim.World.MarkDirtyBlock(Location);

            Result = MutationResult.Success;
        }
    }
}
