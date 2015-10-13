using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem
{
    public class AnimationTimer
    {
        private float CurrentFrame;
        private int FrameCount;
        private bool _Loop = false;
        private float FrameTime;
        private bool Paused = false;

        public void Play(int FrameCount, float FrameTime)
        {
            this.FrameCount = FrameCount;
            CurrentFrame = 0;
            this.FrameTime = FrameTime;
            _Loop = false;
        }

        public void Loop(int FrameCount, float FrameTime)
        {
            this.FrameCount = FrameCount;
            this.FrameTime = FrameTime;
            _Loop = true;
            CurrentFrame = 0;
        }

        public void Pause()
        {
            Paused = true;
        }

        public void Resume()
        {
            Paused = false;
        }

        public void Update(float ElapsedSeconds)
        {
            if (Paused) return;
            CurrentFrame += ElapsedSeconds;
        }

        public int GetFrame()
        {
            var r = (int)(CurrentFrame / FrameTime);
            if (_Loop) return r % FrameCount;
            else if (r >= FrameCount) return FrameCount - 1;
            else return r;
        }
    }
}
