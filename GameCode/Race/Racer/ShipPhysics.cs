using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using BEPUphysics;
using BEPUphysics.BroadPhaseSystems;
using BEPUphysics.Collidables;
using BEPUphysics.Collidables.Events;
using BEPUphysics.Collidables.MobileCollidables;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.CollisionTests;
using BEPUphysics.CollisionTests.CollisionAlgorithms;
using BEPUphysics.DataStructures;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics.MathExtensions;
using BEPUphysics.PositionUpdating;
using BeatShift.GameDebugTools;


namespace BeatShift
{
    enum Quadrant { Front, Back, Left, Right, FrontLeft, FrontRight, BackLeft, BackRight };
    public partial class ShipPhysics //This class is split over multiple '.cs' files.
    {
        //public CompoundBody physicsBody;
        public Entity racerEntity { get { return bepuV.Vehicle.Body; } }
        public ConvexHullShape convexHull;
        public Vector3 ShipPosition { get { return racerEntity.Position; /*shipHull.CenterPosition;*/ } set { racerEntity.Position = value; /*shipHull.CenterPosition = value;*/ } }
        public Matrix DrawOrientationMatrix { get { return Matrix3X3.ToMatrix4X4(bepuV.Vehicle.Body.OrientationMatrix); } } //{ get { var rQ = Quaternion.CreateFromAxisAngle(Vector3.Forward, getRoll()); var oldQ = physicsBody.Orientation; var mulQ = oldQ * rQ; return Matrix.CreateFromQuaternion(mulQ); } }
        public Quaternion DrawOrientation { get { return bepuV.Vehicle.Body.Orientation; } } //{ get { var rQ = Quaternion.CreateFromAxisAngle(Vector3.Forward, getRoll()); var oldQ = physicsBody.Orientation; return oldQ * rQ;  } }
        public float ShipSpeed { get { return getForwardSpeed(); } }
        public float radiusForGrip = 100;
        Vector3[] stabilizerRaycastList;
        public float maxSpeed = 200f;
        Quadrant? opponentQuadrant;
        Quadrant? localQuadrant;

        public bool overturned;
        public float shipRayToTrackTime;

        float previousDirty = 1;

        Ray theRay = new Ray();
        Ray offsetRay = new Ray();

        Vector3 rayColour = Color.AntiqueWhite.ToVector3();

        private Quaternion previousOrientation;
        public Matrix ShipOrientationMatrix
        {
            get
            {
                if (float.IsNaN(racerEntity.Orientation.W))
                {
                    return Matrix.CreateFromQuaternion(previousOrientation);
                }
                return Matrix.CreateFromQuaternion(racerEntity.Orientation);
            }
            set
            {
                racerEntity.Orientation = Quaternion.CreateFromRotationMatrix(value);
            }
        }


        // Map related variables
        public MapData mapData { get; private set; }
        public MapPoint currentProgressWaypoint;
        public MapPoint nearestMapPoint;
        public MapPoint nextWaypoint;
        public Vector3 ShipTrackUp { get { return nearestMapPoint.trackUp; } }

        private Racer parentRacer;
        public bool wrongWay = false;

        public BepuVehicle bepuV;

        //Temporary AI
        int AiSpeed = 250;

        public ShipPhysics(Racer parent)
        {
            parentRacer = parent;
            initializeWaypoints();
            //constructPhysicsBody();
            //Physics.space.Add(physicsBody);
            //physicsBody.CollisionInformation.Hierarchy.CollisionInformation.CollisionRules.InitialCollisionDetected += new BEPUphysics.Events.InitialCollisionDetectedEventHandler(alertCollision); TODO: GET COLLISIONS WORKING
            
            bepuV = new BepuVehicle(Vector3.Zero,importPhysicsHull());
            bepuV.Vehicle.Body.CollisionInformation.Events.ContactCreated += new ContactCreatedEventHandler<EntityCollidable>(Events_InitialCollisionDetected);
            bepuV.Activate();
            placeShipOnStartingGrid(parentRacer.shipNumber);


            if (parentRacer.racerType == RacerType.AI)
            {
                AiSpeed = (new Random(parentRacer.shipNumber * 3 + 29)).Next(200, 290);
            }


        }

