using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.WorldMutations
{
    public class PlaceBlockMutation : WorldMutation
    {
        public Coordinate Location;
        public BlockTemplate BlockType;

        public PlaceBlockMutation(Coordinate Location, BlockTemplate BlockType)
        {
            this.MutationTimeFrame = MutationTimeFrame.BeforeUpdatingConnectivity;

            this.Location = Location;
            this.BlockType = BlockType;
        }

        public override void Apply(Simulation Sim)
        {
            var cell = Sim.World.CellAt(Location);
            
            if (cell.PresentActor != null || (!Task.NoGnomesInArea(Sim, Location)))
            {
                Result = MutationResult.Failure;
                return;
            }

            cell.Block = BlockType;
            Sim.World.MarkDirtyBlock(Location);
            Result = MutationResult.Success;
        }
    }
}
