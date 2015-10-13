using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Game
{
    public class ActorAction
    {
        public virtual void Begin(World World, Actor Actor) { }
        public virtual void Update(World World, Actor Actor, float ElapsedTime) { }
        public virtual void End(World World, Actor Actor) { }
    }
}
