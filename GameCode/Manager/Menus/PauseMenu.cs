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
    class PauseMenu : IMenuPage
    {
        public PauseMenu()
        {
            title = "GAME PAUSED";
        }

        public override void DrawSprites(SpriteBatch spriteBatch)
        {
            //Texture2D background = GameTextures.CountdownReady;
            //Texture2D backgroundBlack = GameTextures.MenuBackgroundBlack;
            //spriteBatch.Draw(GameTextures.MenuBackgroundBlack, new Rectangle(0, 0, BeatShift.graphics.GraphicsDevice.Viewport.Width, BeatShift.graphics.GraphicsDevice.Viewport.Height), Color.White);
            //spriteBatch.Draw(background, new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - background.Width / 2, BeatShift.graphics.GraphicsDevice.Viewport.Height / 2 - background.Height / 2), Color.White);
            spriteBatch.Draw(GameTextures.MenuBackgroundBlackRed, new Rectangle(0, 0, BeatShift.graphics.GraphicsDevice.Viewport.Width, BeatShift.graphics.GraphicsDevice.Viewport.Height), slightlyTransparent);
        }

        public override void enteringMenu()
        {
            MenuManager.anyInput.Update(BeatShift.singleton.currentTime);
            resetMenuSelection();
            base.enteringMenu();
        }

        public override void setupMenuItems()
        {
            addMenuItem("RESUME", (Action)(delegate
            {
                GameLoop.EndPause();
                MenuManager.DisableAllMenus();
            }));
            addMenuItem("OPTIONS", (Action)(delegate
            {
                MenuManager.pausedSystem.setCurrentMenu(MenuPage.Options);
            }));
            addMenuItem("MAIN MENU", (Action)(delegate
            {
                ((ConfirmationMenu)IMenuStack.Confim).initYesNo("WARNING: Finish race and return to the menus?", "Yes", "No",
                    (Action)(delegate
                    {
                        GameLoop.setGameStateAndResetPlayers(GameState.Menu);
                    }),
                    (Action)(delegate
                    {
                        MenuManager.menuBack();
                    }));
                MenuManager.pausedSystem.setCurrentMenu(MenuPage.Confimation);
            }));

        }

        public override void respondToMenuBack()
        {
            GameLoop.EndPause();
            MenuManager.DisableAllMenus();
        }

        public override void overrideMenuPositions()
        {
            //Set title position
            //DrawTitleFromTextCentre = true;
            DrawMenuItemsFromTextCentre = true;
            //TitlePos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - 265, 170);
            //TitleStartPos = new Vector2((BeatShift.graphics.GraphicsDevice.Viewport.Width / 2) +50, 170);

            //Set position of menu items
            //MenuPos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, 220);
            //Offset = new Vector2(0, 80);//additional vertical spaceing.

        }

    }
}
