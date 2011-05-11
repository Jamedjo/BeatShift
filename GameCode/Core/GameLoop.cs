using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeatShift.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BeatShift.Input;
using Microsoft.Xna.Framework.GamerServices;
using BeatShift.Utilities___Misc;

namespace BeatShift
{
    /// <summary>
    /// Describes the state of the game, in a game, or in the menus.
    /// </summary>
    public enum GameState { Menu, LocalGame, NetworkedGame, MultiplayerShipSelect }


    public static class GameLoop
    {
        /// <summary>
        /// The current game state.
        /// Used to decide what we should be rendering/updating at any given point in time.
        /// </summary>
        static GameState currentState;
        public static GameState getCurrentState() { return currentState; }

        private static bool paused = false;
        private static bool pausedForGuide = false;

        public static bool raceComplete = false;

        private static bool[] activeControllers = new bool[4];

        private static IMenuPage pauseMenu = new PauseMenu();
        private static IMenuPage resultsMenu = new ResultsMenu();

        public static void setActiveControllers(bool set, int index)
        {
            activeControllers[index] = set;
        }

        public static void BeginPause(bool UserInitiated)
        {
            paused = true;
            pausedForGuide = !UserInitiated;

            //turn off vibrations
            GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
            GamePad.SetVibration(PlayerIndex.Two, 0.0f, 0.0f);
            GamePad.SetVibration(PlayerIndex.Three, 0.0f, 0.0f);
            GamePad.SetVibration(PlayerIndex.Four, 0.0f, 0.0f);

            if (Race.currentRaceType.actualRaceBegun)
            {
                Race.currentRaceType.totalRaceTime.Stop();
                foreach (Racer racer in Race.currentRacers)
                    racer.raceTiming.stopLapTimer();
                BeatShift.bgm.Pause();
            }

            // Pause during countdown
            if (Race.currentRaceType.raceProcedureBegun && !Race.currentRaceType.actualRaceBegun)
            {
                Race.currentRaceType.countDownTimer.Stop();
            }
            //TODO: Pause audio playback
            //TODO: Pause controller vibration
        }

        public static void EndPause()
        {
            //TODO: Resume audio
            //TODO: Resume controller vibration
            pausedForGuide = false;
            paused = false;
            if (Race.currentRaceType.actualRaceBegun)
            {
                Race.currentRaceType.totalRaceTime.Start();
                foreach (Racer racer in Race.currentRacers)
                    racer.raceTiming.startLapTimer();
                BeatShift.bgm.UnPause();
            }

            // Pause during countdown
            if (Race.currentRaceType.raceProcedureBegun && !Race.currentRaceType.actualRaceBegun)
            {
                Race.currentRaceType.countDownTimer.Start();
            }

        }

        private static void checkPauseKey(GameTime gameTime)
        {
            // If key was not down before, but is down now, we toggle the
            // pause setting
            if (mainGameinput.actionTapped(InputAction.Start))
            {
                if (!paused)
                {
                    BeginPause(true);
                    pauseMenu.enteringMenu();
                    MenuManager.anyInput.Update(gameTime);//Call update an extra time to flush button press which caused menu to show
                }
                else
                {
                    EndPause();
                    pauseMenu.leavingMenu();
                }
            }
        }

        private static void checkPauseGuide()
        {
            // Pause if the Guide is up
            if (!paused && Guide.IsVisible && currentState == GameState.LocalGame)
            {
                BeginPause(false);
                //Console.Write("Game Paused \n");
            }
            // If we paused for the guide, unpause if the guide
            // went away
            else if (paused && pausedForGuide && !Guide.IsVisible && currentState == GameState.LocalGame)
            {
                EndPause();
                //Console.Write("Game Resumed \n");
            }
        }


        private static void checkControllers(GameTime gameTime)
        {
            // Pause if the Guide is up
            if (!paused && currentState == GameState.LocalGame)
                for (int k = 0; k<4 ; k++)
                    if (activeControllers[k] && !GamePad.GetState((PlayerIndex)(k)).IsConnected)
                    {
                        BeginPause(true);
                        pauseMenu.enteringMenu();
                        MenuManager.anyInput.Update(gameTime);
                        break;
                    }
        }

        public static void endGame(GameTime gameTime)
        {
            BeginPause(true);
            raceComplete = true;
            resultsMenu.enteringMenu();
        }

        //Input manager for exiting game
        static IInputManager mainGameinput = new AnyInputManager();
#if WINDOWS
        static Boolean wasF4pressed = false;
#endif

        //TODO: move into quitting function of race type
        public static void setGameStateAndResetPlayers(GameState newState)
        {
            Race.resetPlayers();
            setGameState(newState);
        }

        public static void startLocalGame(GameTime gameTime)
        {
            setGameState(GameState.LocalGame);
            flushInput(gameTime);
        }

        public static void flushInput(GameTime gameTime)
        {
            GameLoop.mainGameinput.Update(gameTime);
        }

