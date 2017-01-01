using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates
{
    public class Grass : BlockTemplate
    {
        public Grass()
        {
            Preview = 33;
            Top = 33;
            NorthSide = 32;
            Bottom = 34;
            Shape = BlockShape.Cube;
            //Hanging = "HangingVines";
            BuildType = global::Game.BuildType.All;
        }
    }
}
