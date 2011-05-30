using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BeatShift.Menus
{
    /// <summary>
    /// The different pages/screens the menu can be on. 
    /// </summary>
    public enum MenuPage { Splash, Main, Options, MapSelect, Multiplayer, SinglePlayerShipSelect, RaceSelect, Loading, HighScore, FinishedLoading, Pause, Results }

    class IMenuStack
    {
        public Stack<MenuPage> MenuTrail = new Stack<MenuPage>();

        private MenuPage rootMenu;//furthest menu to go back to, e.g. Splash

        public Boolean isActive = false;

        IMenuPage currentPage;

        public Boolean DrawBackground = true;

        //Statics for menus as they are universal to all menuSystems/MenuStacks.
        static IMenuPage Splash = new SplashMenu();
        static IMenuPage Main = new MainMenu();
        static IMenuPage Options = new OptionsMenu();
        static IMenuPage MapSelect = new MapSelectMenu();
        static IMenuPage Multiplayer = new MultiplayerMenu();
        static IMenuPage SinglePlayerShipSelect = new SinglePlayerShipSelect();
        static IMenuPage RaceSelect = new RaceSelectMenu();
        static IMenuPage Loading = new LoadingMenu();
        static IMenuPage HighScore = new HighScoreMenu();
        static IMenuPage FinishedLoading = new FinishedLoadingMenu();
        static IMenuPage Pause = new PauseMenu();
        public static IMenuPage Results = new ResultsMenu();

        public IMenuStack(MenuPage rootPage, Boolean drawBackground)
        {
            rootMenu = rootPage;
            currentPage = getIMenuPageFromEnum(rootMenu);
            DrawBackground = drawBackground;
        }


        /// <summary>
        /// Sets the screen/page the menu is currently on and highlights the first item in that menu.
        /// Triggers leavingMenu() and enteringMenu() actions.
        /// </summary>
        /// <param name="page">The page to move to.</param>
        public void setCurrentMenu(MenuPage page)
        {
            MenuTrail.Push(page);
            setCurrentPage(getIMenuPageFromEnum(page));
        }

        IMenuPage getIMenuPageFromEnum(MenuPage page)
        {
            switch (page)
            {
                case MenuPage.Main:
                    return Main;
                case MenuPage.MapSelect:
                    return MapSelect;
                case MenuPage.Multiplayer:
                    return Multiplayer;
                case MenuPage.Options:
                    return Options;
                case MenuPage.SinglePlayerShipSelect:
                    return SinglePlayerShipSelect;
                case MenuPage.RaceSelect:
                    return RaceSelect;
                case MenuPage.Splash:
                    return Splash;
                case MenuPage.Loading:
                    GameLoop.StopTitle();
                    return Loading;
                case MenuPage.HighScore:
                    return HighScore;
                case MenuPage.FinishedLoading:
                    return FinishedLoading;
                case MenuPage.Pause:
                    return Pause;
                case MenuPage.Results:
                    return Results;
                default:
                    throw new Exception();
                    return null;
            }
        }

        void setCurrentPage(IMenuPage page)
        {
            if (page == currentPage) return; //No change so do nothing
            currentPage.leavingMenu();
            currentPage = page;
            page.enteringMenu();
        }

        /// <summary>
        /// Checks the trail for the last menu screen 
        /// </summary>
        public void menuBack()
        {
            if (MenuTrail.Peek() == rootMenu)
                return;
            else
            {
                MenuTrail.Pop();
                MenuPage temp = MenuTrail.Pop();
                if (temp == rootMenu)
                    MenuTrail.Clear();
                setCurrentMenu(temp);
            }
        }

         
        public void Update(GameTime gameTime)
        {   
            //Update current page text values, inputs, and any custom updates
            currentPage.Update(gameTime);
        }

        public void Draw(GameTime gameTime){
            //draw current page

            BeatShift.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            BeatShift.graphics.GraphicsDevice.Viewport = Viewports.fullViewport;
            Rectangle viewArea = new Rectangle(0, 0, BeatShift.graphics.GraphicsDevice.Viewport.Width, BeatShift.graphics.GraphicsDevice.Viewport.Height);
            BeatShift.spriteBatch.Begin();

            //Draw background if DrawBackground boolean not overridden.
            if (DrawBackground)
            {
                BeatShift.spriteBatch.Draw(GameTextures.MenuBackgroundBlue, viewArea, Color.White);
            }

            currentPage.Draw(viewArea);

            BeatShift.spriteBatch.End();

            currentPage.otherDraw();
        }

        public void resetToMain()
        {
            MenuTrail.Clear();
            MenuTrail.Push(rootMenu);
            currentPage = getIMenuPageFromEnum(rootMenu);
            currentPage.enteringMenu();
        }
    }
}
