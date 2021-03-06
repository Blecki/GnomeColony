﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game
{
    public static partial class Generate
    {
        private enum BlockFaceDirection
        {
            Nonspecific,
            Up,
            Down,
            North,
            East,
            South,
            West
        }

        private static Coordinate Neighbor(Coordinate Of, BlockFaceDirection Direction)
        {
            switch (Direction)
            {
                case BlockFaceDirection.Up: return new Coordinate(Of.X, Of.Y, Of.Z + 1);
                case BlockFaceDirection.Down: return new Coordinate(Of.X, Of.Y, Of.Z - 1);
                case BlockFaceDirection.North: return new Coordinate(Of.X, Of.Y + 1, Of.Z);
                case BlockFaceDirection.East: return new Coordinate(Of.X + 1, Of.Y, Of.Z);
                case BlockFaceDirection.South: return new Coordinate(Of.X, Of.Y - 1, Of.Z);
                case BlockFaceDirection.West: return new Coordinate(Of.X - 1, Of.Y, Of.Z);
                default: return Of;
            }
        }

        private static BlockFaceDirection ClassifyNormal(Vector3 Normal)
        {
            if (Gem.Math.Vector.NearlyEqual(new Vector3(0, 0, 1), Normal)) return BlockFaceDirection.Up;
            if (Gem.Math.Vector.NearlyEqual(new Vector3(0, 0, -1), Normal)) return BlockFaceDirection.Down;
            if (Gem.Math.Vector.NearlyEqual(new Vector3(0, 1, 0), Normal)) return BlockFaceDirection.North;
            if (Gem.Math.Vector.NearlyEqual(new Vector3(1, 0, 0), Normal)) return BlockFaceDirection.East;
            if (Gem.Math.Vector.NearlyEqual(new Vector3(0, -1, 0), Normal)) return BlockFaceDirection.South;
            if (Gem.Math.Vector.NearlyEqual(new Vector3(-1, 0, 0), Normal)) return BlockFaceDirection.West;
            return BlockFaceDirection.Nonspecific;
        }

        private static BlockFaceDirection Opposite(BlockFaceDirection Direction)
        {
            switch(Direction)
            {
                case BlockFaceDirection.Up: return BlockFaceDirection.Down;
                case BlockFaceDirection.Down: return BlockFaceDirection.Up;
                case BlockFaceDirection.North: return BlockFaceDirection.South;
                case BlockFaceDirection.East: return BlockFaceDirection.West;
                case BlockFaceDirection.South: return BlockFaceDirection.North;
                case BlockFaceDirection.West: return BlockFaceDirection.East;
                default: throw new InvalidOperationException();
            }
        }

        private class BlockFace
        {
            public BlockFaceDirection Direction;
            public Vector3 Normal;
            public Gem.Geo.Mesh Mesh;
        }

        private class BlockShapeTemplate
        {
            public Gem.Geo.Mesh NavigationMesh;
            public Microsoft.Xna.Framework.Vector3 CenterPoint;
            public List<BlockFace> Faces;
        }

        private static Dictionary<BlockShape, List<BlockShapeTemplate>> ShapeTemplates = null;
        private static Dictionary<int, Vector3[]> ResourceOffsets = null;

        private static IEnumerable<Gem.Geo.Mesh> EnumerateAsUniqueMeshes(Gem.Geo.Mesh Mesh)
        {
            for (int i = 0; i < Mesh.indicies.Length; i += 3)
            {
                var faceMesh = new Gem.Geo.Mesh();
                faceMesh.verticies = new Gem.Geo.Vertex[3];
                for (int x = 0; x < 3; ++x)
                    faceMesh.verticies[x] = Mesh.verticies[Mesh.indicies[i + x]];
                faceMesh.indicies = new short[] { 0, 1, 2 };
                yield return faceMesh;
            }
        }

        private static Vector3 NormalOfFirstFace(Gem.Geo.Mesh Mesh)
        {
            return Gem.Geo.Gen.CalculateNormal(Mesh, 0, 2, 1);
        }

        private static List<Gem.Geo.Mesh> ExplodeMeshFaces(Gem.Geo.Mesh Mesh)
        {
            var result = new List<Gem.Geo.Mesh>();

            foreach (var face in EnumerateAsUniqueMeshes(Mesh))
            {
                var foundMatchingFace = false;

                for (int i = 0; i < result.Count; ++i)
                {
                    if (Gem.Math.Vector.NearlyEqual(NormalOfFirstFace(face), NormalOfFirstFace(result[i])))
                    {
                        result[i] = Gem.Geo.Gen.Merge(face, result[i]);
                        foundMatchingFace = true;
                        break;
                    }
                }

                if (!foundMatchingFace) result.Add(face);
            }

            return result;
        }

        private static void InitializeShapeTemplate(BlockShape Shape, Gem.Geo.Mesh Mesh, Gem.Geo.Mesh NavMesh)
        {
            var l = new List<BlockShapeTemplate>();

            for (int i = 0; i < 4; ++i)
            {
                var r = new BlockShapeTemplate();

                // Generate block faces from raw mesh.
                var mesh = Gem.Geo.Gen.FacetCopy(Gem.Geo.Gen.TransformCopy(Mesh, Matrix.CreateRotationZ((Gem.Math.Angle.PI / 2) * i)));
                Gem.Geo.Gen.CalculateTangentsAndBiNormals(mesh);

                var explodedFaces = ExplodeMeshFaces(mesh);
                r.Faces = explodedFaces.Select(f => new BlockFace
                {
                    Mesh = f,
                    Normal = NormalOfFirstFace(f),
                    Direction = ClassifyNormal(NormalOfFirstFace(f))
                }).ToList();

                // Prepare navigation mesh.
                r.NavigationMesh = Gem.Geo.Gen.TransformCopy(NavMesh, Matrix.CreateRotationZ((Gem.Math.Angle.PI / 2) * i));
                Gem.Geo.Gen.CalculateTangentsAndBiNormals(r.NavigationMesh);

                var centerPointRayOrigin = new Vector3(0.5f, 0.5f, 2.0f);
                var hitCenter = r.NavigationMesh.RayIntersection(new Ray(centerPointRayOrigin, new Vector3(0, 0, -1)));
                if (hitCenter.Intersects)
                    r.CenterPoint = centerPointRayOrigin + (new Vector3(0, 0, -1) * hitCenter.Distance);
                else
                    r.CenterPoint = new Vector3(0.5f, 0.5f, 0.5f);

                l.Add(r);
            }

            ShapeTemplates.Upsert(Shape, l);
        }

        private static void InitializeStaticData()
        {
            if (ShapeTemplates == null)
            {
                ShapeTemplates = new Dictionary<BlockShape, List<BlockShapeTemplate>>();

                InitializeShapeTemplate(BlockShape.Cube,
                    Gem.Geo.Gen.CreateTexturedFacetedCube(),
                    Gem.Geo.Gen.FacetCopy(Gem.Geo.Gen.TransformCopy(Gem.Geo.Gen.CreateQuad(), Matrix.CreateTranslation(0.0f, 0.0f, 1.0f))));

                InitializeShapeTemplate(BlockShape.LowerSlab,
                    Gem.Geo.Gen.TextureAndFacetAsCube(
                        Gem.Geo.Gen.TransformCopy(
                            Gem.Geo.Gen.TransformCopy(
                                Gem.Geo.Gen.CreateCube(),
                                Matrix.CreateScale(1.0f, 1.0f, 0.5f)),
                            Matrix.CreateTranslation(0, 0, -0.25f))),
                    Gem.Geo.Gen.FacetCopy(Gem.Geo.Gen.TransformCopy(Gem.Geo.Gen.CreateQuad(), Matrix.CreateTranslation(0.0f, 0.0f, 0.5f))));

                InitializeShapeTemplate(BlockShape.UpperSlab,
                    Gem.Geo.Gen.TextureAndFacetAsCube(
                        Gem.Geo.Gen.TransformCopy(
                            Gem.Geo.Gen.TransformCopy(
                                Gem.Geo.Gen.CreateCube(),
                                Matrix.CreateScale(1.0f, 1.0f, 0.5f)),
                            Matrix.CreateTranslation(0, 0, 0.25f))),
                    Gem.Geo.Gen.FacetCopy(Gem.Geo.Gen.TransformCopy(Gem.Geo.Gen.CreateQuad(), Matrix.CreateTranslation(0.0f, 0.0f, 1.0f))));

                InitializeShapeTemplate(BlockShape.Slope,
                    Gem.Geo.Gen.TextureAndFacetAsCube(Gem.Geo.Gen.CreateWedge(1.0f)),
                    Gem.Geo.Gen.FacetCopy(Gem.Geo.Gen.CreateSlantedQuad(1.0f)));

                InitializeShapeTemplate(BlockShape.HalfSlopeLow,
                    Gem.Geo.Gen.TextureAndFacetAsCube(Gem.Geo.Gen.CreateWedge(0.5f)),
                    Gem.Geo.Gen.FacetCopy(Gem.Geo.Gen.CreateSlantedQuad(0.5f)));

                InitializeShapeTemplate(BlockShape.HalfSlopeHigh,
                    Gem.Geo.Gen.TextureAndFacetAsCube(
                        Gem.Geo.Gen.TransformCopy(
                            Gem.Geo.Gen.TransformCopy(
                                Gem.Geo.Gen.TransformCopy(
                                    Gem.Geo.Gen.CreateSlantedCube(1.0f),
                                    Matrix.CreateTranslation(0, 0, 0.5f)), // Translate up 
                                Matrix.CreateScale(1.0f, 1.0f, 0.5f)), // Scale in half
                            Matrix.CreateTranslation(0, 0, -0.5f))),
                    Gem.Geo.Gen.FacetCopy(
                        Gem.Geo.Gen.TransformCopy(
                            Gem.Geo.Gen.CreateSlantedQuad(0.5f),
                            Matrix.CreateTranslation(0, 0, 0.5f))));

                InitializeShapeTemplate(BlockShape.Surface,
                    Gem.Geo.Gen.FacetCopy(Gem.Geo.Gen.TransformCopy(Gem.Geo.Gen.CreateQuad(), Matrix.CreateTranslation(0.0f, 0.0f, 0.4f))),
                    Gem.Geo.Gen.FacetCopy(Gem.Geo.Gen.TransformCopy(Gem.Geo.Gen.CreateQuad(), Matrix.CreateTranslation(0.0f, 0.0f, 0.5f))));
            }

            if (ResourceOffsets == null)
            {
                ResourceOffsets = new Dictionary<int, Vector3[]>();

                ResourceOffsets.Add(0, new Vector3[] { new Vector3(0, 0, 0.25f) });
                ResourceOffsets.Add(1, new Vector3[] { new Vector3(0, 0, 0.25f) });
                ResourceOffsets.Add(2, new Vector3[] { new Vector3(-0.25f, -0.25f, 0.25f), new Vector3(0.25f, 0.25f, 0.25f) });
                ResourceOffsets.Add(3, new Vector3[] { new Vector3(-0.25f, -0.25f, 0.25f), new Vector3(0.25f, 0.25f, 0.25f), new Vector3(0.25f, -0.25f, 0.25f) });
                ResourceOffsets.Add(4, new Vector3[] { new Vector3(-0.25f, -0.25f, 0.25f), new Vector3(0.25f, 0.25f, 0.25f), new Vector3(0.25f, -0.25f, 0.25f), new Vector3(-0.25f, 0.25f, 0.25f) });
                ResourceOffsets.Add(5, new Vector3[] { new Vector3(-0.25f, -0.25f, 0.25f), new Vector3(0.25f, 0.25f, 0.25f), new Vector3(0.25f, -0.25f, 0.25f), new Vector3(-0.25f, 0.25f, 0.25f), new Vector3(0, 0, 0.75f) });
                ResourceOffsets.Add(6, new Vector3[] { new Vector3(-0.25f, -0.25f, 0.25f), new Vector3(0.25f, 0.25f, 0.25f), new Vector3(0.25f, -0.25f, 0.25f), new Vector3(-0.25f, 0.25f, 0.25f), new Vector3(-0.25f, -0.25f, 0.75f), new Vector3(0.25f, 0.25f, 0.75f) });
                ResourceOffsets.Add(7, new Vector3[] { new Vector3(-0.25f, -0.25f, 0.25f), new Vector3(0.25f, 0.25f, 0.25f), new Vector3(0.25f, -0.25f, 0.25f), new Vector3(-0.25f, 0.25f, 0.25f), new Vector3(-0.25f, -0.25f, 0.75f), new Vector3(0.25f, 0.25f, 0.75f), new Vector3(0.25f, -0.25f, 0.75f)});
                ResourceOffsets.Add(8, new Vector3[] { new Vector3(-0.25f, -0.25f, 0.25f), new Vector3(0.25f, 0.25f, 0.25f), new Vector3(0.25f, -0.25f, 0.25f), new Vector3(-0.25f, 0.25f, 0.25f), new Vector3(-0.25f, -0.25f, 0.75f), new Vector3(0.25f, 0.25f, 0.75f), new Vector3(0.25f, -0.25f, 0.75f), new Vector3(-0.25f, 0.25f, 0.75f) });
            }
        }

        private static BlockShapeTemplate GetShapeTemplate(BlockShape Shape, int Orientation)
        {
            InitializeStaticData();
            return ShapeTemplates[Shape][Orientation];
        }

        public static Gem.Geo.Mesh GetNavigationMesh(BlockShape Shape, int Orientation)
        {
            InitializeStaticData();
            return ShapeTemplates[Shape][Orientation].NavigationMesh;
        }
        
        public static Vector3 GetCenterPoint(BlockShape Shape, int Orientation)
        {
            InitializeStaticData();
            return ShapeTemplates[Shape][Orientation].CenterPoint;
        }

        private static bool CoincidentVertex(Gem.Geo.Mesh M, Vector3 V)
        {
            foreach (var v in M.verticies)
                if (Gem.Math.Vector.NearlyEqual(v.Position, V)) return true;
            return false;
        }

        private static bool AreCoincident(Gem.Geo.Mesh A, Gem.Geo.Mesh B)
        {
            if (A.VertexCount != B.VertexCount) return false;

            foreach (var v in A.verticies)
                if (!CoincidentVertex(B, v.Position)) return false;
            return true;
        }

        public static Gem.Geo.Mesh ChunkGeometry(CellGrid Grid, int SX, int SY, int SZ, int W, int H, int D, BlockSet Blocks)
        {
            InitializeStaticData();

            var models = new List<Gem.Geo.Mesh>();

            Grid.forRect(SX, SY, SZ, W, H, D, (cell, x, y, z) =>
                {
                    if (cell.Block != null)
                    {
                        if (cell.Block.Shape == BlockShape.Surface && Grid.check(x, y, z + 1) &&
                            Object.ReferenceEquals(Grid.CellAt(x, y, z + 1).Block, cell.Block))
                        { }
                        else
                        {
                            {
                                var shapeTemplate = GetShapeTemplate(cell.Block.Shape, (int)cell.BlockOrientation);

                                var exposedFaces = shapeTemplate.Faces.Where(f =>
                                    {
                                        // The camera can't look up... so cull down faces.
                                        if (f.Direction == BlockFaceDirection.Down) return false;

                                        // If the face is not planar with a block side (eg, a slope) it will never be culled.
                                        if (f.Direction == BlockFaceDirection.Nonspecific) return true;

                                        var neighborCoordinate = Neighbor(new Coordinate(x, y, z), f.Direction);

                                        // Faces on the very edge of the world should be drawn.
                                        if (!Grid.Check(neighborCoordinate)) return true; 

                                        var neighborCell = Grid.CellAt(neighborCoordinate);
                                        // Draw face if there is no neighbor block.
                                        if (neighborCell.Block == null) return true; 

                                        // Draw this face if this block is solid and the neighbor is not.
                                        if (cell.Block.MaterialType == BlockMaterialType.Solid && neighborCell.Block.MaterialType != BlockMaterialType.Solid) 
                                            return true;

                                        // Don't draw this face if this is a cube on a cube - This lets us skip the expensive coincident face checks.
                                        if (cell.Block.Shape == BlockShape.Cube && neighborCell.Block.Shape == BlockShape.Cube)
                                            return false;

                                        // Find the neighboring face that could potentially overlap this face.
                                        var neighborFaceDirection = Opposite(f.Direction);
                                        var neighborShapeTemplate = GetShapeTemplate(neighborCell.Block.Shape, (int)neighborCell.BlockOrientation);
                                        var neighborFace = neighborShapeTemplate.Faces.FirstOrDefault(nf => nf.Direction == neighborFaceDirection);

                                        // If there is no neighbor face on this side of the block, draw the face.
                                        if (neighborFace == null) return true;

                                        // If the faces are coincident, cull this face. 
                                        return !AreCoincident(Gem.Geo.Gen.TransformCopy(f.Mesh, Matrix.CreateTranslation(-f.Normal)), neighborFace.Mesh);
                                    });

                                foreach (var face in exposedFaces)
                                {
                                    var mesh = Gem.Geo.Gen.Copy(face.Mesh);
                                    MorphBlockTextureCoordinates(Blocks.Tiles, cell.Block, mesh, (int)cell.BlockOrientation);
                                    Gem.Geo.Gen.Transform(mesh, Matrix.CreateTranslation(x + 0.5f, y + 0.5f, z + 0.5f));
                                    models.Add(mesh);
                                }
                            }

                            if (!String.IsNullOrEmpty(cell.Block.Hanging) && Grid.check(x, y, z - 1) && !Grid.CellAt(x, y, z - 1).IsSolid)
                            {
                                var hangingBlockTemplate = Blocks.Templates[cell.Block.Hanging];
                                var shapeTemplate = GetShapeTemplate(hangingBlockTemplate.Shape, (int)cell.BlockOrientation);

                                var exposedFaces = shapeTemplate.Faces.Where(f =>
                                {
                                    // Don't draw the top or bottom of fringe blocks.
                                    if (f.Direction == BlockFaceDirection.Up || f.Direction == BlockFaceDirection.Down)
                                        return false;

                                    // Draw this face because it's not planar to the sides.
                                    if (f.Direction == BlockFaceDirection.Nonspecific) return true;

                                    var neighborCoordinate = Neighbor(new Coordinate(x, y, z), f.Direction);
                                    // Draw this face because it's on the edge of the world.
                                    if (!Grid.Check(neighborCoordinate)) return true;

                                    var neighborCell = Grid.CellAt(neighborCoordinate);

                                    // Don't draw fringe if the block next to the parent is solid.
                                    if (neighborCell.Block != null && neighborCell.Block.MaterialType == BlockMaterialType.Solid) return false;

                                    // Don't draw fringe if the block next to the fringe is solid.
                                    var lowerNeighborCell = Grid.CellAt(neighborCoordinate.X, neighborCoordinate.Y, z - 1);
                                    if (lowerNeighborCell.Block != null && lowerNeighborCell.Block.MaterialType == BlockMaterialType.Solid) return false;

                                    return true;
                                });

                                foreach (var face in exposedFaces)
                                {
                                    var mesh = Gem.Geo.Gen.Copy(face.Mesh);
                                    MorphBlockTextureCoordinates(Blocks.Tiles, hangingBlockTemplate, mesh, (int)cell.BlockOrientation);
                                    Gem.Geo.Gen.Transform(mesh, Matrix.CreateTranslation(x + 0.5f, y + 0.5f, z - 0.5f));
                                    models.Add(mesh);
                                }
                            }
                        }
                    }

                    if (cell.Decal != null)
                    {
                        var navMesh = cell.Block == null ? GetNavigationMesh(cell.Decal.Shape, 0) : GetNavigationMesh(cell.Block.Shape, (int)cell.BlockOrientation);
                        var copy = Gem.Geo.Gen.Copy(navMesh);
                        Gem.Geo.Gen.MorphEx(copy, (inV) =>
                        {
                            var r = inV;
                            r.TextureCoordinate = Vector2.Transform(r.TextureCoordinate, Blocks.Tiles.TileMatrix(cell.Decal.Top));
                            return r;
                        });

                        // Nav meshes are not centered at 0.5, so they don't need to be translated as much.
                        Gem.Geo.Gen.Transform(copy, Matrix.CreateTranslation(x + 0.5f, y + 0.5f, z));

                        models.Add(copy);
                    }

                    if (cell.Task != null)
                    {
                        var markerCube = CreateMarkerBlockMesh(Blocks.Tiles, cell.Task.MarkerTile);
                        Gem.Geo.Gen.Transform(markerCube, Matrix.CreateTranslation(x, y, z));
                        models.Add(markerCube);
                    }

                    if (cell.HasFlag(CellFlags.Storehouse) && cell.Navigatable && cell.NavigationMesh != null)
                    {
                        var navMesh = cell.Block == null ? GetNavigationMesh(BlockShape.Cube, 0) : GetNavigationMesh(cell.Block.Shape, (int)cell.BlockOrientation);
                        var copy = Gem.Geo.Gen.Copy(navMesh);
                        Gem.Geo.Gen.MorphEx(copy, (inV) =>
                        {
                            var r = inV;
                            r.TextureCoordinate = Vector2.Transform(r.TextureCoordinate, Blocks.Tiles.TileMatrix(TileNames.Storehouse));
                            return r;
                        });

                        // Nav meshes are not centered at 0.5, so they don't need to be translated as much.
                        Gem.Geo.Gen.Transform(copy, Matrix.CreateTranslation(x + 0.5f, y + 0.5f, z));

                        models.Add(copy);
                    }
                                        
                    var offsetsIndex = 0;
                    if (ResourceOffsets.ContainsKey(cell.Resources.Count))
                        offsetsIndex = cell.Resources.Count;

                    for (int i = 0; i < cell.Resources.Count; ++i)
                    {
                        var resourceCube = CreateResourceBlockMesh(Blocks.Tiles, Blocks.Templates[cell.Resources[i]]);
                        Gem.Geo.Gen.Transform(resourceCube, Matrix.CreateTranslation(cell.CenterPoint 
                            + ResourceOffsets[offsetsIndex][i % ResourceOffsets[offsetsIndex].Length]
                            + new Vector3(0.0f, 0.0f, (cell.Block == null ? 0.0f : cell.Block.ResourceHeightOffset))));
                        models.Add(resourceCube);
                    }

                });

            var result = Gem.Geo.Gen.Merge(models.ToArray());

            if (result.VertexCount == 0) return null; // Empty chunk?

            return result;
        }

        public static List<Gem.Geo.Mesh> CreateNormalBlockMesh(TileSheet Tiles, BlockTemplate Template, int Orientation)
        {
            return ShapeTemplates[Template.Shape][Orientation].Faces.Select(f =>
                {
                    var mesh = Gem.Geo.Gen.Copy(f.Mesh);
                    MorphBlockTextureCoordinates(Tiles, Template, mesh, Orientation);
                    return mesh;
                }).ToList();
        }

        public static void MorphBlockTextureCoordinates(TileSheet Tiles, BlockTemplate Template, Gem.Geo.Mesh Mesh, int Orientation)
        {
            var top = Template.Top;
            var bottom = Template.Bottom == -1 ? Template.Top : Template.Bottom;
            var sides = new int[6];
            sides[0] = Template.Top;
            sides[1] = Template.Bottom == -1 ? sides[0] : Template.Bottom;
            sides[2] = Template.NorthSide == -1 ? sides[0] : Template.NorthSide;
            sides[3] = Template.EastSide == -1 ? sides[2] : Template.EastSide;
            sides[4] = Template.SouthSide == -1 ? sides[2] : Template.SouthSide;
            sides[5] = Template.WestSide == -1 ? sides[3] : Template.WestSide;
        
            var sideMatrix = sides.Select(s => Tiles.TileMatrix(s)).ToArray();
            
            Gem.Geo.Gen.MorphEx(Mesh, (inV) =>
            {
                var r = inV;

                if (r.Normal.Z > 0.2f)
                    r.TextureCoordinate = Vector2.Transform(r.TextureCoordinate, sideMatrix[1]);
                else if (r.Normal.Z < -0.2f)
                    r.TextureCoordinate = Vector2.Transform(r.TextureCoordinate, sideMatrix[0]);
                else
                {
                    var orien = 0;
                    if (r.Normal.Y > 0.2f) orien = 0;
                    else if (r.Normal.Y < -0.2f) orien = 2;
                    else if (r.Normal.X > 0.2f) orien = 1;
                    else if (r.Normal.X < -0.2f) orien = 3;

                    r.TextureCoordinate = Vector2.Transform(
                        r.TextureCoordinate, 
                        sideMatrix[2 + ((orien + Orientation) % 4)]);
                }

                return r;
            });
        }

        private static Gem.Geo.Mesh CreateMarkerBlockMesh(TileSheet Tiles, int MarkerTile)
        {
            var cube = Gem.Geo.Gen.CreateTexturedFacetedCube();
            Gem.Geo.Gen.Transform(cube, Matrix.CreateScale(1.02f)); // Make cube slightly larger.
            Gem.Geo.Gen.Transform(cube, Matrix.CreateTranslation(0.5f, 0.5f, 0.5f)); // re-origin cube.
            
            //Morph cube texture coordinates
            Gem.Geo.Gen.MorphEx(cube, (inV) =>
            {
                var r = inV;
                r.TextureCoordinate = Vector2.Transform(r.TextureCoordinate, Tiles.TileMatrix(MarkerTile));
                return r;
            });

            return cube;
        }

        public static Gem.Geo.Mesh CreateResourceBlockMesh(TileSheet Tiles, BlockTemplate Template)
        {
            var mesh = Gem.Geo.Gen.Merge(CreateNormalBlockMesh(Tiles, Template, 0).ToArray());
            Gem.Geo.Gen.Transform(mesh, Matrix.CreateScale(0.5f));
            return mesh;
        }
    }
}