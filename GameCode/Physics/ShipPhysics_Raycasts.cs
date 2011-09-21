using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;

namespace BeatShift
{
    ///This is an extension of ShipPhysics.cs
    public partial class ShipPhysics
    {

        const int rayCount = 4;
        Vector3[] cSRAAI_rayTruePos = new Vector3[rayCount];
        Vector3[] cSRAAI_offsetRayPos = new Vector3[rayCount];
        Vector3[] cSRAAI_impulseVector = new Vector3[rayCount];
        Boolean[] cSRAAI_results = new Boolean[rayCount];

        Vector3 frontOff;

        public double millisecsLeftTillReset = 4000;
        //private float currentDistanceToNearestWaypoint;
        private Boolean despawnShown = false;


        void setupStabalizerRaycastPositions()
        {
            stabilizerRaycastList = new Vector3[rayCount];//new List<Vector3>();

            float shipWidth = 5f;//TODO:don't set manually
            float shipLength = 7.5f;//TODO:don't set manually
            //float shipWidth = 1.5f;//TODO:don't set manually
            //float shipLength = 5f;//TODO:don't set manually 
            
            //backPosRight = new Vector3(shipWidth / 2, 0f, shipLength / 2);
            //backPosLeft = new Vector3(-shipWidth / 2, 0f, shipLength / 2);
            frontOff = new Vector3(0f, 0f, -shipLength / 2);


            //Front stabilizers, not as far out as physicsBody is not that wide
            stabilizerRaycastList[0] = new Vector3(shipWidth / 4, 0f, -shipLength / 2);
            stabilizerRaycastList[1] = new Vector3(-shipWidth / 4, 0f, -shipLength / 2);

            //Back stabilizers
            stabilizerRaycastList[2] = new Vector3(shipWidth / 2, 0f, shipLength / 2);
            stabilizerRaycastList[3] = new Vector3(-shipWidth / 2, 0f, shipLength / 2);

            //// Front Middle Stabilizer
            //stabilizerRaycastList.Add(new Vector3(0f, 0f, -shipLength / 1.75f)); //TODO: should be 2f
        }

        /// <summary>
        /// Main raycast function. Gets called in update.
        /// </summary>
        /// <param name="gameTime"></param>
        public void UpdateWithRayCasts(GameTime gameTime)
        {
            //If the race hasn't begun don't raycast
            //if (!isRacing)

            // The max distance where ship should try keep it's orientation matched to the track beneth it.
            float stabalizerStickLength = 40f;

            // Cast stabilisation sticks
            bool stabilizersHit = castStabalizerRaysAndApplyImpulses(stabalizerStickLength, 0.77f,false);
            castSingleRayAndApplyImpulse(Vector3.Zero, stabalizerStickLength, 0f, true);//Reduce down velocity if below float distance

            castSingleRayAndApplyImpulseCorrection(22, 160f * (ShipSpeed/80));

            //predictivelyAdaptOrientation();

            // Ifne or all of the stabilizers missed the track, or were too short
            if (!stabilizersHit || overturned)
            {
                bool centreRaycastHit = castSingleRayAndApplyImpulse(Vector3.Zero, 100, 4f, true);

                if (!centreRaycastHit || overturned)
                {
                    //Main raycast stick failed too
                    //Ship is either upside-down or not on the track or race has not begun

                    dealWithShipOffTrack();
                }
            }
        }

        private void predictivelyAdaptOrientation()
        {
            // Raycast diagonally in the direction of motion. (Down*scaleFactor)+(linearVelocity/scaleFactor2),
            // means that the predictive look-ahead works for any motion direction instead of just forwards.
            Vector3 downDirection = racerEntity.OrientationMatrix.Down;//-nearestMapPoint.trackUp;
            Vector3 rayCastDirection = (downDirection * 1f) + (racerEntity.LinearVelocity*0.5f);//Raycast in opposite direction to trackUp.
            Boolean result;
            RayHit rayHit;

            float rayLength = rayCastDirection.Length();
            rayCastDirection.Normalize();

            theRay.Direction = rayCastDirection;
            theRay.Position = racerEntity.Position;
            result = Physics.currentTrackFloor.RayCast(theRay, rayLength, out rayHit);



            if (result)
            {
                //Use normal to calculate angular impulse needed to rotate ship towards a new orientation around the axis defined by the roadSurface vector.

                //Avaliable variables
                //rayHit.Normal, rayHit.T, rayCastDirection
                //nearestMapPoint.roadSurface, tangent, trackUp
                //racerEntity.OrientationMatrix.Up

                //TODO: Also need to add debug graphics for testing this.

                float angle = angleBetween(racerEntity.OrientationMatrix.Up, rayHit.Normal);//TODO: Should probably use the Up from the area of track below the ship instead. Atm, if the ship is pointing towards the track on a flat section; this code creates a large angle/impulse.
                Vector3 angularImpulse = angle * racerEntity.OrientationMatrix.Right * 15f;//TODO: must work out if we should be tilting up or down each time we calculate this (currently only correct when track goes uphill, fails when rayHit detects a downhill, but we could just not do anything when that is the case. Note that up/downhill is based on the motion direction more than the ships orientation)


                Physics.ApplyAngularImpulse(ref angularImpulse, racerEntity);

            }
        }

