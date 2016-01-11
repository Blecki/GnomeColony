using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class WalkTask : Task
    {
        public Task GoalTask;
        private WorldMutations.ActorMoveMutation MoveMutation = null;
        public bool FailedToFindPath = false;

        private static Gem.Pathfinding<Cell> Pathfinding = new Gem.Pathfinding<Cell>(
            (cell) =>
            {
                return new List<Cell>(cell.Links.Select(c => c.Neighbor).Where(c => c.Navigatable));
            },
            (cell) =>
            {
                if (cell.PresentActor != null) return 4.0f;
                return 1.0f;
            });

        public WalkTask(Task GoalTask) : base(new Coordinate(0,0,0))
        {
            this.GoalTask = GoalTask;
            GnomeIcon = GoalTask.GnomeIcon;
        }

        public override TaskStatus QueryStatus(Game Game)
        {
            if (FailedToFindPath) return TaskStatus.Impossible;
            if (MoveMutation != null && !MoveMutation.Done) return TaskStatus.NotComplete;
            if (GoalTask.QueryValidLocation(Game, AssignedGnome.Location)) return TaskStatus.Complete;
            return TaskStatus.NotComplete;
        }

        public override bool QueryValidLocation(Game Game, Coordinate GnomeLocation)
        {
            return true; // This is the task that takes gnomes to valid locations.
        }

        public override void ExecuteTask(Game Game, Gnome Gnome)
        {
            //if (MoveMutation != null && !MoveMutation.Done && !Object.ReferenceEquals(Gnome.CurrentAction, MoveMutation.Action))
            //    throw new InvalidProgramException("MoveAction ended prematurely.");

            if (MoveMutation == null || MoveMutation.Done)
            {
                var pathfindingResult = Pathfinding.Flood(Game.World.CellAt(AssignedGnome.Location),
                                c => GoalTask.QueryValidLocation(Game, c.Location),
                                c => 1.0f);

                if (pathfindingResult.GoalFound)
                {
                    var path = pathfindingResult.FinalNode.ExtractPath();
                    if (path.Count > 1)
                    {
                        var blockingGnome = path[1].PresentActor as Gnome;
                        if (blockingGnome != null)
                        {
                            if (blockingGnome.CanInterupt)
                                blockingGnome.PushTask(new NudgedTask(Gnome, path[1].Location));
                        }
                        else
                        {
                            MoveMutation = new WorldMutations.ActorMoveMutation(AssignedGnome.Location, path[1], Gnome);
                            Game.AddWorldMutation(MoveMutation);
                        }
                    }
                }
                else
                {
                    FailedToFindPath = true;
                }
            }
        }
    }
}
