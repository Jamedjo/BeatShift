using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BeatShift.Input;
using Microsoft.Xna.Framework.Input;

namespace BeatShift.Menus
{
    class LoadingMenu : IMenuPage
    {
        private bool loadComplete;

        public LoadingMenu()
        {
            //title = "Racing Controls";
        }

        public override void DrawSprites(SpriteBatch spriteBatch)
        {
            Texture2D background = GameTextures.TutorialScreen;
            //spriteBatch.Draw(background, new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - background.Width / 2, BeatShift.graphics.GraphicsDevice.Viewport.Height / 2 - background.Height / 2), Color.White);
            spriteBatch.Draw(background, new Rectangle(0,0,BeatShift.graphics.GraphicsDevice.Viewport.Width, BeatShift.graphics.GraphicsDevice.Viewport.Height), Color.White);        
        }


        public override void setupMenuItems()
        {
            addMenuItem("", (Action)(delegate
            {
                if (loadComplete == true)
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
        private void load()
        {
            while (MapManager.currentMap.physicsLoadingThread.IsAlive) { }
            loadComplete = true;
        }

        public override void enteringMenu()
        {
            loadComplete = false;
            base.enteringMenu();
            load();
        }
    }
}
