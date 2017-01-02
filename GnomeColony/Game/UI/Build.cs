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
        public List<PhantomBlock> Placements = null;
        public CellLink.Directions BaseOrientation = CellLink.Directions.North;
        
        public Build(BlockTemplate SelectedBlock)
        {
            this.SelectedBlock = SelectedBlock;
        }

        public override void Update(Game Game)
        {
            if (Game.Input.Check("ROTATE_BLOCK"))
                BaseOrientation = CellLink.Rotate(BaseOrientation);
        }

        public override void Apply(Simulation Sim, WorldSceneNode WorldNode)
        {
            if (Placements == null) return;
            if (Placements.Count(p => p.PlacementAllowed) != Placements.Count) return;

            foreach (var placement in Placements)
            {
                if (placement.Block.Type == BlockType.Decal)
                {
                    placement.TargetCell.Decal = placement.Block;
                    Sim.SetUpdateFlag(placement.FinalPosition);
                }
                else
                {
                    placement.TargetCell.Block = placement.FinalBlock;
                    Sim.SetUpdateFlag(placement.FinalPosition);
                    placement.TargetCell.BlockOrientation = placement.Orientation;
                }
            }

            Placements = null;
        }
        
        private void CheckPlacement(PhantomBlock Block, Simulation Sim, WorldSceneNode WorldNode)
        {
            // Check for combination.
            var actualHover = WorldNode.HoverBlock + Block.Offset;
            if (Sim.World.Check(actualHover))
            {
                var underBlock = Sim.World.CellAt(actualHover);

                if (Block.Block.Type == BlockType.Decal)
                {
                    Block.PlacementAllowed = true;
                    Block.WillCombine = false;
                    Block.FinalPosition = actualHover;
                    Block.TargetCell = underBlock;
                    Block.FinalBlock = Block.Block;
                    return;
                }
                
                if (Block.Block.CanComposite(underBlock.Block))
                {
                    Block.PlacementAllowed = true;
                    Block.WillCombine = true;
                    Block.FinalPosition = actualHover;
                    Block.FinalBlock = Block.Block.Compose(underBlock.Block, Sim.Blocks);
                    Block.TargetCell = underBlock;
                    return;
                }
            }

            var actualPlacement = WorldNode.AdjacentHoverBlock + Block.Offset;
            if (Sim.World.Check(actualPlacement))
            {
                var destinationCell = Sim.World.CellAt(actualPlacement);
                if (destinationCell.Block == null)
                {
                    Block.PlacementAllowed = true;
                    Block.WillCombine = false;
                    Block.FinalPosition = actualPlacement;
                    Block.TargetCell = destinationCell;
                    Block.FinalBlock = Block.Block;
                    return;
                }
            }

            Block.PlacementAllowed = false;
        }

        public override void Hover(Simulation Sim, WorldSceneNode WorldNode)
        {
            if (SelectedBlock == null) return;

            // Normalize into a list of block placements (even single blocks become a list of one) and check all pieces.
            Placements = new List<PhantomBlock>();
            if (SelectedBlock.Type == BlockType.Combined)
                Placements.AddRange(SelectedBlock.CompositeBlocks.Select(b => new PhantomBlock(b)));
            else
                Placements.Add(new PhantomBlock { Block = SelectedBlock });

            // Orient placement to hover face if block is orientable.
            if (SelectedBlock.Orientable)
            {
                var orientation = CellLink.DeriveDirectionFromNormal(WorldNode.HoverNormal);
                foreach (var subBlock in Placements)
                    subBlock.Rotate((int)orientation);
            }

            // Orient placement to hotkey orientation
            foreach (var subBlock in Placements)
                subBlock.Rotate((int)BaseOrientation);

            // Try each placement. If they are all allowed, create preview.
            foreach (var subBlock in Placements)
                CheckPlacement(subBlock, Sim, WorldNode);

            if (Placements.Count(p => p.PlacementAllowed) == Placements.Count)
                WorldNode.SetPhantomPlacements(Placements);
        }
    }
}
