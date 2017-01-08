using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Game
{
    public class BlockButton : Gum.Widget
    {
        public BlockTemplate Template;
      
        protected override Gum.Mesh Redraw()
        {
            var drawArea = this.GetDrawableInterior();
            var tileWidth = (float)drawArea.Width / (float)Template.PreviewDimensions.X;
            var tileHeight = (float)drawArea.Height / (float)Template.PreviewDimensions.Y;
            var result = new List<Gum.Mesh>();
            result.Add(base.Redraw());

            for (var x = 0; x < Template.PreviewDimensions.X; ++x)
                for (var y = 0; y < Template.PreviewDimensions.Y; ++y)
                {
                    var previewIndex = (y * Template.PreviewDimensions.X) + x;
                    var previewTile = new OrientedTile(0);
                    if (Template.PreviewTiles != null && previewIndex < Template.PreviewTiles.Count)
                        previewTile = Template.PreviewTiles[(y * Template.PreviewDimensions.X) + x];
                    var orientationMatrix = Matrix.CreateRotationZ((float)(-Math.PI / 2) * (int)previewTile.Orientation);
                    var tileMatrix = Root.GetTileSheet("tiles").TileMatrix(previewTile.Tile);

                    var quad = Gum.Mesh.Quad()
                        .MorphEx(v =>
                        {
                            v.TextureCoordinate -= new Vector2(0.5f, 0.5f);
                            v.TextureCoordinate = Vector2.Transform(v.TextureCoordinate, orientationMatrix);
                            v.TextureCoordinate += new Vector2(0.5f, 0.5f);
                            v.TextureCoordinate = Vector2.Transform(v.TextureCoordinate, tileMatrix);
                            return v;
                        })
                        .Scale(tileWidth, tileHeight)
                        .Translate((x * tileWidth) + drawArea.X, (y * tileHeight) + drawArea.Y);

                    result.Add(quad);
                }

            return Gum.Mesh.Merge(result.ToArray());
        }
    }
}
