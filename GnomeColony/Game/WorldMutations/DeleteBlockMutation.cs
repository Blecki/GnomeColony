using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.WorldMutations
{
    public class DeleteBlockMutation : WorldMutation
    {
        public Coordinate Location;

        public DeleteBlockMutation(Coordinate Location)
        {
            this.MutationTimeFrame = MutationTimeFrame.BeforeUpdatingConnectivity;
            this.Location = Location;
        }

        public override void Apply(Simulation Sim)
        {
            var cell = Sim.World.CellAt(Location);
            cell.Block = null;
            Sim.SetUpdateFlag(Location);
            Result = MutationResult.Success;
        }
    }
}