        private void dealWithShipOffTrack()
        {
            millisecsLeftTillReset -= (BeatShift.singleton.currentTime.ElapsedGameTime.TotalMilliseconds); //* currentDistanceToNearestWaypoint);
            //physicsBody.LinearDamping = 0.8f;

            if (parentRacer.raceTiming.isRacing && millisecsLeftTillReset > 0)//Checked before updatewithraycasts is called
            {
                parentRacer.isRespawning = true;
                //TODO: wait a bit 
                //currentDistanceToNearestWaypoint = (float)((nextWaypoint.position - ShipPosition).Length()) / 125; //TODO: use actual width of track
                if (!despawnShown && millisecsLeftTillReset < 700)
                {
                    parentRacer.shipDrawing.spawn.Despawn(parentRacer.shipPhysics.ShipPosition, parentRacer.shipPhysics.DrawOrientation);
                    despawnShown = true;
                }
            }
            else
            {
                resetShipOnTrack();
            }
        }

        private void resetShipOnTrack()
        {
            parentRacer.isRespawning = false;
            millisecsLeftTillReset = 5000;
            //public struct ResetColumn { public Vector3 pos; int column; int resetWaypointIncrement; }

            Vector3 originalPos = currentProgressWaypoint.position;
            Vector3 putativePos = currentProgressWaypoint.position; //nearestMapPoint.position;

            float shipLength = 1.5f;//TODO:don't set manually
            int columnInc = 0;
            int waypointInc = 0;
            bool readyToPlaceOnTrack = false;
            ResetColumn newRC = new ResetColumn(putativePos, parentRacer.raceTiming.stopwatch.ElapsedMilliseconds);

            // On each iteration - 5 horizontal slots
            while (!readyToPlaceOnTrack && waypointInc < 8)
            {
                putativePos -= 2 * shipLength * currentProgressWaypoint.tangent;
                // Start at current way point, go left and right to find an empty spot, then start going forward.
                if (Race.currentRaceType.resettingShips.Count == 0)
                {
                    readyToPlaceOnTrack = true;
                    newRC.position = putativePos;
                    newRC.timeFromReset = parentRacer.raceTiming.stopwatch.ElapsedMilliseconds;
                }

                // Go across 5
                while (!readyToPlaceOnTrack && columnInc < 3)
                {
                    foreach (ResetColumn rc in Race.currentRaceType.resettingShips)
                    {
                        if ((rc.position.X - putativePos.X) < shipLength && (rc.position.Y - putativePos.Y) < shipLength && (rc.position.Z - putativePos.Z) < shipLength)
                        {
                            readyToPlaceOnTrack = false;

                        }
                        else
                        {
                            // Found a good location to place the ship
                            readyToPlaceOnTrack = true;
                            newRC.position = putativePos;
                            newRC.timeFromReset = parentRacer.raceTiming.stopwatch.ElapsedMilliseconds;
                            //newRC.column = columnInc;
                            //newRC.resetWaypointIncrement = resetWaypointInc;}
                        }
                    }

                    putativePos += shipLength * currentProgressWaypoint.tangent;
                    columnInc++;
                }
                columnInc = 0;
                waypointInc++;
                currentProgressWaypoint = mapData.nextPoint(currentProgressWaypoint);
            }

            Race.currentRaceType.resettingShips.Add(newRC);

            Vector3 newShipPosition = newRC.position + currentProgressWaypoint.trackUp * 4;

            ShipPosition = newShipPosition;
            racerEntity.LinearVelocity = Vector3.Zero;
            racerEntity.AngularVelocity = Vector3.Zero;
            racerEntity.WorldTransform = Matrix.CreateWorld(newShipPosition, mapData.nextPoint(currentProgressWaypoint).position - newShipPosition, currentProgressWaypoint.trackUp);

            //resetShipAtLastWaypoint(); //TODO: start some kind of animation, white noise
            parentRacer.shipDrawing.spawn.Respawn(parentRacer.shipPhysics.ShipPosition, parentRacer.shipPhysics.DrawOrientation);
            despawnShown = false;
        }

