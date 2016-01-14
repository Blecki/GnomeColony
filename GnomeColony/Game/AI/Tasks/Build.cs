using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Tasks
{
    class Build : Task
    {
        private BlockTemplate BlockType;
        private float Progress = 1.0f;

        private enum States
        {
            Preparing,
            ClearingResources,
            Constructing,
            Finalizing
        }

        private States State = States.Preparing;
        private WorldMutations.ClearResourcesMutation ClearResourcesMutation = null;
        private WorldMutations.PlaceBlockMutation BuildMutation = null;

        public Build(BlockTemplate BlockType, Coordinate Location) : base(Location)
        {
            this.BlockType = BlockType;
            MarkerTile = TileNames.TaskIconBlank;
            GnomeIcon = TileNames.TaskIconBuild;
        }

        public override bool QueryValidLocation(Simulation Sim, Coordinate GnomeLocation)
        {
            return Adjacent(GnomeLocation, Location);
        }

        public override TaskStatus QueryStatus(Simulation Sim)
        {
            if (Object.ReferenceEquals(Sim.World.CellAt(Location).Block, BlockType))
                return TaskStatus.Complete;
            if (!NoGnomesInArea(Sim, Location)) return TaskStatus.Impossible;
            return TaskStatus.NotComplete;
        }

        public override Task Prerequisite(Simulation Sim, Gnome Gnome)
        {
            if (State == States.Preparing)
            {
                var cell = Sim.World.CellAt(Location);
                var excessResources = FindExcessResources(cell, this);
                var unfilledResources = FindUnfilledResourceRequirments(cell, this);

                // Move excess resources off this tile.
                if (excessResources.Count > 0)
                    return new RemoveExcessResource(this);

                if (unfilledResources.Count > 0)
                    return new FillResourceNeed(this);
            }

            return null;
        }

        public override IEnumerable<String> GetRequiredResources()
        {
            if (State == States.Preparing)
            {
                if (BlockType.ConstructionResources != null)
                    foreach (var resource in BlockType.ConstructionResources)
                        yield return resource;
            }
        }

        public override void ExecuteTask(Simulation Sim, Gnome Gnome)
        {
            Gnome.FacingDirection = CellLink.DirectionFromAToB(Gnome.Location, Location);
            var cell = Sim.World.CellAt(Location);

            switch (State)
            {
                case States.Preparing:
                    ClearResourcesMutation = new WorldMutations.ClearResourcesMutation(Location, new List<String>(cell.Resources));
                    Sim.AddWorldMutation(ClearResourcesMutation);
                    State = States.ClearingResources;
                    return;
                case States.ClearingResources:
                    if (ClearResourcesMutation.Result != MutationResult.Success)
                        State = States.Preparing;
                    else
                    {
                        Progress = 1.0f;
                        State = States.Constructing;
                    }
                    return;
                case States.Constructing:
                    Progress -= 0.1f;//Sim.ElapsedSeconds;
                    if (Progress <= 0.0f)
                    {
                        BuildMutation = new WorldMutations.PlaceBlockMutation(Location, BlockType);
                        Sim.AddWorldMutation(BuildMutation);
                        State = States.Finalizing;
                    }
                    return;
                case States.Finalizing:
                    if (BuildMutation.Result == MutationResult.Success)
                        throw new InvalidProgramException("Task should have been deemed completed.");
                    Progress = 0.0f;
                    State = States.Constructing; // Try to build it again.
                    return;
                default:
                    throw new InvalidProgramException("Uknown Case");
            }

        }
    }
}
