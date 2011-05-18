using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BeatShift.Input;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using BeatShift.Util;

namespace BeatShift.Menus
{
    class ResultsMenu : IMenuPage
    {
        string[] results;
        string[] players;

        bool keyboardShown = false;

        //public double[] fastestLap;
        //public PlayerIndex[] playerIndexes;

        bool resultsCalc = false;

        Racer eliminationWinner;

        public ResultsMenu()
        {
            title = "";
        }

        public override void overrideMenuPositions()
        {
            DrawMenuItemsFromTextCentre = true;
            //DrawTitleFromTextCentre = true;
            TitlePos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, 75);
            MenuPos = new Vector2(300, 220);
            TitleStartPos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, -100);
            Offset = new Vector2(0, 50);//additional vertical spaceing.
            TextScale = 0.5f;
        }

        public override void DrawSprites(SpriteBatch spriteBatch)
        {
            Texture2D background = GameTextures.MenuBackgroundBlack;
            spriteBatch.Draw(background, new Rectangle(0, 0, 1300, background.Height / 2 + 50), Color.White);
            spriteBatch.Draw(background, new Rectangle(0, 0, 600, background.Height / 2 + 50), Color.White);
            if (resultsCalc == true && Race.currentRaceType.getRaceTypeString().Equals("LappedRace"))
            {
                for (int i = 0; i < Race.currentRacers.Count; i++)
                {
                    int offset = 70;
                    if (i == 0)
                        DrawMessageColour(BeatShift.newfont, "1st    " + results[0] + "  " + players[0], 500, 150, 0.7f, Color.Goldenrod);
                    //BeatShift.spriteBatch.DrawString(BeatShift.newfont, "1st    " + results[0] + "  " + players[0], new Vector2(500, 150), Color.Goldenrod);
                    else if (i == 1)
                        DrawMessageColour(BeatShift.newfont, "2st    " + results[1] + "  " + players[1], 500, 150 + offset, 0.7f, Color.White);
                    //BeatShift.spriteBatch.DrawString(BeatShift.newfont, "2nd    " + results[1] + "  " + players[1], new Vector2(500, 150 + offset), Color.White);
                    else if (i == 2)
                        DrawMessageColour(BeatShift.newfont, "3rd    " + results[2] + "  " + players[2], 500, 150 + offset * 2, 0.7f, Color.SaddleBrown);
                    //BeatShift.spriteBatch.DrawString(BeatShift.newfont, "3rd    " + results[2] + "  " + players[2], new Vector2(500, 150 + offset * 2), Color.SaddleBrown);
                    else if (i == 3)
                        DrawMessageColour(BeatShift.newfont, "4th    " + results[3] + "  " + players[3], 500, 150 + offset * 3, 0.7f, Color.Gainsboro);
                    //BeatShift.spriteBatch.DrawString(BeatShift.newfont, "4th    " + results[3] + "  " + players[3], new Vector2(500, 150 + offset * 3), Color.Gainsboro);
                    else if (i == 4)
                        DrawMessageColour(BeatShift.newfont, "5th    " + results[4] + "  " + players[4], 500, 150 + offset, 0.7f, Color.Gainsboro);
                    //BeatShift.spriteBatch.DrawString(BeatShift.newfont, "5th    " + results[3] + "  " + players[4], new Vector2(500, 150 + offset * 4), Color.Gainsboro);
                }
            }
            else if (Race.currentRaceType.getRaceTypeString().Equals("EliminationRace"))
            {
                DrawMessageColour(BeatShift.newfont, "Survivor " + eliminationWinner.racerID.ToString(), 500, 300, 0.8f, Color.Goldenrod);
                //BeatShift.spriteBatch.DrawString(BeatShift.newfont, "Survivor:  " + eliminationWinner.racerID.ToString(), new Vector2(500, 300), Color.Goldenrod);
            }
            else if (Race.currentRaceType.getRaceTypeString().Equals("TimeTrialRace"))
            {
                int offset = 70;
                for (int i = 0; i < Race.currentRacers.Count; i++)
                {
                    if (Race.currentRacers[i].raceTiming.fastestLap.TotalMilliseconds < MapManager.currentMap.timeTrialRanks[0])
                        DrawMessageColour(BeatShift.newfont, Race.currentRacers[i].racerID.ToString() + " " + Race.currentRacers[i].raceTiming.getBestLapTime(), 500, 150 + offset, 0.7f, Color.Aqua);
                    //BeatShift.spriteBatch.DrawString(BeatShift.newfont, Race.currentRacers[i].racerID.ToString() + " " + Race.currentRacers[i].raceTiming.getBestLapTime(), new Vector2(500, 150 + offset * i), Color.Aqua);
                    else if (Race.currentRacers[i].raceTiming.fastestLap.TotalMilliseconds < MapManager.currentMap.timeTrialRanks[1])
                        DrawMessageColour(BeatShift.newfont, Race.currentRacers[i].racerID.ToString() + " " + Race.currentRacers[i].raceTiming.getBestLapTime(), 500, 150 + offset, 0.7f, Color.Goldenrod);
                    //BeatShift.spriteBatch.DrawString(BeatShift.newfont, Race.currentRacers[i].racerID.ToString() + " " + Race.currentRacers[i].raceTiming.getBestLapTime(), new Vector2(500, 150 + offset * i), Color.Goldenrod);
                    else if (Race.currentRacers[i].raceTiming.fastestLap.TotalMilliseconds < MapManager.currentMap.timeTrialRanks[2])
                        DrawMessageColour(BeatShift.newfont, Race.currentRacers[i].racerID.ToString() + " " + Race.currentRacers[i].raceTiming.getBestLapTime(), 500, 150 + offset, 0.7f, Color.White);
                    //BeatShift.spriteBatch.DrawString(BeatShift.newfont, Race.currentRacers[i].racerID.ToString() + " " + Race.currentRacers[i].raceTiming.getBestLapTime(), new Vector2(500, 150 + offset * i), Color.White);
                    else if (Race.currentRacers[i].raceTiming.fastestLap.TotalMilliseconds < MapManager.currentMap.timeTrialRanks[3])
                        DrawMessageColour(BeatShift.newfont, Race.currentRacers[i].racerID.ToString() + " " + Race.currentRacers[i].raceTiming.getBestLapTime(), 500, 150 + offset, 0.7f, Color.SaddleBrown);
                    //BeatShift.spriteBatch.DrawString(BeatShift.newfont, Race.currentRacers[i].racerID.ToString() + " " + Race.currentRacers[i].raceTiming.getBestLapTime(), new Vector2(500, 150 + offset * i), Color.SaddleBrown);
                    else
                        DrawMessageColour(BeatShift.newfont, Race.currentRacers[i].racerID.ToString() + " Unranked", 500, 150 + offset, 0.7f, Color.Gainsboro);
                    //BeatShift.spriteBatch.DrawString(BeatShift.newfont, "Unranked " + Race.currentRacers[i].racerID.ToString(), new Vector2(500, 150 + offset * i), Color.Gainsboro);
                }
            }
            if (Race.currentRaceType.getRaceTypeString().Equals("PointsRace"))
            {
                int offset = 70;
                for (int i = 0; i < Race.currentRacers.Count; i++)
                {
                    DrawMessageColour(BeatShift.newfont, Race.currentRacers[i].racerID.ToString() + " " + Race.currentRacers[i].racerPoints.getTotalPoints(), 500, 150 + offset, 0.7f, Color.Goldenrod);
                    offset = offset + 70;
                }
            }
        }

        void keyboardCallback(IAsyncResult result)
        {
            string retval = Guide.EndShowKeyboardInput(result);

            List<HighScoreEntry> l = new List<HighScoreEntry>();

            if (retval == null)
            {
                l.Add(new HighScoreEntry("Anon.", time));
            }
            else
            {
                l.Add(new HighScoreEntry(retval, time));
                //RESET XBOX SCORES
                //l.Add(new HighScoreEntry("Anonymous", 9999999999));
                //l.Add(new HighScoreEntry("Anonymous", 9999999999));
                //l.Add(new HighScoreEntry("Anonymous", 9999999999));
                //l.Add(new HighScoreEntry("Anonymous", 9999999999));
                //l.Add(new HighScoreEntry("Anonymous", 9999999999));
                //l.Add(new HighScoreEntry("Anonymous", 9999999999));
                //l.Add(new HighScoreEntry("Anonymous", 9999999999));
                //l.Add(new HighScoreEntry("Anonymous", 9999999999));
                //l.Add(new HighScoreEntry("Anonymous", 9999999999));
                //l.Add(new HighScoreEntry("Anonymous", 9999999999));
                // Do whatever you want with the string you got from the user, which is now stored in retval
            }
            HighScore.getHighScores(MapManager.currentMap.currentMapName, raceType, l);
        }

        int raceType;
        long time = long.MaxValue;
        PlayerIndex index;

        private void calculateResults()
        {
            results = new string[Race.currentRacers.Count];

            foreach (Racer racer in Race.currentRacers)
                if (racer.raceTiming.finalRaceTime == long.MaxValue)
                    racer.raceTiming.finalRaceTimeString = "DNF";

            if (!Race.currentRaceType.getRaceTypeString().Equals("PointsRace"))
            {
                //List<HighScoreEntry> tempRes = HighScore.getHighScores(MapManager.currentMap, 1);
                raceType = 0;
                for (int i = 0; i < Race.humanRacers.Count(); i++)
                {
                    if (Race.humanRacers[i].raceTiming.fastestLap.TotalMilliseconds * 1000 < time)
                    {
                        time = (long)(Race.humanRacers[i].raceTiming.fastestLap.TotalMilliseconds * 1000);
                        index = Race.humanRacers[i].racingControls.padIndex;
                    }
                }

                List<HighScoreEntry> tempRes = HighScore.getHighScores(MapManager.currentMap.currentMapName, 0);
                
                if(time < tempRes[9].value){


                    keyboardShown = false;
                    if(!Guide.IsVisible) /*CHECK HIGHSCORE*/
                    {
                        while( !keyboardShown )
                        try 
                        {
                            Guide.BeginShowKeyboardInput(index, "Player " + (index.ToString()) + " has set a new lap record", "Please enter your name", "", keyboardCallback, (object)"dontcare");
                        }  
                        catch 
                        {
                            keyboardShown = true;
                            // create a class bool, initialize it to false, then set it to true here so you know you need to try again because it didn't work the first time,  
                            // probably because the Guide became visible between when you checked Guide.IsVisible and when you called Guide.BeginShowKeyboardInput  
                        }  
                        //ENTER NAME AND TIME INTO HIGH SCORES
                    }
                }
            }
            
            //POINTS RACE

            //SAVE

            if (Race.currentRaceType.getRaceTypeString().Equals("LappedRace"))
            {
                results = Race.currentRacers.OrderByDescending(r => r.raceTiming.finalRaceTime).Reverse().Select(r => r.raceTiming.getFinalTotalTime()).ToArray();
                players = Race.currentRacers.OrderByDescending(r => r.raceTiming.finalRaceTime).Reverse().Select(r => r.racerID.ToString()).ToArray();
                resultsCalc = true;
            }
            else if ( Race.currentRaceType.getRaceTypeString().Equals("EliminationRace") )
            {
                foreach (Racer racer in Race.currentRacers)
                {
                    if (racer.raceTiming.isLastToBeEliminated == false)
                        eliminationWinner = racer;
                }
            }
        }

        public override void enteringMenu()
        {
            resetMenuSelection();
            base.enteringMenu();
            calculateResults();
        }

        public override void setupMenuItems()
        {
            addMenuItem("Highscores", (Action)(delegate
            {
                MenuManager.setCurrentMenu(MenuPage.HighScore);
                GameLoop.setGameState(GameState.Menu);
                //MenuManager.setCurrentMenu(MenuPage.HighScore);
                
            }));
            addMenuItem("Main Menu", (Action)(delegate
            {
                GameLoop.setGameStateAndResetPlayers(GameState.Menu);
            }));
            addMenuItem("Exit", (Action)(delegate
            {
                BeatShift.singleton.Exit();
            }));

        }

    }
}
