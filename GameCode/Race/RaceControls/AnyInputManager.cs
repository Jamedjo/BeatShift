using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BeatShift.Input
{
    class AnyInputManager : IInputManager
    {
        IInputManager keyInput = new KeyInputManager();
        IInputManager padOneInput = new PadInputManager(PlayerIndex.One);
        IInputManager padTwoInput = new PadInputManager(PlayerIndex.Two);
        IInputManager padThreeInput = new PadInputManager(PlayerIndex.Three);
        IInputManager padFourInput = new PadInputManager(PlayerIndex.Four);

        ControllerType lastControllerTapped = ControllerType.PadOne;

        //TODO: //KonamiCode track //Track input state during update for a sequence of presses, also used for combo distance.

        #region IInputManager Interface

        Boolean IInputManager.actionPressed(InputAction action)
        {
            return keyInput.actionPressed(action) || padOneInput.actionPressed(action) || padTwoInput.actionPressed(action) || padThreeInput.actionPressed(action) || padFourInput.actionPressed(action);
        }
        Boolean IInputManager.actionTapped(InputAction action)
        {
            if(keyInput.actionTapped(action))
            {
                lastControllerTapped = ControllerType.Keyboard;
                return true;
            }
            if(padOneInput.actionTapped(action))
            {
                lastControllerTapped = ControllerType.PadOne;
                return true;
            }
            if(padTwoInput.actionTapped(action))
            {
                lastControllerTapped = ControllerType.PadTwo;
                return true;
            }
            if(padThreeInput.actionTapped(action))
            {
                lastControllerTapped = ControllerType.PadThree;
                return true;
            }
            if(padFourInput.actionTapped(action))
            {
                lastControllerTapped = ControllerType.PadFour;
                return true;
            }
            return false;
        }

        float IInputManager.getActionValue(InputAction action)
        {
            return Math.Max(Math.Max(Math.Max(Math.Max(keyInput.getActionValue(action), padOneInput.getActionValue(action)), padTwoInput.getActionValue(action)), padThreeInput.getActionValue(action)), padFourInput.getActionValue(action));
        }

        void IInputManager.Update(GameTime gameTime)
        {
            keyInput.Update(gameTime);
            padOneInput.Update(gameTime);
            padTwoInput.Update(gameTime);
            padThreeInput.Update(gameTime);
            padFourInput.Update(gameTime);
        }

        #endregion

        public IInputManager getIInputManager(ControllerType type)
        {
            switch (type)
            {
                case ControllerType.PadOne:
                    return padOneInput;
                case ControllerType.PadTwo:
                    return padTwoInput;
                case ControllerType.PadThree:
                    return padThreeInput;
                case ControllerType.PadFour:
                    return padFourInput;
                case ControllerType.Keyboard:
                default:
                    return keyInput;
            }
        }

        public IInputManager getLastInputTapped()
        {
            return getIInputManager(lastControllerTapped);
        }
    }

    enum ControllerType { PadOne, PadTwo, PadThree, PadFour, Keyboard }
}
