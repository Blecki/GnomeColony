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

        public override TaskStatus QueryStatus(Simulation Sim)
        {
            if (FailedToFindPath) return TaskStatus.Impossible;
            if (GoalTask.QueryValidLocation(Sim, AssignedGnome.Location)) return TaskStatus.Complete;
            return TaskStatus.NotComplete;
        }

        public override bool QueryValidLocation(Simulation Sim, Coordinate GnomeLocation)
        {
            return true; // This is the task that takes gnomes to valid locations.
        }

        public override void ExecuteTask(Simulation Sim)
        {
            var pathfindingResult = Pathfinding.Flood(Sim.World.CellAt(AssignedGnome.Location),
                            c => GoalTask.QueryValidLocation(Sim, c.Location),
                            c => 1.0f);

            if (pathfindingResult.GoalFound)
            {
                var path = pathfindingResult.FinalNode.ExtractPath();
                if (path.Count > 1)
                {
                    var blockingGnome = path[1].PresentActor as Gnome;
                    if (blockingGnome != null)
                    {
                        blockingGnome.Mind.PushTask(new NudgedTask(AssignedGnome, path[1].Location));
                    }
                    else
                    {
                        MoveMutation = new WorldMutations.ActorMoveMutation(AssignedGnome.Location, path[1], AssignedGnome);
                        Sim.AddWorldMutation(MoveMutation);
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
