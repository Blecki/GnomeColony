using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;
using Gem.Render;

namespace Game
{
    public class BlockPropertyConverter : Attribute
    {
        public virtual Object Convert(BlockSetLoadContext Context, Ancora.AstNode Node)
        {
            throw new NotImplementedException();
        }
    }

    public class TileBlockPropertyConverterAttribute : BlockPropertyConverter
    {
        public override object Convert(BlockSetLoadContext Context, Ancora.AstNode Node)
        {
            if (Node.NodeType == "NUMBER") return System.Convert.ToInt32(Node.Value);
            else if (Node.NodeType == "IDENT") return Context.NamedTiles[Node.Value.ToString()];
            else throw new InvalidOperationException("Block property conversion error");
        }
    }

    public class EnumBlockPropertyConverterAttribute : BlockPropertyConverter
    {
        public System.Type EnumType;

        public EnumBlockPropertyConverterAttribute(System.Type EnumType)
        {
            this.EnumType = EnumType;
        }

        public override object Convert(BlockSetLoadContext Context, Ancora.AstNode Node)
        {
            return Enum.Parse(EnumType, Node.Value.ToString());
        }
    }

    public class BoolBlockPropertyConverterAttribute : BlockPropertyConverter
    {
        public override object Convert(BlockSetLoadContext Context, Ancora.AstNode Node)
        {
            if (Node.Value.ToString() == "false") return false;
            else if (Node.Value.ToString() == "true") return true;
            else throw new InvalidOperationException("Block property conversion error");
        }
    }

    public class ListBlockPropertyConverterAttribute : BlockPropertyConverter
    {
        public override object Convert(BlockSetLoadContext Context, Ancora.AstNode Node)
        {
            return Node.Children.Select(c => c.Value.ToString()).ToArray();
        }
    }

    public class BlockBlockPropertyConverterAttribute : BlockPropertyConverter
    {
        public override object Convert(BlockSetLoadContext Context, Ancora.AstNode Node)
        {
            return Node.Value.ToString();
        }
    }
}