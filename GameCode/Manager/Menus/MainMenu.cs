using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BeatShift.Input;
using Microsoft.Xna.Framework.GamerServices;

namespace BeatShift.Menus
{
    class MainMenu : IMenuPage
    {
        public MainMenu()
        {
            title = "START";
        }

        public override void setupMenuItems()
        {
            addMenuItem("RACE", (Action)(delegate{
                MenuManager.setCurrentMenu(MenuPage.RaceSelect);
            }));
            //addMenuItem("START CO-OP RACE", (Action)(delegate
            //{
                //GameLoop.setGameStateAndResetPlayers(GameState.LocalGame); //TODO: this sucks
                //Race.humanRacers[0].racingControls.chosenInput = new CoOpInputManager(new PadInputManager(PlayerIndex.One), new PadInputManager(PlayerIndex.Two));
                //Race.humanRacers[0].constructRandomShip(324);
                //BeatShift.bgm.play();
            //}));
            //addMenuItem("START MULTIPLAYER", (Action)(delegate
            //{
            //    MenuManager.setCurrentMenu(MenuPage.Multiplayer);
            //}));
            addMenuItem("OPTIONS", (Action)(delegate
            {
                MenuManager.setCurrentMenu(MenuPage.Options);
            }));
            addMenuItem("EXIT", (Action)(delegate
            {
                BeatShift.singleton.Exit();
            }));

        }

        public override void overrideMenuPositions()
        {
            //Set title position
            //DrawTitleFromTextCentre = true;
            DrawMenuItemsFromTextCentre = true;
            //TitlePos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, 75);
            //TitleStartPos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, -100);

            //Set position of menu items
            //MenuPos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, 220);
            //Offset = new Vector2(0, 80);//additional vertical spaceing.

        }


    }
}
