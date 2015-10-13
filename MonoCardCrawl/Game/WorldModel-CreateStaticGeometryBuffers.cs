using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game
{
    public static partial class WorldModel
    {
        private struct TileInstance
        {
            internal int X;
            internal int Y;
            internal int Z;
            internal Tile Tile;

            internal TileInstance(Tile Tile, int X, int Y, int Z)
            {
                this.Tile = Tile;
                this.X = X;
                this.Y = Y;
                this.Z = Z;
            }
        }

        public static Gem.Render.SceneNode CreateStaticGeometryBuffers(World From, GraphicsDevice Device)
        {
            var tilePiles = new List<List<TileInstance>>();

            From.Grid.ForEachTile((cell, x, y, z) =>
                {
                    if (cell.Tile == null) return;
                    var existingPile = tilePiles.FirstOrDefault(pile => pile[0].Tile.Combinable(cell.Tile));
                    if (existingPile == null)
                    {
                        existingPile = new List<TileInstance>();
                        tilePiles.Add(existingPile);
                    }
                    existingPile.Add(new TileInstance(cell.Tile, x, y, z));
                });

            var r = new Gem.Render.BranchNode();

            foreach (var pile in tilePiles)
            {
                var mesh = Gem.Geo.Gen.InstanceMerge(
                    pile.Select(i => i.Tile.RenderMesh),
                    pile.Select(t => Matrix.CreateTranslation(t.X + 0.5f, t.Y + 0.5f, t.Z)));
                var compiledMesh = Gem.Geo.CompiledModel.CompileModel(mesh, Device);
                r.Add(new Gem.Render.CompiledMeshNode(compiledMesh, pile[0].Tile.Texture));
            }

            return r;

        }
    }
}