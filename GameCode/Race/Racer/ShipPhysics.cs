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


namespace BeatShift
{
    enum Quadrant { Front, Back, Left, Right, FrontLeft, FrontRight, BackLeft, BackRight };
    public class ShipPhysics
    {
        // Physics related variables
        public CompoundBody physicsBody;
        public ConvexHullShape convexHull;

        

        public Vector3 ShipPosition { get { return physicsBody.Position; /*shipHull.CenterPosition;*/ } set { physicsBody.Position = value; /*shipHull.CenterPosition = value;*/ } }
        public Matrix DrawOrientationMatrix { get { var rQ = Quaternion.CreateFromAxisAngle(Vector3.Forward, getRoll()); var oldQ = physicsBody.Orientation; var mulQ = oldQ * rQ; return Matrix.CreateFromQuaternion(mulQ); } }
        public float ShipSpeed { get { return getForwardSpeed(); } }
        public float radiusForGrip = 100;
        List<Vector3> stabilizerRaycastList;
        public float maxSpeed = 200f; //TODO: ship specific
        Quadrant? opponentQuadrant;//?means nullable, not sure why used here
        Quadrant? localQuadrant;

        private Quaternion previousOrientation;

        public Quaternion ShipOrientationQuaternion
        {
            get
            {
                if (float.IsNaN(physicsBody.Orientation.W))
                {
                    return previousOrientation;
                }
                return physicsBody.Orientation;
            }

            set
            {
                physicsBody.Orientation = value;
            }
        }

