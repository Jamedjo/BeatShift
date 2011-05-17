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

namespace BeatShift.Menus
{
    class LoadingMenu : IMenuPage
    {
        private bool loaded;

        public LoadingMenu()
        {
            //title = "Racing Controls";
        }

        public override void DrawSprites(SpriteBatch spriteBatch)
        {
            Texture2D background = GameTextures.TutorialScreen;
            //spriteBatch.Draw(background, new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - background.Width / 2, BeatShift.graphics.GraphicsDevice.Viewport.Height / 2 - background.Height / 2), Color.White);
            spriteBatch.Draw(background, new Rectangle(0, 0, BeatShift.graphics.GraphicsDevice.Viewport.Width, BeatShift.graphics.GraphicsDevice.Viewport.Height), Color.White);

            //spriteBatch.Draw(background, new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width, BeatShift.graphics.GraphicsDevice.Viewport.Height), Color.White);
            if (loaded)
                spriteBatch.Draw(GameTextures.LoadingBefore, new Rectangle(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - GameTextures.LoadingBefore.Width / 2, BeatShift.graphics.GraphicsDevice.Viewport.Height - 100, GameTextures.LoadingBefore.Width, GameTextures.LoadingBefore.Height), Color.White);
            else
                spriteBatch.Draw(GameTextures.LoadingAfter, new Rectangle(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - GameTextures.LoadingAfter.Width / 2, BeatShift.graphics.GraphicsDevice.Viewport.Height - 100, GameTextures.LoadingAfter.Width, GameTextures.LoadingAfter.Height), Color.White);
        }

        public override void setupMenuItems()
        {
            addMenuItem("", (Action)(delegate
            {
                if (loaded)
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

        void load()
        {
            MapManager.loadMap();
            BeatShift.bgm.loadTrack(SoundManager.trackToLoad);
            if (Options.AddAItoGame)
                Race.setupAIRacers(AiInputManager.numberOfAI);
            /*Parallel.ForEach(Race.currentRacers, racer =>
            {
                racer.Load();
            });*/
            foreach (Racer racer in Race.currentRacers)
            {
                racer.Load();
            }
            GC.Collect();
            loaded = true;
        }

        public override void respondToMenuBack()
        {

        }

        public override void enteringMenu()
        {
            base.enteringMenu();
            loaded = false;
            Parallel.Start(load);
        }
    }
}
