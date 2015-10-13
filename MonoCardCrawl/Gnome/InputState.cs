using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;
using Gem.Render;

namespace Gnome
{
    public class InputState
    {
        public virtual void EnterState(Game Game) { }
        public virtual void Covered(Game Game) { }
        public virtual void Update(Game Game) { }
        public virtual void Exposed(Game Game) { }
        public virtual void LeaveState(Game Game) { }

    }
}