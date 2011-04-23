using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BeatShift.Input
{
    class CoOpInputManager : IInputManager
    {

        IInputManager rhythmPlayerInput;
        IInputManager drivingPlayerInput;

        public CoOpInputManager(IInputManager rhythmPlayerManager, IInputManager drivingPlayerManager)
        {
            rhythmPlayerInput = rhythmPlayerManager;
            drivingPlayerInput = drivingPlayerManager;
        }

        private IInputManager getInputFromAction(InputAction action)
        {
            switch (action)
            {
                case InputAction.Forwards:
                case InputAction.Backwards:
                case InputAction.Boost:
                case InputAction.Left:
                case InputAction.Right:
                    return drivingPlayerInput;
                default:
                    return rhythmPlayerInput;
            }
        }

        #region IInputManager Interface
        Boolean IInputManager.actionPressed(InputAction action)
        {
            return getInputFromAction(action).actionPressed(action);
        }

        Boolean IInputManager.actionTapped(InputAction action)
        {
            return getInputFromAction(action).actionTapped(action);
        }

        float IInputManager.getActionValue(InputAction action)
        {
            return getInputFromAction(action).getActionValue(action);
        }

        void IInputManager.Update(GameTime gameTime)
        {
            drivingPlayerInput.Update(gameTime);
            rhythmPlayerInput.Update(gameTime);
        }
        #endregion
    }
}

