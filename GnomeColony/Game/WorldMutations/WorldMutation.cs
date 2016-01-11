using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public enum MutationTimeFrame
    {
        BeforeUpdatingConnectivity,
        AfterUpdatingConnectivity
    }

    public enum MutationResult
    {
        Success, 
        Failure
    }

    public class WorldMutation
    {
        public MutationResult Result { get; protected set; }
        public MutationTimeFrame MutationTimeFrame = MutationTimeFrame.BeforeUpdatingConnectivity;

        public virtual void Apply(Game Game) { throw new NotImplementedException(); }

        public WorldMutation()
        {
            Result = MutationResult.Failure;
        }
    }
}
