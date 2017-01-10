using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Game
{
    public enum BlockShape
    {
        Cube,
        Slope,
        LowerSlab,
        UpperSlab,
        HalfSlopeLow,
        HalfSlopeHigh,
        Decal,
        Combined
    }
    
    public class PhantomBlock
    {
        public OrientedBlock Block;
        public bool PlacementAllowed = false;
        public bool WillCombine = false;
        public OrientedBlock TargetCell;
        public Gem.Geo.Mesh DecalMesh;
        public Coordinate OriginalOffset;

        public PhantomBlock() { }

        public PhantomBlock(OrientedBlock Source)
        {
            this.Block = new OrientedBlock(Source);
            OriginalOffset = Source.Offset;
        }
    }

    public struct OrientedTile
    {
        public int Tile;
        public Direction Orientation;

        public OrientedTile(int Tile, Direction Orientation = Direction.North)
        {
            this.Tile = Tile;
            this.Orientation = Orientation;
        }
    }

    [Flags]
    public enum BlockPlacementType
    {
        OrientToHoverFace = 1,
        Combine = 2
    }

    public class BlockTemplate
    {
        public String Name;
        public List<OrientedTile> PreviewTiles;
        public Point PreviewDimensions = new Point(1, 1);
        public int Top = 1;
        public int NorthSide = -1;
        public int SouthSide = -1;
        public int EastSide = -1;
        public int WestSide = -1;
        public int Bottom = -1;
        public String Hanging = null;
        public BlockShape Shape;
        public bool Transparent = false;
        public bool EmitsLight = false;

        public bool Orientable = false;
        public BlockPlacementType PlacementType = BlockPlacementType.OrientToHoverFace;

        public List<OrientedBlock> CompositeBlocks;

        public bool ShowInEditor = true;

        public virtual bool CanCompose(OrientedBlock Onto, Direction MyOrientation) 
        { 
            return false; 
        }

        public virtual OrientedBlock Compose(
            OrientedBlock With,
            Direction MyOrientation,
            BlockSet TemplateSet) 
        {
            throw new InvalidOperationException();
        }

        public virtual void Initialize(BlockSet BlockSet) { }

        public OrientedBlock GetTopOfComposite(Direction BaseOrientation)
        {
            if (Shape == BlockShape.Combined)
            {
                if (BaseOrientation != Direction.North) throw new InvalidOperationException();
                return CompositeBlocks[CompositeBlocks.Count - 1];
            }
            else
                return new OrientedBlock
                {
                    Template = this,
                    Offset = new Coordinate(0, 0, 0),
                    Orientation = BaseOrientation
                };
        }

        public OrientedBlock GetBottomOfComposite(Direction BaseOrientation)
        {
            if (Shape == BlockShape.Combined)
            {
                if (BaseOrientation != Direction.North) throw new InvalidOperationException();
                return CompositeBlocks[0];
            }
            else
                return new OrientedBlock
                {
                    Template = this,
                    Offset = new Coordinate(0, 0, 0),
                    Orientation = BaseOrientation
                };
        }

        public OrientedBlock SansTopOfComposite(Direction BaseOrientation)
        {
            if (Shape == BlockShape.Combined)
            {
                if (BaseOrientation != Direction.North) throw new InvalidOperationException();

                if (CompositeBlocks.Count == 2)
                    return new OrientedBlock 
                    { 
                        Template = CompositeBlocks[0].Template,
                        Orientation = CompositeBlocks[0].Orientation
                    };
                else
                    return new OrientedBlock
                    {
                        Template = new BlockTemplate
                            {
                                Shape = BlockShape.Combined,
                                CompositeBlocks = CompositeBlocks.Take(CompositeBlocks.Count - 1).ToList()
                            },
                        Orientation = Direction.North
                    };
            }
            else
                return new OrientedBlock { Template = this, Orientation = BaseOrientation };
        }

        public BlockTemplate ComposeWith(Direction MyOrientation, OrientedBlock NewTop)
        {
            if (Shape == BlockShape.Combined)
            {
                var r = new BlockTemplate
                {
                    Shape = BlockShape.Combined
                };

                r.CompositeBlocks.AddRange(CompositeBlocks);
                r.CompositeBlocks.Add(new OrientedBlock
                    {
                        Template = NewTop.Template,
                        Orientation = NewTop.Orientation
                    });
                return r;
            }
            else
            {
                return new BlockTemplate
                {
                    Shape = BlockShape.Combined,
                    CompositeBlocks = HelperExtensions.MakeList(
                        new OrientedBlock { Template = this, Orientation = MyOrientation },
                        new OrientedBlock { Template = NewTop.Template, Orientation = NewTop.Orientation }
                        )
                };
            }
        }
    }
}
