using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BeatShift.Input;

namespace BeatShift.Menus
{
    class NumberAIMenu : IMenuPage
    {
        public NumberAIMenu()
        {
            title = "SELECT NUMBER OF AI OPPONENTS";
        }

        public override void overrideMenuPositions()
        {
            //DrawTitleFromTextCentre = true;
            DrawMenuItemsFromTextCentre = true;
            //DrawTitleFromTextCentre = true;
            //TitlePos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, 75);
            //MenuPos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, 220);
            //TitleStartPos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, -100);
            ////UseTextWidthAsOffset = true;
            //Offset = new Vector2(0, 40);//additional vertical spaceing.
        }

        public override void setupMenuItems()
        {

            addMenuItem("One", (Action)(delegate
            {
                //Input.AiInputManager.numberOfAI = 1;
                MenuManager.setCurrentMenu(MenuPage.MapSelect);
            }));
            addMenuItem("Two", (Action)(delegate
            {
                //Input.AiInputManager.numberOfAI = 2;
                MenuManager.setCurrentMenu(MenuPage.MapSelect);
            }));
            addMenuItem("Three", (Action)(delegate
            {
                //Input.AiInputManager.numberOfAI = 3;
                MenuManager.setCurrentMenu(MenuPage.MapSelect);
            }));
        }
    }
}
