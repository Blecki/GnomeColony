using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Game
{
    public class Actor
    {
        public Gem.Euler Orientation = new Gem.Euler();
        public Gem.Render.SceneNode Renderable = null;
        public Coordinate Location;
        public Vector3 PositionOffset = Vector3.Zero;
        public Gem.PropertyBag Properties;
        public ActorAction CurrentAction { get; set; }
        public CellLink.Directions FacingDirection = CellLink.Directions.North;

        public List<Component> Components;
        
        public virtual void Create(Gem.PropertyBag Properties)
        {
            this.Properties = Properties.Clone();

            CurrentAction = null;
        }

        public void StepAction()
        {
            if (CurrentAction != null)
                if (CurrentAction.Step(this))
                    CurrentAction = null;
        }

        public void UpdateAction(float StepPercentage)
        {
            if (CurrentAction != null)
                CurrentAction.Update(this, StepPercentage);
        }

        public void UpdatePosition(Simulation Sim)
        {
            Orientation.Position = Location.AsVector3() + PositionOffset;
            if (Sim.World.check(Location.X, Location.Y, Location.Z))
            {
                var cell = Sim.World.CellAt(Location.X, Location.Y, Location.Z);
                if (cell.Block == null)
                    Location.Z -= 1;
                else if (cell.Navigatable)
                    Orientation.Position = cell.CenterPoint + PositionOffset;
            }
        }
    }
}
