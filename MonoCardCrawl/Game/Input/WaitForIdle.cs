using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;
using Gem.Render;

namespace Game.Input
{
    class WaitForIdle : InputState
    {
        Actor BoundActor;

        public WaitForIdle(Actor BoundActor)
        {
            this.BoundActor = BoundActor;
        }

        public override void EnterState(WorldScreen Game, World World)
        {
            World.PrepareCombatGridForPlayerInput();
        }

        public override void Update(WorldScreen Game, World World)
        {
            if (BoundActor.CurrentAction is Actors.Actions.Idle)
            {
                Game.PopInputState();
            }
        }
    }
}