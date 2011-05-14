using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BeatShift.Input;
using Microsoft.Xna.Framework.Input;
using System.Threading;

namespace BeatShift.Menus
{
    class LoadingMenu : IMenuPage
    {
        public Thread loadingThread;

        public LoadingMenu()
        {
            //title = "Racing Controls";
        }

        public override void DrawSprites(SpriteBatch spriteBatch)
        {
            Texture2D background = GameTextures.TutorialScreen;
            //spriteBatch.Draw(background, new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - background.Width / 2, BeatShift.graphics.GraphicsDevice.Viewport.Height / 2 - background.Height / 2), Color.White);
            spriteBatch.Draw(background, new Rectangle(0,0,BeatShift.graphics.GraphicsDevice.Viewport.Width, BeatShift.graphics.GraphicsDevice.Viewport.Height), Color.White);
            if (!loadingThread.IsAlive)
                spriteBatch.DrawString(BeatShift.newfont, "PRESS A TO START", new Vector2(450, 620), Color.Lime);
            else
                spriteBatch.DrawString(BeatShift.newfont, "LOADING...", new Vector2(390, 620), Color.DarkRed);
        }

        public override void setupMenuItems()
        {
            addMenuItem("", (Action)(delegate
            {
                if (!loadingThread.IsAlive)
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
                }
            }));
        }

        //DO THE LOADING HERE
        void load()
        {
            MapManager.loadMap();
            if (Options.AddAItoGame)
                Race.setupAIRacers(AiInputManager.numberOfAI);
            foreach (Racer racer in Race.currentRacers)
                racer.Load();
            GC.Collect();
        }

        public override void respondToMenuBack()
        {

        }

        public override void enteringMenu()
        {
            base.enteringMenu();
            loadingThread = new Thread(load);
            loadingThread.Start();
        }
    }
}
