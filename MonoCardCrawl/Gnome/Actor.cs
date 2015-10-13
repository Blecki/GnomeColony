using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gnome
{
    public class Actor
    {
        public Gem.Euler Orientation = new Gem.Euler();
        public Gem.Render.SceneNode Renderable = null;
        public Coordinate Location;
        public Vector3 PositionOffset = Vector3.Zero;
        public Gem.PropertyBag Properties;
        public ActorAction CurrentAction { get; private set; }
        public ActorAction NextAction { private get; set; }

        public virtual void Create(Gem.PropertyBag Properties)
        {
            this.Properties = Properties.Clone();

            CurrentAction = null;
            NextAction = null;
        }

        public virtual void Update(Game Game)
        {
            if (NextAction != null)
            {
                if (CurrentAction != null) CurrentAction.Abort(Game, this);
                CurrentAction = NextAction;
                NextAction = null;
                CurrentAction.Begin(Game, this);
            }

            if (CurrentAction != null) 
                if (CurrentAction.Update(Game, this, Game.ElapsedSeconds))
                    CurrentAction = null;

            Orientation.Position = Location.AsVector3() + PositionOffset;
            if (Game.World.check(Location.X, Location.Y, Location.Z))
            {
                var cell = Game.World.CellAt(Location.X, Location.Y, Location.Z);
                if (cell.Block == null)
                    Location.Z -= 1;
                else if (cell.Navigatable)
                    Orientation.Position = cell.CenterPoint + PositionOffset;
            }
        }

    }
}
