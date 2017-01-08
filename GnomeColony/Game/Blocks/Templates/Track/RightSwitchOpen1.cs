using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates.Track
{
    public class RightSwitchOpen1 : TrackBase
    {
        public RightSwitchOpen1()
        {
            Top = 132;
            Shape = BlockShape.Decal;
            Orientable = true;
            ShowInEditor = false;
        }
    }
}
