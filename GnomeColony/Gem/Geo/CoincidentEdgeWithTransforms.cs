using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Gem.Math;

namespace Gem.Geo
{
    public partial class Mesh
    {
        private IEnumerable<CoincidentEdge> EnumerateEdgesAsPositionWithTransform(Matrix M)
        {
            return EnumerateEdges().Select(e => new CoincidentEdge
            {
                P0 = Vector3.Transform(verticies[e.I0].Position, M),
                P1 = Vector3.Transform(verticies[e.I1].Position, M)
            });
        }

        public static CoincidentEdge? FindCoincidentEdgeWithTransforms(Mesh A, Matrix AM, Mesh B, Matrix BM)
        {
            foreach (var aEdge in A.EnumerateEdgesAsPositionWithTransform(AM))
                foreach (var bEdge in B.EnumerateEdgesAsPositionWithTransform(BM))
                    if (CoincidentEdge.NearlyEqual(aEdge, bEdge))
                        return aEdge;

            return null;
        }

    }
}
