using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates.Track
{
    public class Junction : BlockTemplate
    {
        public Junction()
        {
            Top = 192;
            Shape = BlockShape.Decal;
            Orientable = true;
            ShowInEditor = false;
        }
    }
}