        private void initializeWaypoints()
        {
            mapData = MapManager.currentMap.CurrentMapData;
            currentProgressWaypoint = mapData.getStartPoint();
            nearestMapPoint = mapData.getStartPoint();
            nextWaypoint = mapData.nextPoint(currentProgressWaypoint);
        }


        private static List<Vector3> importPhysicsHull()
        {
            Vector3[] verts;
            int[] indics;
            Model shipPhysicsModel = BeatShift.contentManager.Load<Model>("Models/Ships/ShipPhysics");
            Matrix[] transforms = new Matrix[shipPhysicsModel.Bones.Count];
            shipPhysicsModel.CopyAbsoluteBoneTransformsTo(transforms);
            Physics.GetVerticesAndIndicesFromModelPipeline(shipPhysicsModel, out verts, out indics);
            return verts.ToList();
        }

        private void placeShipOnStartingGrid(int shipNumber)
        {
            resetShipAndFaceNextWaypoint(getStartingGridPosition(shipNumber));
        }

        public void resetShipAtLastWaypoint()
        {
            resetShipAndFaceNextWaypoint(nearestMapPoint.position);// currentProgressWaypoint.position);
        }

        private void resetShipAndFaceNextWaypoint(Vector3 newShipPosition)
        {
            if (float.IsNaN(racerEntity.Position.X) == true)
            {
                previousOrientation = racerEntity.Orientation;
            }
            //previousOrientationMatrix = physicsBody.OrientationMatrix;


            //Make ship face towards the next waypoint at start, so it faces the right way around the track.
            ShipPosition = newShipPosition;
            racerEntity.WorldTransform = Matrix.CreateWorld(newShipPosition, mapData.nextPoint(currentProgressWaypoint).position - newShipPosition, currentProgressWaypoint.trackUp);
        }

        private Vector3 getStartingGridPosition(int shipNumber)
        {
            int col = shipNumber % 3;
            int row = shipNumber / 3;

            Vector3 startPoint = mapData.getStartPoint().position;//Start above the road not in the middle of it.//TODO:make sure this is not passed by reference
            startPoint += Vector3.Transform(new Vector3(col * 14f, 0, row * 16f), Matrix.CreateWorld(Vector3.Zero, mapData.nextPoint(currentProgressWaypoint).position - startPoint, currentProgressWaypoint.trackUp));//Offset each new ships position by 5

            return startPoint;
        }

        public void applyImpulseInSurfacePlane(Vector3 impulse)
        {
            Vector3 nearestNormal = nearestMapPoint.trackUp;//TODO: use resulting normal from downwards raycast
            nearestNormal.Normalize();
            Vector3 newImpulse = impulse - (Vector3.Dot(impulse,nearestNormal) * nearestNormal);
            
            racerEntity.ApplyImpulse(racerEntity.Position, newImpulse);
        }

        public void couteractDrift()
        {
            //Apply sliding friction
            Vector3 v = racerEntity.LinearVelocity;

            if (v.Length() == 0)
                return;
            Vector3 dir = ShipOrientationMatrix.Forward;
            v.Normalize();
            //dir.Normalize();

            if (v == dir)
                return;

            //float angle = (float)Math.Acos(Vector3.Dot(v, dir) / (v.Length() * dir.Length()));

            //float speed = v.Length();
            //dir *= speed * 1f;


            //physicsBody.ApplyImpulse(physicsBody.CenterOfMass, dir);

            float anglularLength = racerEntity.AngularVelocity.Length();
            float angLenSquared = anglularLength * anglularLength;
            float centripetalF = radiusForGrip * angLenSquared;//physicsBody.Mass * r * angLenSquared;
            if (centripetalF <= 0)
                return;
            if (Vector3.Dot(v, ShipOrientationMatrix.Right) < 0)
            {
                racerEntity.ApplyImpulse(racerEntity.Position, ShipOrientationMatrix.Right * centripetalF);
            }
            else
            {
                racerEntity.ApplyImpulse(racerEntity.Position, ShipOrientationMatrix.Left * centripetalF);
            }

        }

