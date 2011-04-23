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


namespace BeatShift
{
    public class ShipSelect
    {
        public Boolean Enabled = false;
        public Boolean Visible = false;

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
            if (Options.UseKeyboardAsPad2) ssarea[1] = new SplitScreenArea(new KeyInputManager());
            else ssarea[1] = new SplitScreenArea(new PadInputManager(PlayerIndex.Two));
            ssarea[2] = new SplitScreenArea(new PadInputManager(PlayerIndex.Three));
            ssarea[3] = new SplitScreenArea(new PadInputManager(PlayerIndex.Four));
        }

        /// <summary>
        /// Changes the players ship to the next ship in the list
        /// </summary>
        /// <param name="player">which players ship to change</param>
       /* public void switchShipRight(int player)
        {
            //find what ship the player has at the moment
            ShipType ship = MainGame.players.storeSelectedShip(player).getType();
            int pos = 0;
            //find the position in the list the current ship is at
            for (int i = 0; i < shipsList.Count - 1; i++)
            {
                if (shipsList[i] == ship) { pos = i; }
            }
            //change ship to one to the right
            if (pos != shipsList.Count - 1)
            {
                MainGame.players.setSelectedShipType(player, shipsList[pos + 1]);
            }
        }*/
        /// <summary>
        /// Changes the players ship to the ship before it in the list
        /// </summary>
        /// <param name="player">which players ship to change</param>
        /*public void switchShipLeft(int player)
        {
            //find what ship the player has at the moment
            ShipType ship = MainGame.players.storeSelectedShip(player).getType();
            int pos = 0;
            //find the position in the list the current ship is at
            for (int i = 0; i < shipsList.Count - 1; i++)
            {
                if (shipsList[i] == ship) { pos = i; }
            }
            //change ship to one to the right
            if (pos != 0)
            {
                MainGame.players.setSelectedShipType(player, shipsList[pos - 1]);
            }
            else
            {
                MainGame.players.setSelectedShipType(player, shipsList[shipsList.Count - 1]);
            }
        }*/
        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            //Update gamePad states and keyboardStates for all input managers
            for (int i = 0; i < 4; i++)
            {
                ssarea[i].Update();
            }

            //"signing in" for each player A will make the ship appear B makes it dissapear.
            for (int i = 0; i < 4; i++)
            {
                if (ssarea[i].input.actionTapped(InputAction.MenuAccept))
                {
                    ssarea[i].setActive(true);
                    Race.humanRacers[i].shipDrawing.isVisible = true;
                }
                if (ssarea[i].input.actionTapped(InputAction.MenuBack))
                {
                    if (ssarea[i].isActive)
                    {
                        ssarea[i].setActive(false);
                        Race.humanRacers[i].shipDrawing.isVisible = false;
                    }
                    else
                    {
                        //Back button presed on inactive window, go back to main menu.
                        GameLoop.setGameStateAndResetPlayers(GameState.Menu);
                    }
                }
            }

            //for each controller
            //if player not active
            //if 'a' is pressed make active  and set game.players.setShipVisibility(index,true) to make ship visible
            //if 'b' or 'back' pressed go to last menu

            // is player ISACTIVE (by pressing a) do:
            // if b is pressed make not active and set visibility to false
            // if up is pressed change color, left right change ship

            //changes the hue for each player
            for (int i = 0; i < 4; i++)
            {
                if (ssarea[i].input.actionPressed(InputAction.MenuDown))
                {
                    increaseShipHue(ssarea[i], 2.5f);
                    Race.humanRacers[i].setColour((int)ssarea[i].hue);
                }
                if (ssarea[i].input.actionPressed(InputAction.MenuUp))
                {
                    decreaseShipHue(ssarea[i], 2.5f);
                    Race.humanRacers[i].setColour((int)ssarea[i].hue);
                }
            }

            //changes the ship to the left/right right for all players (in theory, but switches atm.)
            for (int i = 0; i < 4; i++)
            {
                if (ssarea[i].input.actionTapped(InputAction.MenuLeft)||ssarea[i].input.actionTapped(InputAction.MenuRight))
                {
                    if (Race.humanRacers[i].shipName == ShipName.Omicron)
                    {
                        Race.humanRacers[i].shipName = ShipName.Skylar;
                    }
                    else { Race.humanRacers[i].shipName = ShipName.Omicron; }
                }
            }

            //----------------------------change so only saves active players

            //start button changes game state
            if (ssarea[0].input.actionTapped(InputAction.Start) || ssarea[1].input.actionTapped(InputAction.Start) || ssarea[2].input.actionTapped(InputAction.Start) || ssarea[3].input.actionTapped(InputAction.Start))
            {
                GameLoop.setGameState(GameState.LocalGame);

                //If controller was not plugged remove that player
                Race.removeNonVisibleRacers();
            }
        }



        /// <summary>
        /// Draws borders and graphics on splitscreen ship selection menu. Includes button graphics, arrows and text.
        /// </summary>
        /// <param name="gameTime">game time</param>
        public void Draw(GameTime gameTime)
        {
            BeatShift.graphics.GraphicsDevice.Viewport = Viewports.fullViewport;
            //TODO: Iterate over viewports instead.

            BeatShift.spriteBatch.Begin();
            //draw lines down middle of screen to make the split visible.
            //done in mainGame

            //for each viewport/controller
            for (int i = 0; i < 4; i++)
            {
                if (ssarea[i].isActive)// is player ISACTIVE (by pressing a) do:
                {
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
        public void Update()
        {
            input.Update();
        }
    }
}