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

        public float SimStepTime { get; private set; }
        public float SimStepLength { get; private set; }
        public float SimStepPercentage { get { return SimStepTime / SimStepLength; } }
        
        private WorldSceneNode WorldSceneNode;

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

            World = new CellGrid(16, 16, 16);

            World.forAll((t, x, y, z) =>
                {
                    if (z <= 1) t.Block = Blocks.Templates["Grass"];
                    else t.Block = null;
                });

            World.CellAt(4, 4, 1).SetFlag(CellFlags.Storehouse, true);

            World.CellAt(1, 1, 2).Block = Blocks.Templates["Slope"];
            World.CellAt(1, 1, 2).BlockOrientation = CellLink.Directions.North;

            World.CellAt(1, 2, 2).Block = Blocks.Templates["Slope"];
            World.CellAt(1, 2, 2).BlockOrientation = CellLink.Directions.East;

            World.CellAt(1, 3, 2).Block = Blocks.Templates["Slope"];
            World.CellAt(1, 3, 2).BlockOrientation = CellLink.Directions.South;

            World.CellAt(1, 4, 2).Block = Blocks.Templates["Slope"];
            World.CellAt(1, 4, 2).BlockOrientation = CellLink.Directions.West;

            World.CellAt(8, 8, 6).Block = Blocks.Templates["Grass"];

            World.CellAt(6, 6, 1).Decal = Blocks.Templates["TrackH"];
            World.CellAt(7, 6, 2).Block = Blocks.Templates["Slope"];
            World.CellAt(7, 6, 2).BlockOrientation = CellLink.Directions.West;
            World.CellAt(7, 6, 2).Decal = Blocks.Templates["TrackV"];

            Actors = new List<Actor>();
            Tasks = new List<Task>();
            Minds = new List<GnomeMind>();

            World.PrepareNavigation();
            World.MarkDirtyChunk();

            for (int i = 0; i < 4; ++i)
            {
                var gnomeActor = new Gnome(this, Blocks.Tiles);
                gnomeActor.Location = new Coordinate(0, i, 1);
                Actors.Add(gnomeActor);
                Minds.Add(gnomeActor.Mind);
            }            
        }

        public BranchNode CreateSceneNode()
        {
            var result = new BranchNode();

            WorldSceneNode = new WorldSceneNode(World, new WorldSceneNodeProperties
            {
                HiliteTexture = TileNames.HoverHilite,
                BlockSet = Blocks
            });
            result.Add(WorldSceneNode);
            result.Add(new ActorSceneNode(Actors));
            result.UpdateWorldTransform(Matrix.Identity);
            WorldSceneNode.UpdateGeometry();
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

            World.RelinkDirtyBlocks();

            foreach (var mutation in WorldMutations.Where(m => m.MutationTimeFrame == MutationTimeFrame.AfterUpdatingConnectivity))
                mutation.Apply(this);

            WorldMutations.Clear();

            if (World.ChunkDirty)
            {
                WorldSceneNode.UpdateGeometry();
                World.ClearChunkDirtyFlag();
            }
        }

        public void Update(Game Game, float ElapsedSeconds)
        {
            SimStepTime += ElapsedSeconds;

            if (SimStepTime > SimStepLength)
            {
                foreach (var actor in Actors)
                    actor.StepAction();
                foreach (var mind in Minds)
                    mind.Update(Game, this);
                SimStepTime -= SimStepLength;

                ForgetCompletedTasks();
                ApplyMutations();
            }           

            foreach (var actor in Actors)
                actor.UpdateAction(SimStepPercentage);

            foreach (var actor in Actors)
                actor.UpdatePosition(this);
        }
    }
}
