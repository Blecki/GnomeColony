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

        public override bool QueryValidLocation(Simulation Sim, Coordinate GnomeLocation)
        {
            return Adjacent(GnomeLocation, Location);
        }

        public override TaskStatus QueryStatus(Simulation Sim)
        {
            if (State == States.Done) return TaskStatus.Complete;
            if (!NoGnomesInArea(Sim, Location)) return TaskStatus.Impossible;
            return TaskStatus.NotComplete;
        }

        public override Task Prerequisite(Simulation Sim)
        {
            if (State != States.Mining) return null;

            if (AssignedGnome.CarryingResource) return new Deposit();
            if (Sim.World.CellAt(Location).Resources.Count != 0) return new RemoveExcessResource(this.Location);
            return null;
        }

        public override void ExecuteTask(Simulation Sim)
        {
            switch (State)
            {
                case States.Mining:
                    AssignedGnome.FacingDirection = CellLink.DirectionFromAToB(AssignedGnome.Location, Location);

                    Progress -= 0.1f;//Game.ElapsedSeconds;
                    if (Progress <= 0.0f)
                    {
                        MineMutation = new WorldMutations.RemoveBlockMutation(Location, AssignedGnome);
                        Sim.AddWorldMutation(MineMutation);
                        State = States.Finalizing;
                    }

                    return;
                case States.Finalizing:
                    if (MineMutation.Result == MutationResult.Failure)
                        State = States.Mining;
                    else
                    {
                        State = States.Done;
                        Sim.AddTask(new RemoveExcessResource(Location));
                    }
                    return;
                case States.Done:
                    throw new InvalidProgramException("Task should have been deemed completed.");
            }            
        }
    }
}
