using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.WorldMutations
{
    public class ActorMoveMutation : WorldMutation
    {
        public Coordinate Location;
        public Cell Onto;
        public Actor Actor;
        
        public ActorMoveMutation(Coordinate Location, Cell Onto, Actor Actor)
        {
            this.MutationTimeFrame = MutationTimeFrame.AfterUpdatingConnectivity;

            this.Location = Location;
            this.Onto = Onto;
            this.Actor = Actor;
        }

        public override void Apply(Simulation Sim)
        {
            var cell = Sim.World.CellAt(Location);

            if (Onto.PresentActor != null)
            {
                Result = MutationResult.Failure;
                return;
            }

            var stepIndex = cell.Links.FindIndex(l => Object.ReferenceEquals(l.Neighbor, Onto));
            if (stepIndex < 0 || stepIndex >= cell.Links.Count)
            {
                Result = MutationResult.Failure;
                return;
            }

            cell.PresentActor = null;
            Onto.PresentActor = Actor;

            // Todo: Reimplement smooth movement.
            //Actor.CurrentAction = new MoveAction(cell, cell.Links[stepIndex].Direction);
            Actor.FacingDirection = cell.Links[stepIndex].Direction;

            Actor.Location = Onto.Location;

            Result = MutationResult.Success;
        }
    }
}
