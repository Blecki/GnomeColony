using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates
{
    public class Track : BlockTemplate
    {
        public Track()
        {
            Preview = 128;
            Top = 128;
            Type = BlockType.Decal;
        }
    }
}
