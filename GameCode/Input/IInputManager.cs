using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BeatShift.Input
{
    public interface IInputManager
    {
        /// <summary>
        /// Determines if the key/button relating to action is currently pressed in.
        /// </summary>
        /// <param name="action">The action to use to look up key/button to check</param>
        /// <returns>True if that button is currently pressed in. If analogue then true if over a threshold.</returns>
        Boolean actionPressed(InputAction action);

        /// <summary>
        /// Determines if the key/button has been tapped.
        /// </summary>
        /// <param name="action">The action which is being checked for a change of state.</param>
        /// <returns>Returns true if the button/key was just released after earlier being pressed</returns>
        Boolean actionTapped(InputAction action);

        /// <summary>
        /// Gets analogue value where possible for an action.
        /// Keys and On/Off buttons will return 1 if pressed and zero otherwise
        /// Triggers will return their values
        /// Thumbsticks inputs will calculate a positive value in the given direction, or return zero.
        /// </summary>
        /// <param name="action">The action to check</param>
        /// <returns>Returns value between zero and 1 from input relating the given InputAction</returns>
        float getActionValue(InputAction action);

        /// <summary>
        /// Updates relevent input states.
        /// </summary>
        void Update(GameTime gameTime);
    }
}
