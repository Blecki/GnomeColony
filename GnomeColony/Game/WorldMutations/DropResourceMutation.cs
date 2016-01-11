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
        public int Resource;
        public Gnome Gnome;

        public DropResourceMutation(Coordinate Location, int Resource, Gnome Gnome)
        {
            this.MutationTimeFrame = MutationTimeFrame.BeforeUpdatingConnectivity;

            this.Location = Location;
            this.Resource = Resource;
            this.Gnome = Gnome;
        }

        public override void Apply(Game Game)
        {
            var cell = Game.World.CellAt(Location);

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
            Gnome.CarriedResource = 0;
            Game.World.MarkDirtyBlock(Location);

            Result = MutationResult.Success;
        }
    }
}
