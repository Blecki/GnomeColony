using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;
using Gem.Render;

namespace Game
{
    public class Simulation
    {
        public CellGrid World { get; private set; }
        private List<Actor> Actors;
        private List<Task> Tasks;
        private List<GnomeMind> Minds;
        public BlockSet Blocks;
        private List<WorldMutation> WorldMutations = new List<WorldMutation>();
        private List<Module> Modules = new List<Module>();

        public float SimStepTime { get; private set; }
        public float SimStepLength { get; private set; }
        public float SimStepPercentage { get { return SimStepTime / SimStepLength; } }
        
        public void AddWorldMutation(WorldMutation Mutation)
        {
            WorldMutations.Add(Mutation);
        }

        public void SetUpdateFlag(Coordinate Coordinate)
        {
            World.MarkDirtyBlock(Coordinate);
        }

        public void AddTask(Task Task)
        {
            if (World.Check(Task.Location))
            {
                Task.IsTopLevelTask = true;

                var cell = World.CellAt(Task.Location);
                cell.Task = Task;
                Tasks.Add(Task);
                SetUpdateFlag(Task.Location);
            }
        }

        public Task FindTask(Gnome Gnome)
        {
            return Tasks.FirstOrDefault(t => t.AssignedGnome == null);
        }

        public void AbandonTask(Task Task)
        {
            Task.AssignedGnome = null;
            
            if (Task.IsTopLevelTask)
            {
                Tasks.Remove(Task);
                Tasks.Add(Task);
            }
        }

        public Simulation(EpisodeContentManager Content, float SimStepLength)
        {
            this.SimStepLength = SimStepLength;
            SimStepTime = 0.0f;

            var definitionFile = Content.OpenUnbuiltTextStream("blocks.txt").ReadToEnd();
            var loadedBlocks = BlockSetLoader.LoadDefinitionFile(definitionFile);
            Blocks = new BlockSet
            {
                Tiles = new TileSheet(Content.Load<Texture2D>("tiles"), 16, 16),
                Templates = loadedBlocks.NamedBlocks
            };

            World = new CellGrid(64, 64, 64);

            World.forAll((t, x, y, z) =>
                {
                    if (z == 2) t.Block = Blocks.Templates["Grass"];
                    else if (z < 2) t.Block = Blocks.Templates["Dirt"];
                    else t.Block = null;
                });

            Actors = new List<Actor>();
            Tasks = new List<Task>();
            Minds = new List<GnomeMind>();

            var renderModule = new RenderModule.RenderModule();
            Modules.Add(renderModule);
        }

        public BranchNode CreateSceneNode()
        {
            var result = new BranchNode();
            foreach (var module in Modules)
            {
                var node = module.CreateSceneNode(this);
                if (node != null) result.Add(node);
            }
            return result;
        }

        public void End()
        {
        }

        private void ForgetCompletedTasks()
        { 
            for (var i = 0; i < Tasks.Count; )
            {
                if (Tasks[i].QueryStatus(this) == TaskStatus.Complete)
                {
                    World.CellAt(Tasks[i].Location).Task = null;
                    SetUpdateFlag(Tasks[i].Location);
                    Tasks.RemoveAt(i);
                }
                else
                    ++i;
            }
        }

        private void ApplyMutations()
        {
            foreach (var mutation in WorldMutations.Where(m => m.MutationTimeFrame == MutationTimeFrame.BeforeUpdatingConnectivity))
                mutation.Apply(this);

            //World.RelinkDirtyBlocks();

            foreach (var mutation in WorldMutations.Where(m => m.MutationTimeFrame == MutationTimeFrame.AfterUpdatingConnectivity))
                mutation.Apply(this);

            WorldMutations.Clear();
        }

        public void Update(Game Game, float ElapsedSeconds)
        {
            SimStepTime += ElapsedSeconds;
            ForgetCompletedTasks();
            ApplyMutations();

            if (SimStepTime > SimStepLength)
            {
                foreach (var actor in Actors)
                    actor.StepAction();
                foreach (var mind in Minds)
                    mind.Update(Game, this);
                SimStepTime -= SimStepLength;

                foreach (var module in Modules)
                    module.SimStep();
            }           

            foreach (var actor in Actors)
                actor.UpdateAction(SimStepPercentage);

            foreach (var actor in Actors)
                actor.UpdatePosition(this);

            foreach (var module in Modules)
                module.Update(ElapsedSeconds);
        }
    }
}
