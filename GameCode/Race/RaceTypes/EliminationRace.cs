﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

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

        public override string getRaceTypeString()
        {
            return "EliminationRace";
        }

        public override void finishRaceForTheRacer(Stopwatch finishTime, Racer racer, int _, int __)
        {
            racer.raceTiming.isRacing = false;
            racer.raceTiming.hasCompletedRace = true;
            racer.raceTiming.finalRaceTime = finishTime.ElapsedMilliseconds;

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
            Race.currentRacers[0].raceTiming.startNewLapTimer();
        }
    }
}
