using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace VelesConflict.Animators
{
    enum AnimatorState
    {
        Started,Stoped
    }
    interface IAnimator<T> where T:struct
    {
        AnimatorState State { get; }
        bool Reverse { get; set; }
        int SleepFor { get; set; }
        int Duration { get; set; }
        T AtStart { get; set; }
        T Value { get; }
        T AtEnd { get; set; }

        void Start();
        void Stop();
        void Reset();

        void Update(GameTime gameTime);
        
    }
}
