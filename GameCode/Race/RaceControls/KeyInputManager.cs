using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace BeatShift.Input
{
    class KeyInputManager : IInputManager
    {
        //TODO: fullscreen triggered (F4 pressed)

        private KeyboardState currentKeyState;
        private KeyboardState lastKeyState;

        public KeyInputManager()
        {
            currentKeyState = Keyboard.GetState();
            lastKeyState = currentKeyState;
        }


        #region IInputManager Interface

        Boolean IInputManager.actionPressed(InputAction action)
        {
            return Keyboard.GetState().IsKeyDown(InputLayout.getKey(action));
        }

        Boolean IInputManager.actionTapped(InputAction action)
        {
            return wasKeyTapped(InputLayout.getKey(action));
        }

        float IInputManager.getActionValue(InputAction action)
        {
            if (((IInputManager) this).actionPressed(action))
                return 1f;
            else return 0f;
        }

        void IInputManager.Update(GameTime gameTime)
        {
            lastKeyState = currentKeyState;
            currentKeyState = Keyboard.GetState();
        }

        #endregion


        public Boolean wasKeyTapped(Keys check)
        {
            return (currentKeyState.IsKeyDown(check) && !lastKeyState.IsKeyDown(check));
        }
    }
}
