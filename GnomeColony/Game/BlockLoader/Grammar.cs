using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class BlockGrammar : Ancora.Grammar
    {
        public BlockGrammar()
        {
            var ws = Maybe(Token(c => " \r\n\t".Contains(c))).WithMutator(n => null);
            var semi = Character(';').WithMutator(n => null);

            var identifierStartChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_";
            var identifierChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-0123456789_";
            var identifier = Identifier(c => identifierStartChars.Contains(c), c => identifierChars.Contains(c)).Ast("IDENT");
            var number = Token(c => "0123456789".Contains(c)).Ast("NUMBER");
            var list = (Character('[').WithMutator(n => null) + ws +
                DelimitedList((ws + identifier + ws).WithMutator(n => n.Children[0]), Character(',').WithMutator(n => null))
                    .WithMutator(n =>
                    {
                        var r = new Ancora.AstNode { NodeType = "LIST" };
                        r.Children.Add(n.Children[0]);
                        foreach (var sub in n.Children[1].Children)
                            r.Children.Add(sub.Children[0]);
                        return r;
                    }) + ws + Character(']').WithMutator(n => null)).WithMutator(n => n.Children[0]);

            var tileDefinition = (Keyword("tile") + ws + identifier + ws + number + ws + semi).WithMutator(n =>
            {
                n.NodeType = "TILE";
                n.Value = n.Children[1].Value;
                var value = n.Children[2];
                n.Children.Clear();
                n.Children.Add(value);
                return n;
            });

            var blockMember = (ws + identifier + ws + (identifier | number | list) + ws + semi).WithMutator(n =>
            {
                n.NodeType = "MEMBER";
                n.Value = n.Children[0].Value;
                var value = n.Children[1].Children[0];
                n.Children.Clear();
                n.Children.Add(value);
                return n;
            });

            var blockBody = (Character('{').WithMutator(n => null) + ws + NoneOrMany(blockMember) + ws + '}').WithMutator(n => n.Children[0]);

            var blockDefinition = (Keyword("block") + ws + identifier + ws + Maybe(blockBody) + ws + semi).WithMutator(n =>
            {
                n.NodeType = "BLOCK";
                n.Value = n.Children[1].Value;

                if (n.Children[2].Children.Count > 0)
                {
                    var members = n.Children[2].Children[0];
                    n.Children = members.Children;
                }
                else
                    n.Children.Clear();

                return n;
            });

            var file = NoneOrMany((ws + (tileDefinition | blockDefinition).WithMutator(n => n.Children[0]) + ws).WithMutator(n => n.Children[0])).Ast("ROOT");

            Root = file;
        }
    }
}
