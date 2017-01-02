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
        public RenderModule.RenderModule RenderModule;

        public void SetUpdateFlag(Coordinate Coordinate)
        {
            World.MarkDirtyBlock(Coordinate);
        }

        public Simulation(EpisodeContentManager Content)
        {
            Blocks = BlockSet.FromReflection();
            Blocks.Tiles = new TileSheet(Content.Load<Texture2D>("tiles"), 16, 16);

            World = new CellGrid(64, 64, 64);

            World.forAll((t, x, y, z) =>
                {
                    if (z == 2) t.Block = Blocks.Templates["Grass"];
                    else if (z < 2) t.Block = Blocks.Templates["Dirt"];
                    else t.Block = null;
                });

            RenderModule = new RenderModule.RenderModule();
        }

        public void End()
        {
        }
    }
}
