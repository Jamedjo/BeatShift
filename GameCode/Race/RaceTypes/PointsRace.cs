using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using BeatShift.Input;

namespace BeatShift
{
    // When creating this class ensure that only one racer is in the race.

    public class PointsRace : IRaceType
    {
        public PointsRace(int MAX_LAPS_IN)
        {
            maxLaps = MAX_LAPS_IN;
            areLapsRequired = true;
            areRanksRequired = true;

            displayCurrentLapOutofTotalLaps = true;
            displayCurrentBestLap = false;
            displayCurrentLapTime = false;
            displayCurrentRank = true;
            displayMinimap = true;
            displayTotalPoints = true;
        }

        public override bool hasTheRacerFinished(int laps, int points, int level, bool last, Stopwatch stopwatch)
        {
            if (laps >= maxLaps)
                return true;
            else
                return false;
        }

        public override string getRaceTypeString()
        {
            return "LappedRace";
        }

        public override void finishRaceForTheRacer(Stopwatch finishTime, Racer racer, int _, int __)
        {
            racer.raceTiming.isRacing = false;
            racer.raceTiming.hasCompletedRace = true;
            racer.raceTiming.finalRaceTime = finishTime.ElapsedMilliseconds;
            racer.racingControls.setChosenInput(new AiInputManager(racer));

            TimeSpan ts = finishTime.Elapsed;
            racer.raceTiming.finalRaceTimeString = racer.raceTiming.convertTimeSpanToString(ts);
        }

        public override void startRaceVirtual()
        {
            totalRaceTime.Start();

            //CHANGE TO ALL RACEES THREW LOOP, BE RIGHT BACK TOO
            foreach(Racer racer in Race.currentRacers)
                racer.raceTiming.startLapTimer();
        }
    }
}