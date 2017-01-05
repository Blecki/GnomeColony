﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates
{
    public class GrassHalfSlopeHigh : BlockTemplate
    {
        public GrassHalfSlopeHigh()
        {
            Preview = 39;
            Top = 33;
            NorthSide = 40;
            SouthSide = 32;
            EastSide = 39;
            WestSide = 39;
            Bottom = 34;
            Shape = BlockShape.HalfSlopeHigh;
            Orientable = true;
            //Hanging = "HangingVines";
        }
    }
}