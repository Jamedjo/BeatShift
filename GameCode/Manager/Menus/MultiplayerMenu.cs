using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BeatShift.Menus
{
    class MultiplayerMenu : IMenuPage
    {
        public MultiplayerMenu()
        {
            title = "Multiplayer";
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
            addMenuItem("SPLIT SCREEN" , (Action)(delegate
            {
                MenuManager.setCurrentMenu(MenuPage.RaceSelect);
            }));
            addMenuItem("CREATE LIVE SESSION", (Action)(delegate
            {
                //BeatShift.networkedGame.startSession(SessionType.Create);
                //GameLoop.setGameStateAndResetPlayers(GameState.NetworkedGame);
            }));
            addMenuItem("JOIN LIVE SESSION", (Action)(delegate
            {
                //BeatShift.networkedGame.startSession(SessionType.Join);
                //GameLoop.setGameStateAndResetPlayers(GameState.NetworkedGame);
            }));

        }

    }
}
