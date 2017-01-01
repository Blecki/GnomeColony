using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates
{
    public class Dirt : BlockTemplate
    {
        public Dirt()
        {
            Preview = 34;
            Top = 34;
            NorthSide = 34;
            Bottom = 34;
            Shape = BlockShape.Cube;
        }
    }
}
