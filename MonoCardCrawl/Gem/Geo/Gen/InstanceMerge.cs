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
        public static Mesh InstanceMerge(
            IEnumerable<Mesh> Meshes,
            IEnumerable<Matrix> Transformation)
        {
            var r = new Mesh();

            var totalVerticies = Meshes.Sum(m => m.VertexCount);
            var totalIndicies = Meshes.Sum(m => m.indicies.Length);

            r.verticies = new Vertex[totalVerticies];
            r.indicies = new short[totalIndicies];

            int vertexInsertLocation = 0;
            int indexInsertLocation = 0;
            
            foreach (var instance in Meshes.Zip(Transformation, (m, t) => Tuple.Create(m,t)))
            {
                var mesh = instance.Item1;
                var transform = instance.Item2;

                for (int i = 0; i < mesh.VertexCount; ++i)
                {
                    r.verticies[i + vertexInsertLocation].Position = Vector3.Transform(mesh.verticies[i].Position, transform);
                    r.verticies[i + vertexInsertLocation].TextureCoordinate = mesh.verticies[i].TextureCoordinate;
                }

                Vector3 scale;
                Vector3 trans;
                Quaternion rot;
                transform.Decompose(out scale, out rot, out trans);
                for (int i = 0; i < mesh.VertexCount; ++i)
                {
                    r.verticies[i + vertexInsertLocation].Normal = Vector3.Transform(mesh.verticies[i].Normal, rot);
                    r.verticies[i + vertexInsertLocation].Tangent = Vector3.Transform(mesh.verticies[i].Tangent, rot);
                    r.verticies[i + vertexInsertLocation].BiNormal = Vector3.Transform(mesh.verticies[i].BiNormal, rot);
                }

                for (int i = 0; i < mesh.indicies.Length; ++i)
                    r.indicies[i + indexInsertLocation] = (short)(vertexInsertLocation + mesh.indicies[i]);

                vertexInsertLocation += mesh.VertexCount;
                indexInsertLocation += mesh.indicies.Length;
            }

            return r;
        }
    }
}