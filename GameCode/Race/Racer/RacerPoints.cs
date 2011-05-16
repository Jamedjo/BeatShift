using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BeatShift
{
    public class RacerPoints
    {
        private double points = 0;
        private int waypointsHitSinceLastCalculation = 0;
        private int framesWithBoostSinceLastCalculation = 0;
        private double levelFramesSinceLastCalculation = 0;
        private double lastCalculateTime=0;


        public void newWaypointHit()
        {
            waypointsHitSinceLastCalculation++;
        }

        public void Update(GameTime gameTime, bool isBoosting, int level)
        {
            if(isBoosting) framesWithBoostSinceLastCalculation++;
            levelFramesSinceLastCalculation += level*level;

            if (gameTime.TotalGameTime.TotalMilliseconds - lastCalculateTime > 500)
            {
                //Do calculation
                double waySpeed = (1 + 3 * waypointsHitSinceLastCalculation) * 0.8;


                //points += result of calulation
                //send message to animationHUD to display points gained (and multiplier)

                levelFramesSinceLastCalculation = 0;
                framesWithBoostSinceLastCalculation = 0;
                waypointsHitSinceLastCalculation = 0;
                lastCalculateTime = BeatShift.singleton.currentTime.TotalGameTime.TotalMilliseconds;
            }

        }

        public double getPoints()
        {
            return points;
        }

    }
}
