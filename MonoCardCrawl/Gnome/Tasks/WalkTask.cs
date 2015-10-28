using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gnome
{
    public class WalkTask : Task
    {
        public Task GoalTask;
        private MoveAction Action = null;
        public bool FailedToFindPath = false;

        public WalkTask(Task GoalTask) : base(new Coordinate(0,0,0))
        {
            this.GoalTask = GoalTask;
            GnomeIcon = GoalTask.GnomeIcon;
        }

        public override TaskStatus QueryStatus(Game Game)
        {
            if (FailedToFindPath) return TaskStatus.Impossible;
            if (Action != null && !Action.Done) return TaskStatus.NotComplete;
            if (GoalTask.QueryValidLocation(Game, AssignedGnome.Location)) return TaskStatus.Complete;
            return TaskStatus.NotComplete;
        }

        public override bool QueryValidLocation(Game Game, Coordinate GnomeLocation)
        {
            return true; // This is the task that takes gnomes to valid locations.
        }

        public override void ExecuteTask(Game Game, Gnome Gnome)
        {
            if (Action == null || Action.Done)
            {
                var pathfindingResult = Game.Pathfinding.Flood(Game.World.CellAt(AssignedGnome.Location),
                                c => GoalTask.QueryValidLocation(Game, c.Location),
                                c => 1.0f);
                if (pathfindingResult.GoalFound)
                {
                    var path = pathfindingResult.FinalNode.ExtractPath();
                    if (path.Count > 1)
                    {
                        var stepIndex = path[0].Links.FindIndex(l => Object.ReferenceEquals(l.Neighbor, path[1]));
                        if (stepIndex >= 0 && stepIndex < path[0].Links.Count)
                        {
                            Action = new MoveAction(path[0].Links[stepIndex].Direction, 0.5f);
                            AssignedGnome.NextAction = Action;
                            AssignedGnome.FacingDirection = path[0].Links[stepIndex].Direction;
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
