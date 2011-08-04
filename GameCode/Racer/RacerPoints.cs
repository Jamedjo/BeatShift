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
        private int framesTotalSinceCalc = 0;
        private double lastCalculateTime = 0;

        //private int waypointsSinceLastMessage = 0;//If very few waypoints are hit during the race//TODO:use?

        private double pointsToMessage = 0;
        private double lastMessageTime = 0;

        public SlidingPopup pointsPopupManager = new SlidingPopup(new Vector2(0, -100));


        public void newWaypointHit()
        {
            waypointsHitSinceLastCalculation++;
        }

        public void Update(GameTime gameTime, bool isBoosting, int level)
        {
            if (Race.currentRaceType.GetType() != typeof(PointsRace)) return;
            framesWithBoostSinceLastCalculation++;
            if(isBoosting) framesWithBoostSinceLastCalculation++;
            levelFramesSinceLastCalculation += (level+1);
            framesTotalSinceCalc++;

            if (gameTime.TotalGameTime.TotalMilliseconds - lastCalculateTime > 500)
            {
                //Do calculation
                double waySpeed = waypointsHitSinceLastCalculation * 3.7;
                var calc = (levelFramesSinceLastCalculation / framesTotalSinceCalc) 
                    * (framesWithBoostSinceLastCalculation / framesTotalSinceCalc) 
                    * ((waypointsHitSinceLastCalculation + 1) * waypointsHitSinceLastCalculation);
                
                // Console.WriteLine("levels:" + levelFramesSinceLastCalculation + ", boost:" + framesWithBoostSinceLastCalculation + ", wayspeed:" + waypointsHitSinceLastCalculation);
                // Console.WriteLine("newPoints:" + calc + ", totalPoints:" + points);

                //points += result of calulation
                //send message to animationHUD to display points gained (and multiplier)

                
                //if(waypointsHitSinceLastCalculation> 1&& calc>0)
                {
                    points += calc;
                    pointsToMessage += calc;
                }

                levelFramesSinceLastCalculation = 0;
                framesWithBoostSinceLastCalculation = 0;
                waypointsHitSinceLastCalculation = 0;
                framesTotalSinceCalc=0;

                lastCalculateTime = BeatShift.singleton.currentTime.TotalGameTime.TotalMilliseconds;
                
            }

            if (gameTime.TotalGameTime.TotalMilliseconds - lastMessageTime > 2000)
            {
               if(pointsToMessage>0) pointsPopupManager.addPopup(GameTextures.PointsBackground,pointsToMessage.ToString(), 500);
                pointsToMessage = 0;
                lastMessageTime = BeatShift.singleton.currentTime.TotalGameTime.TotalMilliseconds;
            }

        }

        public int getTotalPoints()
        {
            return (int)points;
        }

    }
}
