using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework;
using VelesConflict.Animators;
using VelesConflict.Shared;
using Microsoft.Xna.Framework.Graphics;

namespace VelesConflict.UI
{
    enum MenuState
    {
        TransitionIn, Sustain, TransitionOut, ReverseTransitionOut, ReverseTransitionIn
    }
    class MenuManager
    {
        public Dictionary<string, Menu> Menues { get; private set; }
        public MenuState State { get; private set; }
        
        private Menu Current { get; set; }
        private Menu Next { get; set; }
        
        Stack<Menu> HistoryStack;

        public MenuManager()
        {
            Menues = new Dictionary<string, Menu>();
            HistoryStack = new Stack<Menu>();
            State = MenuState.Sustain;
        }
        public void SetMenu(string Menu)
        {
            Current = Menues[Menu];
        }

        public void PreviousMenu()
        {
            if (State != MenuState.Sustain)
                return;
            if (HistoryStack.Count == 0)
            {
                Globals.Game.Exit();
                return;
            }
            Next = HistoryStack.Pop();
            State = MenuState.TransitionOut;
            Current.OutVectorAnimator.Reset();
            Current.OutVectorAnimator.Start();
        }
        public void Update(GameTime gameTime)
        {
            if (State == MenuState.TransitionOut && Current.OutVectorAnimator.State == AnimatorState.Stoped)
            {
                State = MenuState.TransitionIn;
                Current.OutVectorAnimator.Stop();
                Current = Next;
                Next = null;
                Current.InVectorAnimator.Reset();
                Current.InVectorAnimator.Start();
            }
            else if (State == MenuState.TransitionIn && Current.InVectorAnimator.State == AnimatorState.Stoped)
            {
                Current.InVectorAnimator.Stop();
                State = MenuState.Sustain;
            }

            #region Sustain
            if (State == MenuState.Sustain)
            {
                TouchCollection collection = TouchPanel.GetState();
                foreach (TouchLocation location in collection)
                {
                    Rectangle touchRectangle = new Rectangle((int)location.Position.X, (int)location.Position.Y, 10, 10);
                    if (location.State == TouchLocationState.Pressed)
                    {
                        foreach (IButton button in Current.Buttons)
                        {
                            if (touchRectangle.Intersects(button.Rectangle))
                            {
                                button.ButtonState = GameButtonState.Hovering;
                                break;
                            }
                        }
                    }
                    else if (location.State == TouchLocationState.Released)
                    {
                        foreach (IButton button in Current.Buttons)
                        {
                            if (button.ButtonState == GameButtonState.Hovering)
                            {
                                if (touchRectangle.Intersects(button.Rectangle))
                                {

                                    if (button.OnClick != null)
                                    {
                                        button.OnClick(button, EventArgs.Empty);
                                    }
                                    if (!String.IsNullOrWhiteSpace(button.NextMenu))
                                    {
                                        if (!button.NextMenu.Equals("%Back%"))
                                        {
                                            HistoryStack.Push(Current);


                                            Next = Menues[button.NextMenu];
                                            State = MenuState.TransitionOut;
                                            Current.OutVectorAnimator.Reset();
                                            Current.OutVectorAnimator.Start();
                                        }
                                        else
                                        {
                                            PreviousMenu();
                                        }
                                    }
                                    break;
                                }
                                button.ButtonState = GameButtonState.Normal;
                            }
                            else
                                button.ButtonState = GameButtonState.Normal;
                        }
                    }
                }
            }
            #endregion
            else if (State == MenuState.TransitionOut)
            {
                //BackgroundAnimator.Update(gameTime);
                Current.OutVectorAnimator.Update(gameTime);
                foreach (IButton button in Current.Buttons)
                {
                    button.Offset = Current.OutVectorAnimator.Value;
                }
            }
            else
            {
                //BackgroundAnimator.Update(gameTime);
                Current.InVectorAnimator.Update(gameTime);
                foreach (IButton button in Current.Buttons)
                {
                    button.Offset = Current.InVectorAnimator.Value;
                }
            }
        }

        public void Draw()
        {
            Current.Draw();
        }
    }
}
