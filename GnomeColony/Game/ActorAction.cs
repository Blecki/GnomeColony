using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Game
{
    public enum ActorActionFlags
    {
        Interuptible = 1
    }

    public class ActorAction
    {
        public virtual void Begin(Game Game, Actor Actor) { }
        public virtual bool Update(Game Game, Actor Actor, float ElapsedTime) { return true; }

        private ActorActionFlags Flags = 0;

        public void SetFlag(ActorActionFlags Flag, bool Value)
        {
            if (Value) Flags |= Flag;
            else Flags &= ~Flag;
        }

        public bool HasFlag(ActorActionFlags Flag)
        {
            return Flags.HasFlag(Flag);
        }

        public bool Interuptible { get { return HasFlag(ActorActionFlags.Interuptible); } }
    }
}
