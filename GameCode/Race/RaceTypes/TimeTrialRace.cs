using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using BeatShift.Input;
using Microsoft.Xna.Framework.Input;

namespace BeatShift
{
    public class TimeTrialRace : IRaceType
    {
        public TimeTrialRace()
        {
            maxLaps = 1;
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
            if (racer.racingControls.chosenInput.GetType() == typeof(PadInputManager))
                GamePad.SetVibration(((PadInputManager)racer.racingControls.chosenInput).getPlayerIndex(), 0.0f, 0.0f);
            racer.racingControls.chosenInput = new AiInputManager(racer);

            String ts = racer.raceTiming.getBestLapTime();
            racer.raceTiming.finalRaceTimeString = ts ;
        }

        public override void startRaceVirtual()
        {
            totalRaceTime.Start();
            foreach (Racer racer in Race.currentRacers)
            {
                racer.raceTiming.startLapTimer();
            }
        }

        /*public override void finishWholeRace()
        {
            totalRaceTime.Stop();

            //TODO: save to file, read records from file and display a table of results
            //If faster then insert into record

            totalRaceTime.Reset();
        }*/
    }
}