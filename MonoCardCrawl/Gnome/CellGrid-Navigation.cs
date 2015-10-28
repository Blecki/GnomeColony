using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;

namespace Gnome
{
    public partial class CellGrid
    {
        public void PrepareNavigation()
        {
            //Prepare cells
            forAll((c, x, y, z) => UpdateNavigation(c, x, y, z));

            //Detect connected edges.
            forAll((c, x, y, z) => DetectLinks(c, x, y));
        }

        private void DetectLinks(Cell c, int x, int y)
        {
            if (c.Navigatable)
            {
                DetectConnectedInColumn(c, CellLink.Directions.North, x, y - 1);
                DetectConnectedInColumn(c, CellLink.Directions.East, x + 1, y);
                DetectConnectedInColumn(c, CellLink.Directions.South, x, y + 1);
                DetectConnectedInColumn(c, CellLink.Directions.West, x - 1, y);
            }
        }

        private void UpdateNavigation(Cell c, int x, int y, int z)
        {
            c.Navigatable = false;
            c.Links.Clear();
            c.NavigationMesh = null;
            c.CenterPoint = new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);
            
            if (c.IsSolid)
            {
                c.NavigationMesh = Gem.Geo.Gen.Copy(Generate.GetNavigationMesh(c.Block.Shape));
                Gem.Geo.Gen.Transform(c.NavigationMesh, Matrix.CreateTranslation(x + 0.5f, y + 0.5f, z));
                Gem.Geo.Gen.CalculateTangentsAndBiNormals(c.NavigationMesh);

                var centerPointRayOrigin = new Vector3(x + 0.5f, y + 0.5f, z + 2.0f);
                var hitCenter = c.NavigationMesh.RayIntersection(new Ray(centerPointRayOrigin, new Vector3(0, 0, -1)));
                if (hitCenter.Intersects)
                    c.CenterPoint = centerPointRayOrigin + (new Vector3(0, 0, -1) * hitCenter.Distance);
                
                c.Navigatable = true;

                // Block must have two clear blocks above it to be navigatable.
                if (z != (this.depth - 1))
                    if (CellAt(x, y, z + 1).IsSolid)
                        c.Navigatable = false;
                if (z != (this.depth - 2))
                    if (CellAt(x, y, z + 2).IsSolid)
                        c.Navigatable = false;
            }
        }

        private void DetectConnectedInColumn(Cell From, CellLink.Directions Direction,
            int CX, int CY)
        {
            if (CX < 0 || CX >= this.width || CY < 0 || CY >= this.height) return;
            var baseMesh = From.NavigationMesh;

            forRect(CX, CY, 0, 1, 1, this.depth, (neighbor, x, y, z) =>
                {
                    if (neighbor.Navigatable)
                    {
                        var coincidentEdge = Gem.Geo.Mesh.FindCoincidentEdge(neighbor.NavigationMesh, baseMesh);
                        if (coincidentEdge.HasValue)
                            From.Links.Add(new CellLink
                            {
                                Direction = Direction,
                                Neighbor = neighbor,
                                EdgePoint = (coincidentEdge.Value.P0 + coincidentEdge.Value.P1) / 2.0f,
                                LinkZOffset = 0.0f
                            });
                    }
                });
        }

        private void RelinkColumn(int X, int Y)
        {
            forRect(X, Y, 0, 1, 1, this.depth, (c, x, y, z) =>
            {
                UpdateNavigation(c, x, y, z);
                DetectLinks(c, x, y);
            });

            RemoveLinksFromColumn(X - 1, Y, CellLink.Directions.East);
            RemoveLinksFromColumn(X + 1, Y, CellLink.Directions.West);
            RemoveLinksFromColumn(X, Y - 1, CellLink.Directions.South);
            RemoveLinksFromColumn(X, Y + 1, CellLink.Directions.North);

            LinkColumnInDirection(X - 1, Y, CellLink.Directions.East, X, Y);
            LinkColumnInDirection(X + 1, Y, CellLink.Directions.West, X, Y);
            LinkColumnInDirection(X, Y - 1, CellLink.Directions.South, X, Y);
            LinkColumnInDirection(X, Y + 1, CellLink.Directions.North, X, Y);
        }

        private void RemoveLinksFromColumn(int X, int Y, CellLink.Directions Direction)
        {
            if (this.check(X, Y, 0))
                this.forRect(X, Y, 0, 1, 1, this.depth, (c, x, y, z) =>
                    {
                        if (c.Links != null) c.Links.RemoveAll(l => l.Direction == Direction);
                    });
        }

        private void LinkColumnInDirection(int X, int Y, CellLink.Directions Direction, int CX, int CY)
        {
            if (this.check(X, Y, 0))
                this.forRect(X, Y, 0, 1, 1, this.depth, (c, x, y, z) =>
                    {
                        if (c.Navigatable) DetectConnectedInColumn(c, Direction, CX, CY);
                    });
        }
    }
}
