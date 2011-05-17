using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BeatShift.Input;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;

namespace BeatShift.Menus
{
    class HighScoreMenu : IMenuPage
    {
        public HighScoreMenu()
        {
            title = "HIGH SCORES";
        }

        public override void DrawSprites(SpriteBatch spriteBatch)
        {
            //Texture2D background = GameTextures.CountdownReady;
            //Texture2D backgroundBlack = GameTextures.MenuBackgroundBlack;
            //spriteBatch.Draw(GameTextures.MenuBackgroundBlack, new Rectangle(0, 0, BeatShift.graphics.GraphicsDevice.Viewport.Width, BeatShift.graphics.GraphicsDevice.Viewport.Height), Color.White);
            //spriteBatch.Draw(background, new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - background.Width / 2, BeatShift.graphics.GraphicsDevice.Viewport.Height / 2 - background.Height / 2), Color.White);
            spriteBatch.Draw(GameTextures.MenuBackgroundBlackRed, new Rectangle(0, 0, BeatShift.graphics.GraphicsDevice.Viewport.Width, BeatShift.graphics.GraphicsDevice.Viewport.Height), slightlyTransparent);
            BeatShift.spriteBatch.DrawString(BeatShift.newfont, MapManager.currentMap.currentMapName + " FASTEST LAPS", new Vector2(500, 150), Color.Aqua);
        }

        public override void enteringMenu()
        {
            resetMenuSelection();
            base.enteringMenu();
        }

        public override void setupMenuItems()
        {
            addMenuItem("CONTINUE", (Action)(delegate
            {
                MenuManager.setCurrentMenu(MenuPage.Main);
                GameLoop.setGameStateAndResetPlayers(GameState.Menu);
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
