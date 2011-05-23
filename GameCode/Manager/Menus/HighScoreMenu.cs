using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BeatShift.Input;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using BeatShift.Util;

namespace BeatShift.Menus
{
    class HighScoreMenu : IMenuPage
    {
        private List<HighScoreEntry> highScores;
        public HighScoreMenu()
        {
            title = "";
        }

        public override void DrawSprites(SpriteBatch spriteBatch)
        {
            //Texture2D background = GameTextures.CountdownReady;
            //Texture2D backgroundBlack = GameTextures.MenuBackgroundBlack;
            //spriteBatch.Draw(GameTextures.MenuBackgroundBlack, new Rectangle(0, 0, BeatShift.graphics.GraphicsDevice.Viewport.Width, BeatShift.graphics.GraphicsDevice.Viewport.Height), Color.White);
            //spriteBatch.Draw(background, new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - background.Width / 2, BeatShift.graphics.GraphicsDevice.Viewport.Height / 2 - background.Height / 2), Color.White);
            spriteBatch.Draw(GameTextures.MenuBackgroundBlackRed, new Rectangle(0, 0, BeatShift.graphics.GraphicsDevice.Viewport.Width, BeatShift.graphics.GraphicsDevice.Viewport.Height), slightlyTransparent);
            BeatShift.spriteBatch.DrawString(BeatShift.newfont, MapManager.currentMap.currentMapName + " fastest laps", new Vector2(370, 150), Color.Aqua);
            int offset = 0;
            for (int i = 0; i < highScores.Count; i++)
            {
                long timeLong = highScores[i].value * 10;
                DateTime time = new DateTime(Math.Min(timeLong,DateTime.MaxValue.Ticks));
                DrawMessageColour(BeatShift.newfont, i + ". " + highScores[i].name + " : " + time.Minute + ":"+time.Second+":"+time.Millisecond, 360, 230 + offset, 0.7f, Color.PapayaWhip);
                offset = offset + 40;
            }
        }

        public override void enteringMenu()
        {
            resetMenuSelection();
            highScores = HighScore.getHighScores(MapManager.currentMap.currentMapName, 0);
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

            MenuPos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width /2, BeatShift.graphics.GraphicsDevice.Viewport.Height - 50);
            //Set position of menu items
            //MenuPos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, 220);
            //Offset = new Vector2(0, 80);//additional vertical spaceing.

        }

    }
}
