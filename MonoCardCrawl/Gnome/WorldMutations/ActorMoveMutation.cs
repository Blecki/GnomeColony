using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gnome.WorldMutations
{
    public class ActorMoveMutation : WorldMutation
    {
        public Coordinate Location;
        public Cell Onto;
        public Actor Actor;
        public MoveAction Action;
        public bool Done
        {
            get
            {
                if (Action == null) return true;
                return Action.Done;
            }
        }

        public ActorMoveMutation(Coordinate Location, Cell Onto, Actor Actor)
        {
            this.MutationTimeFrame = MutationTimeFrame.AfterUpdatingConnectivity;

            this.Location = Location;
            this.Onto = Onto;
            this.Actor = Actor;
        }

        public override void Apply(Game Game)
        {
            var cell = Game.World.CellAt(Location);

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

            Action = new MoveAction(cell, cell.Links[stepIndex].Direction, 0.5f);
            Actor.NextAction = Action;
            Actor.FacingDirection = cell.Links[stepIndex].Direction;

            Actor.Location = Onto.Location;

            Result = MutationResult.Success;
        }
    }
}
