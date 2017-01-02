using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates
{
    public class GrassHighSlab : BlockTemplate
    {
        public GrassHighSlab()
        {
            Preview = 40;
            Top = 33;
            NorthSide = 40;
            Bottom = 34;
            Shape = BlockShape.UpperSlab;
            //Hanging = "HangingVines";
        }

        public override bool CanComposite(BlockTemplate Onto)
        {
            if (Onto is GrassLowSlab) return true;
            return false;
        }

        public override BlockTemplate Compose(BlockTemplate With, BlockSet TemplateSet)
        {
            if (With is GrassLowSlab) return TemplateSet.Templates["Grass"];

            return new BlockTemplate
            {
                Type = BlockType.Combined,
                CompositeBlocks = HelperExtensions.MakeList(
                    new SubBlock { Block = TemplateSet.Templates["GrassLowSlab"] }, 
                    new SubBlock { Block = this })
            };
        }
    }
}
