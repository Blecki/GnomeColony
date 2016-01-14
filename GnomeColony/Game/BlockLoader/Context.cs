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
    public class BlockSetLoadContext
    {
        public Dictionary<String, int> NamedTiles = new Dictionary<String,int>();
        public Dictionary<String, BlockTemplate> NamedBlocks = new Dictionary<String, BlockTemplate>();
    }
}
