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
    public class BlockSet
    {
        public Dictionary<String, BlockTemplate> Templates;
        public TileSheet Tiles;

        public static BlockSet FromReflection()
        {
            var r = new BlockSet { Templates = new Dictionary<string, BlockTemplate>() };

            foreach (var type in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsSubclassOf(typeof(BlockTemplate)))
                    r.Templates.Add(type.Name, Activator.CreateInstance(type) as BlockTemplate);
            }

            foreach (var template in r.Templates)
                template.Value.Initialize(r);

            return r;
        }
    }
}
