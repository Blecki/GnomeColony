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
        public BlockSet Blocks;
        private List<Module> Modules = new List<Module>();

        public float SimStepTime { get; private set; }
        public float SimStepLength { get; private set; }
        public float SimStepPercentage { get { return SimStepTime / SimStepLength; } }
        
        public void SetUpdateFlag(Coordinate Coordinate)
        {
            World.MarkDirtyBlock(Coordinate);
        }

        public Simulation(EpisodeContentManager Content, float SimStepLength)
        {
            this.SimStepLength = SimStepLength;
            SimStepTime = 0.0f;

            Blocks = BlockSet.FromReflection();
            Blocks.Tiles = new TileSheet(Content.Load<Texture2D>("tiles"), 16, 16);

            World = new CellGrid(64, 64, 64);

            World.forAll((t, x, y, z) =>
                {
                    if (z == 2) t.Block = Blocks.Templates["Grass"];
                    else if (z < 2) t.Block = Blocks.Templates["Dirt"];
                    else t.Block = null;
                });

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

        public void Update(Game Game, float ElapsedSeconds)
        {
            SimStepTime += ElapsedSeconds;

            if (SimStepTime > SimStepLength)
            {
                SimStepTime -= SimStepLength;

                foreach (var module in Modules)
                    module.SimStep();
            }           

            foreach (var module in Modules)
                module.Update(ElapsedSeconds);
        }
    }
}
