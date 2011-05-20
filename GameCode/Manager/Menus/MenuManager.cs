using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BeatShift.Input;

namespace BeatShift.Menus
{
    /// <summary>
    /// The different pages/screens the menu can be on. 
    /// </summary>
    public enum MenuPage { Splash, Main, Options, MapSelect, Multiplayer, SinglePlayerShipSelect, RaceSelect, Loading, HighScore, FinishedLoading}

    static class MenuManager
    {
        public static Boolean Enabled = true;
        public static Boolean Visible = true;

        public static Stack<MenuPage> MenuTrail = new Stack<MenuPage>();

        static IMenuPage currentPage;
        static IMenuPage Splash;
        static IMenuPage Main;
        static IMenuPage Options;
        static IMenuPage MapSelect;
        static IMenuPage Multiplayer;
        static IMenuPage SinglePlayerShipSelect;
        static IMenuPage RaceSelect;
        static IMenuPage Loading;
        static IMenuPage HighScore;
        static IMenuPage FinishedLoading;

        public static IInputManager anyInput = new AnyInputManager();

        public static void Initialize()
        {
            Main = new MainMenu();
            Options = new OptionsMenu();
            MapSelect = new MapSelectMenu();
            Multiplayer = new MultiplayerMenu();
            SinglePlayerShipSelect = new SinglePlayerShipSelect();
            RaceSelect = new RaceSelectMenu();
            Splash = new SplashMenu();
            Loading = new LoadingMenu();
            HighScore = new HighScoreMenu();
            FinishedLoading = new FinishedLoadingMenu();

            //currentPage = Main;
            currentPage = Splash;
            currentPage.enteringMenu();
        }

        static void setCurrentPage(IMenuPage page)
        {
            if (page == currentPage) return; //No change so do nothing
            currentPage.leavingMenu();
            currentPage = page;
            page.enteringMenu();
        }

        /// <summary>
        /// Checks the trail for the last menu screen 
        /// </summary>
        public static void menuBack()
        {
            if (MenuTrail.Peek() == MenuPage.Main)
                return;
            else
            {
                MenuTrail.Pop();
                MenuPage temp = MenuTrail.Pop();
                if (temp == MenuPage.Main)
                    MenuTrail.Clear();
                setCurrentMenu(temp);
            }
        }

        /// <summary>
        /// Sets the screen/page the menu is currently on and highlights the first item in that menu.
        /// Triggers leavingMenu() and enteringMenu() actions.
        /// </summary>
        /// <param name="page">The page to move to.</param>
        public static void setCurrentMenu(MenuPage page)
        {
            MenuTrail.Push(page);
            switch (page)
            {
                case MenuPage.Main:
                    setCurrentPage(Main);
                    break;
                case MenuPage.MapSelect:
                    setCurrentPage(MapSelect);
                    break;
                case MenuPage.Multiplayer:
                    setCurrentPage(Multiplayer);
                    break;
                case MenuPage.Options:
                    setCurrentPage(Options);
                    break;
                case MenuPage.SinglePlayerShipSelect:
                    setCurrentPage(SinglePlayerShipSelect);
                    break;
                case MenuPage.RaceSelect:
                    setCurrentPage(RaceSelect);
                    break;
                case MenuPage.Splash:
                    setCurrentPage(Splash);
                    break;
                case MenuPage.Loading:
                    setCurrentPage(Loading);
                    GameLoop.StopTitle();
                    break;
                case MenuPage.HighScore:
                    setCurrentPage(HighScore);
                    break;
                case MenuPage.FinishedLoading:
                    setCurrentPage(FinishedLoading);
                    break;
                default:
                    throw new Exception();
            }
        }

        public static void Update(GameTime gameTime)
        {
            //Update input manager state ready for menu page to use
            anyInput.Update(gameTime);
            
            //Update current page text values, inputs, and any custom updates
            currentPage.Update(gameTime);
        }

        public static void Draw(GameTime gameTime){
            if (!currentPage.ToString().Equals("PauseMenu") || !currentPage.ToString().Equals("ResultsMenu") )
            {
                Rectangle viewArea = new Rectangle(0, 0, BeatShift.graphics.GraphicsDevice.Viewport.Width, BeatShift.graphics.GraphicsDevice.Viewport.Height);
                BeatShift.spriteBatch.Begin();
                BeatShift.spriteBatch.Draw(GameTextures.MenuBackgroundBlue, viewArea, Color.White);
                BeatShift.spriteBatch.End();
            }
            //draw current page
            currentPage.Draw();
        }


        public static void resetToMain()
        {
            MenuManager.MenuTrail.Clear();
            MenuManager.MenuTrail.Push(MenuPage.Main);
            currentPage = Main;
        }
    }

}
