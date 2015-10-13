using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gnome
{
    public class ActorAction
    {
        public virtual void Begin(Game Game, Actor Actor) { }
        public virtual bool Update(Game Game, Actor Actor, float ElapsedTime) { return true; }
        public virtual void Abort(Game Game, Actor Actor) { }
    }
}
