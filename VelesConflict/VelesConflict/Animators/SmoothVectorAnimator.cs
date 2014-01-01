using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace VelesConflict.Animators
{
    class SmoothVectorAnimator:IAnimator<Vector2>
    {
        int Passed = 0;
        public bool Reverse
        {
            get;
            set;
        }

        public AnimatorState State
        {
            get;
            private set;
        }

        public int SleepFor
        {
            get;
            set;
        }

        public int Duration
        {
            get;
            set;
        }

        public Vector2 AtStart
        {
            get;
            set;
        }

        public Vector2 Value
        {
            get
            {
                if (Passed < SleepFor)
                    return AtStart;
                else if (Passed > SleepFor + Duration)
                    return AtEnd;
                else
                    return Vector2.SmoothStep(AtStart, AtEnd, (Passed - SleepFor) / (float)Duration);
            }
        }

        public Vector2 AtEnd
        {
            get;
            set;
        }

        public SmoothVectorAnimator()
        {
            State = AnimatorState.Stoped;
        }



        public void Start()
        {
            State = AnimatorState.Started;

        }
        public void Stop()
        {
            State = AnimatorState.Stoped;
        }

        public void Reset()
        {
            State = AnimatorState.Stoped;
            Passed = 0;
            if (Reverse)
                Passed = Duration + SleepFor;
        }

        public void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (State == AnimatorState.Stoped)
                return;
            Passed += (Reverse ? -1 : 1) * (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (Reverse ? (Passed <= 0) : (Passed > Duration + SleepFor))
            {
                State = AnimatorState.Stoped;
            }
        }




        

        
    }
}
