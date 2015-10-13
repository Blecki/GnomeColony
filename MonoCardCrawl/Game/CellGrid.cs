using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Game
{
    public class CellGrid : Gem.Common.Grid3D<Cell>
    {
        private Cell CellProxy = new Cell();

        public CellGrid(int width, int height, int depth) : base(width, height, depth)
        {
        }

        public Cell CellAt(int X, int Y, int Z)
        {
            if (check(X, Y, Z)) return this[X, Y, Z];
            else return CellProxy;
        }

        public void ForEachTile(Action<Cell,int,int,int> Callback)
        {
            this.forRect(0, 0, 0, width, height, depth, Callback);
        }
    }
}