        /// <summary>
        /// Casts rays in given list (the stabilizers) and only applies impulses if they all hit the track
        /// Does this by calling
        /// </summary>
        /// <param name="positionOffsetList"></param>
        /// <param name="stickLength"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public Boolean castStabalizerRaysAndApplyImpulses(float stickLength, float power, bool adjustVelocity)
        {
            //positionOffsetList = stabilizerRaycastList;
            float toi;
            shipRayToTrackTime = 0;

            for (int i = 0; i < rayCount; i++)
            {
                cSRAAI_results[i] = castSingleRay(stabilizerRaycastList[i], stickLength, power, out toi, out cSRAAI_rayTruePos[i], out cSRAAI_offsetRayPos[i], out cSRAAI_impulseVector[i], adjustVelocity);
                shipRayToTrackTime += toi;
                // Console.WriteLine(shipRayToTrackTime);
                //if (i == 4) castSingleForwardRayAndApplyImpulse(positionOffsetList.ElementAt(i), stickLength);
            }

            shipRayToTrackTime *= 0.25f;

            //Boolean allRaysHit = result.All((r) => (r == true));
            bool allRaysHit = true;
            foreach (bool r in cSRAAI_results)
            {
                if (!r)
                {
                    allRaysHit = false;
                    break;
                }
            }

            if (allRaysHit)
            {
                for (int i = 0; i < rayCount; i++)
                {
                    racerEntity.ApplyImpulse(cSRAAI_rayTruePos[i], cSRAAI_impulseVector[i]);
                    //parentRacer.shipDrawing.drawArrowListRays.Add(new D_Arrow { pos = cSRAAI_offsetRayPos[i], dir = rayCastDirection, col = Color.Red.ToVector3() });
                    //parentRacer.shipDrawing.drawArrowListRays.Add(new D_Arrow { pos = cSRAAI_rayTruePos[i], dir = cSRAAI_impulseVector[i], col = Color.AntiqueWhite.ToVector3() });
                }
            }
            return allRaysHit;
        }

        public Boolean castSingleRayAndApplyImpulse(Vector3 positionOffset, float stickLength, float power, bool adjustVelocity)
        {
            Vector3 rayTruePos;
            Vector3 offsetRayPos;
            Vector3 impulseVector;
            float timeOfImpact;
            Boolean result = castSingleRay(positionOffset, stickLength, power, out timeOfImpact, out rayTruePos, out offsetRayPos, out impulseVector, adjustVelocity);

            shipRayToTrackTime = timeOfImpact;

            if (result)
            {
                racerEntity.ApplyImpulse(rayTruePos, impulseVector);
                //parentRacer.shipDrawing.drawArrowListRays.Add(new D_Arrow { pos = offsetRayPos, dir = rayCastDirection, col = Color.Red.ToVector3() });
                //parentRacer.shipDrawing.drawArrowListRays.Add(new D_Arrow { pos = rayTruePos, dir = impulseVector, col = Color.AntiqueWhite.ToVector3() });
            }

            return result;
        }