        public void UpdateWaypoints(GameTime gameTime)
        {
            nearestMapPoint = mapData.nearestMapPoint(ShipPosition);
            if(gameTime.TotalGameTime.Milliseconds%500 ==0) nearestMapPoint.pointHit();//togle colour of nearest mapPoint every 500ms

            //Check for collision with next waypoint
            ////If distance between ship and waypoint is less than waypoint radius they have collided. (Was using the sphere around the waypoint)
            ////float distance = (ShipPosition - nextWaypoint.position).Length();
            ////if (distance < nextWaypoint.getWidth())

            //Now using the plane defined by the tangent as the waypoint
            //If the Vector from the waypoint to the ship is within 90 degrees of tangent it is on the positive side of the waypoint plane
            if (Vector3.Dot(nextWaypoint.tangent, (ShipPosition - nextWaypoint.position)) >= 0)
            {
                //nextWaypoint.pointHit();
                currentProgressWaypoint = nextWaypoint;
                if (currentProgressWaypoint.getIndex() == 0)
                {
                    //Start point has been reached, shut down the ship if it's the last lap
                    if(parentRacer.GetType()==typeof(RacerHuman))
                        SoundManager.LapComplete();
                    parentRacer.raceTiming.finishLap();//++laps
                    //TODO: shift into 
                }
                nextWaypoint = mapData.nextPoint(currentProgressWaypoint);
                parentRacer.racerPoints.newWaypointHit();
            }

            //Check for collision with wrongway waypoint (3 previous)
            //TODO: If nearestMapPoint is far away from the currentProgressWaypoint then wrong way?
            //OR actually use direction: if if direction ship faceing significantly opposite to direction to next waypoint for a long time
            //Problems with both. If wrong way for long time then when you turn around it should stop saying wrong way:
            //even if map direction is opposite by track direction not
            //even if still far away from next waypoint.
            //So use direction of change of nearest waypoint in the array of waypoints: get the index of the nearest waypoint and track its changes.

            //If distance between ship and waypoint is less than waypoint radius they have collided.
            MapPoint wrongwayPoint = mapData.wrongwayPoint(currentProgressWaypoint);
            //distance = (ShipPosition - wrongwayPoint.position).Length();
            Boolean behindWrongwaypoint = (Vector3.Dot(wrongwayPoint.tangent, (ShipPosition - wrongwayPoint.position)) < 0);

            if (Vector3.Dot(racerEntity.OrientationMatrix.Forward, nearestMapPoint.tangent) < -0.2 && parentRacer.raceTiming.isRacing)
            {
                //System.Diagnostics.Debug.WriteLine("Wrong Way!!! Point:" + wrongwayPoint.getIndex() + "\n");
                currentProgressWaypoint = wrongwayPoint;//TODO: remove this line?
                wrongWay = true;
            }
            else { wrongWay = false; }
        }

        #region Ship Raycasts


        public double millisecsLeftTillReset = 4000;
        private float currentDistanceToNearestWaypoint;
        private Boolean despawnShown = false;
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

        //    // Cast stabilisation sticks
        //    bool stabilizersHit = castStabalizerRaysAndApplyImpulses(stabalizerStickLength, 0.77f,false);
        //    castSingleRayAndApplyImpulse(Vector3.Zero, stabalizerStickLength, 0f, true);//Reduce down velocity if below float distance

