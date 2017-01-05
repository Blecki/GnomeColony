using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates
{
    public class TrackSmallCurve : TrackBase
    {
        public TrackSmallCurve()
        {
            Preview = 160;
            Top = 160;
            Shape = BlockShape.Decal;
            Orientable = true;
            ShowInEditor = true;
        }
    }
}
