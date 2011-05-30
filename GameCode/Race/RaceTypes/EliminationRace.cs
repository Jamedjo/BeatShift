using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;
using BeatShift.Input;

namespace BeatShift
{
    // When creating this class ensure that only one racer is in the race.

    public class EliminationRace : IRaceType
    {
        public EliminationRace(int MAX_LAPS_IN)
        {
            maxLaps = MAX_LAPS_IN;
            areLapsRequired = true;
            areRanksRequired = true;

            displayCurrentRank = true;
            displayMinimap = true;
        }

        public override bool hasTheRacerFinished(int laps, int points, int level, bool last, Stopwatch stopwatch)
        {
            if (last)
                return true;
            else
                return false;
        }

        public override RaceType getRaceType()
        {
            return RaceType.EliminiationRace;
        }

        public override void finishRaceForTheRacer(Stopwatch finishTime, Racer racer, int _, int __)
        {
            racer.raceTiming.isRacing = false;
            racer.raceTiming.hasCompletedRace = true;
            racer.raceTiming.finalRaceTime = finishTime.ElapsedMilliseconds;
            if (racer.racingControls.chosenInput.GetType() == typeof(PadInputManager))
                GamePad.SetVibration(racer.racingControls.padIndex, 0.0f, 0.0f);

            TimeSpan ts = finishTime.Elapsed;
            racer.raceTiming.finalRaceTimeString = racer.raceTiming.convertTimeSpanToString(ts);

            // TODO: Display, you lasted ....
            // TODO: explode the ship or something and remove from physics
            
            // WARNING: this may not *cleanly* remove the ship from the race
            racer.shipDrawing.isVisible = false;
            racer.shipPhysics.removeFromPhysicsEngine();

        }

        public override void startRaceVirtual()
        {
            totalRaceTime.Start();
            foreach (Racer racer in Race.currentRacers)
                racer.raceTiming.startLapTimer();
        }
    }
}
