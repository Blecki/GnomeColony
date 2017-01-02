﻿using System;
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
            foreach (var template in Sim.Blocks.Templates)
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
                var mouseIsOnTopFace = WorldNode.HoverNormal.Z > 0.5f;
                var underBlock = Sim.World.CellAt(WorldNode.HoverBlock);
                if (mouseIsOnTopFace && SelectedBlock.CanComposite(underBlock.Block))
                {
                    underBlock.Block = SelectedBlock.Compose(underBlock.Block, Sim.Blocks);
                    Sim.SetUpdateFlag(WorldNode.HoverBlock);
                }
                else
                {
                    var cell = Sim.World.CellAt(WorldNode.AdjacentHoverBlock);
                    if (cell.Block != null) return;
                    cell.BlockOrientation = CellLink.Directions.North;
                    if (SelectedBlock.Orientable)
                        cell.BlockOrientation = CellLink.DeriveDirectionFromNormal(WorldNode.HoverNormal);
                    cell.Block = SelectedBlock;
                    Sim.World.MarkDirtyBlock(WorldNode.AdjacentHoverBlock);
                }
            }
        }

        public override void Hover(Simulation Sim, WorldSceneNode WorldNode)
        {
            if (SelectedBlock == null) return;

            if (Sim.World.Check(WorldNode.AdjacentHoverBlock))
            {
                var phantom = new Cell();
                
                //If on top face 
                var mouseIsOnTopFace = WorldNode.HoverNormal.Z > 0.5f;
                var underBlock = Sim.World.CellAt(WorldNode.HoverBlock);
                if (mouseIsOnTopFace && SelectedBlock.CanComposite(underBlock.Block))
                {
                    phantom.Location = WorldNode.HoverBlock;
                    phantom.BlockOrientation = underBlock.BlockOrientation;
                    phantom.Block = SelectedBlock;
                }
                else
                {
                    phantom.Location = WorldNode.AdjacentHoverBlock;
                    phantom.BlockOrientation = CellLink.Directions.North;
                    if (SelectedBlock.Orientable)
                        phantom.BlockOrientation = CellLink.DeriveDirectionFromNormal(WorldNode.HoverNormal);
                    phantom.Block = SelectedBlock;
                }

                WorldNode.SetPhantomPlacementCell(phantom);
            }
        }
    }
}
