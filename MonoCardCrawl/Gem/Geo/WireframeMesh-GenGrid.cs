using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gem.Geo
{
    public partial class WireframeMesh
    {
        public static WireframeMesh GenerateGrid(float Width, float Height, int DivisionsX, int DivisionsY)
        {
            var r = new WireframeMesh();
            var stepX = Width / (float)DivisionsX;
            var stepY = Height / (float)DivisionsY;

            var columns = DivisionsX + 1;
            var rows = DivisionsY + 1;

            r.verticies = new Vertex[columns * rows];
            r.indicies = new short[(columns + rows) * 2];


            for (var x = 0; x < columns; ++x)
                for (var y = 0; y < rows; ++y)
                    r.verticies[(y * columns) + x].Position = new Vector3(x * stepX, y * stepY, 0.0f);

            //Create column indicies.
            var topOffset = 0;
            var bottomOffset = columns * DivisionsY;

            for (var i = 0; i < columns; ++i)
            {
                r.indicies[i * 2] = (short)(topOffset + i);
                r.indicies[(i * 2) + 1] = (short)(bottomOffset + i);
            }

            //Create row indicies.
            var insertOffset = columns * 2;
            var leftOffset = 0;
            var rightOffset = columns - 1;

            for (var i = 0; i < rows; ++i)
            {
                r.indicies[insertOffset + (i * 2)] = (short)(i * columns + leftOffset);
                r.indicies[insertOffset + (i * 2) + 1] = (short)(i * columns + rightOffset);
            }

            return r;
        }
    }
}