using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem.Common
{
    public class Grid3D<T> where T : new()
    {
        public int width { get; private set; }
        public int height { get; private set; }
        public int depth { get; private set; }

        private T[] tiles;

        public Grid3D(int width, int height, int depth)
        {
            this.width = width;
            this.height = height;
            this.depth = depth;

            tiles = new T[this.width * this.height * this.depth];
            for (int i = 0; i < tiles.Length; ++i) tiles[i] = new T();
        }

        public Grid3D(int width, int height, int depth, Func<int,int,int,T> CellCreator)
        {
            this.width = width;
            this.height = height;
            this.depth = depth;

            tiles = new T[this.width * this.height * this.depth];

            forAll((c, x, y, z) => this[x, y, z] = CellCreator(x, y, z));
        }


        public int Normalize(int x, int y, int z)
        {
            return (z * height * width) + (y * width) + x;
        }

        public T this[int x, int y, int z]
        {
            get { return tiles[Normalize(x, y, z)]; }
            set { tiles[Normalize(x, y, z)] = value; }
        }

        public void forRect(int x, int y, int z, int w, int h, int d, Action<T, int, int, int> func)
        {
            for (var _x = (x < 0 ? 0 : x); _x < (x + w) && _x < width; ++_x)
                for (var _y = (y < 0 ? 0 : y); _y < (y + h) && _y < height; ++_y)
                    for (var _z = (z < 0 ? 0 : z); _z < (z + d) && _z < depth; ++_z)
                        func(this[_x, _y, _z], _x, _y, _z);
        }

        public void forAll(Action<T, int, int, int> func)
        {
            forRect(0, 0, 0, width, height, depth, func);
        }

        public A abortedForRect<A>(int x, int y, int z, int w, int h, int d, Func<T, int, int, int, A> func)
        {
            for (var _x = (x < 0 ? 0 : x); _x < (x + w) && _x < width; ++_x)
                for (var _y = (y < 0 ? 0 : y); _y < (y + h) && _y < height; ++_y)
                    for (var _z = (z < 0 ? 0 : z); _z < (z + d) && _z < depth; ++_z)
                    {
                        var _a = func(this[_x, _y, _z], _x, _y, _z);
                        if (_a != null) return _a;
                    }
            return default(A);
        }

        public T worldIndex(float x, float y, float z)
        {
            return this[(int)System.Math.Floor(x), (int)System.Math.Floor(y), (int)System.Math.Floor(z)];
        }

        public bool check(int x, int y, int z)
        {
            return x >= 0 && x < width && y >= 0 && y < height && z >= 0 && z < depth;
        }

        public bool worldCheck(float x, float y, float z)
        {
            return check((int)System.Math.Floor(x), (int)System.Math.Floor(y), (int)System.Math.Floor(z));
        }

        internal void resize(int nw, int nh, int nd)
        {
            var ntiles = new T[nw * nh * nd];
            for (int x = 0; x < nw; ++x)
                for (int y = 0; y < nh; ++y)
                    for (int z = 0; z < nd; ++z)
                        if (check(x, y, z))
                            ntiles[(z * nh * nw) + (y * nw) + x] = this[x, y, z];
                        else
                            ntiles[(z * nh * nw) + (y * nw) + x] = new T();

            this.width = nw;
            this.height = nh;
            this.depth = nd;

            tiles = ntiles;
        }
    }
}