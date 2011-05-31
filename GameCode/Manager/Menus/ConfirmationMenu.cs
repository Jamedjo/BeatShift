using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BeatShift.Util;

namespace BeatShift.Menus
{
    class ConfirmationMenu : IMenuPage
    {
        public bool debugOptions = true;

        public ConfirmationMenu()
        {
            title = "";
        }

        public void initYesNo(String newTitle, String yes, String no, Action yesAction, Action noAction)
        {
            title = newTitle;

            //Setup menu items
            menuItems = new List<MenuItem>();
            addMenuItem(yes, yesAction);
            addMenuItem(no, noAction);
        }

        public override void overrideMenuPositions()
        {
            DrawMenuItemsFromTextCentre = true;
            //DrawTitleFromTextCentre = true;
            //TitlePos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, 75);
            MenuPos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, 250);
            //TitleStartPos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, -100);
            //UseTextWidthAsOffset = true;
            Offset = new Vector2(0, 35);//additional vertical spaceing.
            TextScale = 0.5f;
        }

        public override void setupMenuItems()
        {
        }
    }
}
