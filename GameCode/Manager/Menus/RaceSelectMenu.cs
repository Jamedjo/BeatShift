using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BeatShift.Menus
{
    class RaceSelectMenu : IMenuPage
    {
        public RaceSelectMenu()
        {
            title = "Select Race Type";
        }

        public override void overrideMenuPositions()
        {
            DrawMenuItemsFromTextCentre = true;
            DrawTitleFromTextCentre = true;
            TitlePos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, 75);
            MenuPos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, 220);
            TitleStartPos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, -100);
            Offset = new Vector2(0, 40);//additional vertical spaceing.
        }

        public override void setupMenuItems()
        {
            addMenuItem("LAPPED RACE", (Action)(delegate
            {
                Race.currentRaceType = new LappedRace(3);
                Race.resetPlayers();
                MenuManager.setCurrentMenu(MenuPage.MapSelect);
            }));
            addMenuItem("TIME TRIAL", (Action)(delegate
            {
                Race.currentRaceType = new TimeTrialRace();
                Race.resetPlayers();
                MenuManager.setCurrentMenu(MenuPage.MapSelect);
            }));
            addMenuItem("ELIMINATION", (Action)(delegate
            {
                Race.currentRaceType = new EliminationRace(10); //TODO: define how many racers we want in menu
                Race.resetPlayers();
                MenuManager.setCurrentMenu(MenuPage.MapSelect);
            }));
            addMenuItem("POINTS FRENZY", (Action)(delegate
            {
                Race.currentRaceType = new BeatPointsRace(new TimeSpan(0,3,30)); //TODO: define length of race in menu
                Race.resetPlayers();
                MenuManager.setCurrentMenu(MenuPage.MapSelect);
            }));

        }

    }
}