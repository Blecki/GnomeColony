using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game
{
    public class GnomeMind
    {
        public Stack<Task> TaskStack = new Stack<Task>();
        public Gnome Owner;

        public GnomeMind(Gnome Owner)
        {
            this.Owner = Owner;
        }
            
        public Task PushTask(Task Task)
        {
            Task.AssignedGnome = Owner;
            TaskStack.Push(Task);
            return Task;
        }

        public int GetStateIcon()
        {
            if (TaskStack.Count != 0)
                return TaskStack.Peek().GnomeIcon;
            else return TileNames.TaskIconBlank;
        }

        public void Update(Game Game, Simulation Sim)
        {
            if (TaskStack.Count == 0)
            {
                // Find a new task.
                var newTask = Sim.FindTask(Owner);
                if (newTask != null)
                    PushTask(newTask);
                else
                    if (Owner.CarriedResource != 0) PushTask(new Tasks.Deposit());
            }

            var spins = 0;
            while (spins < 10) 
            {
                ++spins;
                if (TaskStack.Count == 0) return;

                var currentTask = TaskStack.Peek();
            
                var status = currentTask.QueryStatus(Sim);
                if (status == TaskStatus.Complete)
                    TaskStack.Pop();
                else if (status == TaskStatus.Impossible)
                {
                    // The task, or a prerequisite, is revealed to be impossible. Abandon the task stack entirely.
                    while (TaskStack.Count != 0)
                        Sim.AbandonTask(TaskStack.Pop());
                }
                else
                {
                    var prerequisite = currentTask.Prerequisite(Sim, Owner);
                    if (prerequisite != null)
                        PushTask(prerequisite);
                    else
                    {
                        // All prerequisites met.
                        if (currentTask.QueryValidLocation(Sim, Owner.Location))
                        {
                            currentTask.ExecuteTask(Sim, Owner);
                            break;
                        }
                        else
                            PushTask(new WalkTask(currentTask));
                    }
                }
            }
        }
    }
}