        //    castSingleRayAndApplyImpulseCorrection(22, 160f * (ShipSpeed/80));
        //    // Ifne or all of the stabilizers missed the track, or were too short
        //    if (!stabilizersHit || overturned)
        //    {
                bool centreRaycastHit = castSingleRayAndApplyImpulse(Vector3.Zero, 100, 4f,true);

                

                if (!centreRaycastHit || overturned)
                {
                    //Main raycast stick failed too
                    //Ship is either upside-down or not on the track or race has not begun

                    millisecsLeftTillReset -= (BeatShift.singleton.currentTime.ElapsedGameTime.TotalMilliseconds); //* currentDistanceToNearestWaypoint);
                    //physicsBody.LinearDamping = 0.8f;

                    if (parentRacer.raceTiming.isRacing && millisecsLeftTillReset > 0)//Checked before updatewithraycasts is called
                    {

                       // Console.WriteLine("RESPAWNING...");

                        
                        parentRacer.isRespawning = true;
                        //TODO: wait a bit 
                        //currentDistanceToNearestWaypoint = (float)((nextWaypoint.position - ShipPosition).Length()) / 125; //TODO: use actual width of track

                        //physicsBody.LinearDamping = 0.7f; //BUG!!!

                        //Console.WriteLine(currentDistanceToNearestWaypoint);
                        if (!despawnShown && millisecsLeftTillReset < 700)
                        {
                            parentRacer.shipDrawing.spawn.Despawn(parentRacer.shipPhysics.ShipPosition, parentRacer.shipPhysics.DrawOrientation);
                            despawnShown = true;
                        }
                             

                    }
                    else
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
                        while(!readyToPlaceOnTrack && waypointInc < 8)
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
                            while (!readyToPlaceOnTrack && columnInc<3)
                            {
                                foreach(ResetColumn rc in Race.currentRaceType.resettingShips)
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

                        Vector3 newShipPosition = newRC.position + currentProgressWaypoint.trackUp*4;

                        ShipPosition = newShipPosition;
                        racerEntity.LinearVelocity = Vector3.Zero;
                        racerEntity.AngularVelocity = Vector3.Zero;
                        racerEntity.WorldTransform = Matrix.CreateWorld(newShipPosition, mapData.nextPoint(currentProgressWaypoint).position - newShipPosition, currentProgressWaypoint.trackUp);

                        //resetShipAtLastWaypoint(); //TODO: start some kind of animation, white noise
                        parentRacer.shipDrawing.spawn.Respawn(parentRacer.shipPhysics.ShipPosition, parentRacer.shipPhysics.DrawOrientation);
                        despawnShown = false;
                    }

                }

            //}
        }

        ///// <summary>
        ///// Casts rays in given list (the stabilizers) and only applies impulses if they all hit the track
        ///// Does this by calling
        ///// </summary>
        ///// <param name="positionOffsetList"></param>
        ///// <param name="stickLength"></param>
        ///// <param name="power"></param>
        ///// <returns></returns>
        //public Boolean castStabalizerRaysAndApplyImpulses(float stickLength, float power, bool adjustVelocity)
        //{
        //    //positionOffsetList = stabilizerRaycastList;
        //    float toi;
        //    shipRayToTrackTime = 0;

        //    for (int i = 0; i < rayCount; i++)
        //    {
        //        cSRAAI_results[i] = castSingleRay(stabilizerRaycastList[i], stickLength, power, out toi, out cSRAAI_rayTruePos[i], out cSRAAI_offsetRayPos[i], out cSRAAI_impulseVector[i], adjustVelocity);
        //        shipRayToTrackTime += toi;
        //        // Console.WriteLine(shipRayToTrackTime);
        //        //if (i == 4) castSingleForwardRayAndApplyImpulse(positionOffsetList.ElementAt(i), stickLength);
        //    }

        //    shipRayToTrackTime *= 0.25f;

