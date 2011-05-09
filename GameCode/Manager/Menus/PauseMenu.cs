﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BeatShift.Input;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;

namespace BeatShift.Menus
{
    class PauseMenu : IMenuPage
    {
        public PauseMenu()
        {
            title = "Paused";
        }

        public override void DrawSprites(SpriteBatch spriteBatch)
        {
            Texture2D background = GameTextures.CountdownReady;
            Texture2D backgroundBlack = GameTextures.MenuBackgroundBlack;
            spriteBatch.Draw(GameTextures.MenuBackgroundBlack, new Rectangle(0, 0, BeatShift.graphics.GraphicsDevice.Viewport.Width, BeatShift.graphics.GraphicsDevice.Viewport.Height), Color.White);
            spriteBatch.Draw(background, new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - background.Width / 2, BeatShift.graphics.GraphicsDevice.Viewport.Height / 2 - background.Height / 2), Color.White);

        }

        public override void enteringMenu()
        {
            resetMenuSelection();
            base.enteringMenu();
        }

        public override void setupMenuItems()
        {
            addMenuItem("Resume", (Action)(delegate
            {
                GameLoop.EndPause();
            }));
            addMenuItem("Main Menu", (Action)(delegate
            {
                GameLoop.setGameStateAndResetPlayers(GameState.Menu);
            }));
            addMenuItem("Exit", (Action)(delegate
            {
                BeatShift.singleton.Exit();
            }));

        }

    }
}
