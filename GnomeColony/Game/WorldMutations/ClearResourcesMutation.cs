using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.WorldMutations
{
    public class ClearResourcesMutation : WorldMutation
    {
        public Coordinate Location;
        public List<int> ExpectedResources;

        public ClearResourcesMutation(Coordinate Location, List<int> ExpectedResources)
        {
            this.MutationTimeFrame = MutationTimeFrame.BeforeUpdatingConnectivity;

            this.Location = Location;
            this.ExpectedResources = ExpectedResources;
        }

        public override void Apply(Simulation Sim)
        {
            var cell = Sim.World.CellAt(Location);

            if (cell.Resources.Count != ExpectedResources.Count)
            {
                Result = MutationResult.Failure;
                return;
            }

            var pairs = ExpectedResources.OrderBy(i => i).Zip(cell.Resources.OrderBy(i => i), (a, b) => Tuple.Create(a, b));

            foreach (var pair in pairs)
                if (pair.Item1 != pair.Item2)
                {
                    Result = MutationResult.Failure;
                    return;
                }

            cell.Resources.Clear();            
            Sim.World.MarkDirtyBlock(Location);

            Result = MutationResult.Success;
        }
    }
}
