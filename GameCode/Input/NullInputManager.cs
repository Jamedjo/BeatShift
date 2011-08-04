using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BeatShift.Input
{
    class NullInputManager : IInputManager
    {
        Boolean IInputManager.actionPressed(InputAction action)
        {
            return false;
        }

        Boolean IInputManager.actionTapped(InputAction action)
        {
            return false;
        }

        float IInputManager.getActionValue(InputAction action)
        {
            return 0f;
        }

        void IInputManager.Update(GameTime gameTime)
        {

        }
    }
}
