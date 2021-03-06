using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using BeatShift.Input;
using BeatShift.Menus;


namespace BeatShift
{
    public class ShipSelect
    {
        public Boolean Enabled = false;
        public Boolean Visible = false;
        public int[] signedInPlayers = new int[4];
        public Boolean signInFlag;

        //PlayerIndex[] playerIndex = new PlayerIndex[4];
        List<ShipName> shipsList = new List<ShipName>();

        SplitScreenArea[] ssarea = new SplitScreenArea[4];
        
        void increaseShipHue(SplitScreenArea ssarea, float byValue)
        {
            ssarea.hue += byValue;
            ssarea.hue %= 360;
        }
        void decreaseShipHue(SplitScreenArea ssarea, float byValue)
        {
            ssarea.hue -= byValue;
            if (ssarea.hue < 0)
            {
                ssarea.hue %= 360;//modulo takes sign of shipHue so still negative
                ssarea.hue = 360 + ssarea.hue;
            }
        }

        /// <summary>
        /// This constructor is called at the very start of the game when the menus are created
        /// </summary>
        public ShipSelect()
        {
            resetInputsAndScreens();

            //save in a list all the ships in the enum type in Ship.cs
            foreach ( var type in Utils.GetValues<ShipName>())
            {
                shipsList.Add(type);
            }   
        }

        public void resetInputsAndScreens()
        {
            ssarea[0] = new SplitScreenArea(new PadInputManager(PlayerIndex.One));
            ssarea[1] = new SplitScreenArea(new PadInputManager(PlayerIndex.Two));
            if (Options.UseKeyboardAsPad2) ssarea[2] = new SplitScreenArea(new KeyInputManager());
            else ssarea[2] = new SplitScreenArea(new PadInputManager(PlayerIndex.Three));
            ssarea[3] = new SplitScreenArea(new PadInputManager(PlayerIndex.Four));

            //ssarea[0] = new SplitScreenArea(new KeyInputManager());
            //ssarea[1] = new SplitScreenArea(new KeyInputManager());
            //ssarea[2] = new SplitScreenArea(new KeyInputManager());
            //ssarea[3] = new SplitScreenArea(new KeyInputManager());
        }

        public void enteringState()
        {
            resetInputsAndScreens();
#if DEBUG
            if (AiInputManager.testAI)
            {
                for (int i = 0; i < 4; i++)
                {
                    //ssarea[i].setActive(true);
                    Race.humanRacers[i].shipDrawing.isVisible = true;
                }
            }
#endif
        }

        /// <summary>
        /// Changes the players ship to the next ship in the list
        /// </summary>
        /// <param name="player">which players ship to change</param>
        public void switchShipRight(int player)
        {
            if (ssarea[player].input.actionTapped(InputAction.Right))
            {
                //find what ship the player has at the moment
                //Race.humanRacers[player].shipName = (ShipName)IMenuPage.getCurrentItem();
                ShipName ship = Race.humanRacers[player].shipName;
                int pos = 0;
                //find the position in the list the current ship is at
                for (int i = 0; i < shipsList.Count; i++)
                    if (shipsList[i] == ship)
                        pos = i;
                //change ship to one to the right
                if (pos != shipsList.Count - 1)
                    Race.humanRacers[player].shipName = shipsList[pos + 1];
            }
        }

        /// <summary>
        /// Changes the players ship to the ship before it in the list
        /// </summary>
        /// <param name="player">which players ship to change</param>
        public void switchShipLeft(int player)
        {
            if (ssarea[player].input.actionTapped(InputAction.Left))
            {
                //find what ship the player has at the moment
                ShipName ship = Race.humanRacers[player].shipName;
                int pos = 0;
                //find the position in the list the current ship is at
                for (int i = 0; i < shipsList.Count; i++)
                    if (shipsList[i] == ship)
                        pos = i;
                //change ship to one to the left
                if (pos != 0)
                    Race.humanRacers[player].shipName = shipsList[pos - 1];
            }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            bool activePlayerPressedStart = false;

            //For each quadrent area of the screen/for each potential player   
            for (int i = 0; i < 4; i++)
            {
                //Update gamePad states and keyboardStates for all input managers
                ssarea[i].Update(gameTime);

                //A makes player active
                respondToButtonA(i);

#if XBOX
                //"signing in"
                signIn(i);
#endif

                // B makes it dissapear if already active, otherwise return to main menu.
                respondToMenuBack(i);

                //changes the hue for each player
                //respondToUpDown(i);

                //changes the ship to the left/right right for all players
                switchShipLeft(i);
                switchShipRight(i);
               
                activePlayerPressedStart = activePlayerPressedStart || (ssarea[i].input.actionTapped(InputAction.Start) && (ssarea[i].isActive));
            }


            //start button changes game state
            if (activePlayerPressedStart)
            {
                setupGameAndChangeState(gameTime);
            }
        }

