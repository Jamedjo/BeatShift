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
                foreach (Racer racer in currentRacers)
                    racer.Update(gameTime);
            }
            else
            {
                // Only update the camera if the race has not started yet
                foreach (RacerHuman racer in humanRacers)
                    racer.localCamera.Update(gameTime);
            }
        }

        public static void Draw(GameTime gameTime)
        {
            foreach (RacerHuman h_racer in humanRacers)
            {
                if (MapManager.Visible)
                    MapManager.currentMap.Draw(gameTime, h_racer.localCamera);

                if (MapManager.Visible && Options.DrawWaypoints)
                    MapManager.currentMap.drawSpheres(gameTime, h_racer.localCamera);


                foreach (Racer allRacer in currentRacers)
                {
                    if (allRacer.shipDrawing.isVisible)
                    {
                        allRacer.shipDrawing.Draw(gameTime, h_racer.localCamera, (allRacer == (Racer)h_racer));
                    }
                }

                //if(Physics.Visible)
                //    Physics.Draw(gameTime);
                HeadsUpDisplay.DrawHUD(h_racer.localCamera, h_racer, gameTime);
            }

            if (humanRacers.Count == 4)
                HeadsUpDisplay.DrawSplitBars();
        }

        public static string randomString(int length)
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
            for (int i = 0; i < numberOfAiRacers; i++)
            {
                Racer r = new Racer(new RacerId(randomString(8)), currentRacers.Count, RacerType.AI);
                currentRacers.Add(r);
                r.insertShipOnMap(RacerType.AI);
            }
            //setupViewports();
        }

        public static void setupSelectionRacers(int numberOfPlayersInSelectionScreen, Boolean areShipsVisible)
        {
            //while (!BeatShift.beatShift.IsActive) { }
            getFullListOfRacerIDsFromSignedInPeople();
            // Add 'None' racers for ship selection screen
            currentRacers.Clear();
            for (int i = 0; i < numberOfPlayersInSelectionScreen; i++) 
                
            currentRacers.Add(new RacerHuman(new RacerId(randomString(3)), currentRacers.Count, RacerType.None, humanRacers.Count, false));

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
                    humanRacers[0].localCamera.Viewport = Viewports.topViewport;
                    humanRacers[1].localCamera.Viewport = Viewports.bottomViewport;
                    break;
                case 3:
                    humanRacers[0].localCamera.Viewport = Viewports.topViewport;
                    humanRacers[1].localCamera.Viewport = Viewports.bottomLeftViewport;
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
        /// Remove all players and cameras ready for a new game. Unload any content.
        /// </summary>
        public static void resetPlayers()
        {
            currentRacers = new List<Racer>();
        }
    }
}
