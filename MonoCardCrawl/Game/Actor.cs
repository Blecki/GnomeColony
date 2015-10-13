using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Game
{
    public class Actor : SharpRuleEngine.RuleObject
    {
        public static void DeclareRules(SharpRuleEngine.RuleEngine GlobalRules)
        {

        }

        public World World;
        public Gem.Euler Orientation = new Gem.Euler();
        public Vector3 MotionDelta = Vector3.Zero;
        public Gem.Render.SceneNode Renderable = null;
        public Gem.Gui.GuiSceneNode PopupGui = null;
        public Gem.PropertyBag Properties;

        public ActorAction CurrentAction { get; private set; }
        public ActorAction NextAction { private get; set; }
        
        public virtual void Create(Gem.PropertyBag Properties)
        {
            CurrentAction = null;
            NextAction = null;

            this.Properties = Properties.Clone();
        }

        public override SharpRuleEngine.RuleEngine GlobalRules { get { return World.GlobalRules; } }

        public virtual void Update(World World, float ElapsedTime)
        {
            if (NextAction != null)
            {
                if (CurrentAction != null) CurrentAction.End(World, this);
                CurrentAction = NextAction;
                NextAction = null;
                CurrentAction.Begin(World, this);
            }

            if (CurrentAction != null) CurrentAction.Update(World, this, ElapsedTime);
        }
    }
}
