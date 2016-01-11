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
        public struct CoincidentEdge
        {
            public Vector3 P0;
            public Vector3 P1;

            public static bool NearlyEqual(CoincidentEdge A, CoincidentEdge B)
            {
                if (Vector.NearlyEqual(A.P0, B.P0) && Vector.NearlyEqual(A.P1, B.P1)) return true;
                if (Vector.NearlyEqual(A.P0, B.P1) && Vector.NearlyEqual(A.P1, B.P0)) return true;
                return false;
            }
        }

        public struct Edge
        {
            public int I0;
            public int I1;
        }

        public IEnumerable<Edge> EnumerateEdges()
        {
            for (int tIndex = 0; tIndex < indicies.Length; tIndex += 3)
            {
                yield return new Edge { I0 = indicies[tIndex], I1 = indicies[tIndex + 1] };
                yield return new Edge { I0 = indicies[tIndex + 1], I1 = indicies[tIndex + 2] };
                yield return new Edge { I0 = indicies[tIndex + 2], I1 = indicies[tIndex] };
            }
        }

        public IEnumerable<CoincidentEdge> EnumerateEdgesAsPosition()
        {
            return EnumerateEdges().Select(e => new CoincidentEdge
            {
                P0 = verticies[e.I0].Position,
                P1 = verticies[e.I1].Position
            });
        }

        public static CoincidentEdge? FindCoincidentEdge(Mesh A, Mesh B)
        {
            foreach (var aEdge in A.EnumerateEdgesAsPosition())
                foreach (var bEdge in B.EnumerateEdgesAsPosition())
                    if (CoincidentEdge.NearlyEqual(aEdge, bEdge))
                        return aEdge;

            return null;
        }

    }
}
