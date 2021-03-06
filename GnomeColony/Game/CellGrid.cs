﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Game
{
    public partial class CellGrid : Gem.Common.Grid3D<Cell>
    {
        private Cell CellProxy = new Cell();
        private List<Coordinate> DirtyBlocks = new List<Coordinate>();
        public List<Coordinate> DirtyChunks = new List<Coordinate>();
        
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
            var chunkCoordinate = new Coordinate(Coordinate.X / 16, Coordinate.Y / 16, Coordinate.Z / 16);
            if (!DirtyChunks.Contains(chunkCoordinate))
                DirtyChunks.Add(chunkCoordinate);
        }

        public void RelinkDirtyBlocks()
        {
            if (DirtyBlocks.Count == 0) return;

            foreach (var block in DirtyBlocks)
                RelinkColumn(block.X, block.Y);

            DirtyBlocks.Clear();
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