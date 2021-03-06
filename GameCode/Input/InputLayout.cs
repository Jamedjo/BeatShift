﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace BeatShift.Input
{
    public enum InputAction { Forwards, Backwards, Left, Right, Boost, Green, Red, Blue, Yellow, MenuAccept, MenuBack, MenuUp, MenuDown, MenuLeft, MenuRight, Start, BackButton, PadUp, PadDown, PadLeft, PadRight, CameraToggle, CameraReverse }
    //Could have 'duplicates' and then assign same array values. e.g. keys[brake]=keys[backwards]
    //MenuAccept Could be keys.Enter and Buttons.A
    //Combos can use WASD directions instead of ABXY

    //TODO: make ControllerLayout class which extends InputLayout by 
    //      using :InputLayout and :base() and implement it.

    public static class InputLayout
    {
        //Arrays to hold keys and values.
        //'?' indicates that the values are Nullable, and that null is the default element.
        static Keys?[] keys;
        static Buttons?[] buttons;

        //Extending this class with different keys/buttons will create custom controller layouts.
        static InputLayout()
        {
            //Setup arrays to hold keys and values. Assign null values so can check if set.
            keys = new Keys?[Utils.GetValues<InputAction>().Count()];
            buttons = new Buttons?[Utils.GetValues<InputAction>().Count()];

            setupAction(InputAction.Forwards, Keys.Up, Buttons.RightTrigger);
            setupAction(InputAction.Backwards, Keys.Down, Buttons.LeftTrigger);
            setupAction(InputAction.Left, Keys.Left, Buttons.LeftThumbstickLeft);
            setupAction(InputAction.Right, Keys.Right, Buttons.LeftThumbstickRight);
            setupAction(InputAction.Boost, Keys.Space, Buttons.RightShoulder);
            setupAction(InputAction.Green, Keys.S, Buttons.A);
            setupAction(InputAction.Red, Keys.D, Buttons.B);
            setupAction(InputAction.Blue, Keys.A, Buttons.X);
            setupAction(InputAction.Yellow, Keys.W, Buttons.Y);
            setupAction(InputAction.MenuUp, Keys.Up, Buttons.LeftThumbstickUp);
            setupAction(InputAction.MenuDown, Keys.Down, Buttons.LeftThumbstickDown);
            setupAction(InputAction.MenuLeft, Keys.Left, Buttons.RightThumbstickLeft);
            setupAction(InputAction.MenuRight, Keys.Right, Buttons.RightThumbstickRight);
            setupAction(InputAction.MenuBack, Keys.Escape, Buttons.B);
            setupAction(InputAction.MenuAccept, Keys.Enter, Buttons.A);
            setupAction(InputAction.Start, Keys.LeftShift, Buttons.Start);
            setupAction(InputAction.BackButton, Keys.Escape, Buttons.Back);
            setupAction(InputAction.PadUp, Keys.NumPad8, Buttons.DPadUp);
            setupAction(InputAction.PadDown, Keys.NumPad2, Buttons.DPadDown);
            setupAction(InputAction.PadLeft, Keys.NumPad4, Buttons.DPadLeft);
            setupAction(InputAction.PadRight, Keys.NumPad6, Buttons.DPadRight);
            setupAction(InputAction.CameraToggle, Keys.LeftControl, Buttons.LeftShoulder);
            setupAction(InputAction.CameraReverse, Keys.LeftAlt, Buttons.RightStick);

            //throw error if any of arrays are empty as this means action not set up
            for (int i = 0; i < Utils.GetValues<InputAction>().Count(); i++)
            {
                if (keys[i] == null || buttons[i] == null)
                    throw new Exception("InputManager has not had all InputActions set");
            }
        }
        static void setupAction(InputAction action, Keys key, Buttons button)
        {
            keys[(int)action] = key;
            buttons[(int)action] = button;
        }
        public static Keys getKey(InputAction action)
        {
            return (Keys)keys[(int)action];
        }
        public static Buttons getButton(InputAction action)
        {
            return (Buttons)buttons[(int)action];
        }
    }
}
