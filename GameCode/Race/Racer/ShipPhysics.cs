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
        public Entity racerEntity { get { return physicsEntityBody; } }
        private CompoundBody physicsEntityBody;

        public Vector3 ShipPosition { get { return racerEntity.Position; /*shipHull.CenterPosition;*/ } set { racerEntity.Position = value; /*shipHull.CenterPosition = value;*/ } }
        public Matrix DrawOrientationMatrix { get { var rQ = Quaternion.CreateFromAxisAngle(Vector3.Forward, getRoll()); var oldQ = physicsEntityBody.Orientation; var mulQ = oldQ * rQ; return Matrix.CreateFromQuaternion(mulQ); } }
        public Quaternion DrawOrientation { get { var rQ = Quaternion.CreateFromAxisAngle(Vector3.Forward, getRoll()); var oldQ = physicsEntityBody.Orientation; return oldQ * rQ; } }
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


        //Temporary AI
        int AiSpeed = 250;

        public ShipPhysics(Racer parent)
        {
            parentRacer = parent;
            initializeWaypoints();
            setupStabalizerRaycastPositions();
            constructPhysicsBody();

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

        private void constructPhysicsBody()
        {
            ConvexHullShape hull = new ConvexHullShape(importPhysicsHull());
            hull.CollisionMargin = 0.6f;
            var bodies = new List<CompoundShapeEntry>()
                {
                    new CompoundShapeEntry(hull, Vector3.Zero, 25f)
                };

            physicsEntityBody = new CompoundBody(bodies, 60f);
            //body.CollisionInformation.LocalPosition = new Vector3(0, .5f, 0);//Moves center of gravity position to adjust stability.

            physicsEntityBody.IsAlwaysActive = true;

            //physicsBody.CenterOfMassOffset = new Vector3(0, 0f, 0);//Becareful with this as forces/impulses act from here including raycasts
            physicsEntityBody.LinearDamping = 0.5f;//As there is rarely friction must slow ship down every update
            physicsEntityBody.AngularDamping = 0.94f;
            physicsEntityBody.Material.KineticFriction = 2f;

            physicsEntityBody.PositionUpdateMode = PositionUpdateMode.Continuous;

            physicsEntityBody.CollisionInformation.Events.ContactCreated += new ContactCreatedEventHandler<EntityCollidable>(Events_InitialCollisionDetected);

            Physics.space.Add(physicsEntityBody);
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
            Physics.space.Remove(physicsEntityBody);
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

                parentRacer.raceTiming.previousRoll = Math.Max(-0.65f,Math.Min(0.65f, MathHelper.Lerp(parentRacer.raceTiming.previousRoll, -(0.322f * (angularSize) / 3.5f) * 6f * (getForwardSpeed() / maxSpeed), 0.08f)));
                return parentRacer.raceTiming.previousRoll;
            }
            else
                return 0f;
        }

        //public void counteractPitching()
        //{
        //    Vector3 target = Vector3.Cross(nearestMapPoint.tangent, nearestMapPoint.trackUp);//could have just used nearestMapPoint.roadSurface here.

        //    //Vector3.Lerp(camLastUp, getUp(), 0.1f);

        //    physicsBody.AngularVelocity -= target * 0.01f * (Vector3.Dot(physicsBody.AngularVelocity, target)); //Takes rotation up/down relative to track direction and reduces it?

        //    //Quaternion.Slerp(physicsBody., Quaternion.CreateFromAxisAngle(target, 0f), 0.8f);

        //}
    }
}
