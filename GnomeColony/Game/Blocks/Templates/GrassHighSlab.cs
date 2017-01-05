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

    }
}
