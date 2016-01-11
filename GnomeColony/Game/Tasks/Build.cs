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

        public override bool QueryValidLocation(Game Game, Coordinate GnomeLocation)
        {
            return Adjacent(GnomeLocation, Location);
        }

        public override TaskStatus QueryStatus(Game Game)
        {
            if (Object.ReferenceEquals(Game.World.CellAt(Location).Block, BlockType))
                return TaskStatus.Complete;
            if (!NoGnomesInArea(Game, Location)) return TaskStatus.Impossible;
            return TaskStatus.NotComplete;
        }

        public override Task Prerequisite(Game Game, Gnome Gnome)
        {
            if (State == States.Preparing)
            {
                var cell = Game.World.CellAt(Location);
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

        public override IEnumerable<int> GetRequiredResources()
        {
            if (State == States.Preparing)
            {
                if (BlockType.ConstructionResources != null)
                    foreach (var resource in BlockType.ConstructionResources)
                        yield return resource;
            }
        }

        public override void ExecuteTask(Game Game, Gnome Gnome)
        {
            Gnome.FacingDirection = CellLink.DirectionFromAToB(Gnome.Location, Location);
            var cell = Game.World.CellAt(Location);

            switch (State)
            {
                case States.Preparing:
                    ClearResourcesMutation = new WorldMutations.ClearResourcesMutation(Location, new List<int>(cell.Resources));
                    Game.AddWorldMutation(ClearResourcesMutation);
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
                    Progress -= Game.ElapsedSeconds;
                    if (Progress <= 0.0f)
                    {
                        BuildMutation = new WorldMutations.PlaceBlockMutation(Location, BlockType);
                        Game.AddWorldMutation(BuildMutation);
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