        private void signIn(int i)
        {
            Race.getFullListOfRacerIDsFromSignedInPeople();
            Array.Clear(signedInPlayers, 0, signedInPlayers.Length);
            foreach (SignedInGamer gamer in Gamer.SignedInGamers)
            {
                signedInPlayers[(int)gamer.PlayerIndex] = 1;
            }

            if (!LiveServices.GuideIsVisible()) if (ssarea[i].isActive && (Race.humanRacers[i].shipDrawing.isVisible == false))
                {
                    if (signedInPlayers[i] == 1)
                    {
                        Race.humanRacers[i].shipDrawing.isVisible = true;
                    }
                    else
                        ssarea[i].setActive(false);
                }
        }

        private void setupGameAndChangeState(GameTime gameTime)
        {
            //make sure physics loads
            //while (MapManager.currentMap.physicsLoadingThread.IsAlive) { }
            Race.isPrimed = false;
            //set input managers
            for (int k = 0; k < 4; k++)
            {
                Race.humanRacers[k].setupRacingControls(ssarea[k].input);
            }


            //Race.getFullListOfRacerIDsFromSignedInPeople();

            //If controller was not plugged-in/selected remove that player
            Race.removeNonVisibleRacers();

            GameLoop.setGameState(GameState.Menu);
            MenuManager.mainMenuSystem.setCurrentMenu(MenuPage.Loading);
        }

        private void respondToMenuBack(int i)
        {
            if (ssarea[i].input.actionTapped(InputAction.MenuBack))
            {
                if (ssarea[i].isActive)
                {
                    ssarea[i].setActive(false);
                    Race.humanRacers[i].shipDrawing.isVisible = false;
                }
                else if( !ssarea[0].isActive && !ssarea[1].isActive && !ssarea[2].isActive && !ssarea[3].isActive )
                {
                    //Back button presed on inactive window, go back to main menu.
                    GameLoop.setGameStateAndResetPlayers(GameState.Menu);
                }
                GameLoop.setActiveControllers(false, i);
            }
        }

        private void respondToButtonA(int i)
        {
            if (ssarea[i].input.actionTapped(InputAction.MenuAccept))
            {
                ssarea[i].setActive(true);
#if XBOX
                if (signedInPlayers[i] == 0)
                    Guide.ShowSignIn(4, false);
#endif
#if WINDOWS
                Race.humanRacers[i].shipDrawing.isVisible = true;
#endif
                GameLoop.setActiveControllers(true, i);
            }
        }

        //private void respondToUpDown(int i)
        //{
        //    if (ssarea[i].input.actionPressed(InputAction.MenuDown))
        //    {
        //        increaseShipHue(ssarea[i], 2.5f);
        //        Race.humanRacers[i].setColour((int)ssarea[i].hue);
        //    }
        //    if (ssarea[i].input.actionPressed(InputAction.MenuUp))
        //    {
        //        decreaseShipHue(ssarea[i], 2.5f);
        //        Race.humanRacers[i].setColour((int)ssarea[i].hue);
        //    }
        //}

        /// <summary>
        /// Draws borders and graphics on splitscreen ship selection menu. Includes button graphics, arrows and text.
        /// </summary>
        /// <param name="gameTime">game time</param>
        public void Draw(GameTime gameTime)
        {
            BeatShift.graphics.GraphicsDevice.Viewport = Viewports.fullViewport;
            Rectangle viewArea = new Rectangle(0, 0, BeatShift.graphics.GraphicsDevice.Viewport.Width, BeatShift.graphics.GraphicsDevice.Viewport.Height);
            //TODO: Iterate over viewports instead.

            BeatShift.spriteBatch.Begin();
            //draw lines down middle of screen to make the split visible.
            //done in mainGame

            BeatShift.spriteBatch.Draw(GameTextures.MenuBackgroundBlue, viewArea, Color.White);

            //for each viewport/controller
            for (int i = 0; i < 4; i++)
            {
                if (ssarea[i].isActive)// is player ISACTIVE (by pressing a) do:
                {
                    Race.isPrimed = true;
                    //BeatShift.spriteBatch.DrawString(BeatShift.newfont, "Press start to begin Race", new Vector2(300, 300), Color.Black);
                    // draw name of ship above ship
                    // draw verticle hue bar next to ship
                    // draw arrows either side of ship
                }
                else
                {
                    // if not active draw image of controller button 'a'
                    Vector2 pos = new Vector2(Race.localCameras[i].Viewport.X + Race.localCameras[i].Viewport.Width / 2, Race.localCameras[i].Viewport.Y + Race.localCameras[i].Viewport.Height / 2);
                    ButtonDraw.DrawButton(BeatShift.spriteBatch,ButtonImage.A,pos,0.8f);
                }
            }

            BeatShift.spriteBatch.End();
        }
    }

    public class SplitScreenArea
    {
        public IInputManager input;
        public float hue =0;
        public ShipName type ;
        public Boolean isActive = false;

        public SplitScreenArea(IInputManager inputManager)
        {
            input = inputManager;
        }
        public void setActive(bool active)
        {
            isActive = active;
        }
        public void Update(GameTime gameTime)
        {
            input.Update(gameTime);
        }
    }
}