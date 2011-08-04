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

        public IInputManager getNewIInputManager(ControllerType type)
        {
            switch (type)
            {
                case ControllerType.PadOne:
                    return new PadInputManager(PlayerIndex.One);
                case ControllerType.PadTwo:
                    return new PadInputManager(PlayerIndex.Two);
                case ControllerType.PadThree:
                    return new PadInputManager(PlayerIndex.Three);
                case ControllerType.PadFour:
                    return new PadInputManager(PlayerIndex.Four);

                case ControllerType.Keyboard:
                default:
                    return new KeyInputManager();
            }
        }

        public IInputManager useLastInputTappedToCreateNewIInputManager()
        {
            return getNewIInputManager(lastControllerTapped);
        }
    }

    enum ControllerType { PadOne, PadTwo, PadThree, PadFour, Keyboard }
}
