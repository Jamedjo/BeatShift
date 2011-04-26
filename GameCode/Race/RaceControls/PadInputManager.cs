using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BeatShift.Input
{
    class PadInputManager : IInputManager
    {
        //TODO: Need to check pad.isConnected in some of the methods in this class?
        //GamePad.GetCapabilities().GamePadType //test for wheel

        private PlayerIndex index;
        private GamePadState currentPadState;
        private GamePadState lastPadState;

        public PlayerIndex getPlayerIndex()
        {
            return index;
        }

        public PadInputManager(PlayerIndex playerIndex)
        {
            index = playerIndex;
            currentPadState = GamePad.GetState(index);
            lastPadState = currentPadState;
        }

        #region IInputManager Interface

        Boolean IInputManager.actionPressed(InputAction action)
        {
            return GamePad.GetState(index).IsButtonDown(InputLayout.getButton(action));
        }

        Boolean IInputManager.actionTapped(InputAction action)
        {
            return wasButtonTapped(InputLayout.getButton(action));
        }

        float IInputManager.getActionValue(InputAction action)
        {
            return getButtonValue(InputLayout.getButton(action));
        }

        void IInputManager.Update(GameTime gameTime)
        {
            //if (currentPadState.IsConnected)
            lastPadState = currentPadState;
            currentPadState = GamePad.GetState(index);
        }

        #endregion

        public Boolean wasButtonTapped(Buttons check)
        {
            return (currentPadState.IsButtonDown(check) && !lastPadState.IsButtonDown(check));
        }

        public float getButtonValue(Buttons check)
        {
            float unclampedValue = 0f;
            switch (check)
            {
                case Buttons.LeftTrigger:
                    unclampedValue = currentPadState.Triggers.Left;
                    break;
                case Buttons.RightTrigger:
                    unclampedValue = currentPadState.Triggers.Right;
                    break;

                //Thumbsticks have a vector, with each component ranging from -1 to 1
                //Get value or '-value' so that a positive value is given in the direction being checked.
                case Buttons.LeftThumbstickRight:
                    unclampedValue = currentPadState.ThumbSticks.Left.X;
                    break;
                case Buttons.LeftThumbstickLeft:
                    unclampedValue = -currentPadState.ThumbSticks.Left.X;
                    break;
                case Buttons.LeftThumbstickUp:
                    unclampedValue = currentPadState.ThumbSticks.Left.Y;
                    break;
                case Buttons.LeftThumbstickDown:
                    unclampedValue = -currentPadState.ThumbSticks.Left.Y;
                    break;

                case Buttons.RightThumbstickRight:
                    unclampedValue = currentPadState.ThumbSticks.Right.X;
                    break;
                case Buttons.RightThumbstickLeft:
                    unclampedValue = -currentPadState.ThumbSticks.Right.X;
                    break;
                case Buttons.RightThumbstickUp:
                    unclampedValue = currentPadState.ThumbSticks.Right.Y;
                    break;
                case Buttons.RightThumbstickDown:
                    unclampedValue = -currentPadState.ThumbSticks.Right.Y;
                    break;

                //Button does not give an analog value
                default:
                    if(currentPadState.IsButtonDown(check))
                        return 1f;
                    else return 0f;
            }

            //If a negative value in thumbstick direction then return zero for that direction.
            if (unclampedValue < 0f)
                return 0f;

            return unclampedValue;
        }

    }
}
