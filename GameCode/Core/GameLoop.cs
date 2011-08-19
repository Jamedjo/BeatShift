using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeatShift.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BeatShift.Input;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Audio;
using ParallelTasks;
using DPSF.ParticleSystems;
using DPSF;
using BeatShift.GameDebugTools;

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
        public static SoundBank menuBank;
        public static WaveBank wavBank;
        //private static ParticleSystemManager particleManager;
        static Cue titleMusic;
        private static bool paused = false;
        private static bool pausedForGuide = false;

        public static bool raceComplete = false;

        private static bool[] activeControllers = new bool[4];//Array to show which controllers should be active. Game pauses if any of these are unplugged.

        public static void playTitle()
        {
            try
            {
                if (!titleMusic.IsPlaying)
                {
                    titleMusic.Play();
                }
            }
            catch (Exception e)
            {
                titleMusic = menuBank.GetCue("Title");
                while (titleMusic.IsPreparing)
                {
                }
                titleMusic.Play();
            }
        }

        public static void StopTitle()
        {
            if (titleMusic != null && !titleMusic.IsStopped && titleMusic.IsPlaying)
            {
                titleMusic.Stop(AudioStopOptions.AsAuthored);
            }
            titleMusic = menuBank.GetCue("Title");
            GC.Collect();
        }

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
                    MenuManager.EnableSystem(MenuStack.Paused);
                    MenuManager.anyInput.Update(gameTime);
                }
                else if(MenuManager.pausedSystem.isActive)
                {
                    EndPause();
                    MenuManager.DisableAllMenus();
                }
            }
        }

        private static void checkPauseGuide()
        {
            // Pause if the Guide is up
            if (!paused && LiveServices.GuideIsVisible())// && currentState == GameState.LocalGame)
            {
                BeginPause(false);
                //Console.Write("Game Paused \n");
            }
            // If we paused for the guide, unpause if the guide
            // went away
            else if (paused && pausedForGuide && !LiveServices.GuideIsVisible())// && currentState == GameState.LocalGame)
            {
                pausedForGuide = false;

                if(MenuManager.pausedSystem.isActive==false) //Checks that the game wasn't double-paused with both guide+pauseMenu
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
                        MenuManager.EnableSystem(MenuStack.Paused);
                        MenuManager.anyInput.Update(gameTime);
                        break;
                    }
        }

        public static void endGame(GameTime gameTime)
        {
            BeginPause(true);
            raceComplete = true;
            MenuManager.EnableSystem(MenuStack.PostRace);
            ((ResultsMenu)IMenuStack.Results).calculateResults();
            MenuManager.anyInput.Update(gameTime);
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

            MenuManager.DisableAllMenus();
            //BeatShift.networkedGame.Enabled = false;
            //BeatShift.networkedGame.Visible = false;
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
                    MenuManager.EnableSystem(MenuStack.Main);
                    MenuManager.mainMenuSystem.setCurrentMenu(MenuPage.Main);
                    BeatShift.bgm.stop();
                    playTitle();
                    break;
                case GameState.NetworkedGame:
                    //BeatShift.networkedGame.Enabled = true;
                    //BeatShift.networkedGame.Visible = true;
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
                    break;
                case GameState.MultiplayerShipSelect:
                    MenuManager.mainMenuSystem.MenuTrail.Push(MenuPage.MapSelect);
                    Race.setupSelectionRacers(4,false);
                    Race.Enabled = true;
                    Race.Visible = true;
                    BeatShift.shipSelect.Visible = true;
                    BeatShift.shipSelect.Enabled = true;
                    BeatShift.shipSelect.enteringState();
                    playTitle();
                    break;
            }
        }

        public static void Update(GameTime gameTime)
        {
#if DEBUG
            DebugSystem.Instance.TimeRuler.StartFrame();
#endif
            using (new ProfileSection("Background", Color.Blue))
            {
                // Check to see if the user has paused or unpaused
                BeatShift.bgm.Update();
                // Task music = Parallel.Start();
                if (currentState == GameState.LocalGame || currentState == GameState.NetworkedGame)
                    checkPauseKey(gameTime);

                checkPauseGuide();

#if XBOX
            checkControllers(gameTime);
#endif

                // If the user hasn't paused, Update normally

                LiveServices.Update(gameTime);

                //Update mainGameInput
                if(!LiveServices.GuideIsVisible()) mainGameinput.Update(gameTime);
            }

            if (!pausedForGuide)
            {
                MenuManager.Update(gameTime);
            }

            if (!paused)
            {
                Boolean raceUpdated = false;
                if (GameLoop.getCurrentState() == GameState.LocalGame)
                {
                    //IWork pudate = new IWork();
                    //particles = Parallel.Start(()=>BeatShift.particleManager.UpdateAllParticleSystems((float)gameTime.ElapsedGameTime.TotalSeconds));
                    //BeatShift.particleManager.UpdateAllParticleSystems((float)gameTime.ElapsedGameTime.TotalSeconds);
                }
                
                //Update all managed timers.
                RunningTimer.Update(gameTime);

                if (BeatShift.shipSelect.Enabled)
                    BeatShift.shipSelect.Update(gameTime);

                //if (networkedGame.Enabled) networkedGame.Update(gameTime);

                using (new ProfileSection("Physics", Color.Red))
                {
                    if (Physics.Enabled)
                        Physics.Update(gameTime);
                }


                using (new ProfileSection("Race", Color.Yellow))
                {

                    if (Race.Enabled)
                    {
                        Race.Update(gameTime);
                        raceUpdated = true;
                        HeadsUpDisplay.Update(gameTime);
                    }
                }


                //What??
                if (MapManager.Enabled&&!raceUpdated)
                    Race.Update(gameTime);


            }


            //full screen option
#if WINDOWS
            //F4 press triggers fullscreen
            if (Keyboard.GetState().IsKeyDown(Keys.F4)) wasF4pressed = true;
            if (wasF4pressed) if (Keyboard.GetState().IsKeyUp(Keys.F4))
                {
                    wasF4pressed = false;
                    BeatShift.graphics.ToggleFullScreen();
                }
#endif
            //music.Wait();


            BeatShift.engine.Update();
        }

        public static void Draw(GameTime gameTime)
        {
            BeatShift.graphics.GraphicsDevice.Viewport = Viewports.fullViewport;
            Rectangle viewArea = new Rectangle(0, 0, BeatShift.graphics.GraphicsDevice.Viewport.Width, BeatShift.graphics.GraphicsDevice.Viewport.Height);
            
            //Clear screen to black
            BeatShift.graphics.GraphicsDevice.Clear(Color.SteelBlue);
            if(Globals.PostProcess) PostProcessFx.BeginDraw();//Set RenderTarget to bloom target instead of buffer

            if (BeatShift.shipSelect.Visible && !( paused || LiveServices.GuideIsVisible() ) )
            {
                BeatShift.shipSelect.Draw(gameTime);
            }

            //Draw Main menu system if active, before drawing ship-selection ships
            if(MenuManager.mainMenuSystem.isActive) MenuManager.Draw(gameTime);

            //Begin 3D drawing region

            //Draw map
            if (Race.Visible)
            {
                Race.DrawMap(gameTime);
            }

            //Draw ships/racers and HUD
            if (Race.Visible)
            {
                Race.Draw3D(gameTime);
                Race.Draw2D(gameTime);
            }

            //Begin glow pass
            if (Globals.PostProcess)
            {
                PostProcessFx.BeginGlowPass();
                Race.Draw3D(gameTime);//Fake bit of geometry
                PostProcessFx.Draw(gameTime); //Apply bloom on 3D elements and draw to backbuffer.
            }


            //Draw any other menu systems (Pause/PostGame) which are active
            if (!MenuManager.mainMenuSystem.isActive) MenuManager.Draw(gameTime);



            //if (networkedGame.Visible) networkedGame.Draw(gameTime);

        }

    }
}
