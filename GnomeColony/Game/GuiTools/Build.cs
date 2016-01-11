using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Gem;

namespace Game.GuiTools
{
    public class Build : GuiTool
    {
        public BlockTemplate SelectedBlock = null;
        public Gem.Gui.UIItem BlockContainer = null;

        public Build()
        {
            this.Icon = TileNames.TaskIconBuild;
        }

        public override void Selected(Game Game, Gem.Gui.UIItem GuiRoot)
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
            foreach (var template in Game.BlockTemplates)
            {
                var lambdaTemplate = template;
                var child = HoverTest.CreateGuiSprite(new Rectangle(x, 16, 32, 32), template.Value.Preview, Game.BlockTiles);
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

        public override void Deselected(Game Game, Gem.Gui.UIItem GuiRoot)
        {
            if (BlockContainer != null) GuiRoot.RemoveChild(BlockContainer);
            BlockContainer = null;
        }

        public override void Apply(Game Game, WorldSceneNode WorldNode)
        {
            if (SelectedBlock == null) return;

            if (Game.World.Check(WorldNode.AdjacentHoverBlock))
            {
                var cell = Game.World.CellAt(WorldNode.AdjacentHoverBlock);
                if (cell.Block != null) return;
                cell.Block = Game.BlockTemplates[BlockTypes.Scaffold];
                cell.BlockOrientation = CellLink.Directions.North;

                if (SelectedBlock.Orientable)
                    cell.BlockOrientation = CellLink.DeriveDirectionFromNormal(WorldNode.HoverNormal);
                
                
                Game.AddTask(new Tasks.Build(SelectedBlock, WorldNode.AdjacentHoverBlock));


            }
        }
    }
}
