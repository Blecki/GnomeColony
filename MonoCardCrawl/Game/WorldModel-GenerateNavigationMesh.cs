using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Game
{
    public static partial class WorldModel
    {
        public static NavigationMesh GenerateNavigationMesh(CellGrid From)
        {
            var meshes = new List<Gem.Geo.Mesh>();
            From.ForEachTile((c, x, y, z) =>
                {
                    // Avoid adding tiles that have a tile directly above them.
                    if (z != (From.depth - 1))
                        if (From.CellAt(x, y, z + 1).Tile != null) return;

                    if (c.Tile != null)
                    {
                        var mesh = c.Tile.NavigationMesh;
                        if (mesh != null)
                        {
                            mesh = Gem.Geo.Gen.Copy(mesh);
                            Gem.Geo.Gen.Transform(mesh, Matrix.CreateTranslation(x + 0.5f, y + 0.5f, z));
                            mesh.Tag = c;
                            meshes.Add(mesh);
                        }
                    }
                });

            var rawNavMesh = Gem.Geo.Gen.Merge(meshes.ToArray());
            rawNavMesh = Gem.Geo.Gen.WeldCopy(rawNavMesh);
            
            var navMesh = new Gem.Geo.EdgeMesh(rawNavMesh);
            return NavigationMesh.Create(navMesh);
        }
    }
}