using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates
{
    public class HangingVines : BlockTemplate
    {
        public HangingVines()
        {
            Top = -1;
            NorthSide = 36;
            Bottom = -1;
            Shape = BlockShape.Cube;
            ShowInEditor = false;
        }
    }
}
