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
            title = "BeatShift";
        }

        public override void setupMenuItems()
        {
            addMenuItem("Start Singleplayer", (Action)(delegate{
                MenuManager.setCurrentMenu(MenuPage.RaceSelect);
#if XBOX
                Race.getFullListOfRacerIDsFromSignedInPeople();
                if (Race.racerIDs != null && Race.racerIDs.Count() == 0)
                    Guide.ShowSignIn(1, false);
#endif
            }));
            addMenuItem("Start Co-Op Race", (Action)(delegate
            {
                //GameLoop.setGameStateAndResetPlayers(GameState.LocalGame); //TODO: this sucks
                //Race.humanRacers[0].racingControls.chosenInput = new CoOpInputManager(new PadInputManager(PlayerIndex.One), new PadInputManager(PlayerIndex.Two));
                //Race.humanRacers[0].constructRandomShip(324);
                //BeatShift.bgm.play();
            }));
            addMenuItem("Start Multiplayer", (Action)(delegate
            {
                MenuManager.setCurrentMenu(MenuPage.Multiplayer);
            }));
            addMenuItem("Options", (Action)(delegate
            {
                MenuManager.setCurrentMenu(MenuPage.Options);
            }));
            addMenuItem("Exit", (Action)(delegate
            {
                BeatShift.singleton.Exit();
            }));

        }

    }
}