        // Used to shoot a ray forward and treat the result as if it were a shot from the front sticks
        void castSingleRayAndApplyImpulseCorrection(float stickLength, float power)
        {
            Vector3 rayTruePos;
            Vector3 impulseVector;
            float timeOfImpact;
            Vector3 positionOffset = frontOff;

            Vector3 neck = racerEntity.OrientationMatrix.Backward + racerEntity.OrientationMatrix.Up;
            neck.Normalize();

            Vector3 rayCastDirection = racerEntity.OrientationMatrix.Forward;//Raycast in opposite direction to trackUp.
            Boolean result;
            RayHit rayHit;
            impulseVector = Vector3.Zero;

            float vOffset = 0f;
            Vector3 verticalOffset = Vector3.Zero;

            //Cast ray from above ship to give leyway.
            vOffset = 20f;//The distance above the ship to search for the track.
            verticalOffset = -rayCastDirection;
            verticalOffset.Normalize();
            verticalOffset *= vOffset;
            stickLength += vOffset;


            rayTruePos = racerEntity.Position + Vector3.Transform(positionOffset, racerEntity.Orientation);
            offsetRay.Position = rayTruePos + verticalOffset;
            offsetRay.Direction = rayCastDirection;

            result = Physics.currentTrackFloor.RayCast(offsetRay, stickLength, out rayHit); //make stick length speed dependent
            timeOfImpact = rayHit.T;
            float offsetTimeOfImpact = timeOfImpact - vOffset;//Calculate ray from verticle centre of ship, not offset position.
            float dirtyMultiplier = 1;

            if (offsetTimeOfImpact > 0)
            {
                dirtyMultiplier = (100 / offsetTimeOfImpact);
            }

            if (dirtyMultiplier < 1)
            {
                dirtyMultiplier = 1;
            }


            previousDirty = MathHelper.Lerp(previousDirty, dirtyMultiplier, 0.00125f);

            //Console.WriteLine(previousDirty);

            if (result)
            {
                impulseVector = racerEntity.OrientationMatrix.Up * power;// (power + (float)Math.Min(1, previousDirty / 2.1));
            }

            //shipRayToTrackTime = timeOfImpact;

            if (result)
            {
                racerEntity.ApplyImpulse(racerEntity.Position, impulseVector);
                racerEntity.ApplyImpulse(rayTruePos, racerEntity.OrientationMatrix.Up * Math.Max(0, Math.Min(6, previousDirty)));

                //parentRacer.shipDrawing.drawArrowListRays.Add(new D_Arrow { pos = offsetRayPos, dir = rayCastDirection, col = Color.Red.ToVector3() });
                //parentRacer.shipDrawing.drawArrowListRays.Add(new D_Arrow { pos = rayTruePos, dir = impulseVector, col = Color.AntiqueWhite.ToVector3() });
            }

        }

        public Boolean castSingleRay(Vector3 positionOffset, float stickLength, float power, out float timeOfImpact, out Vector3 rayTruePos, out Vector3 offsetRayPos, out Vector3 impulseVector, bool adjustVelocity)
        {
            //Vector3 rayCastDirection = Matrix.Identity.Down;//Always raycast in gravity direction.
            //Vector3 rayCastDirection = ship.OrientationMatrix.Down;//Raycast down from ship
            Vector3 rayCastDirection = -nearestMapPoint.trackUp;//Raycast in opposite direction to trackUp.
            Boolean result;
            RayHit rayHit;
            impulseVector = Vector3.Zero;

            float vOffset = 0f;
            Vector3 verticalOffset = Vector3.Zero;

            //Cast ray from above ship to give leyway.
            vOffset = 20f;//The distance above the ship to search for the track.
            verticalOffset = -rayCastDirection;
            verticalOffset.Normalize();
            verticalOffset *= vOffset;
            stickLength += vOffset;


            rayTruePos = racerEntity.Position + Vector3.Transform(positionOffset, racerEntity.Orientation);
            offsetRayPos = rayTruePos + verticalOffset;

            theRay.Direction = rayCastDirection;
            theRay.Position = offsetRayPos;
            result = Physics.currentTrackFloor.RayCast(theRay, stickLength, out rayHit);
            timeOfImpact = rayHit.T;
            float offsetTimeOfImpact = timeOfImpact - vOffset;//Calculate ray from verticle centre of ship, not offset position.


            if (result)
            {
                impulseVector = rayCastDirection * (float)calculateImpulseSizeFromRay(offsetTimeOfImpact, rayCastDirection, adjustVelocity) * power;
            }
            return result;
        }

