using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace BeatShift
{
    public class TimeTrialRace : IRaceType
    {
        public TimeTrialRace()
        {
            maxLaps = 3;
            areLapsRequired = true;
            areRanksRequired = false;

            displayCurrentLapOutofTotalLaps = true;
            displayCurrentBestLap = true;
            displayCurrentLapTime = true;
            displayMinimap = true;
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
            return "TimeTrialRace";
        }

        public override void finishRaceForTheRacer(Stopwatch finishTime, Racer racer, int _, int __)
        {
            racer.raceTiming.isRacing = false;
            racer.raceTiming.hasCompletedRace = true;
            racer.raceTiming.finalRaceTime = finishTime.ElapsedMilliseconds;

            TimeSpan ts = finishTime.Elapsed;
            racer.raceTiming.finalRaceTimeString = racer.raceTiming.convertTimeSpanToString(ts);
        }

        public override void startRaceVirtual()
        {
            totalRaceTime.Start();
            foreach (Racer racer in Race.currentRacers)
            {
                racer.raceTiming.startNewLapTimer();
            }
        }

        public override void finishWholeRace()
        {
            totalRaceTime.Stop();

            //TODO: save to file, read records from file and display a table of results
            //If faster then insert into record

            totalRaceTime.Reset();
        }
    }
}