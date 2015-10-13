using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Input
{
    public class TurnScheduler : InputState
    {
        private List<Actor> Actors;
        private Actor CurrentActor = null;


        public TurnScheduler(IEnumerable<Actor> Actors)
        {
            this.Actors = new List<Actor>(Actors);
        }

        public override void EnterState(WorldScreen Game, World World)
        {
            AdvanceTurn(Game);
        }

        public override void Exposed(WorldScreen Game, World World)
        {
            AdvanceTurn(Game);
        }

        private void AdvanceTurn(WorldScreen Game)
        {
            if (Actors.Count == 0) return;
            if (CurrentActor == null) CurrentActor = Actors[0];
            else
            {
                var cIndex = Actors.FindIndex(a => Object.ReferenceEquals(CurrentActor, a));
                cIndex += 1;
                if (cIndex == Actors.Count) cIndex = 0;
                CurrentActor = Actors[cIndex];
            }

            Game.PushInputState(new PlayerTurn(CurrentActor));
        }
    }
}
