using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BeatShift.Input;
using Microsoft.Xna.Framework.GamerServices;

namespace BeatShift.Menus
{
    public enum MenuStack { Main, Paused, PostRace }

    static class MenuManager
    {
        //public static Boolean Enabled = true;
        //public static Boolean Visible = true;

        static List<IMenuStack> menuSystems = new List<IMenuStack>();

        public static IInputManager anyInput = new AnyInputManager();

        public static IMenuStack mainMenuSystem = new IMenuStack(MenuPage.Splash,true);
        public static IMenuStack pausedSystem = new IMenuStack(MenuPage.Pause,false);
        public static IMenuStack postRaceSystem = new IMenuStack(MenuPage.Results,false);

        public static void Initialize()
        {
            menuSystems.Add(mainMenuSystem);
            menuSystems.Add(pausedSystem);
            menuSystems.Add(postRaceSystem);

            mainMenuSystem.isActive = true;
        }

        static IMenuStack getMenuStackFromEnum(MenuStack stack)
        {
            switch (stack)
            {
                case MenuStack.Main:
                    return mainMenuSystem;
                case MenuStack.Paused:
                    return pausedSystem;
                case MenuStack.PostRace:
                    return postRaceSystem;
                default:
                    throw new Exception();
                    return null;
            }
        }

        public static void EnableSystem(MenuStack stack)
        {
            DisableAllMenus();
            getMenuStackFromEnum(stack).isActive = true;
            getMenuStackFromEnum(stack).resetToMain();
        }

        public static void DisableAllMenus()
        {
            foreach (IMenuStack m in menuSystems)
            {
                m.isActive = false;
            }
        }

        public static void menuBack()
        {
            foreach (IMenuStack m in menuSystems)
            {
                if (m.isActive)
                    m.menuBack();
            }
        }

        public static void Update(GameTime gameTime)
        {
            //Update input manager state ready for menu page to use
            if (!LiveServices.GuideIsVisible()) anyInput.Update(gameTime);

            //Update menu stacks
            foreach (IMenuStack m in menuSystems)
            {
                if (m.isActive)
                    m.Update(gameTime);
            }

        }

        public static void Draw(GameTime gameTime){
            foreach (IMenuStack m in menuSystems)
            {
                if(m.isActive)
                    m.Draw(gameTime);
            }
        }

    }

}
