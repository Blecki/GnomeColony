using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Templates.Track
{
    public class LeftSwitchOpen1 : TrackBase
    {
        public LeftSwitchOpen1()
        {
            Top = 134;
            Shape = BlockShape.Decal;
            Orientable = true;
            ShowInEditor = false;
        }
    }
}
