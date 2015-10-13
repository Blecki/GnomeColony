using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;

namespace Game
{
    public class CombatGrid
    {
        internal Gem.Common.Grid3D<CombatCell> Cells;

        public static CombatGrid CreateFromCellGrid(CellGrid From)
        {
            var r = new CombatGrid();
            r.Cells = new Gem.Common.Grid3D<CombatCell>(From.width, From.height, From.depth, (x,y,z) => null);

            //Prepare cells
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
                        mesh = Gem.Geo.Gen.FacetCopy(mesh);
                        Gem.Geo.Gen.Transform(mesh, Matrix.CreateTranslation(x + 0.5f, y + 0.5f, z));
                        Gem.Geo.Gen.CalculateTangentsAndBiNormals(mesh);

                        var combatCell = new CombatCell();
                        combatCell.Mesh = mesh;
                        combatCell.ParentCell = c;
                        combatCell.Coordinate = new Coordinate(x, y, z);
                        var centerPointRayOrigin  = new Vector3(x + 0.5f, y + 0.5f, z + 2.0f);
                        var hitCenter = mesh.RayIntersection(new Ray(centerPointRayOrigin, new Vector3(0,0,-1)));
                        if (hitCenter.Intersects)
                            combatCell.CenterPoint = centerPointRayOrigin + (new Vector3(0, 0, -1) * hitCenter.Distance);
                        else
                            combatCell.CenterPoint = new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);
                        
                        r.Cells[x, y, z] = combatCell;
                    }
                }
            });

            //Detect connected edges.
            r.Cells.forAll((c, x, y, z) =>
                {
                    if (c != null)
                    {
                        c.Links = new List<CombatCell.Link>();
                        DetectConnectedInColumn(c, CombatCell.Direction.North, x, y - 1, r);
                        DetectConnectedInColumn(c, CombatCell.Direction.East, x + 1, y, r);
                        DetectConnectedInColumn(c, CombatCell.Direction.South, x, y + 1, r);
                        DetectConnectedInColumn(c, CombatCell.Direction.West, x - 1, y, r);
                    }
                });

            return r;
        }

        private static void DetectConnectedInColumn(CombatCell From, CombatCell.Direction Direction,
            int CX, int CY, CombatGrid Grid)
        {
            if (CX < 0 || CX >= Grid.Cells.width || CY < 0 || CY >= Grid.Cells.height) return;

            Grid.Cells.forRect(CX, CY, 0, 1, 1, Grid.Cells.depth, (neighbor, x, y, z) =>
                {
                    if (neighbor != null)
                    {
                        var coincidentEdge = Gem.Geo.Mesh.FindCoincidentEdge(neighbor.Mesh, From.Mesh);
                        if (coincidentEdge.HasValue)
                            From.Links.Add(new CombatCell.Link
                            {
                                Direction = Direction,
                                Neighbor = neighbor,
                                EdgePoint = (coincidentEdge.Value.P0 + coincidentEdge.Value.P1) / 2.0f
                            });

                    }
                });
        }

        public Gem.Geo.Mesh.RayIntersectionResult RayIntersection(Ray Ray)
        {
            var closestIntersection = new Gem.Geo.Mesh.RayIntersectionResult
            {
                Distance = float.PositiveInfinity,
                Intersects = false
            };

            Cells.forAll((c, x, y, z) =>
            {
                if (c == null) return;
                var interimResult = c.Mesh.RayIntersection(Ray);
                if (interimResult.Intersects && interimResult.Distance <= closestIntersection.Distance)
                {
                    closestIntersection = interimResult;
                    closestIntersection.Tag = c;
                }
            });

            return closestIntersection;
        }

        public void ClearTemporaryData()
        {
            Cells.forAll((c, x, y, z) =>
                {
                    if (c != null)
                    {
                        c.Visible = false;
                        c.Texture = 0;
                        c.AnchoredActor = null;
                        c.ClickAction = null;
                    }
                });
        }
    }
}
