using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gem
{
    public class Animation
    {
        public String Name;
        public List<int> Frames;
        public float FrameTime;

        public Animation() { }

        public Animation(String Name, float FrameTime, params int[] Frames)
        {
            this.Name = Name;
            this.FrameTime = FrameTime;
            this.Frames = new List<int>(Frames);
        }
    }

    public class SpriteSheet
    {
        public int Columns;
        public int Rows;

        public SpriteSheet(int Columns, int Rows)
        {
            this.Columns = Columns;
            this.Rows = Rows;
        }

        public float SpriteWidth { get { return 1.0f / Columns; } }
        public float SpriteHeight { get { return 1.0f / Rows; } }

        public Matrix GetFrameTransform(int Frame, bool Flip)
        {
            var frameX = Frame % Columns;
            var frameY = Frame / Columns;

            if (Flip)
                return Matrix.CreateScale(-SpriteWidth, SpriteHeight, 1.0f)
                    * Matrix.CreateTranslation((frameX + 1) * SpriteWidth, frameY * SpriteHeight, 0.0f);
            else
                return Matrix.CreateScale(SpriteWidth, SpriteHeight, 1.0f)
                   * Matrix.CreateTranslation(frameX * SpriteWidth, frameY * SpriteHeight, 0.0f);
        }
    }

    public class AnimationSet
    {
        private Dictionary<String, Animation> Animations = new Dictionary<String, Animation>();

        public AnimationSet() { }

        public AnimationSet(params Animation[] Anims)
        {
            foreach (var anim in Anims) Animations.Upsert(anim.Name, anim);
        }

        public void AddAnimation(String Name, float FrameTime, params int[] Frames)
        {
            var anim = new Animation { FrameTime = FrameTime, Frames = new List<int>(Frames) };
            Animations.Upsert(Name, anim);
        }

        public Animation FindAnimation(String Name)
        {
            if (Animations.ContainsKey(Name)) return Animations[Name];
            else return null;
        }
    }
}