        /// <summary>
        /// Changes the current game state to newState.
        /// </summary>
        /// <param name="newState">The state the game should be set to after this call.</param>
        public static void setGameState(GameState newState)
        {
            if (currentState == newState)
                return;//No change.
            currentState = newState;

            BeatShift.graphics.GraphicsDevice.Viewport = Viewports.fullViewport;

            MenuManager.Enabled = false;
            MenuManager.Visible = false;
            BeatShift.networkedGame.Enabled = false;
            BeatShift.networkedGame.Visible = false;
            MapManager.Enabled = false;
            MapManager.Visible = false;
            Race.Enabled = false;
            Race.Visible = false;
            Physics.Enabled = false;
            BeatShift.shipSelect.Enabled = false;
            BeatShift.shipSelect.Visible = false;
            paused = false;
            raceComplete = false;

            //physics.collisionSystem.cl

            switch (newState)
            {
                case GameState.Menu:
                    MenuManager.Enabled = true;
                    MenuManager.Visible = true;
                    BeatShift.bgm.stop();
                    break;
                case GameState.NetworkedGame:
                    BeatShift.networkedGame.Enabled = true;
                    BeatShift.networkedGame.Visible = true;
                    //physics.collisionSystem.AddBody(tempMap.rigidBody);
                    break;
                case GameState.LocalGame:
                    Physics.Enabled = true;
                    Race.Enabled = true;
                    Race.Visible = true;
                    MapManager.Enabled = true;
                    MapManager.Visible = true;
                    Race.setupViewports();
                    Race.currentRaceType.startRaceProcedure();
                    MenuManager.resetToMain();
                    break;
                case GameState.MultiplayerShipSelect:
                    MenuManager.MenuTrail.Push(MenuPage.MapSelect);
                    Race.setupSelectionRacers(4,false);
                    Race.Enabled = true;
                    Race.Visible = true;
                    BeatShift.shipSelect.Visible = true;
                    BeatShift.shipSelect.Enabled = true;
                    BeatShift.shipSelect.enteringState();
                    break;
            }
        }

        public static void Update(GameTime gameTime)
        {
            // Check to see if the user has paused or unpaused
            if(currentState == GameState.LocalGame || currentState == GameState.NetworkedGame)
                checkPauseKey(gameTime);

            checkPauseGuide();

#if XBOX
            checkControllers(gameTime);
#endif

            // If the user hasn't paused, Update normally

            BeatShift.gamerServices.Update(gameTime);

            mainGameinput.Update(gameTime);
            if (!paused)
            {
                //Update all managed timers.
                RunningTimer.Update(gameTime);

                if (MenuManager.Enabled)
                    MenuManager.Update(gameTime);
                if (Race.Enabled)
                {
                    Race.Update(gameTime);
                    HeadsUpDisplay.Update(gameTime);
                }
                //if (MapManager.Enabled)
                //    MapManager.tempMap.Update(gameTime);
                if (BeatShift.shipSelect.Enabled)
                    BeatShift.shipSelect.Update(gameTime);
                //if (networkedGame.Enabled) networkedGame.Update(gameTime);
                if (Physics.Enabled)
                    Physics.Update(gameTime);
                if (MapManager.Enabled)
                    Race.Update(gameTime);
                if (Race.Visible && !MenuManager.Enabled && BeatShift.emitter != null)
                {
                    BeatShift.emitter.Update(gameTime);
                }
                // Game should be exited through the menu systems.
                // Allows the game to return to main menu
                //if (mainGameinput.actionTapped(InputAction.BackButton))
                //{
                //    //MenuManager.setCurrentMenu(MenuPage.Main);
                //    GameLoop.setGameStateAndResetPlayers(GameState.Menu);
                //}
            }

            if( paused && !pausedForGuide )
            {
                if (raceComplete)
                    resultsMenu.Update(gameTime);
                else
                    pauseMenu.Update(gameTime);
                MenuManager.anyInput.Update(gameTime);
            }

            //full screen option
#if WINDOWS
            // NO
            //F4 press triggers fullscreen
            //if (Keyboard.GetState().IsKeyDown(Keys.F4)) wasF4pressed = true;
            //if (wasF4pressed) if (Keyboard.GetState().IsKeyUp(Keys.F4))
            //    {
            //        wasF4pressed = false;
            //        BeatShift.graphics.ToggleFullScreen();
            //    }
#endif
            BeatShift.bgm.Update();
        }

        public static void Draw(GameTime gameTime)
        {
            BeatShift.graphics.GraphicsDevice.Viewport = Viewports.fullViewport;
            Rectangle viewArea = new Rectangle(0, 0, BeatShift.graphics.GraphicsDevice.Viewport.Width, BeatShift.graphics.GraphicsDevice.Viewport.Height);

            //Draw Scene background
            BeatShift.graphics.GraphicsDevice.Clear(Color.Black);

            BeatShift.spriteBatch.Begin();
            //if (menuSystem.Visible || shipSelect.Visible)
            {
                //GraphicsDevice.Clear(Color.CornflowerBlue);
                BeatShift.spriteBatch.Draw(GameTextures.MenuBackgroundBlue, viewArea, Color.White);
            }
            BeatShift.spriteBatch.End();

            //Draw other Drawable Game Classes
            if (MenuManager.Visible)
            {
                MenuManager.Draw(gameTime);
            }
            if (BeatShift.shipSelect.Visible && !( paused || Guide.IsVisible ) )
            {
                BeatShift.shipSelect.Draw(gameTime);
            }
            if (Race.Visible)
            {
                Race.Draw(gameTime);
            }

            if (paused && !pausedForGuide)
                if (raceComplete)
                    resultsMenu.Draw();
                else
                    pauseMenu.Draw();
            //if (networkedGame.Visible) networkedGame.Draw(gameTime);
        }

    }
}
