using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates
{
    public class TrackSmallJunction : BlockTemplate
    {
        public TrackSmallJunction()
        {
            Preview = 192;
            Top = 192;
            Shape = BlockShape.Decal;
            Orientable = true;
        }
    }
}