        /// <summary>
        /// Calculates the size and +ve/-ve of the impulse to apply at a position, from the ray's time of impact
        /// </summary>
        /// <param name="offsetTimeOfImpact">Time of impact minus the verticle offset from which the rays were cast. Negative if ship below the track.</param>
        /// <param name="rayDirection">Direction the ray was cast in.</param>
        /// <returns></returns>
        public float calculateImpulseSizeFromRay(float offsetTimeOfImpact, Vector3 rayDirection, bool adjustVelocity)
        {
            Vector3 impulseDirection = rayDirection;
            float floatDistance = 1.5f;
            offsetTimeOfImpact -= floatDistance;

            //If below float height above track
            if (offsetTimeOfImpact < 0f)
            {
                offsetTimeOfImpact = -offsetTimeOfImpact;
                impulseDirection = -impulseDirection;

                //if (adjustVelocity)
                //{
                //    float velocityDownwards = Vector3.Dot(physicsBody.LinearVelocity, rayDirection) / rayDirection.Length();
                //    if (velocityDownwards > 0f)
                //    {
                //        //Set velocity in downwards direction to zero by redcuing overall velocity vector
                //        physicsBody.LinearVelocity -= ((velocityDownwards) * rayDirection) * 0.7f;

                //        //set velocity up/down component to a new value
                //        float upMotionFloat = 0.2f;
                //        physicsBody.LinearVelocity += (-rayDirection * upMotionFloat);
                //    }
                //}

                //if (Globals.TestState == 0)
                return -1 * Math.Max(offsetTimeOfImpact * 15f, 0f);
                //if (Globals.TestState == 1)
               //     return -1 * Math.Max(offsetTimeOfImpact * 30f, 0f);
                //if (Globals.TestState == 2)
                //    return -1 * Math.Max(offsetTimeOfImpact * 45f, 0f);
                //if (Globals.TestState == 3)
                //    return -1 * Math.Max(offsetTimeOfImpact * 60f, 0f);
                //if (Globals.TestState == 4)
                //  return -1 * Math.Max(offsetTimeOfImpact * 90f, 0f);
                //if (Globals.TestState == 5)
                //    return -1 * Math.Max(offsetTimeOfImpact * 150f, 0f);
                //if (Globals.TestState == 6)
                //    return -1 * Math.Max(offsetTimeOfImpact * 1500f, 0f);
                ////return -offsetTimeOfImpact * 1.6f;
            }
            else if (offsetTimeOfImpact > 0f)
            {
                float downVelocityMagnitude = Vector3.Dot(racerEntity.LinearVelocity, rayDirection) / rayDirection.Length();

                //if(adjustVelocity)
                //{
                //    // Gentle bobbing
                //    if (downVelocityMagnitude < -25f && offsetTimeOfImpact > 14f)
                //        physicsBody.LinearVelocity -= (rayDirection) * 0.3f;

                //    // Stop this thing flying into space if we're far away from the track AND going up
                //    if (downVelocityMagnitude < -25f && offsetTimeOfImpact > 16f)
                //        physicsBody.LinearVelocity -= (downVelocityMagnitude * rayDirection * 0.39f);
                //}

                // Terminal velocity based on max speed in raycast direction, once going down past terminal velocity only apply small impulses
                float terminalVelocitySpeed = 25f;
                if (downVelocityMagnitude >= terminalVelocitySpeed)
                {
                    // Setting it to zero would disable stabilizers after a fall.
                    offsetTimeOfImpact /= 12f;
                }

                //// Stop it going thru the floor
                //if (offsetTimeOfImpact < 6)
                //{
                //    offsetTimeOfImpact *= 80 / offsetTimeOfImpact;
                //}

                // Calculate impulse
                float impulse;
                //if (Globals.TestState == 0)
                //  impulse = (float)Math.Min(offsetTimeOfImpact * 90f, 80f);//Graph of y=x is used //If nearestMapPoint is tagged with Jump, then reduce this max to 30
                //else if (Globals.TestState == 1)
                impulse = (float)Math.Min(offsetTimeOfImpact * 9f, 30f);//Graph of y=x is used
                //else if (Globals.TestState == 2)
                //    impulse = (float)Math.Min(offsetTimeOfImpact * 15f, 60f);//Graph of y=x is used
                //else if (Globals.TestState == 3)
                //    impulse = (float)Math.Min(offsetTimeOfImpact * 60f, 100f);//Graph of y=x is used
                //else if (Globals.TestState == 4)
                //    impulse = (float)Math.Min(offsetTimeOfImpact * 900f, 70f);//Graph of y=x is used
                //else if (Globals.TestState == 5)
                //    impulse = (float)Math.Min(offsetTimeOfImpact * 90f, 3000f);//Graph of y=x is used
                //else //(Globals.TestState == 6)
                //    impulse = (float)Math.Min(offsetTimeOfImpact * 90f, 30000f);//Graph of y=x is used

                return impulse;
            }

            // Time of impact was 0 so 0 impulse
            return 0f;
        }

        //// Return whether the ship is on the track
        //public bool shipNearGround()
        //{
        //    float stickLength = 40f;
        //    Vector3 rayCastDirection = -nearestMapPoint.trackUp;//Raycast in opposite direction to trackUp.
        //    rayCastDirection.Normalize();
        //    Boolean result;
        //    RayHit rayHit;
        //    result = Physics.currentTrackFloor.RayCast(new Ray(physicsBody.Position, rayCastDirection), stickLength, out rayHit);

        //    return result;
        //}

    }
}
