using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BeatShift.Menus
{
    class MapSelectMenu : IMenuPage
    {
        public MapSelectMenu()
        {
            title = "SELECT MAP";
        }

        public override void overrideMenuPositions()
        {
            //DrawTitleFromTextCentre = true;
            DrawMenuItemsFromTextCentre = true;
            //DrawMenuItemsFromTextCentre = true;
            //DrawTitleFromTextCentre = true;
            //TitlePos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, 75);
            //MenuPos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, 220);
            //TitleStartPos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, -100);
            //UseTextWidthAsOffset = true;
            //Offset = new Vector2(0, 40);//additional vertical spaceing.
        }

        public override void setupMenuItems()
        {

            addMenuItem("CITY MAP", (Action)(delegate
            {
                MapManager.mapToLoad = MapName.CityMap;
                SoundManager.trackToLoad = "City";
                GameLoop.setGameState(GameState.MultiplayerShipSelect);
            }));
            addMenuItem("DESERT MAP", (Action)(delegate
            {
                MapManager.mapToLoad = MapName.DesertMap;
                SoundManager.trackToLoad = "Desert";
                GameLoop.setGameState(GameState.MultiplayerShipSelect);
                //if (isSinglePlayer()) MenuManager.setCurrentMenu(MenuPage.SinglePlayerShipSelect);
                //else GameLoop.setGameState(GameState.MultiplayerShipSelect);
            }));
            addMenuItem("SPACE MAP", (Action)(delegate
            {
                MapManager.mapToLoad = MapName.SpaceMap;
                SoundManager.trackToLoad = "Space";
                //if (isSinglePlayer()) 
                //    MenuManager.setCurrentMenu(MenuPage.SinglePlayerShipSelect);
                //else
                GameLoop.setGameState(GameState.MultiplayerShipSelect);
            }));
        }

        //public bool isSinglePlayer()
        //{
        //    if(MenuManager.MenuTrail.Contains(MenuPage.Multiplayer)) return false;
        //    else return true;
        //    //if (Race.isSinglePlayer) return true;
        //    //else if (Race.isMultiplayer) return false;
        //    //else throw new Exception("Exception, moving between menus without single/multiplayer defined");
        //}
    }
}
