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
        public String CarriedResource = null;
        public GnomeMind Mind;

        public Gnome(Simulation Sim, TileSheet Sheet)
        {
            Mind = new GnomeMind(this);
            Renderable = new GnomeNode(this, Sim, Sheet);
        }

        public bool CarryingResource { get { return !String.IsNullOrEmpty(CarriedResource); } }
   
    }
}
