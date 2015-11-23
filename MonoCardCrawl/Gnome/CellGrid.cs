using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gnome
{
    public partial class CellGrid : Gem.Common.Grid3D<Cell>
    {
        private Cell CellProxy = new Cell();
        private List<Coordinate> DirtyBlocks = new List<Coordinate>();
        public bool ChunkDirty { get; private set; }
        public void ClearChunkDirtyFlag() { ChunkDirty = false; }
        public void MarkDirtyChunk() { ChunkDirty = true; }

        public CellGrid(int width, int height, int depth)
            : base(width, height, depth, (x, y, z) => new Cell { Location = new Coordinate(x, y, z) })
        {
        }

        public Cell CellAt(int X, int Y, int Z)
        {
            if (check(X, Y, Z)) return this[X, Y, Z];
            else return CellProxy;
        }

        public void MarkDirtyBlock(Coordinate Coordinate)
        {
            DirtyBlocks.Add(Coordinate);
        }

        public void RelinkDirtyBlocks()
        {
            if (DirtyBlocks.Count == 0) return;

            foreach (var block in DirtyBlocks)
                RelinkColumn(block.X, block.Y);

            DirtyBlocks.Clear();
            MarkDirtyChunk();
        }

        public bool Check(Coordinate C)
        {
            return check(C.X, C.Y, C.Z);
        }

        public Cell CellAt(Coordinate C)
        {
            return CellAt(C.X, C.Y, C.Z);
        }
    }
}