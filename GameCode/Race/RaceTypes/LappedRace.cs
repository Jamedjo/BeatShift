using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using BeatShift.Input;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace BeatShift
{
    // When creating this class ensure that only one racer is in the race.

    public class LappedRace : IRaceType
    {
        public LappedRace(int MAX_LAPS_IN)
        {
            maxLaps = MAX_LAPS_IN;
            areLapsRequired = true;
            areRanksRequired = true;

            displayCurrentLapOutofTotalLaps = true;
            displayCurrentBestLap = true;
            displayCurrentLapTime = true;
            displayCurrentRank = true;
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
            return "LappedRace";
        }

        public override void finishRaceForTheRacer(Stopwatch finishTime, Racer racer, int _, int __)
        {
            racer.raceTiming.isRacing = false;
            racer.raceTiming.hasCompletedRace = true;
            racer.raceTiming.finalRaceTime = finishTime.ElapsedMilliseconds;
            if (racer.racingControls.chosenInput.GetType() == typeof(PadInputManager))
            {
                GamePad.SetVibration(((PadInputManager)racer.racingControls.chosenInput).getPlayerIndex(), 0.0f, 0.0f);
//                if (((PadInputManager)racer.racingControls.chosenInput).getPlayerIndex()==PlayerIndex.One)
  //              System.Diagnostics.Debug.WriteLine("sdfg");
            }
            racer.racingControls.chosenInput = new AiInputManager(racer);

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