        //    //Boolean allRaysHit = result.All((r) => (r == true));
        //    bool allRaysHit = true;
        //    foreach (bool r in cSRAAI_results)
        //    {
        //        if (!r)
        //        {
        //            allRaysHit = false;
        //            break;
        //        }
        //    }

        //    if (allRaysHit)
        //    {
        //        for (int i = 0; i < rayCount; i++)
        //        {
        //            physicsBody.ApplyImpulse(cSRAAI_rayTruePos[i], cSRAAI_impulseVector[i]);
        //            //parentRacer.shipDrawing.drawArrowListRays.Add(new D_Arrow { pos = cSRAAI_offsetRayPos[i], dir = rayCastDirection, col = Color.Red.ToVector3() });
        //            //parentRacer.shipDrawing.drawArrowListRays.Add(new D_Arrow { pos = cSRAAI_rayTruePos[i], dir = cSRAAI_impulseVector[i], col = Color.AntiqueWhite.ToVector3() });
        //        }
        //    }
        //    return allRaysHit;
        //}

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

        //// Used to shoot a ray forward and treat the result as if it were a shot from the front sticks
        //void castSingleRayAndApplyImpulseCorrection(float stickLength, float power)
        //{
        //    Vector3 rayTruePos;
        //    Vector3 impulseVector;
        //    float timeOfImpact;
        //    Vector3 positionOffset = frontOff;

        //    Vector3 neck = physicsBody.OrientationMatrix.Backward + physicsBody.OrientationMatrix.Up;
        //    neck.Normalize();

        //    Vector3 rayCastDirection = physicsBody.OrientationMatrix.Forward;//Raycast in opposite direction to trackUp.
        //    Boolean result;
        //    RayHit rayHit;
        //    impulseVector = Vector3.Zero;

        //    float vOffset = 0f;
        //    Vector3 verticalOffset = Vector3.Zero;

        //    //Cast ray from above ship to give leyway.
        //    vOffset = 20f;//The distance above the ship to search for the track.
        //    verticalOffset = -rayCastDirection;
        //    verticalOffset.Normalize();
        //    verticalOffset *= vOffset;
        //    stickLength += vOffset;


        //    rayTruePos = physicsBody.Position + Vector3.Transform(positionOffset, physicsBody.Orientation);
        //    offsetRay.Position = rayTruePos + verticalOffset;
        //    offsetRay.Direction = rayCastDirection;
         
        //    result = Physics.currentTrackFloor.RayCast(offsetRay, stickLength, out rayHit); //make stick length speed dependent
        //    timeOfImpact = rayHit.T;
        //    float offsetTimeOfImpact = timeOfImpact - vOffset;//Calculate ray from verticle centre of ship, not offset position.
        //    float dirtyMultiplier=1;

        //    if (offsetTimeOfImpact > 0)
        //    {
        //        dirtyMultiplier = (100 / offsetTimeOfImpact);
        //    }
            
        //    if (dirtyMultiplier<1)
        //    {
        //        dirtyMultiplier = 1;
        //    }


        //    previousDirty = MathHelper.Lerp(previousDirty, dirtyMultiplier, 0.00125f);

        //    //Console.WriteLine(previousDirty);

        //    if (result)
        //    {
        //        impulseVector = physicsBody.OrientationMatrix.Up * power;// (power + (float)Math.Min(1, previousDirty / 2.1));
        //    }

        //    //shipRayToTrackTime = timeOfImpact;

        //    if (result)
        //    {
        //        physicsBody.ApplyImpulse(physicsBody.Position, impulseVector);
        //        physicsBody.ApplyImpulse(rayTruePos, physicsBody.OrientationMatrix.Up * Math.Max(0, Math.Min(6, previousDirty)));

        //        //parentRacer.shipDrawing.drawArrowListRays.Add(new D_Arrow { pos = offsetRayPos, dir = rayCastDirection, col = Color.Red.ToVector3() });
        //        //parentRacer.shipDrawing.drawArrowListRays.Add(new D_Arrow { pos = rayTruePos, dir = impulseVector, col = Color.AntiqueWhite.ToVector3() });
        //    }

