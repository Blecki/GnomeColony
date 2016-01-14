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
    public class BlockSetLoader
    {
        public static BlockSetLoadContext LoadDefinitionFile(String FileData)
        {
            var parser = new BlockGrammar();
            var parsed = parser.Root.Parse(new Ancora.StringIterator(FileData));
            if (parsed.ResultType != Ancora.ResultType.Success)
                throw new InvalidOperationException("Failed parsing block definitions: " + parsed.FailReason.Message);
            if (!parsed.After.AtEnd)
                throw new InvalidOperationException("Failed parsing block definitions: Failed to consume entire file.");

            var context = new BlockSetLoadContext();

            foreach (var node in parsed.Node.Children)
                if (node.NodeType == "BLOCK")
                {
                    var blockTemplate = new BlockTemplate() { Name = node.Value.ToString() };
                    context.NamedBlocks.Upsert(blockTemplate.Name, blockTemplate);
                }
            
            foreach (var node in parsed.Node.Children)
            {
                if (node.NodeType == "TILE")
                    context.NamedTiles.Upsert(node.Value.ToString(), Convert.ToInt32(node.Children[0].Value));
                else if (node.NodeType == "BLOCK")
                {
                    var name = node.Value.ToString();
                    var blockTemplate = context.NamedBlocks[name];

                    foreach (var member in node.Children)
                    {
                        var memberName = member.Value.ToString();
                        var field = typeof(BlockTemplate).GetField(memberName);
                        if (field == null) throw new InvalidOperationException("Failed parsing block definitions: Unknown member " + memberName + ".");
                        var converterAttribute = field.GetCustomAttributes(false).FirstOrDefault(a => a is BlockPropertyConverter) as BlockPropertyConverter;
                        if (converterAttribute == null) throw new InvalidOperationException("Failed parsing block definitions: Member " + memberName + " does not have a converter.");
                        field.SetValue(blockTemplate, converterAttribute.Convert(context, member.Children[0]));
                    }
                }
            }

            return context;            
        }
    }
}
