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
using System.Diagnostics;
using System.Collections;
using BeatShift.Menus;
using BeatShift.Cameras;
using ParallelTasks;
using BeatShift.Input;

namespace BeatShift
{
    public static class Race
    {
        public static Boolean Enabled = false;
        public static Boolean Visible = false;

        public static IRaceType currentRaceType { get; set; }
        public static List<Racer> currentRacers { get; private set; }
        public static List<RacerHuman> humanRacers { get { return currentRacers.OfType<RacerHuman>().ToList(); } }
        public static List<CameraWrapper> localCameras { get { return humanRacers.Select(hr => hr.localCamera).ToList(); } }

        public static List<RacerId> racerIDs = new List<RacerId>();
        public static bool isPrimed = false;


        public static string[] AInames;

        public static void removeNonVisibleRacers()
        {
            currentRacers = currentRacers.Where(r => r.shipDrawing.isVisible).ToList();
        }

        public static void Update(GameTime gameTime)
        {
            if (Race.currentRaceType.raceProcedureBegun)
            {
                // If the race has started update everything
                currentRaceType.Update(gameTime);

                if (Globals.UpdateRaceWithParallel)
                {
                    Parallel.ForEach(currentRacers, racer =>
                    {
                        racer.Update(gameTime);
                    }
                    );
                }
                else
                {
                    foreach (Racer racer in currentRacers)
                    {
                        racer.Update(gameTime);
                    }
                }
            }
            else
            {
                // Only update the camera if the race has not started yet
                Parallel.ForEach(humanRacers, racer =>
                {
                    racer.localCamera.Update(gameTime);
                }
                );

                foreach (Racer racer in currentRacers)
                {
                    //Update ship animatiions
                    racer.shipDrawing.Update(gameTime);
                }
            }
        }

        public static void DrawMap(GameTime gameTime)
        {
            if (MapManager.Visible)
            {
                //Set default render states once
                //BeatShift.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                BeatShift.graphics.GraphicsDevice.BlendState = BlendState.Opaque;


                //Skybox has no depthStencil to keep it behind everything else, Clamp to avoid texture seams.
                BeatShift.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.None;
                BeatShift.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
                foreach (RacerHuman h_racer in humanRacers)
                {
                    MapManager.currentMap.DrawSkybox(gameTime, h_racer.localCamera);
                }
                BeatShift.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;//Return depthstencil states after drawing skybox.


                //Set display states for drawing map
                BeatShift.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
                foreach (RacerHuman h_racer in humanRacers)
                {
                    MapManager.currentMap.Draw(gameTime, h_racer.localCamera);
                }


                if (Options.DrawWaypoints)
                {
                    //Set display states
                    BeatShift.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                    BeatShift.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    BeatShift.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

                    foreach (RacerHuman h_racer in humanRacers)
                    {
                        MapManager.currentMap.drawSpheres(gameTime, h_racer.localCamera);
                    }

                    //Return display State
                    BeatShift.graphics.GraphicsDevice.BlendState = BlendState.Opaque;
                }
            }

        }

        public static void Draw3D(GameTime gameTime)
        {
            //Set display states
            BeatShift.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            BeatShift.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;//.LinearWrap;

            foreach (RacerHuman h_racer in humanRacers)
            {
                BeatShift.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                foreach (Racer allRacer in currentRacers)
                {
                    if (allRacer.shipDrawing.isVisible)
                    {
                        allRacer.shipDrawing.Draw(gameTime, h_racer.localCamera, (allRacer == (Racer)h_racer));
                    }
                }
            }

        }


        public static void Draw2D(GameTime gameTime)
        {
            if (Globals.DisplayHUD)
            {
                foreach (RacerHuman h_racer in humanRacers)
                {
                    BeatShift.graphics.GraphicsDevice.Viewport = h_racer.localCamera.Viewport;

                    //if(Physics.Visible)
                    //    Physics.Draw(gameTime);
                    //BeatShift.graphics.GraphicsDevice.Viewport = h_racer.localCamera.Viewport;
                    BeatShift.spriteBatch.Begin();
                    HeadsUpDisplay.DrawHUD(h_racer.localCamera, h_racer, gameTime);

                    //If testing AI through a human racer, draw AI debug HUD
                    if (AiInputManager.testAI && (h_racer.racingControls != null) && (h_racer.racingControls.chosenInput != null) && h_racer.racingControls.chosenInput.GetType().Equals(typeof(AiInputManager)))
                    {
                        ((AiInputManager)h_racer.racingControls.chosenInput).DrawAiHUD(h_racer.localCamera, gameTime);
                    }

                    BeatShift.spriteBatch.End();
                }

                BeatShift.spriteBatch.Begin();

                BeatShift.singleton.GraphicsDevice.Viewport = Viewports.fullViewport;

                if (humanRacers.Count == 2)
                {
                    HeadsUpDisplay.DrawSplitBarsTwoPlayer();
                }
                else if (humanRacers.Count == 3)
                {
                    HeadsUpDisplay.DrawSplitBarsThreePlayer();
                }
                else if (humanRacers.Count == 4)
                {
                    HeadsUpDisplay.DrawSplitBarsFourPlayer();
                    if (Race.isPrimed)
                        BeatShift.spriteBatch.Draw(GameTextures.Start, new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - GameTextures.Start.Width / 2, 0), Color.White);
                }
                BeatShift.spriteBatch.End();
            }
            else BeatShift.singleton.GraphicsDevice.Viewport = Viewports.fullViewport;
        }


