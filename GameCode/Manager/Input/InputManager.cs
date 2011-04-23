using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace BeatShift.Input
{

    //back/accept button pressed? for menus, responds to all controllers?
    //fullscreen triggered (F4 pressed)
    //KonamiCode track //Track input state during update for a sequence of presses, also used for combo distance.

    public enum InputType { None, Keyboard, PadOne, PadTwo, PadThree, PadFour, AI, AllConnected }//All and AI and CoOp not implemented yet
   
    public class InputManager
    {
        //Need to check pad.isConnected in some of the methods in this class.
        InputType type;

        GamePadState currentPadState;
        KeyboardState currentKeyState;
        GamePadState lastPadState;
        KeyboardState lastKeyState;
        //GamePad.GetCapabilities().GamePadType //test for wheel

        public InputManager(InputType inputType)
        {
            type = inputType;

            currentPadState = GamePad.GetState(getIndex());
            currentKeyState = Keyboard.GetState();
            lastKeyState = currentKeyState;
            lastPadState = currentPadState;
        }
        private PlayerIndex getIndex()
        {
            switch (type)
            {
                case InputType.PadTwo:
                    return PlayerIndex.Two;
                case InputType.PadThree:
                    return PlayerIndex.Three;
                case InputType.PadFour:
                    return PlayerIndex.Four;
                default:// ChosenInputType.PadOne:
                    return PlayerIndex.One;
            }
        }
        public Boolean wasButtonTapped(Buttons check)
        {
            return (currentPadState.IsButtonDown(check) && !lastPadState.IsButtonDown(check));
        }
        public Boolean wasKeyTapped(Keys check)
        {
            return (currentKeyState.IsKeyDown(check) && !lastKeyState.IsKeyDown(check));
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

        public Boolean actionPressed(InputAction action)
        {
            if (type == InputType.Keyboard || type == InputType.AllConnected)
                return Keyboard.GetState().IsKeyDown(InputLayout.getKey(action));

            return GamePad.GetState(getIndex()).IsButtonDown(InputLayout.getButton(action));
        }
        public Boolean actionTapped(InputAction action)
        {
            if (type == InputType.Keyboard || type == InputType.AllConnected)
                return wasKeyTapped(InputLayout.getKey(action));

            return wasButtonTapped(InputLayout.getButton(action));
        }

        /// <summary>
        /// Gets analogue value where possible for an action.
        /// Keys and On/Off buttons will return 1 if pressed and zero otherwise
        /// Triggers will return their values
        /// Thumbsticks inputs will calculate a positive value in the given direction, or return zero.
        /// </summary>
        /// <param name="action">The action to check</param>
        /// <returns>Returns value between zero and 1 from input relating the given InputAction</returns>
        public float getActionValue(InputAction action)
        {
            return getButtonValue(InputLayout.getButton(action));
        }

        public void Update()
        {
            
            //if (currentPadState.IsConnected)
            lastPadState = currentPadState;
            currentPadState = GamePad.GetState(getIndex());

            if (type == InputType.Keyboard || type == InputType.AllConnected)
            {
                lastKeyState = currentKeyState;
                currentKeyState = Keyboard.GetState();
            }


        }
    }

}
