using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game
{
    public class Gnome : Actor
    {
        public int CarriedResource = 0;
        public GnomeMind Mind;

        public Gnome(Simulation Sim, TileSheet Sheet)
        {
            Mind = new GnomeMind(this);
            Renderable = new GnomeNode(this, Sim, Sheet);
        }
   
    }
}