        static string randomString(int length)
        {
            string tempString = Guid.NewGuid().ToString().ToLower();
            tempString = tempString.Replace("-", "");
            while (tempString.Length < length)
            {
                tempString += tempString;
            }
            tempString = tempString.Substring(0, length);
            return tempString;
        }

        public static void setupAIRacers(int numberOfAiRacers)
        {
            //Add AI racers
            initializeAiNames();
            if (!currentRaceType.getRaceType().Equals(RaceType.TimeTrialRace))
            {
                for (int i = 0; i < numberOfAiRacers; i++)
                {
                    Racer r = new Racer(new RacerId(pickAiName(i)), currentRacers.Count+1, RacerType.AI);
                    currentRacers.Add(r);
                }
            }
            //setupViewports();
        }

        public static void setupSelectionRacers(int numberOfPlayersInSelectionScreen, Boolean areShipsVisible)
        {
            //while (!BeatShift.beatShift.IsActive) { }
            getFullListOfRacerIDsFromSignedInPeople();
            // Add 'None' racers for ship selection screen
            currentRacers.Clear();
            initializeAiNames();
            for (int i = 0; i < numberOfPlayersInSelectionScreen; i++)
            {
                currentRacers.Add(new RacerHuman(new RacerId(randomString(3)), currentRacers.Count, RacerType.None, humanRacers.Count, false));
                try
                {
                    currentRacers[i].racerID = new RacerId(racerIDs[i].gamer);
                }
                catch
                {
                    currentRacers[i].racerID = new RacerId(pickAiName(i));
                }
            }

            foreach (Racer racer in currentRacers)
            {
                racer.shipDrawing.isVisible = areShipsVisible;
            }

            setupViewports();
        }

        public static void getFullListOfRacerIDsFromSignedInPeople()
        {
            racerIDs.Clear();
            foreach (SignedInGamer gamer in Gamer.SignedInGamers)
            {
                if (gamer != null)
                {
                    racerIDs.Add(new RacerId(gamer));
                }
            }
        }

        public static void setupViewports()
        {
            switch (humanRacers.Count)
            {
                case 0:
                    break;
                case 1:
                    humanRacers[0].localCamera.Viewport = Viewports.fullViewport;
                    break;
                case 2:
                    humanRacers[0].localCamera.Viewport = Viewports.leftViewPort;
                    humanRacers[1].localCamera.Viewport = Viewports.rightViewPort;
                    break;
                case 3:
                    humanRacers[0].localCamera.Viewport = Viewports.leftViewPort;
                    humanRacers[1].localCamera.Viewport = Viewports.topRightViewport;
                    humanRacers[2].localCamera.Viewport = Viewports.bottomRightViewport;
                    break;
                case 4:
                    humanRacers[0].localCamera.Viewport = Viewports.topLeftViewport;
                    humanRacers[1].localCamera.Viewport = Viewports.topRightViewport;
                    humanRacers[2].localCamera.Viewport = Viewports.bottomLeftViewport;
                    humanRacers[3].localCamera.Viewport = Viewports.bottomRightViewport;
                    break;
            }

        }

        /// <summary>
        /// Chooses a new AI name
        /// </summary>
        public static void initializeAiNames()
        {
            AInames = new string[6];
            AInames[0] = "Simba";
            AInames[1] = "Scar";
            AInames[2] = "Mufasa";
            AInames[3] = "Timone";
            AInames[4] = "Pumba";
            AInames[5] = "Zazu";

            Random random = new Random();
            AInames = AInames.OrderByDescending(x=>random.Next(0,1000)).ToArray();
        }

        public static string pickAiName(int racerNumber)
        {
            return AInames[racerNumber];
        }

        /// <summary>
        /// Remove all players and cameras ready for a new game. Unload any content.
        /// </summary>
        public static void resetPlayers()
        {
            currentRacers = new List<Racer>();
        }

    }
}
