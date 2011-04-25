using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace BeatShift
{
    // This race works by seeing how many points you can get in a certain time limit.

    public class BeatPointsRace : IRaceType
    {
        public BeatPointsRace(TimeSpan targetTimeIn)
        {
            targetTime = targetTimeIn;
            areLapsRequired = false;
            areRanksRequired = false;
            // TODO: displayPoints = true;
        }

        public override bool hasTheRacerFinished(int laps, int points, int level, bool last, Stopwatch stopwatch)
        {
            if (stopwatch.Elapsed.CompareTo(targetTime) >= 0)
                return true;
            else
                return false;
        }

        public override string getRaceTypeString()
        {
            return "BeatPointsRace";
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
                racer.raceTiming.startLapTimer();
        }
    }
}
