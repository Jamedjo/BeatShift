using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BeatShift
{

    // Default race type has:-
    // 1) Countdown
    // 2) Continuous ranking and final ranking
    // 3) Timer for whole race
    // 4) Ending when all ships have done the

    // NB Each ship knows how many laps it has done and when it has finished

    /// <summary>
    ///  This deals with laps, poisitions (indirectly) and finishing.
    /// </summary>
    public abstract class IRaceType
    {
        // Race beginning variables
        public bool raceProcedureBegun      { get; protected set; }
        public bool actualRaceBegun         { get; protected set; }
        
        // Countdown variables
        public bool isTimerRequired         { get; protected set; }
        public Stopwatch countDownTimer     { get; protected set; }
        public int countDownInt             { get; protected set; }
        public Texture2D countdownState     { get; protected set; }
        public bool countDownRunning        { get; protected set; }
        
        // Lap variables
        public bool areLapsRequired         { get; protected set; }
        public int maxLaps                 { get; protected set; }
        public Stopwatch totalRaceTime      { get; protected set; }
        public List<ResetColumn> resettingShips { get; set; }
        List<ResetColumn> newList;

        // Rank variables
        public bool areRanksRequired        { get; protected set; }
        public List<Racer> rankings         { get; protected set; }
        public int updatePeriod             { get; private set; }
        public int lastUpdatedTimer         { get; private set; }
        public List<Racer> finalRankings    { get; protected set; }

        // Beat points variables
        public bool arePointsRequired       { get; protected set; }
        public int maxPoints;
        public TimeSpan targetTime;

        // HUD Variables
        public bool displayCurrentLapOutofTotalLaps = false;
        public bool displayCurrentBestLap = false;
        public bool displayCurrentLapTime = false;
        public bool displayCurrentRank = false;
        public bool displayMinimap = false;

        // Race ending variables
        public Stopwatch endRaceTimer { get; protected set; }
        public static bool raceOver          { get; protected set; }

        List<ResetColumn> newList = new List<ResetColumn>();

        public IRaceType()
        {
            resettingShips = new List<ResetColumn>();
            actualRaceBegun = false;
            raceProcedureBegun = false;

            countDownTimer = new Stopwatch();
            endRaceTimer = new Stopwatch();
            countdownState = GameTextures.CountdownReady;
            countDownRunning = false;
            newList = new List<ResetColumn>();

            totalRaceTime = new Stopwatch();
            updatePeriod = 400;
        }

        public virtual void Update(GameTime gameTime)
        {
            updateStandardCountdown(gameTime);
            updateStandardDuringRaceItems(gameTime);
            updateStandardFinishRaceItems(gameTime);
        }

        #region Standard Race Beginning Methods

        public virtual void startRaceProcedure()
        {
            // Not the actual race but the putting of the cars on the map before countdown
            raceProcedureBegun = true;
            raceOver = false;

            // Give each ship a ship physics
            Console.Write("Started adding physics to each ship...");
            foreach (Racer racer in Race.currentRacers)
            {
                RacerType newRacerType = racer.racerType;
                if (newRacerType == RacerType.None) newRacerType = RacerType.LocalHuman;
                racer.insertShipOnMap(newRacerType);
            }
            Console.WriteLine("Done");
            
            // Construct the rankings for each ship
            if (areRanksRequired)
            {
                rankings = new List<Racer>();
                calculateRanks();
            }

            // Begin the countdown
            countDownRunning = true;
            countDownTimer.Start();
        }

        protected void updateStandardCountdown(GameTime gameTime)
        {
            if (countDownTimer.IsRunning == true)
            {
                runStandardCountdown(gameTime, true, 6);
            }
            if (endRaceTimer.IsRunning == true)
            {
                runStandardCountdown(gameTime, false, 3);
            }
        }

        protected void runStandardCountdown(GameTime gameTime, bool startOfRace, int countdownLength)
        {
            if (startOfRace == true)
            {
                countDownInt = countdownLength - countDownTimer.Elapsed.Seconds;
                switch (countDownInt)
                {
                    case 3:
                        SoundManager.RaceStart(3);
                        countdownState = GameTextures.Countdown3;
                        break;
                    case 2:
                        SoundManager.RaceStart(2);
                        countdownState = GameTextures.Countdown2;
                        break;
                    case 1:
                        SoundManager.RaceStart(1);
                        countdownState = GameTextures.Countdown1;
                        break;
                }
            }
            else
                countDownInt = countdownLength - endRaceTimer.Elapsed.Seconds;

            if (countDownInt < 1)
            {
                if (startOfRace == true)
                {
                    countDownRunning = false;
                    countDownTimer.Reset();
                    countdownState = GameTextures.CountdownGo;
                    // Call GO sound effect
                    // Call music sound effect
                    SoundManager.RaceStart(0);
                    startRace();
                }
                else
                {
                    //SoundManager.RaceComplete();
                    GameLoop.endGame(gameTime);
                    endRaceTimer.Reset();
                }
            }
        }

        public virtual void startRace()
        {
            if (!actualRaceBegun)
            {
                actualRaceBegun = true;
                BeatShift.bgm.play();
                foreach (Racer racer in Race.currentRacers)
                {
                    racer.raceTiming.isRacing = true;
                }
                startRaceVirtual();
            }
        }

        public virtual void startRaceVirtual() { }

        #endregion

        #region Standard Mid-race Methods

        protected void updateStandardDuringRaceItems(GameTime gameTime)
        {
            if (areRanksRequired)
            {
                // Update ranks during the race at "updatePeriod" intervals
                lastUpdatedTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (!everyoneHasFinished() && lastUpdatedTimer >= updatePeriod)
                {
                    lastUpdatedTimer -= updatePeriod;
                    calculateRanks();
                }
            }
            clearOutResettingShips();
        }

        public void clearOutResettingShips()
        {
            newList.Clear();
            foreach (ResetColumn rc in Race.currentRaceType.resettingShips)
            {
                if (!(this.totalRaceTime.ElapsedMilliseconds - rc.timeFromReset > 2000f))
                {
                    newList.Add(rc);
                }
            }
            Race.currentRaceType.resettingShips = newList;
        }


        public bool everyoneHasFinished()
        {
            bool result = false;

            // False if in countdown sequence
            result = result || countDownTimer.IsRunning == true;

            // False if race not commenced
            result = result || actualRaceBegun == false;

            int countEliminated = 0;

            // False if at least one ship is still going
            if (Race.currentRaceType.getRaceTypeString().Equals("EliminationRace"))
            {
                foreach (Racer racer in Race.currentRacers)
                {
                    if (racer.raceTiming.isLastToBeEliminated == true)
                        countEliminated++;
                }
                if (countEliminated == Race.currentRacers.Count - 1)
                    return true;
                else
                    return false;
            }
            else
            {
                foreach (Racer racer in Race.currentRacers)
                {
                    result = result || racer.raceTiming.isRacing;
                }
            }

            return !result;
        }

        public void calculateRanks()
        {
            int j = 1;
            rankings.Clear();
            var shipsStillRacingToRank = new List<Racer>();
            Race.currentRacers.ForEach(ship => ship.raceTiming.updateRankingData());

            // Gather unfinished ships and set j to rank from after the last ship to have just finished
            foreach (Racer racer in Race.currentRacers)
            {
                if (!racer.raceTiming.hasCompletedRace)
                    shipsStillRacingToRank.Add(racer);
                else
                    j++;
            }

            // If there are ships still left to rank then sort by progress then nearest waypoint
            if (shipsStillRacingToRank.Count > 1)
            {
                rankings = shipsStillRacingToRank.OrderByDescending(i => i.raceTiming.currentRaceProgress).ThenBy(i => i.raceTiming.currentDistanceToNearestWaypoint).ToList();
                rankings.ForEach(i => i.raceTiming.currentRanking = j++);
            }
        }

        #endregion

        #region Standard Race Finishing Methods

        protected void updateStandardFinishRaceItems(GameTime gameTime)
        {

            // If finished: finish the race once all is said and done
            if (everyoneHasFinished())
            {
                finishWholeRace();
                // Run cinematics and results table
                // Console.WriteLine("All players finished: your ranking was - " + Players.playerShips[0].currentRanking);
            }
        }

        public virtual void finishWholeRace()
        {
            totalRaceTime.Stop();
            totalRaceTime.Reset();
            endRaceTimer.Start();
            // WARNING: May be problematic on xbox //Also causes build to fail if on PC with some xbox projects loaded
            //totalRaceTime.Restart();
        }

        // Each race type defines its own way to finish a racer
        public abstract bool hasTheRacerFinished(int laps, int points, int level, bool last, Stopwatch stopwatch);
        public abstract void finishRaceForTheRacer(Stopwatch finishTime, Racer racer, int points, int level);

        #endregion

        #region HUD Display Methods

        public abstract string getRaceTypeString();

        #endregion

        #region Standard Quitting Methods

        // At the end of the race: Race.currentRaceType.raceProcedureBegun = false
        // Remove the physics from each ship
        // Remove the map from physics
        // Race.enabled =false
        // Race.visible = false
        // delete currentRaceType?

        #endregion
    }
}
