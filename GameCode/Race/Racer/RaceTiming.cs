using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace BeatShift
{
    public class RaceTiming
    {
        ///Current lap time
        ///Best lap time
        ///laps so far
        ///Overall time elapsed
        ///etc.

        Racer currentRacer;

        //Time related
        public Stopwatch stopwatch;
        public TimeSpan fastedLap;
        public int currentLap;
        public int racePosition;
        public long finalRaceTime;
        public string finalRaceTimeString;

        // Ranking related variables
        public int currentRanking;
        public float rankingData;
        public float currentRaceProgress;
        public float currentDistanceToNearestWaypoint;

        // Beat Points related variables
        public int currentPoints;
        public int currentBeatLevel;

        // End-condition related variables
        public bool isRacing;
        public bool hasCompletedRace;
        public bool isLastToBeEliminated;

        // HUD related
        public float previousBoost = 0f;
        public float previousLapProgress = 0f;
        public bool displayWrongWay = false;
        public int lastUpdatedTimer;
        public String speedToDisplay = String.Format("{0:0000}", 0);
        public float previousSpeed = 0f;
        public float previousRoll = 0f;

        public RaceTiming(Racer racer)
        {
            currentRacer = racer;
            isRacing = false;
            hasCompletedRace = false;
            isLastToBeEliminated = false;
            stopwatch = new Stopwatch();
            fastedLap = new TimeSpan(0);
        }

        public void Update()
        {
            if (Race.currentRaceType.hasTheRacerFinished(currentLap, 0, 0, isLastToBeEliminated, stopwatch))
                Race.currentRaceType.finishRaceForTheRacer(Race.currentRaceType.totalRaceTime, currentRacer, currentPoints, currentBeatLevel);
        }
         
        public void finishLap()
        {
            currentLap++;
            if (currentLap == 1 || stopwatch.Elapsed.TotalMilliseconds < fastedLap.TotalMilliseconds)
                fastedLap = stopwatch.Elapsed;


            // Penultimate racer destroys the guy in last place on each lap
            int racersStillGoing = Race.currentRacers.Count;
            if (racersStillGoing-1 == racePosition)
            {
                isLastToBeEliminated = true;
            }

            resetLapTimer();
        }

        public string convertTimeSpanToString(TimeSpan ts)
        {
            string elapsedTime = String.Format("{1:00}:{2:00}:{3:00}",
                                 ts.Hours, ts.Minutes, ts.Seconds,
                                 ts.Milliseconds / 10);
            return elapsedTime;
        }


        public string getCurrentLapTime()
        {
            TimeSpan ts = stopwatch.Elapsed;
            return convertTimeSpanToString(ts);
        }

        public string getBestLapTime()
        {
            return convertTimeSpanToString(fastedLap);
        }

        public string getFinalTotalTime()
        {
            return finalRaceTimeString;
        }

        public void startLapTimer()
        {
            stopwatch.Start();
        }

        public void stopLapTimer()
        {
            stopwatch.Stop();
        }

        public void resetLapTimer()
        {
            stopwatch.Stop();
            stopwatch.Reset();
            stopwatch.Start();
            //stopwatch.Restart(); // WARNING: May be problematic on xbox //Also causes build to fail if on PC with some xbox projects loaded
        }

        public void updateRankingData()
        {
            if (currentRacer.shipPhysics == null) return;
            currentRaceProgress = 100f * ((float)currentRacer.shipPhysics.mapData.previousPoint(currentRacer.shipPhysics.nextWaypoint).getIndex() / (float)currentRacer.shipPhysics.mapData.mapPoints.Count);
            currentRaceProgress += currentLap * 100;

            currentDistanceToNearestWaypoint = (float)((currentRacer.shipPhysics.nextWaypoint.position - currentRacer.shipPhysics.ShipPosition).Length());
        }
    }
}
