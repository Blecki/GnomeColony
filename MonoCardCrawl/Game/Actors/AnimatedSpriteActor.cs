using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Game.Actors
{
    public class AnimatedSpritePropertyBag : QuadActorPropertyBag
    {
        public Gem.AnimationSet Animations
        {
            get { return GetPropertyAs<Gem.AnimationSet>("animations", () => new Gem.AnimationSet()); }
            set { Upsert("animations", value); }
        }

        public Gem.SpriteSheet SpriteSheet
        {
            get { return GetPropertyAs<Gem.SpriteSheet>("sprite-sheet"); }
            set { Upsert("sprite-sheet", value); }
        }

        public Texture2D HoverOverlay { set { Upsert("hover-overlay", value); } }
        public bool HiliteOnHover { set { Upsert("hilite-on-hover", value); } }
    }

    public class AnimatedSpriteActor : QuadActor
    {
        protected Gem.AnimationTimer AnimationTimer = new Gem.AnimationTimer();
        protected Gem.SpriteSheet SpriteSheet;
        private Gem.Animation CurrentAnimation;
        private Gem.AnimationSet Animations;

        public enum Facing
        {
            Left,
            Right
        }

        public Facing FacingDirection;

        public override void Create(Gem.PropertyBag Properties)
        {
            base.Create(Properties);

            Animations = Properties.GetPropertyAs<Gem.AnimationSet>("animations");
            SpriteSheet = Properties.GetPropertyAs<Gem.SpriteSheet>("sprite-sheet");
            PlayAnimation("idle");
            
            Perform<Actor>("enter-run").Do((actor) => { 
                PlayAnimation("run"); 
                return SharpRuleEngine.PerformResult.Continue; 
            });
            Perform<Actor>("enter-idle").Do((actor) => { PlayAnimation("idle"); return SharpRuleEngine.PerformResult.Continue; });

            MeshNode.HoverOverlay = Properties.GetPropertyAs<Texture2D>("hover-overlay", () => null);
            MeshNode.HiliteOnHover = Properties.GetPropertyAs<bool>("hilite-on-hover", () => false);
        }

        public void PlayAnimation(String Name)
        {
            CurrentAnimation = Animations.FindAnimation(Name);
            if (CurrentAnimation != null)
                AnimationTimer.Loop(CurrentAnimation.Frames.Count, CurrentAnimation.FrameTime);
        }

        public override void Update(World World, float ElapsedSeconds)
        {
            base.Update(World, ElapsedSeconds);

            AnimationTimer.Update(ElapsedSeconds);

            if (MotionDelta.X > 0)
                FacingDirection = Facing.Right;
            else if (MotionDelta.X < 0)
                FacingDirection = Facing.Left;

            if (CurrentAnimation != null && AnimationTimer.GetFrame() < CurrentAnimation.Frames.Count)
                MeshNode.UVTransform = SpriteSheet.GetFrameTransform(CurrentAnimation.Frames[AnimationTimer.GetFrame()], FacingDirection == Facing.Left);
        }
    }
}
