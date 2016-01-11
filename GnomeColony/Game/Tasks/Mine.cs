using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Tasks
{
    class Mine : Task
    {
        private float Progress = 1.0f;

        private enum States
        {
            Mining,
            Finalizing,
            Done
        }

        private States State = States.Mining;
        private WorldMutations.RemoveBlockMutation MineMutation = null;

        public Mine(Coordinate Location) : base(Location)
        {
            MarkerTile = TileNames.TaskMarkerMine;
            GnomeIcon = TileNames.TaskIconMine;
        }

        public override bool QueryValidLocation(Game Game, Coordinate GnomeLocation)
        {
            return Adjacent(GnomeLocation, Location);
        }

        public override TaskStatus QueryStatus(Game Game)
        {
            if (State == States.Done) return TaskStatus.Complete;
            if (!NoGnomesInArea(Game, Location)) return TaskStatus.Impossible;
            return TaskStatus.NotComplete;
        }

        public override Task Prerequisite(Game Game, Gnome Gnome)
        {
            if (State != States.Mining) return null;

            if (Gnome.CarriedResource != 0) return new Deposit();
            if (Game.World.CellAt(Location).Resources.Count != 0) return new RemoveExcessResource(this.Location);
            return null;
        }

        public override void ExecuteTask(Game Game, Gnome Gnome)
        {
            switch (State)
            {
                case States.Mining:
                    Gnome.FacingDirection = CellLink.DirectionFromAToB(Gnome.Location, Location);

                    Progress -= Game.ElapsedSeconds;
                    if (Progress <= 0.0f)
                    {
                        MineMutation = new WorldMutations.RemoveBlockMutation(Location, Gnome);
                        Game.AddWorldMutation(MineMutation);
                        State = States.Finalizing;
                    }

                    return;
                case States.Finalizing:
                    if (MineMutation.Result == MutationResult.Failure)
                        State = States.Mining;
                    else
                    {
                        State = States.Done;
                        Game.AddTask(new RemoveExcessResource(Location));
                    }
                    return;
                case States.Done:
                    throw new InvalidProgramException("Task should have been deemed completed.");
            }            
        }
    }
}
