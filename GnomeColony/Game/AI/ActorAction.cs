using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Game
{
    public class ActorAction
    {
        public virtual void Begin(Actor Actor) { }
        public virtual bool Step(Actor Actor) { return true; }
        public virtual void Update(Actor Actor, float StepPercentage) { }
    }
}
