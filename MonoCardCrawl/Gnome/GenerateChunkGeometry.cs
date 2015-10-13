using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gnome
{
    public static partial class Generate
    {
        public static Gem.Geo.Mesh ChunkGeometry(CellGrid Grid, TileSheet Tiles)
        {
            var models = new List<Gem.Geo.Mesh>();

            Grid.forAll((cell, x, y, z) =>
                {
                    if (cell.Block != null)
                    {
                        var cube = CreateNormalBlockMesh(Tiles, cell.Block);
                        Gem.Geo.Gen.Transform(cube, Matrix.CreateTranslation(x, y, z));
                        models.Add(cube);
                    }

                    if (cell.Task != null)
                    {
                        var markerCube = CreateMarkerBlockMesh(Tiles, cell.Task.MarkerTile);
                        Gem.Geo.Gen.Transform(markerCube, Matrix.CreateTranslation(x, y, z));
                        models.Add(markerCube);
                    }

                    if (cell.Navigatable && cell.NavigationMesh != null)
                    {
                        var navMesh = Gem.Geo.Gen.TransformCopy(cell.NavigationMesh, Matrix.CreateTranslation(0.0f, 0.0f, 0.02f));
                        Gem.Geo.Gen.MorphEx(navMesh, (inV) =>
                        {
                            var r = inV;
                            r.TextureCoordinate = Vector2.Transform(r.TextureCoordinate, Tiles.TileMatrix(3));
                            return r;
                        });
                        models.Add(navMesh);
                    }

                    if (cell.Resource != null && cell.Resource.Filled)
                    {
                        var resourceCube = CreateResourceBlockMesh(Tiles, cell.Resource.BlockType);
                        Gem.Geo.Gen.Transform(resourceCube, Matrix.CreateTranslation(x + 0.5f, y + 0.5f, z));
                        models.Add(resourceCube);
                    }

                });

            return Gem.Geo.Gen.Merge(models.ToArray());
        }

        private static Gem.Geo.Mesh CreateNormalBlockMesh(TileSheet Tiles, BlockTemplate Template)
        {
            var cube = Gem.Geo.Gen.CreateTexturedFacetedCube();
            Gem.Geo.Gen.Transform(cube, Matrix.CreateTranslation(0.5f, 0.5f, 0.5f)); // re-origin cube.
            MorphBlockTextureCoordinates(Tiles, Template, cube);
            return cube;
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
                    r.TextureCoordinate = Vector2.Transform(r.TextureCoordinate,
                        Tiles.TileMatrix(Template.Side));
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
            var cube = Gem.Geo.Gen.CreateTexturedFacetedCube();
            MorphBlockTextureCoordinates(Tiles, Template, cube);
            Gem.Geo.Gen.Transform(cube, Matrix.CreateScale(0.5f));
            return cube;
        }
    }
}