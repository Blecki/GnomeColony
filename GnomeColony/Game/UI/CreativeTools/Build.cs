using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Gem;
using Game.RenderModule;

namespace Game.Creative
{
    public class Build : GuiTool
    {
        public BlockTemplate SelectedBlock = null;
        public Gem.Gui.UIItem BlockContainer = null;

        public Build()
        {
            this.Icon = TileNames.TaskIconBuild;
        }

        public override void Selected(Simulation Sim, Gem.Gui.UIItem GuiRoot)
        {
            this.SelectedBlock = null;

            BlockContainer = new Gem.Gui.UIItem(Gem.Gui.Shape.CreateQuad(96, 8, 512, 128),
                new Gem.Gui.GuiProperties
                {
                    Transparent = false,
                    BackgroundColor = new Microsoft.Xna.Framework.Vector3(0.8f, 0.2f, 0.2f)
                });

            GuiRoot.AddChild(BlockContainer);

            var x = 96 + 8;
            foreach (var template in Sim.Blocks.Templates.Where(t => t.Value.BuildType != BuildType.None))
            {
                var lambdaTemplate = template;
                var child = CommandInput.CreateGuiSprite(new Rectangle(x, 16, 32, 32), template.Value.Preview, Sim.Blocks.Tiles);
                child.Properties[0].Values.Upsert("click-action", new Action(() =>
                {
                    this.SelectedBlock = lambdaTemplate.Value;
                    GuiRoot.RemoveChild(BlockContainer);
                    BlockContainer = null;
                }));
                BlockContainer.AddChild(child);
                x += 32;
            }
        }

        public override void Deselected(Simulation Sim, Gem.Gui.UIItem GuiRoot)
        {
            if (BlockContainer != null) GuiRoot.RemoveChild(BlockContainer);
            BlockContainer = null;
        }

        public override void Apply(Simulation Sim, WorldSceneNode WorldNode)
        {
            if (SelectedBlock == null) return;

            if (Sim.World.Check(WorldNode.AdjacentHoverBlock))
            {
                var cell = Sim.World.CellAt(WorldNode.AdjacentHoverBlock);
                if (cell.Block != null) return;
                cell.BlockOrientation = CellLink.Directions.North;
                if (SelectedBlock.Orientable)
                    cell.BlockOrientation = CellLink.DeriveDirectionFromNormal(WorldNode.HoverNormal);
                var mutation = new WorldMutations.PlaceBlockMutation(WorldNode.AdjacentHoverBlock, SelectedBlock);
                Sim.AddWorldMutation(mutation);
            }
        }
    }
}
