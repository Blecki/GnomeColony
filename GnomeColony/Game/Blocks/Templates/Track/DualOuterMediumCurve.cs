using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates.Track
{
    public class DualOuterMediumCurve : TrackBase
    {
        public DualOuterMediumCurve()
        {
            Top = 194;
            Shape = BlockShape.Decal;
            Orientable = true;
            ShowInEditor = false;
        }
    }
}
