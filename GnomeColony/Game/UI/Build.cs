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
        public Direction BaseOrientation = Direction.North;
        
        public Build(BlockTemplate SelectedBlock)
        {
            this.SelectedBlock = SelectedBlock;

            this.HiliteFaces = 0;
            if (SelectedBlock.PlacementType.HasFlag(BlockPlacementType.OrientToHoverFace))
                this.HiliteFaces = HiliteFace.Sides | HiliteFace.Top;
            else if (SelectedBlock.PlacementType.HasFlag(BlockPlacementType.Combine))
                this.HiliteFaces |= HiliteFace.Top;
        }

        public override void OnSelected(SimulationGame Game)
        {
            Game.Input.BindAction("ROTATE-BLOCK", () => 
                BaseOrientation = Directions.Rotate(BaseOrientation));
        }

        public override void OnDeselected(SimulationGame Game)
        {
            Game.Input.ClearAction("ROTATE-BLOCK");
        }

        public override void Apply(Simulation Sim, WorldSceneNode WorldNode)
        {
            if (Placements == null) return;
            if (Placements.Count(p => p.PlacementAllowed) != Placements.Count) return;

            foreach (var placement in Placements)
            {
                placement.TargetCell.CopyFrom(placement.Block);
                Sim.SetUpdateFlag(placement.Block.Offset);
            }

            Placements = null;
        }

        private enum PlacementAttemptResult
        {
            Success,
            TriedToCombineWithNull,
            BlockedFromAbove,
            Failure
        }

        private PlacementAttemptResult AttemptCombinedPlacement(PhantomBlock Phantom, Coordinate At, Simulation Sim)
        {
            if (!Sim.World.Check(At)) return PlacementAttemptResult.Failure;

            var underBlock = Sim.World.CellAt(At);
            if (underBlock.Template == null) return PlacementAttemptResult.TriedToCombineWithNull;
            if (!Phantom.Block.Template.CanCompose(underBlock, Phantom.Block.Orientation))
                return PlacementAttemptResult.Failure;

            var overBlockCoord = At + new Coordinate(0, 0, 1);
            if (Sim.World.Check(overBlockCoord))
            {
                var underShape = underBlock.Template.GetBottomOfComposite(underBlock.Orientation).Template.Shape;
                if (underShape == BlockShape.Cube || underShape == BlockShape.UpperSlab)
                {
                    var overBlock = Sim.World.CellAt(overBlockCoord);
                    if (overBlock.Template != null && overBlock.Template.GetBottomOfComposite(overBlock.Orientation).Template.Shape != BlockShape.UpperSlab)
                        return PlacementAttemptResult.BlockedFromAbove;
                }
            }

            Phantom.PlacementAllowed = true;
            Phantom.WillCombine = true;
            Phantom.Block = Phantom.Block.Template.Compose(
                underBlock,
                Phantom.Block.Orientation, Sim.Blocks);
            Phantom.Block.Offset = At;
            Phantom.TargetCell = underBlock;

            return PlacementAttemptResult.Success;
        }

        private void CheckPlacement(PhantomBlock Phantom, Simulation Sim, WorldSceneNode WorldNode)
        {
            Phantom.PlacementAllowed = false;
            

            if (Phantom.Block.Template.PlacementType.HasFlag(BlockPlacementType.Combine))
            {
                var attemptResult = AttemptCombinedPlacement(Phantom, WorldNode.HoverBlock + Phantom.Block.Offset, Sim);
                if (attemptResult == PlacementAttemptResult.BlockedFromAbove)
                    AttemptCombinedPlacement(Phantom, WorldNode.HoverBlock + Phantom.Block.Offset + new Coordinate(0, 0, 1), Sim);
                if (attemptResult == PlacementAttemptResult.TriedToCombineWithNull)
                    AttemptCombinedPlacement(Phantom, WorldNode.HoverBlock + Phantom.Block.Offset + new Coordinate(0, 0, -1), Sim);

                if (Phantom.PlacementAllowed)
                {
                    Phantom.DecalMesh = Generate.GenerateDecalTestGeometry(Phantom.Block, Sim.Blocks.Tiles);
                    return;
                }
            }
            
            if (Phantom.Block.Template.PlacementType.HasFlag(BlockPlacementType.OrientToHoverFace))
            {
                // Todo: Can't place if below has a decal.
                var actualPlacement = WorldNode.AdjacentHoverBlock + Phantom.Block.Offset;
                if (Sim.World.Check(actualPlacement))
                {
                    var destinationCell = Sim.World.CellAt(actualPlacement);
                    if (destinationCell.Template == null)
                    {
                        Phantom.PlacementAllowed = true;
                        Phantom.WillCombine = false;
                        Phantom.Block.Offset = actualPlacement;
                        Phantom.TargetCell = destinationCell;
                        return;
                    }
                }
            }

            Phantom.Block.Offset = WorldNode.AdjacentHoverBlock + Phantom.Block.Offset;
            Phantom.WillCombine = false;
        }

        public override void Hover(Simulation Sim, WorldSceneNode WorldNode)
        {
            if (SelectedBlock == null) return;

            // Normalize into a list of block placements (even single blocks become a list of one) and check all pieces.
            Placements = new List<PhantomBlock>();
            if (SelectedBlock.Shape == BlockShape.Combined)
                Placements.AddRange(SelectedBlock.CompositeBlocks.Select(b => new PhantomBlock(b)));
            else
                Placements.Add(new PhantomBlock
                {
                    Block = new OrientedBlock(SelectedBlock, Direction.North),
                    OriginalOffset = new Coordinate(0, 0, 0)
                });

            // Orient placement to hover face if block is orientable.
            if (SelectedBlock.PlacementType == BlockPlacementType.OrientToHoverFace && SelectedBlock.Orientable)
            {
                var orientation = Directions.DeriveDirectionFromNormal(WorldNode.HoverNormal);
                foreach (var subBlock in Placements)
                    subBlock.Block.Rotate((int)orientation);
            }

            // Orient placement to hotkey orientation
            foreach (var subBlock in Placements)
                subBlock.Block.Rotate((int)BaseOrientation);

            // Try each placement. If they are all allowed, create preview.
            foreach (var subBlock in Placements)
                CheckPlacement(subBlock, Sim, WorldNode);

            // So far, they are allowed. We need to check any decals for coincident edges as well.
            foreach (var a in Placements)
                foreach (var b in Placements)
                {
                    if (Object.ReferenceEquals(a, b)) continue;
                    if (!a.PlacementAllowed || !b.PlacementAllowed) continue;
                    if (a.DecalMesh == null || b.DecalMesh == null) continue;
                    if (!Coordinate.Adjacent(a.OriginalOffset, b.OriginalOffset)) continue;
                    var coincidentEdge = Gem.Geo.Mesh.FindCoincidentEdge(a.DecalMesh, b.DecalMesh);
                    if (!coincidentEdge.HasValue)
                    {
                        a.PlacementAllowed = false;
                        b.PlacementAllowed = false;
                    }
                }

            //if (Placements.Count(p => p.PlacementAllowed) == Placements.Count)
                WorldNode.SetPhantomPlacements(Placements);
        }

        public override void UnHover()
        {
            Placements = null;
        }
    }
}
