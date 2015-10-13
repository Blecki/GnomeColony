using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gnome
{
    public class WaitAction : ActorAction
    {
        public float TotalTime;
        public float ElapsedTime = 0.0f;
        public bool Done = false;

        public WaitAction(float TotalTime)
        {
            this.TotalTime = TotalTime;
        }

        public override bool Update(Game Game, Actor Actor, float ElapsedTime)
        {
            this.ElapsedTime += ElapsedTime;
            
            if (this.ElapsedTime >= this.TotalTime)
            {
                Done = true;
                return true;
            }

            return false;
        }

    }
}