        //}

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
            float floatDistance = 2f;
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
                    //return -1 * Math.Max(offsetTimeOfImpact * 15f, 0f);
                //if (Globals.TestState == 1)
                //    return -1 * Math.Max(offsetTimeOfImpact * 30f, 0f);
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

        #endregion

        #region Race Physics

        public int getLapPercentage()
        {
            return (int)(100 * ((double)mapData.previousPoint(nextWaypoint).getIndex() / (double)mapData.mapPoints.Count));
        }

        public void getRankingData(out float raceProgress, out float nearestWaypointDistance)
        {
            raceProgress = 100f * ((float)mapData.previousPoint(nextWaypoint).getIndex() / (float)mapData.mapPoints.Count);
            raceProgress += parentRacer.raceTiming.currentLap * 100;

            nearestWaypointDistance = (float)((nextWaypoint.position - ShipPosition).Length());
        }

        public void updateRankingData()
        {
            parentRacer.raceTiming.currentRaceProgress = 100f * ((float)mapData.previousPoint(nextWaypoint).getIndex() / (float)mapData.mapPoints.Count);
            parentRacer.raceTiming.currentRaceProgress += parentRacer.raceTiming.currentLap * 100;

            parentRacer.raceTiming.currentDistanceToNearestWaypoint = (float)((nextWaypoint.position - ShipPosition).Length());
        }


        #endregion

        public void checkIfOverturned(GameTime gameTime)
        {

            // Check to see if the ship should be reenabled. This is probably not the correct place for this.
            if (parentRacer.isRespawning && parentRacer.respawnTime.CompareTo(gameTime.TotalGameTime) <= 0)
            {
                parentRacer.isRespawning = false;
            }

            Vector3 shipUp = racerEntity.OrientationMatrix.Up;
            Vector3 trackUp = nearestMapPoint.trackUp;

            shipUp.Normalize();
            trackUp.Normalize();

            overturned = Vector3.Dot(shipUp, trackUp) <= 0;
        }

        public void removeFromPhysicsEngine()
        {
            Physics.space.Remove(bepuV.Vehicle);
        }

        public float getForwardSpeed()
        {
            return Vector3.Dot(racerEntity.LinearVelocity, this.ShipOrientationMatrix.Forward); // physicsBody.OrientationMatrix.Forward);
        }

        //TODO: finish and rename!
        public float getRoll()
        {
            if (Race.currentRaceType.raceProcedureBegun)
            {
                

                // Max Roll = arctan(1/3) prbly 1/4 in reality to be sure //TODO: using code below already
                Vector3 a = Vector3.Up;
                Vector3 b = racerEntity.BufferedStates.Entity.AngularVelocity;
                float angularSize = Vector3.Dot(a, b);
                //return -(0.322f * (angularSize) / 3.5f) * 5f * (getForwardSpeed() / maxSpeed);

                parentRacer.raceTiming.previousRoll = Math.Min(5f, MathHelper.Lerp(parentRacer.raceTiming.previousRoll, -(0.322f * (angularSize) / 3.5f) * 6f * (getForwardSpeed() / maxSpeed), 0.08f));
                return parentRacer.raceTiming.previousRoll;
            }
            else
                return 0f;
        }

        //public void counteractPitching()
        //{
        //    Vector3 target = Vector3.Cross(nearestMapPoint.tangent, nearestMapPoint.trackUp);

        //    //Vector3.Lerp(camLastUp, getUp(), 0.1f);

        //    physicsBody.AngularVelocity -= target * 0.01f * (Vector3.Dot(physicsBody.AngularVelocity, target));

        //    //Quaternion.Slerp(physicsBody., Quaternion.CreateFromAxisAngle(target, 0f), 0.8f);

        //}
    }
}
