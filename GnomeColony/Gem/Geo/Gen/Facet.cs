using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gem.Geo
{
    public partial class Gen
    {
        //Explode mesh into unique triangles for facetted look.
        public static Mesh FacetCopy(Mesh m)
        {
            var result = new Mesh();
            result.verticies = new Vertex[m.indicies.Length];
            result.indicies = new short[m.indicies.Length];

            for (short i = 0; i < m.indicies.Length; ++i)
            {
                result.verticies[i] = m.verticies[m.indicies[i]];
                result.indicies[i] = i;
            }

            for (short i = 0; i < result.verticies.Length; i += 3)
            {
                var normal = Gen.CalculateNormal(result, i, i + 1, i + 2);
                for (int j = 0; j < 3; ++j)
                    result.verticies[i + j].Normal = normal;
            }
            return result;
        }

        public static Mesh Invert(Mesh m)
        {
            var result = new Mesh();
            result.verticies = new Vertex[m.verticies.Length];
            result.indicies = new short[m.indicies.Length];

            for (short i = 0; i < m.verticies.Length; ++i)
            {
                result.verticies[i] = m.verticies[i];
                result.verticies[i].Normal *= -1.0f;
            }

            for (short i = 0; i < m.indicies.Length; i += 3)
            {
                result.indicies[i] = m.indicies[i];
                result.indicies[i + 1] = m.indicies[i + 2];
                result.indicies[i + 2] = m.indicies[i + 1];
            }

            return result;
        }

        public static void CalculateTangentsAndBiNormals(Mesh m)
        {
            for (short i = 0; i < m.verticies.Length; i += 3)
            {
                var normals = Gen.CalculateTangentAndBinormal(m, i, i + 1, i + 2);
                for (int j = 0; j < 3; ++j)
                {
                    m.verticies[i + j].Tangent = normals.Item1;
                    m.verticies[i + j].BiNormal = normals.Item2;
                }
            }
        }
    }
}