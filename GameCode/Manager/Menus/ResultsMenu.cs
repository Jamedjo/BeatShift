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
    class ResultsMenu : IMenuPage
    {
        string[] results;
        string[] players;
        bool resultsCalc = false;

        public ResultsMenu()
        {
            title = "Race Results";
        }

        public override void overrideMenuPositions()
        {
            DrawMenuItemsFromTextCentre = true;
            DrawTitleFromTextCentre = true;
            TitlePos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, 75);
            MenuPos = new Vector2(300, 220);
            TitleStartPos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, -100);
            Offset = new Vector2(0, 40);//additional vertical spaceing.
        }

        public override void DrawSprites(SpriteBatch spriteBatch)
        {
            Texture2D background = GameTextures.MenuBackgroundBlack;
            spriteBatch.Draw(background, new Rectangle(0, 0, 1300, background.Height / 2 + 50), Color.White);
            spriteBatch.Draw(background, new Rectangle(0, 0, 600, background.Height / 2 + 50), Color.White);
            if (resultsCalc == true)
            {
                for (int i = 0; i < Race.currentRacers.Count(); i++)
                {
                    int offset = 70;
                    if ( i == 0 )
                        BeatShift.spriteBatch.DrawString(BeatShift.newfont, "1st    " + results[0] + "  " + players[0], new Vector2(500, 150), Color.Goldenrod);
                    else if (i == 1)
                        BeatShift.spriteBatch.DrawString(BeatShift.newfont, "2nd   " + results[1] + "  " + players[1], new Vector2(500, 150 + offset), Color.White);
                    else if (i == 2)
                        BeatShift.spriteBatch.DrawString(BeatShift.newfont, "3rd    " + results[2] + "  " + players[2], new Vector2(500, 150 + offset * 2), Color.SaddleBrown);
                    else if (i == 3)
                        BeatShift.spriteBatch.DrawString(BeatShift.newfont, "4th    " + results[3] + "  " + players[3], new Vector2(500, 150 + offset * 3), Color.Gainsboro);
                    else if (i == 4)
                        BeatShift.spriteBatch.DrawString(BeatShift.newfont, "5th    " + results[3] + "  " + players[4], new Vector2(500, 150 + offset * 4), Color.Gainsboro);
                }
            }
        }

        private void calculateResults()
        {
            results = new string[Race.currentRacers.Count()];

            results = Race.currentRacers.OrderByDescending(r => r.raceTiming.finalRaceTime).Reverse().Select(r => r.raceTiming.getFinalTotalTime()).ToArray();
            players = Race.currentRacers.OrderByDescending(r => r.raceTiming.finalRaceTime).Reverse().Select(r => r.racerID.ToString()).ToArray();
            resultsCalc = true;
        }

        public override void enteringMenu()
        {
            resetMenuSelection();
            base.enteringMenu();
            calculateResults();
        }

        public override void setupMenuItems()
        {
            addMenuItem("Next Race", (Action)(delegate
            {

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