        public Matrix ShipOrientationMatrix
        {
            get
            {
                if (float.IsNaN(physicsBody.Orientation.W))
                {
                    return Matrix.CreateFromQuaternion(previousOrientation);
                }
                return Matrix.CreateFromQuaternion(physicsBody.Orientation);
            }
            set
            {
                physicsBody.Orientation = Quaternion.CreateFromRotationMatrix(value);
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

        //Temporary AI
        int AiSpeed = 216;

        public ShipPhysics(Racer parent)
        {
            parentRacer = parent;

            initializeWaypoints();

            setupStabalizerRaycastPositions();

            constructPhysicsBody();



            Physics.space.Add(physicsBody);

            //physicsBody.CollisionInformation.Hierarchy.CollisionInformation.CollisionRules.InitialCollisionDetected += new BEPUphysics.Events.InitialCollisionDetectedEventHandler(alertCollision); TODO: GET COLLISIONS WORKING
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

        private void constructPhysicsBody()
        {
            CollisionRules cr = new CollisionRules();
            //cr.Group = Physics.noSelfCollideGroup;

            //Physics.space.NarrowPhase.Pairs..Settings.CollisionDetection.CollisionGroupRules.Add(new CollisionGroupPair(noSelfCollideGroup, noSelfCollideGroup), CollisionRule.NoPair);
            cr.Personal = CollisionRule.NoSolver;
            //cr.Specific = CollisionRule.Normal.

            convexHull = new ConvexHullShape(importPhysicsHull());
            //convexHull.CollisionMargin = 0.4f;


            //BoxShape b1 = new BoxShape(2f, 0.8f, 8f);
            //b1.CollisionMargin = 0.4f;
            //BoxShape b2 = new BoxShape(4f, 1f, 6f);
            //b2.CollisionMargin = 0.4f;

            var bodies = new List<CompoundChildData>()
            {
                new CompoundChildData(new CompoundShapeEntry(convexHull, Vector3.Zero, 25f),cr)
                //new DynamicCompoundChildData(new CompoundChildData(new CompoundShapeEntry(b1, Vector3.Zero) ,cr), 15f),
                //new DynamicCompoundChildData(new CompoundChildData(new CompoundShapeEntry(b2, Vector3.Zero) ,cr), 45f)
            };

            //CompoundShapeEntry cse = new CompoundShapeEntry(convexHull, Vector3.Zero);

            //var body = new CompoundBody(

            //Build the first body

            //var cb1 = new CompoundBody(bodies);

            physicsBody = new CompoundBody(bodies, 60f);

            //physicsBody.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;

            //Console.WriteLine("SHIPcolisionmargin" + shipHull.CollisionMargin);
            physicsBody.IsAlwaysActive = true;

            //physicsBody.CenterOfMassOffset = new Vector3(0, 0f, 0);//Becareful with this as forces/impulses act from here including raycasts
            physicsBody.LinearDamping = 0.5f;//As there is rarely friction must slow ship down every update
            physicsBody.AngularDamping = 0.94f;
            physicsBody.Material.KineticFriction = 2f;

            physicsBody.CollisionInformation.Events.ContactCreated += new ContactCreatedEventHandler<EntityCollidable>(Events_InitialCollisionDetected);
        }

        void setupStabalizerRaycastPositions()
        {
            stabilizerRaycastList = new List<Vector3>();

            float shipWidth = 5f;//TODO:don't set manually
            float shipLength = 7.5f;//TODO:don't set manually
            //float shipWidth = 1.5f;//TODO:don't set manually
            //float shipLength = 5f;//TODO:don't set manually

            //Front stabilizers, not as far out as physicsBody is not that wide
            stabilizerRaycastList.Add(new Vector3(shipWidth / 4, 0f, -shipLength / 2));
            stabilizerRaycastList.Add(new Vector3(-shipWidth / 4, 0f, -shipLength / 2));

            //Back stabilizers
            stabilizerRaycastList.Add(new Vector3(shipWidth / 2, 0f, shipLength / 2));
            stabilizerRaycastList.Add(new Vector3(-shipWidth / 2, 0f, shipLength / 2));

            //// Front Middle Stabilizer
            //stabilizerRaycastList.Add(new Vector3(0f, 0f, -shipLength / 1.75f)); //TODO: should be 2f
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
            if (float.IsNaN(physicsBody.Position.X) == true)
            {
                previousOrientation = physicsBody.Orientation;
            }
            //previousOrientationMatrix = physicsBody.OrientationMatrix;


            //Make ship face towards the next waypoint at start, so it faces the right way around the track.
            ShipPosition = newShipPosition;
            physicsBody.WorldTransform = Matrix.CreateWorld(newShipPosition, mapData.nextPoint(currentProgressWaypoint).position - newShipPosition, currentProgressWaypoint.trackUp);
        }

        private Vector3 getStartingGridPosition(int shipNumber)
        {
            int col = shipNumber % 3;
            int row = shipNumber / 3;

            Vector3 startPoint = mapData.getStartPoint().position;//Start above the road not in the middle of it.//TODO:make sure this is not passed by reference
            startPoint += Vector3.Transform(new Vector3(col * 9.5f, 0, row * 14f), Matrix.CreateWorld(Vector3.Zero, mapData.nextPoint(currentProgressWaypoint).position - startPoint, currentProgressWaypoint.trackUp));//Offset each new ships position by 5

            return startPoint;
        }

        public void couteractDrift()
        {
            //Apply sliding friction
            Vector3 v = physicsBody.LinearVelocity;

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

            float anglularLength = physicsBody.AngularVelocity.Length();
            float angLenSquared = anglularLength * anglularLength;
            float centripetalF = radiusForGrip * angLenSquared;//physicsBody.Mass * r * angLenSquared;
            if (centripetalF <= 0)
                return;
            if (Vector3.Dot(v, ShipOrientationMatrix.Right) < 0)
            {
                physicsBody.ApplyImpulse(physicsBody.Position, ShipOrientationMatrix.Right * centripetalF);
            }
            else
            {
                physicsBody.ApplyImpulse(physicsBody.Position, ShipOrientationMatrix.Left * centripetalF);
            }

        }

        public void UpdateWaypoints()
        {
            nearestMapPoint = mapData.nearestMapPoint(ShipPosition, nearestMapPoint);

            //Check for collision with next waypoint
            ////If distance between ship and waypoint is less than waypoint radius they have collided. (Was using the sphere around the waypoint)
            ////float distance = (ShipPosition - nextWaypoint.position).Length();
            ////if (distance < nextWaypoint.getWidth())

            //Now using the plane defined by the tangent as the waypoint
            //If the Vector from the waypoint to the ship is within 90 degrees of tangent it is on the positive side of the waypoint plane
            if (Vector3.Dot(nextWaypoint.tangent, (ShipPosition - nextWaypoint.position)) >= 0)
            {
                nextWaypoint.pointHit();
                currentProgressWaypoint = nextWaypoint;
                if (currentProgressWaypoint.getIndex() == 0)
                {
                    //Start point has been reached, shut down the ship if it's the last lap
                    SoundManager.LapComplete();
                    parentRacer.raceTiming.finishLap();//++laps
                    //TODO: shift into 
                }
                nextWaypoint = mapData.nextPoint(currentProgressWaypoint);
            }

            //Check for collision with wrongway waypoint (3 previous)
            //TODO: If nearestMapPoint is far away from the currentProgressWaypoint then wrong way?
            //OR actually use direction: if if direction ship faceing significantly opposite to direction to next waypoint for a long time
            //Problems with both. If wrong way for long time then when you turn around it should stop saying wrong way:
            //even if map direction is opposite by track direction not
            //even if still far away from next waypoint.
            //So use direction of change of nearest waypoint in the array of waypoints: get the index of the nearest waypoint and track its changes.

            //If distance between ship and waypoint is less than waypoint radius they have collided.
            MapPoint wrongwayPoint = mapData.wrongwayPoint(mapData.wrongwayPoint(currentProgressWaypoint));
            //distance = (ShipPosition - wrongwayPoint.position).Length();
            Boolean behindWrongwaypoint = (Vector3.Dot(wrongwayPoint.tangent, (ShipPosition - wrongwayPoint.position)) < 0);
            
            if (Vector3.Dot(physicsBody.OrientationMatrix.Forward, nearestMapPoint.tangent)< -0.2 && parentRacer.raceTiming.isRacing)
            {
                //System.Diagnostics.Debug.WriteLine("Wrong Way!!! Point:" + wrongwayPoint.getIndex() + "\n");
                currentProgressWaypoint = wrongwayPoint;
                wrongWay = true;
            }
            else { wrongWay = false; }
        }

        public void UpdateFromAI()
        {
            if (parentRacer.raceTiming.isRacing == true)
            {
                physicsBody.ApplyImpulse(physicsBody.Position, Vector3.Normalize(nextWaypoint.position - ShipPosition) * AiSpeed);
            }
        }

        private double lastWallHitTime = 0;//variable is local to this function. TODO: move to top
        private double lastThisShipTime = 0;

        // public delegate void ContactCreatedEventHandler<T>(T sender, BEPUphysics.Collidables.Collidable other, BEPUphysics.NarrowPhaseSystems.Pairs.CollidablePairHandler pair, BEPUphysics.CollisionTests.ContactData contact)

        private void Events_InitialCollisionDetected(EntityCollidable sender, Collidable info, CollidablePairHandler pair, ContactData contact)
        {
            try
            {
                //var contact = pair.Contacts[0].Contact;

                // Select collisionInformation for object in contact with instead of the ships own collisionInformation
                Collidable candidate = (pair.BroadPhaseOverlap.EntryA == physicsBody.CollisionInformation ? pair.BroadPhaseOverlap.EntryB : pair.BroadPhaseOverlap.EntryA) as Collidable;
                if (candidate.Equals(Physics.currentTrackFloor))
                {

                    // Bounce up a little on collisions
                    Vector3 rayDirection = -nearestMapPoint.trackUp;
                    float downVelocityMagnitude = Vector3.Dot(physicsBody.LinearVelocity, rayDirection) / rayDirection.Length();
                    physicsBody.LinearVelocity -= rayDirection * downVelocityMagnitude;
                    physicsBody.LinearVelocity -= (rayDirection * 5f);

                    //physicsBody.ApplyLinearImpulse(-rayDirection * 2.5f);

                    //TODO: DISPLAY LOADS OF PARTICLES TO HIDE OUR CRAP PHYSICS!!!
                    //BeatShift.emitter = new ParticleEmitter((Func<Vector3>)delegate { return contacts[0].Position; }, BeatShift.settingsb, BeatShift.pEffect);
                }
                else if (candidate.Equals(Physics.currentTrackWall))
                {
                    ////Only hit wall once per collision by only ignoring hits which are close in time.
                    //if (BeatShift.singleton.currentTime.TotalGameTime.TotalMilliseconds - lastWallHitTime > 100)
                    //{
                    //    //Console.WriteLine("Creating Bounce");
                    //    lastWallHitTime = BeatShift.singleton.currentTime.TotalGameTime.TotalMilliseconds;

                    ////Stop all rotation
                    //physicsBody.AngularVelocity = new Vector3(0, 0, 0);
                    parentRacer.lastCollisionPoint= contact.Position ;
                    //Initiate controller vibration
                    parentRacer.isColliding = true;
                    SoundManager.Collision();
                    //Calculate direction to bounce
                    Vector3 v1 = Vector3.Cross(nearestMapPoint.trackUp, contact.Normal);
                    Vector3 bounceVector = Vector3.Cross(nearestMapPoint.trackUp, v1);
                    bounceVector.Normalize();

                    //Remove ships velocity towards the wall
                    //by setting velocity component in direction of bounce to 0
                    float shipVelocity_inWallBounceDirection = Vector3.Dot(physicsBody.LinearVelocity, bounceVector);// no need to divide by bounceVector.Length() as normalized
                    physicsBody.LinearVelocity -= bounceVector * shipVelocity_inWallBounceDirection;
                    //Console.WriteLine("velocityTowardsWall is " + shipVelocity_inWallBounceDirection);

                    //Slow ship down in other components
                    physicsBody.LinearVelocity *= 0.97f;

                    //Get scale of bounce based on speed ship was moving towards the wall.
                    //Minimum bounce until 5 speed, max bounce after 100;
                    float minmaxV = Math.Min(100, Math.Max(20, (Math.Abs(shipVelocity_inWallBounceDirection))));
                    float bounceScale = minmaxV / 20;//scale velocity down so max bounce is 5 times as strong as bounce at 20mph, and 20mph bounce is unscaled

                    //Apply a bounce impulse
                    physicsBody.ApplyImpulse(physicsBody.Position, bounceVector * 1200 * bounceScale);
                    //parentRacer.shipDrawing.drawArrowListPermanent.Add(new D_Arrow { pos = contact.Position, dir = bounceVector * 4f, col = Color.DarkBlue.ToVector3() });
                    //}
                }
                else
                {
                    ////Only hit wall once per collision by only ignoring hits which are close in time.
                    //if (BeatShift.singleton.currentTime.TotalGameTime.TotalMilliseconds - lastThisShipTime > 100)
                    //{
                    //    //Console.WriteLine("Creating Bounce");
                    //    lastThisShipTime = BeatShift.singleton.currentTime.TotalGameTime.TotalMilliseconds;

                    foreach (Racer r in Race.currentRacers)
                    {
                        CompoundBody collidedBody = r.shipPhysics.physicsBody;
                        if (candidate.Equals(collidedBody.CollisionInformation))
                        {
                            //Console.WriteLine("ship hit ship ");

                            Vector3 colVel = collidedBody.LinearVelocity;
                            r.raceTiming.previousSpeedOfCollidedBody = Math.Abs(colVel.Length());


                            //// Deal with on top collision (NOT TESTED)
                            //if (r.shipPhysics.shipRayToTrackTime < 23f && shipRayToTrackTime > 23f)
                            //{
                            //    Console.WriteLine("Landed on top");
                            //    float impulseLeft = Vector3.Dot(physicsBody.Position - r.shipPhysics.physicsBody.Position, physicsBody.WorldTransform.Left);
                            //    physicsBody.ApplyImpulse(physicsBody.Position, physicsBody.WorldTransform.Left * impulseLeft);

                            //}


                            /////////////////////////
                            //Find opponents quadrant
                            /////////////////////////
                            Vector3 opponentContactVector = contact.Position - collidedBody.Position;

                            //Find quarent
                            Quadrant oFL_BR = Quadrant.FrontLeft;
                            Quadrant oFR_BL = Quadrant.FrontRight;

                            //old line test was between (contactVector, collidedBody.OrientationMatrix.Forward)
                            Matrix3X3 oM = collidedBody.OrientationMatrix;//opponent Matrix
                            if (Vector3.Dot(opponentContactVector, oM.Backward + oM.Right) > 0) oFL_BR = Quadrant.BackRight;
                            if (Vector3.Dot(opponentContactVector, oM.Backward + oM.Left) > 0) oFR_BL = Quadrant.BackLeft;

                            //Quadrent? result = null;// '?' makes it nullable

                            if ((oFL_BR == Quadrant.FrontLeft) && (oFR_BL == Quadrant.FrontRight)) opponentQuadrant = Quadrant.Front;
                            else if ((oFL_BR == Quadrant.BackRight) && (oFR_BL == Quadrant.BackLeft)) opponentQuadrant = Quadrant.Back;
                            else if ((oFL_BR == Quadrant.FrontLeft) && (oFR_BL == Quadrant.BackLeft)) opponentQuadrant = Quadrant.Left;
                            else if ((oFL_BR == Quadrant.BackRight) && (oFR_BL == Quadrant.FrontRight)) opponentQuadrant = Quadrant.Right;

                            /////////////////////
                            //Find local quadrant
                            /////////////////////
                            Vector3 localContactVector = contact.Position - physicsBody.Position;

                            Vector3 contactVector = contact.Position - collidedBody.Position;

                            //Find quarent
                            Quadrant lFL_BR = Quadrant.FrontLeft;
                            Quadrant lFR_BL = Quadrant.FrontRight;

                            //old line test was between (contactVector, collidedBody.OrientationMatrix.Forward)
                            Matrix3X3 lM = collidedBody.OrientationMatrix;//opponent Matrix
                            if (Vector3.Dot(localContactVector, lM.Backward + lM.Right) > 0) lFL_BR = Quadrant.BackRight;
                            if (Vector3.Dot(localContactVector, lM.Backward + lM.Left) > 0) lFR_BL = Quadrant.BackLeft;

                            //Quadrent? result = null;// '?' makes it nullable

                            if ((lFL_BR == Quadrant.FrontLeft) && (lFR_BL == Quadrant.FrontRight)) localQuadrant = Quadrant.Front;
                            else if ((lFL_BR == Quadrant.BackRight) && (lFR_BL == Quadrant.BackLeft)) localQuadrant = Quadrant.Back;
                            else if ((lFL_BR == Quadrant.FrontLeft) && (lFR_BL == Quadrant.BackLeft)) localQuadrant = Quadrant.Left;
                            else if ((lFL_BR == Quadrant.BackRight) && (lFR_BL == Quadrant.FrontRight)) localQuadrant = Quadrant.Right;


                            //Console.WriteLine("Collision in quadrent " + result.ToString());

                            /////////////////////////////////////
                            /////////////////////////////////////
                            /////////////////////////////////////


                            Vector3 bounceVector = physicsBody.Position - collidedBody.Position;
                            bounceVector.Normalize();


                            float shipVelocity_inShipBounceDirection = Vector3.Dot(physicsBody.LinearVelocity, bounceVector);// no need to divide by bounceVector.Length() as normalized
                            Vector3 relativeVelocity = collidedBody.LinearVelocity - physicsBody.LinearVelocity;
                            float realtiveVelInShipBounceDirection = Vector3.Dot(physicsBody.LinearVelocity, bounceVector) - Vector3.Dot(collidedBody.LinearVelocity, bounceVector);// no need to divide by bounceVector.Length() as normalized

                            bool localQuadIsSide = (localQuadrant == Quadrant.Left || localQuadrant == Quadrant.Right);
                            bool opponentQuadIsSide = (opponentQuadrant == Quadrant.Left || opponentQuadrant == Quadrant.Right);

                            if (localQuadrant == Quadrant.Front)
                            {
                                var shipLength = 7.5f; //TODO: change
                                Vector3 frontOff = new Vector3(0f, 0f, -shipLength / 2);
                                var frontPost = physicsBody.Position + Vector3.Transform(frontOff, physicsBody.Orientation);
                                
                                Vector3 fusionVector = frontPost - collidedBody.Position;
                                Vector3 direction = physicsBody.OrientationMatrix.Left;
                                if (Vector3.Dot(physicsBody.OrientationMatrix.Left, fusionVector) < 0)
                                {
                                    //Bend to right
                                    direction *= -1;
                                }

                                Vector3 a = collidedBody.Position - physicsBody.Position;
                                Vector3 b = physicsBody.OrientationMatrix.Forward;

                                var tau = Math.Sin(angleBetween(a, b)) / a.Length();
                                float scaleFactor = (float)Math.Abs(2f - tau);

                                physicsBody.ApplyImpulse(physicsBody.Position, direction * (200*scaleFactor));
                            }


                            if (physicsBody.LinearVelocity.Length() < 50) //TODO: tweak
                            {
                                /////////////////
                                //Bounce in a similar way to when hitting walls

                                //Slow ship down in other components
                                physicsBody.LinearVelocity *= 0.94f;

                                //Get scale of bounce based on speed ship was moving towards the ship.
                                //Minimum bounce until 5 speed, max bounce after 100;
                                float minmaxV = Math.Min(50, Math.Max(10, (Math.Abs(shipVelocity_inShipBounceDirection))));
                                //float bounceScale = minmaxV / 20;//scale velocity down so max bounce is 5 times as strong as bounce at 20mph, and 20mph bounce is unscaled
                                physicsBody.LinearVelocity -= bounceVector * shipVelocity_inShipBounceDirection;

                                //Apply a bounce impulse
                                physicsBody.ApplyImpulse(physicsBody.Position, bounceVector * 25 * minmaxV);
                            }
                            

                            else if (localQuadIsSide && opponentQuadIsSide)
                            {
                                /////////////////
                                //Bounce in a similar way to when hitting walls

                                //Slow ship down in other components
                                physicsBody.LinearVelocity *= 0.94f;

                                //Get scale of bounce based on speed ship was moving towards the ship.
                                //Minimum bounce until 5 speed, max bounce after 100;
                                float minmaxV = Math.Min(100, Math.Max(5, (Math.Abs(shipVelocity_inShipBounceDirection))));
                                //float bounceScale = minmaxV / 20;//scale velocity down so max bounce is 5 times as strong as bounce at 20mph, and 20mph bounce is unscaled
                                physicsBody.LinearVelocity -= bounceVector * shipVelocity_inShipBounceDirection;

                                //Apply a bounce impulse
                                physicsBody.ApplyImpulse(physicsBody.Position, bounceVector * 15 * minmaxV);


                            }

                            else if ((localQuadrant == Quadrant.Front) && opponentQuadrant == Quadrant.Back)
                            {
                                /////////////////
                                //Create ramming effect

                                float shipWidth = 5f;//TODO:don't set manually //TODO: optimize
                                float shipLength = 7.5f;//TODO:don't set manually
                                Vector3 backPosRight = new Vector3(shipWidth / 2, 0f, shipLength / 2);//x+-
                                Vector3 backPosLeft = new Vector3(-shipWidth / 2, 0f, shipLength / 2);
                                var worldCoordinates = physicsBody.Position + Vector3.Transform(backPosLeft, physicsBody.Orientation);

                                physicsBody.LinearVelocity -= bounceVector * realtiveVelInShipBounceDirection;


                                // Dampens both ships' speeds
                                if (localQuadrant == Quadrant.Front) physicsBody.LinearVelocity *= 0.98f;
                                else physicsBody.LinearVelocity *= 0.92f;

                                //Create ramming effect by speeding up front ship and slowing down back ship
                                float skewfactor = 7 / 8;
                                if (localQuadrant == Quadrant.Front) skewfactor = 1 / 9;

                                //physicsBody.LinearVelocity = skewfactor * physicsBody.LinearVelocity + (1 - skewfactor) * collidedBody.LinearVelocity;
                                if (physicsBody.LinearVelocity.Length() != 0)
                                {
                                    Vector3 normalisedVel = physicsBody.LinearVelocity;
                                    normalisedVel.Normalize();
                                    physicsBody.LinearVelocity = normalisedVel * (skewfactor * physicsBody.LinearVelocity.Length() + (1 - skewfactor) * collidedBody.LinearVelocity.Length());
                                }

                                //float minmaxV = Math.Min(100, Math.Max(5, (Math.Abs(realtiveVelInShipBounceDirection))));
                                //physicsBody.ApplyImpulse(physicsBody.Position, -bounceVector * 5 * minmaxV);// * angleModifier);
                            }

                            //else if () //FrontFront, BackBack, BackLeft, BackSide, FrontSide
                            //{



                            //}
                            else
                            {
                                /////////////////
                                //Bounce in a similar way to when hitting walls, but less violently than side to sides

                                //Slow ship down in other components
                                //if (localQuadrant == Quadrant.Front) physicsBody.LinearVelocity *= 0.98f;
                                //else 
                                physicsBody.LinearVelocity *= 0.92f;

                                //Get scale of bounce based on speed ship was moving towards the ship.
                                //Minimum bounce until 5 speed, max bounce after 100;
                                float minmaxV = Math.Min(100, Math.Max(25, (Math.Abs(shipVelocity_inShipBounceDirection))));
                                //float bounceScale = minmaxV / 20;//scale velocity down so max bounce is 5 times as strong as bounce at 20mph, and 20mph bounce is unscaled
                                physicsBody.LinearVelocity -= bounceVector * shipVelocity_inShipBounceDirection;

                                //Apply a bounce impulse
                                physicsBody.ApplyImpulse(physicsBody.Position, bounceVector * 12 * minmaxV);
                            }



                            //Console.WriteLine("Collision in quadrent " + result.ToString());

                            /////////////////////////////////
                            ////Remove ships velocity towards the other ship
                            ////by setting velocity component in direction of bounce to 0
                            //float shipVelocity_inShipBounceDirection = Vector3.Dot(physicsBody.LinearVelocity, bounceVector);// no need to divide by bounceVector.Length() as normalized

                            ////Get scale of bounce based on speed ship was moving towards the ship.
                            ////Minimum bounce until 5 speed, max bounce after 100;
                            //float minmaxV = Math.Min(100, Math.Max(5, (Math.Abs(shipVelocity_inShipBounceDirection))));
                            ////float bounceScale = minmaxV / 20;//scale velocity down so max bounce is 5 times as strong as bounce at 20mph, and 20mph bounce is unscaled

                            ////Apply a bounce impulse
                            //physicsBody.ApplyImpulse(physicsBody.Position, bounceVector * 25 * minmaxV);
                            /////////////////////////////////

                            //float bounceVal = 2000f;
                            //if (!((Vector3.Dot(collidedBody.Position - contact.Position, collidedBody.OrientationMatrix.Left) < 0)))
                            //{
                            //    bounceVal = -bounceVal;
                            //    Console.Out.WriteLine("Bounce Inverted");
                            //}
                            //else
                            //    Console.Out.WriteLine("Bounce Normal");

                            //Keep the slower ship still, while pushing the other ship around it
                            if (physicsBody.LinearVelocity.Length() > collidedBody.LinearVelocity.Length())
                            {
                                //Bounce the ship to the left
                                //physicsBody.ApplyImpulse(physicsBody.Position, collidedBody.OrientationMatrix.Left * 2000);
                            }
                            r.lastCollisionPoint =  contact.Position;
                            //Initiate controller vibration
                            r.isColliding = true;
                            SoundManager.Collision();
                            //Vector3 relativeVelocity = collidedBody.LinearVelocity - physicsBody.LinearVelocity;

                            //float realtiveVelInShipBounceDirection = Vector3.Dot(physicsBody.LinearVelocity, bounceVector) - Vector3.Dot(collidedBody.LinearVelocity, bounceVector);// no need to divide by bounceVector.Length() as normalized

                            //float shipWidth = 5f;//TODO:don't set manually //TODO: optimize
                            //float shipLength = 7.5f;//TODO:don't set manually
                            //Vector3 backPosRight = new Vector3(shipWidth / 2, 0f, shipLength / 2);//x+-
                            //Vector3 backPosLeft = new Vector3(-shipWidth / 2, 0f, shipLength / 2);
                            //var worldCoordinates = physicsBody.Position + Vector3.Transform(backPosLeft, physicsBody.Orientation);

                            //Vector3.tran
                            ////Front stabilizers, not as far out as physicsBody is not that wide
                            //stabilizerRaycastList.Add(new Vector3(shipWidth / 4, 0f, -shipLength / 2));
                            //stabilizerRaycastList.Add(new Vector3(-shipWidth / 4, 0f, -shipLength / 2));

                            ////Back stabilizers
                            //stabilizerRaycastList.Add(new Vector3(shipWidth / 2, 0f, shipLength / 2));
                            //stabilizerRaycastList.Add(new Vector3(-shipWidth / 2, 0f, shipLength / 2));


                            //physicsBody.LinearVelocity -= bounceVector * realtiveVelInShipBounceDirection;


                            // Dampens both ships' speeds
                            //physicsBody.LinearVelocity *= 0.94f;

                            //Create ramming effect by speeding up front ship and slowing down back ship
                            //float skewfactor = 7 / 8;
                            //physicsBody.LinearVelocity = skewfactor * physicsBody.LinearVelocity + (1 - skewfactor) * collidedBody.LinearVelocity;
                            //if (physicsBody.LinearVelocity.Length() != 0)
                            //{
                            //    Vector3 normalisedVel = physicsBody.LinearVelocity;
                            //    normalisedVel.Normalize();
                            //    physicsBody.LinearVelocity = normalisedVel * (skewfactor * physicsBody.LinearVelocity.Length() + (1 - skewfactor) * collidedBody.LinearVelocity.Length());
                            //}
                            ////else move slightly in direction of bounce vector from other ship

                            //float minmaxV = Math.Min(100, Math.Max(5, (Math.Abs(realtiveVelInShipBounceDirection))));

                            //physicsBody.ApplyImpulse(physicsBody.Position, -bounceVector * 5 * minmaxV);// * angleModifier);

                            //TODO: fire off crap on collision
                            //BeatShift.emitter = new ParticleEmitter((Func<Vector3>)delegate { return contacts[0].Position; }, BeatShift.settingsb, BeatShift.pEffect);
                        }
                    //}
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Unfound pair");
            }
        }

        float angleBetween(Vector3 a, Vector3 b)
        {
            return (float)Math.Acos(Vector3.Dot(a,b)/(a.Length()*b.Length())     );
        }

        //bool lineTest(Vector3 one, Vector3 two)
        //{

        //}

        //bool isFrontFace(Vector3 contactPosition, CompoundBody collidedBody, Vector3 backPosLeft, Vector3 backPosRight)
        //{
        //    float angleLeft = angleBetween(backPosLeft - collidedBody.Position, collidedBody.OrientationMatrix.Forward);
        //    float angleRight = angleBetween(backPosRight - collidedBody.Position, collidedBody.OrientationMatrix.Forward);

        //    Vector3 target = contactPosition - collidedBody.Position;
        //    float angleAtBack = angleBetween(target, collidedBody.OrientationMatrix.Backward);

        //    if (angleAtBack < angleLeft || angleAtBack < angleRight)
        //    {
        //        return true;
        //    }
        //    return false;
        //}


        //bool isBackFace(Vector3 contactPosition, CompoundBody collidedBody, Vector3 backPosLeft, Vector3 backPosRight)
        //{
        //    float angleLeft = angleBetween(backPosLeft-collidedBody.Position, collidedBody.OrientationMatrix.Backward);
        //    float angleRight = angleBetween(backPosRight-collidedBody.Position, collidedBody.OrientationMatrix.Backward);

        //    Vector3 target = contactPosition - collidedBody.Position;
        //    float angleAtBack = angleBetween(target, collidedBody.OrientationMatrix.Backward);

        //    if (angleAtBack < angleLeft || angleAtBack < angleRight)
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        //float angleBetween(Vector3 one, Vector3 two)
        //{
        //    return (float)Math.Acos(Vector3.Dot(one, two) / (one.Length() * two.Length()));
        //}

        #region Ship Raycasts


        public double millisecsLeftTillReset = 4000;
        private float currentDistanceToNearestWaypoint;

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
            Boolean stabilizersHit = castStabalizerRaysAndApplyImpulses(stabilizerRaycastList, stabalizerStickLength, 0.77f);


            // Ifne or all of the stabilizers missed the track, or were too short
            if (!stabilizersHit || overturned)
            {
                Boolean centreRaycastHit = castSingleRayAndApplyImpulse(Vector3.Zero, 100, 4f);
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
                        physicsBody.WorldTransform = Matrix.CreateWorld(newShipPosition, mapData.nextPoint(currentProgressWaypoint).position - newShipPosition, currentProgressWaypoint.trackUp);

                        //resetShipAtLastWaypoint(); //TODO: start some kind of animation, white noise
                    }

                }

            }


        }

        /// <summary>
        /// Casts rays in given list (the stabilizers) and only applies impulses if they all hit the track
        /// Does this by calling
        /// </summary>
        /// <param name="positionOffsetList"></param>
        /// <param name="stickLength"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public Boolean castStabalizerRaysAndApplyImpulses(IEnumerable<Vector3> positionOffsetList, float stickLength, float power)
        {
            float toi;
            Vector3[] rayTruePos = new Vector3[positionOffsetList.Count()];
            Vector3[] offsetRayPos = new Vector3[positionOffsetList.Count()];
            Vector3[] impulseVector = new Vector3[positionOffsetList.Count()];
            Boolean[] result = new Boolean[positionOffsetList.Count()];
            shipRayToTrackTime = 0;

            for (int i = 0; i < positionOffsetList.Count(); i++)
            {
                result[i] = castSingleRay(positionOffsetList.ElementAt(i), stickLength, power, out toi, out rayTruePos[i], out offsetRayPos[i], out impulseVector[i]);
                shipRayToTrackTime += toi;
                // Console.WriteLine(shipRayToTrackTime);
                //if (i == 4) castSingleForwardRayAndApplyImpulse(positionOffsetList.ElementAt(i), stickLength);
            }

            shipRayToTrackTime *= 0.25f;

            Boolean allRaysHit = result.All((r) => (r == true));
            if (allRaysHit)
            {
                for (int i = 0; i < positionOffsetList.Count(); i++)
                {
                    physicsBody.ApplyImpulse(rayTruePos[i], impulseVector[i]);
                    //parentRacer.shipDrawing.drawArrowListRays.Add(new D_Arrow { pos = offsetRayPos, dir = rayCastDirection, col = Color.Red.ToVector3() });
                    parentRacer.shipDrawing.drawArrowListRays.Add(new D_Arrow { pos = rayTruePos[i], dir = impulseVector[i], col = Color.AntiqueWhite.ToVector3() });
                }
            }
            return allRaysHit;
        }

        public Boolean castSingleRayAndApplyImpulse(Vector3 positionOffset, float stickLength, float power)
        {
            Vector3 rayTruePos;
            Vector3 offsetRayPos;
            Vector3 impulseVector;
            float timeOfImpact;
            Boolean result = castSingleRay(positionOffset, stickLength, power, out timeOfImpact, out rayTruePos, out offsetRayPos, out impulseVector);

            shipRayToTrackTime = timeOfImpact;

            if (result)
            {
                physicsBody.ApplyImpulse(rayTruePos, impulseVector);
                //parentRacer.shipDrawing.drawArrowListRays.Add(new D_Arrow { pos = offsetRayPos, dir = rayCastDirection, col = Color.Red.ToVector3() });
                parentRacer.shipDrawing.drawArrowListRays.Add(new D_Arrow { pos = rayTruePos, dir = impulseVector, col = Color.AntiqueWhite.ToVector3() });
            }

            return result;
        }

        //public void castSingleForwardRayAndApplyImpulse(Vector3 positionOffset, float stickLength )
        //{
        //    float power;
        //    float timeOfImpact;
        //    Vector3 rayTruePos;
        //    Vector3 offsetRayPos;
        //    Vector3 impulseVector;
        //    float shipLength = 7.5f;//TODO:don't set manually

        //    //Raycast ahead and down slightly
        //    Vector3 rayCastDirection = physicsBody.OrientationMatrix.Forward;
        //    //rayCastDirection.Normalize();

        //    power = 0.33f;

        //    Boolean result;
        //    RayHit rayHit;
        //    impulseVector = Vector3.Zero;

        //    float vOffset = 0f;
        //    Vector3 verticalOffset = Vector3.Zero;

        //    //Cast ray from above ship to give leyway.
        //    vOffset = 20f;//The distance above the ship to search for the track.
        //    verticalOffset = physicsBody.OrientationMatrix.Up;
        //    verticalOffset.Normalize();
        //    verticalOffset *= vOffset;
        //    stickLength += vOffset;


        //    rayTruePos = physicsBody.Position + Vector3.Transform(positionOffset, physicsBody.Orientation);
        //    offsetRayPos = rayTruePos + verticalOffset + new Vector3(0f, 0f, -shipLength / 1.75f); // TODO: change to proper front point

        //    result = Physics.currentTrackFloor.RayCast(new Ray(offsetRayPos, rayCastDirection), stickLength, out rayHit);
        //    timeOfImpact = rayHit.T;
        //    float offsetTimeOfImpact = timeOfImpact - vOffset;//Calculate ray from verticle centre of ship, not offset position.


        //    if (result)
        //    {
        //        impulseVector = physicsBody.OrientationMatrix.Up * (float)calculateImpulseSizeFromRay(offsetTimeOfImpact, rayCastDirection) * power;
        //    }


        //    physicsBody.ApplyImpulse(rayTruePos, impulseVector);
        //    parentRacer.shipDrawing.drawArrowListRays.Add(new D_Arrow { pos = rayTruePos, dir = impulseVector, col = Color.AntiqueWhite.ToVector3() });
        //}

        public Boolean castSingleRay(Vector3 positionOffset, float stickLength, float power, out float timeOfImpact, out Vector3 rayTruePos, out Vector3 offsetRayPos, out Vector3 impulseVector)
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


            rayTruePos = physicsBody.Position + Vector3.Transform(positionOffset, physicsBody.Orientation);
            offsetRayPos = rayTruePos + verticalOffset;

            result = Physics.currentTrackFloor.RayCast(new Ray(offsetRayPos, rayCastDirection), stickLength, out rayHit);
            timeOfImpact = rayHit.T;
            float offsetTimeOfImpact = timeOfImpact - vOffset;//Calculate ray from verticle centre of ship, not offset position.


            if (result)
            {
                impulseVector = rayCastDirection * (float)calculateImpulseSizeFromRay(offsetTimeOfImpact, rayCastDirection) * power;
            }
            return result;
        }

        /// <summary>
        /// Calculates the size and +ve/-ve of the impulse to apply at a position, from the ray's time of impact
        /// </summary>
        /// <param name="offsetTimeOfImpact">Time of impact minus the verticle offset from which the rays were cast. Negative if ship below the track.</param>
        /// <param name="rayDirection">Direction the ray was cast in.</param>
        /// <returns></returns>
        public float calculateImpulseSizeFromRay(float offsetTimeOfImpact, Vector3 rayDirection)
        {
            Vector3 impulseDirection = rayDirection;
            float floatDistance = 1f;
            offsetTimeOfImpact -= floatDistance;

            //If below float height above track
            if (offsetTimeOfImpact < 0)
            {
                offsetTimeOfImpact = -offsetTimeOfImpact;
                impulseDirection = -impulseDirection;


                float velocityDownwards = Vector3.Dot(physicsBody.LinearVelocity, rayDirection) / rayDirection.Length();
                if (velocityDownwards > 0)
                {
                    //Set velocity in downwards direction to zero by redcuing overall velocity vector
                    physicsBody.LinearVelocity -= ((velocityDownwards) * rayDirection) * 0.7f;

                    //set velocity up/down component to a new value
                    float upMotionFloat = 0.2f;
                    physicsBody.LinearVelocity += (-rayDirection * upMotionFloat);
                }

                return -offsetTimeOfImpact * 1.6f;
            }
            else if (offsetTimeOfImpact > 0)
            {
                float downVelocityMagnitude = Vector3.Dot(physicsBody.LinearVelocity, rayDirection) / rayDirection.Length();

                // Gentle bobbing
                if (downVelocityMagnitude < -25f && offsetTimeOfImpact > 14)
                    physicsBody.LinearVelocity -= (rayDirection) * 0.3f;

                // Stop this thing flying into space if we're far away from the track AND going up
                if (downVelocityMagnitude < -25f && offsetTimeOfImpact > 16)
                    physicsBody.LinearVelocity -= (downVelocityMagnitude * rayDirection * 0.39f);

                // Terminal velocity based on max speed in raycast direction, once going down past terminal velocity only apply small impulses
                float terminalVelocitySpeed = 25f;
                if (downVelocityMagnitude >= terminalVelocitySpeed)
                {
                    // Setting it to zero would disable stabilizers after a fall.
                    offsetTimeOfImpact /= 12;
                }

                // Calculate impulse
                float impulse = (float)Math.Min(offsetTimeOfImpact * 9, 30);//Graph of y=x is used

                return impulse;
            }

            // Time of impact was 0 so 0 impulse
            return 0f;
        }

        // Return whether the ship is on the track
        public bool shipNearGround()
        {
            float stickLength = 40f;
            Vector3 rayCastDirection = -nearestMapPoint.trackUp;//Raycast in opposite direction to trackUp.
            rayCastDirection.Normalize();
            Boolean result;
            RayHit rayHit;
            result = Physics.currentTrackFloor.RayCast(new Ray(physicsBody.Position, rayCastDirection), stickLength, out rayHit);

            return result;
        }

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

        public bool overturned; //todo: put at top
        public float shipRayToTrackTime;

        public void checkIfOverturned(GameTime gameTime)
        {

            // Check to see if the ship should be reenabled. This is probably not the correct place for this.
            if (parentRacer.isRespawning && parentRacer.respawnTime.CompareTo(gameTime.TotalGameTime) <= 0)
            {
                parentRacer.isRespawning = false;
            }

            Vector3 shipUp = physicsBody.OrientationMatrix.Up;
            Vector3 trackUp = nearestMapPoint.trackUp;

            shipUp.Normalize();
            trackUp.Normalize();

            overturned = Vector3.Dot(shipUp, trackUp) <= 0;
        }

        public void removeFromPhysicsEngine()
        {
            Physics.space.Remove(physicsBody);
        }

        public float getForwardSpeed()
        {
            return Vector3.Dot(physicsBody.LinearVelocity, this.ShipOrientationMatrix.Forward); // physicsBody.OrientationMatrix.Forward);
        }

        //TODO: finish and rename!
        public float getRoll()
        {
            if (Race.currentRaceType.raceProcedureBegun)
            {
                

                // Max Roll = arctan(1/3) prbly 1/4 in reality to be sure //TODO: using code below already
                Vector3 a = Vector3.Up;
                Vector3 b = physicsBody.BufferedStates.Entity.AngularVelocity;
                float angularSize = Vector3.Dot(a, b);
                //return -(0.322f * (angularSize) / 3.5f) * 5f * (getForwardSpeed() / maxSpeed);

                parentRacer.raceTiming.previousRoll = Math.Min(5f, MathHelper.Lerp(parentRacer.raceTiming.previousRoll, -(0.322f * (angularSize) / 3.5f) * 6f * (getForwardSpeed() / maxSpeed), 0.08f));
                return parentRacer.raceTiming.previousRoll;
            }
            else
                return 0f;
        }

        public void counteractPitching()
        {
            Vector3 target = Vector3.Cross(nearestMapPoint.tangent, nearestMapPoint.trackUp);

            //Vector3.Lerp(camLastUp, getUp(), 0.1f);

            physicsBody.AngularVelocity -= target * 0.01f * (Vector3.Dot(physicsBody.AngularVelocity, target));

            //Quaternion.Slerp(physicsBody., Quaternion.CreateFromAxisAngle(target, 0f), 0.8f);

        }
    }
}
