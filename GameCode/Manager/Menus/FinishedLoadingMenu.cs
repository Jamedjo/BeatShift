using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BeatShift.Input;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using ParallelTasks;
using BeatShift.Util;

namespace BeatShift.Menus
{
    class FinishedLoadingMenu : IMenuPage
    {

        public FinishedLoadingMenu()
        {
            //title = "Racing Controls";
        }

        public override void DrawSprites(SpriteBatch spriteBatch)
        {
            int x = BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - GameTextures.LoadingBefore.Width / 2;

            Vector2 start = new Vector2(x, 0);
            Vector2 finish = new Vector2(x, BeatShift.graphics.GraphicsDevice.Viewport.Height/2);

            Vector2 pos = Vector2.Lerp(start, finish, animateInLerp);

            spriteBatch.Draw(GameTextures.LoadingBefore, 
                new Rectangle((int)pos.X, (int)pos.Y, GameTextures.LoadingBefore.Width, GameTextures.LoadingBefore.Height), Color.White);
        }

        public override void setupMenuItems()
        {
            addMenuItem("", (Action)(delegate
            {
                    GameLoop.setGameState(GameState.LocalGame);
#if WINDOWS
                    if ((Keyboard.GetState().IsKeyDown(Keys.Enter)) && !AiInputManager.testAI)
                    {
                        Race.humanRacers[0].racingControls.chosenInput = new KeyInputManager();
                        //TODO: Make it so the input type corresponds to the input pressed
                        //Could be done by checking each possible input manually, bypassing inputManger if necessary
                    }
#endif
            }));
        }

        public override void respondToMenuBack()
        {

        }

        public override void enteringMenu()
        {
            base.enteringMenu();
        }
    }
}
