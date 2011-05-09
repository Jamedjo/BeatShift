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
        public ResultsMenu()
        {
            title = "Race Results";
        }

        public override void DrawSprites(SpriteBatch spriteBatch)
        {
            Texture2D background = GameTextures.CountdownReady;
            spriteBatch.Draw(background, new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - background.Width / 2, BeatShift.graphics.GraphicsDevice.Viewport.Height / 2 - background.Height / 2), Color.White);
        }

        private void calculateResults()
        {
            string[] results = new string[Race.currentRacers.Count()];
            foreach (Racer racer in Race.currentRacers)
                results[racer.raceTiming.racePosition] = racer.raceTiming.getFinalTotalTime();
            for (int i = 0; i < Race.currentRacers.Count(); i++)
                Console.WriteLine(results[i]);
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
