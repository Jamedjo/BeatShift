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
            title = "Select Map";
        }

        public override void overrideMenuPositions()
        {
            DrawMenuItemsFromTextCentre = true;
            DrawTitleFromTextCentre = true;
            TitlePos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, 75);
            MenuPos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, 220);
            TitleStartPos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, -100);
            //UseTextWidthAsOffset = true;
            Offset = new Vector2(0, 40);//additional vertical spaceing.
        }

        public override void setupMenuItems()
        {

            addMenuItem("SPACE MAP", (Action)(delegate
            {
                MapManager.mapToLoad = MapName.SpaceMap;

                //if (isSinglePlayer()) 
                //    MenuManager.setCurrentMenu(MenuPage.SinglePlayerShipSelect);
                //else
                    GameLoop.setGameState(GameState.MultiplayerShipSelect);
            }));
            addMenuItem("CITY MAP", (Action)(delegate
            {
                MapManager.mapToLoad = MapName.CityMap;
                GameLoop.setGameState(GameState.MultiplayerShipSelect);
            }));
            addMenuItem("DESERT MAP", (Action)(delegate
            {
                //MapManager.mapToLoad = MapName.DesertMap;

                //if (isSinglePlayer()) MenuManager.setCurrentMenu(MenuPage.SinglePlayerShipSelect);
                //else GameLoop.setGameState(GameState.MultiplayerShipSelect);
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
