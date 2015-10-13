using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gnome
{
    public class TileSheet
    {
        public Texture2D Texture { get; private set; }
        public int TileWidth { get; private set; }
        public int TileHeight { get; private set; }

        public int Columns { get { return Texture.Width / TileWidth; } }
        public int Rows { get { return Texture.Height / TileHeight; } }
        public int Row(int TileIndex) { return TileIndex / Columns; }
        public int Column(int TileIndex) { return TileIndex % Columns; }
        public float TileUStep { get { return 1.0f / Columns; } }
        public float TileVStep { get { return 1.0f / Rows; } }
        public float ColumnU(int Column) { return TileUStep * Column; }
        public float RowV(int Row) { return TileVStep * Row; }
        public float TileU(int TileIndex) { return ColumnU(Column(TileIndex)); }
        public float TileV(int TileIndex) { return RowV(Row(TileIndex)); }

        // Generate UV transform matricies that align the UV range 0..1 to a tile.
        public Matrix ScaleMatrix { get { return Matrix.CreateScale(1.0f / Columns, 1.0f / Rows, 1.0f); } }
        public Matrix TranslationMatrix(int Column, int Row) { return Matrix.CreateTranslation(TileUStep * Column, TileVStep * Row, 0.0f); }
        public Matrix TileMatrix(int Column, int Row) { return ScaleMatrix * TranslationMatrix(Column, Row); }
        public Matrix TileMatrix(int TileIndex) { return TileMatrix(Column(TileIndex), Row(TileIndex)); }
        public Matrix TileMatrix(int Column, int Row, int ColumnSpan, int RowSpan)
        {
            return Matrix.CreateScale(ColumnSpan, RowSpan, 1.0f) * TileMatrix(Column, Row);
        }

        public TileSheet(Texture2D Texture, int TileWidth, int TileHeight)
        {
            this.Texture = Texture;
            this.TileWidth = TileWidth;
            this.TileHeight = TileHeight;
        }
    }
}
