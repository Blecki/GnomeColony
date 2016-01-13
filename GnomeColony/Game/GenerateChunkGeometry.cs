using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game
{
    public static partial class Generate
    {
        private static Dictionary<BlockShape, BlockShapeTemplate> ShapeTemplates = null;
        private static Dictionary<int, Vector3[]> ResourceOffsets = null;

        private static void InitializeStaticData()
        {
            if (ShapeTemplates == null)
            {
                ShapeTemplates = new Dictionary<BlockShape, BlockShapeTemplate>();

                ShapeTemplates.Add(BlockShape.Cube,
                    new BlockShapeTemplate
                    {
                        Mesh = Gem.Geo.Gen.CreateTexturedFacetedCube(),
                        NavigationMesh = Gem.Geo.Gen.FacetCopy(Gem.Geo.Gen.TransformCopy(Gem.Geo.Gen.CreateQuad(), Matrix.CreateTranslation(0.0f, 0.0f, 1.0f)))
                    });

                ShapeTemplates.Add(BlockShape.Slab,
                    new BlockShapeTemplate
                    {
                        Mesh = Gem.Geo.Gen.TransformCopy(
                            Gem.Geo.Gen.CreateTexturedFacetedCube(), 
                            Matrix.CreateScale(1.0f, 1.0f, 0.5f)),
                        NavigationMesh = Gem.Geo.Gen.FacetCopy(Gem.Geo.Gen.TransformCopy(Gem.Geo.Gen.CreateQuad(), Matrix.CreateTranslation(0.0f, 0.0f, 0.5f)))
                    });

                ShapeTemplates.Add(BlockShape.Slope,
                    new BlockShapeTemplate
                    {
                        Mesh = Gem.Geo.Gen.TextureAndFacetAsCube(Gem.Geo.Gen.CreateWedge(1.0f)),
                        NavigationMesh = Gem.Geo.Gen.FacetCopy(
                            Gem.Geo.Gen.CreateSlantedQuad(1.0f))
                    });
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

        public static Gem.Geo.Mesh GetNavigationMesh(BlockShape Shape)
        {
            InitializeStaticData();
            return ShapeTemplates[Shape].NavigationMesh;
        }

        public static Gem.Geo.Mesh GetMesh(BlockShape Shape)
        {
            InitializeStaticData();
            return ShapeTemplates[Shape].Mesh;
        }

        public static Gem.Geo.Mesh ChunkGeometry(CellGrid Grid, TileSheet Tiles, BlockTemplateSet Templates)
        {
            InitializeStaticData();

            var models = new List<Gem.Geo.Mesh>();

            Grid.forAll((cell, x, y, z) =>
                {
                    if (cell.Block != null)
                    {
                        var cube = CreateNormalBlockMesh(Tiles, cell.Block);

                        if (cell.Block.Orientable)
                            Gem.Geo.Gen.Transform(cube, Matrix.CreateRotationZ(
                                (Gem.Math.Angle.PI / 2) * (int)cell.BlockOrientation));

                        Gem.Geo.Gen.Transform(cube, Matrix.CreateTranslation(x + 0.5f, y + 0.5f, z + 0.5f));

                        models.Add(cube);

                        if (cell.Block.Hanging != -1 && Grid.check(x,y,z-1) && Grid.CellAt(x,y,z-1).Block == null)
                        {
                            var hangingBlock = Templates[cell.Block.Hanging];
                            cube = CreateNormalBlockMesh(Tiles, hangingBlock);
                            if (hangingBlock.Orientable)
                                Gem.Geo.Gen.Transform(cube, Matrix.CreateRotationZ(
                                    (Gem.Math.Angle.PI / 2) * (int)cell.BlockOrientation));

                            Gem.Geo.Gen.Transform(cube, Matrix.CreateTranslation(x + 0.5f, y + 0.5f, z - 0.5f));

                            models.Add(cube);
                        }
                    }

                    if (cell.Task != null)
                    {
                        var markerCube = CreateMarkerBlockMesh(Tiles, cell.Task.MarkerTile);
                        Gem.Geo.Gen.Transform(markerCube, Matrix.CreateTranslation(x, y, z));
                        models.Add(markerCube);
                    }

                    if (cell.HasFlag(CellFlags.Storehouse) && cell.Navigatable && cell.NavigationMesh != null)
                    {
                        var navMesh = Gem.Geo.Gen.TransformCopy(cell.NavigationMesh, Matrix.CreateTranslation(0.0f, 0.0f, 0.02f));
                        Gem.Geo.Gen.MorphEx(navMesh, (inV) =>
                        {
                            var r = inV;
                            r.TextureCoordinate = Vector2.Transform(r.TextureCoordinate, Tiles.TileMatrix(TileNames.Storehouse));
                            return r;
                        });
                        models.Add(navMesh);
                    }

                    //if (cell.Block != null && cell.NavigationMesh != null)
                    //{
                    //    var navMesh = Gem.Geo.Gen.TransformCopy(cell.NavigationMesh, Matrix.CreateTranslation(0.0f, 0.0f, 0.02f));
                    //    Gem.Geo.Gen.MorphEx(navMesh, (inV) =>
                    //    {
                    //        var r = inV;
                    //        r.TextureCoordinate = Vector2.Transform(r.TextureCoordinate, Tiles.TileMatrix(3));
                    //        return r;
                    //    });
                    //    models.Add(navMesh);
                    //}

                    var offsetsIndex = 0;
                    if (ResourceOffsets.ContainsKey(cell.Resources.Count))
                        offsetsIndex = cell.Resources.Count;

                    for (int i = 0; i < cell.Resources.Count; ++i)
                    {
                        var resourceCube = CreateResourceBlockMesh(Tiles, Templates[cell.Resources[i]]);
                        Gem.Geo.Gen.Transform(resourceCube, Matrix.CreateTranslation(cell.CenterPoint 
                            + ResourceOffsets[offsetsIndex][i % ResourceOffsets[offsetsIndex].Length]
                            + new Vector3(0.0f, 0.0f, (cell.Block == null ? 0.0f : cell.Block.ResourceHeightOffset))));
                        models.Add(resourceCube);
                    }

                });

            return Gem.Geo.Gen.Merge(models.ToArray());
        }

        private static Gem.Geo.Mesh CreateNormalBlockMesh(TileSheet Tiles, BlockTemplate Template)
        {
            var mesh = Gem.Geo.Gen.Copy(ShapeTemplates[Template.Shape].Mesh);
            MorphBlockTextureCoordinates(Tiles, Template, mesh);
            return mesh;
        }

        private static void MorphBlockTextureCoordinates(TileSheet Tiles, BlockTemplate Template, Gem.Geo.Mesh cube)
        {
            Gem.Geo.Gen.MorphEx(cube, (inV) =>
            {
                var r = inV;

                if (r.Normal.Z > 0.1f)
                    r.TextureCoordinate = Vector2.Transform(r.TextureCoordinate,
                        Tiles.TileMatrix(Template.Bottom));
                else if (r.Normal.Z < -0.1f)
                    r.TextureCoordinate = Vector2.Transform(r.TextureCoordinate,
                        Tiles.TileMatrix(Template.Top));
                else
                {
                    if (Template.SideB != -1)
                    {
                        if (r.Normal.X != 0)
                            r.TextureCoordinate = Vector2.Transform(r.TextureCoordinate, Tiles.TileMatrix(Template.SideA));
                        else
                            r.TextureCoordinate = Vector2.Transform(r.TextureCoordinate, Tiles.TileMatrix(Template.SideB));
                    }
                    else
                        r.TextureCoordinate = Vector2.Transform(r.TextureCoordinate, Tiles.TileMatrix(Template.SideA));
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
            var mesh = CreateNormalBlockMesh(Tiles, Template);
            Gem.Geo.Gen.Transform(mesh, Matrix.CreateScale(0.5f));
            return mesh;
        }
    }
}