using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.Collidables.MobileCollidables;
using BEPUphysics.Collidables;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.CollisionTests;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities;
using BEPUphysics.MathExtensions;


namespace BeatShift
{
    ///This is an extension of ShipPhysics.cs
    public partial class ShipPhysics
    {
        private void Events_InitialCollisionDetected(EntityCollidable sender, Collidable info, CollidablePairHandler pair, ContactData contact)
        {
            try
            {
                //var contact = pair.Contacts[0].Contact;

                //if(contact.PenetrationDepth>2f)
                //DebugSystem.Instance.DebugCommandUI.Echo(contact.PenetrationDepth.ToString());

                // Select collisionInformation for object in contact with instead of the ships own collisionInformation
                Collidable candidate = (pair.BroadPhaseOverlap.EntryA == racerEntity.CollisionInformation ? pair.BroadPhaseOverlap.EntryB : pair.BroadPhaseOverlap.EntryA) as Collidable;
                if (candidate.Equals(Physics.currentTrackFloor))
                {

                    ReactToTrackHit(contact);
                }
                else if (candidate.Equals(Physics.currentTrackWall))
                {
                    ReactToWallHit(contact);
                }
                else
                {

                    ReactToShipShipCollision(contact, candidate);
                }
            }
            catch (Exception e)
            {
                // System.Diagnostic.Debug.WriteLine("Unfound pair");
            }
        }

        private void ReactToTrackHit(ContactData contact)
        {
            // Bounce up a little on collisions
            Vector3 hitUpDirection = -contact.Normal;//-nearestMapPoint.trackUp;
            float downVelocityMagnitude = Vector3.Dot(racerEntity.LinearVelocity, hitUpDirection) / hitUpDirection.Length();
            racerEntity.LinearVelocity -= hitUpDirection * downVelocityMagnitude;
            //racerEntity.LinearVelocity -= (hitUpDirection * 5f);

            //racerEntity.ApplyLinearImpulse(-hitUpDirection * 2.5f);

            //TODO: DISPLAY LOADS OF PARTICLES TO HIDE OUR CRAP PHYSICS!!!
            //BeatShift.emitter = new ParticleEmitter((Func<Vector3>)delegate { return contacts[0].Position; }, BeatShift.settingsb, BeatShift.pEffect);
        }

        private void ReactToShipShipCollision(ContactData contact, Collidable candidate)
        {
            foreach (Racer r in Race.currentRacers)
            {
                Entity collidedBody = r.shipPhysics.racerEntity;
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
                    Vector3 localContactVector = contact.Position - racerEntity.Position;

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


                    Vector3 bounceVector = racerEntity.Position - collidedBody.Position;
                    bounceVector.Normalize();


                    float shipVelocity_inShipBounceDirection = Vector3.Dot(racerEntity.LinearVelocity, bounceVector);// no need to divide by bounceVector.Length() as normalized
                    Vector3 relativeVelocity = collidedBody.LinearVelocity - racerEntity.LinearVelocity;
                    float realtiveVelInShipBounceDirection = Vector3.Dot(racerEntity.LinearVelocity, bounceVector) - Vector3.Dot(collidedBody.LinearVelocity, bounceVector);// no need to divide by bounceVector.Length() as normalized

                    bool localQuadIsSide = (localQuadrant == Quadrant.Left || localQuadrant == Quadrant.Right);
                    bool opponentQuadIsSide = (opponentQuadrant == Quadrant.Left || opponentQuadrant == Quadrant.Right);

                    if (localQuadrant == Quadrant.Front)
                    {
                        var frontPost = racerEntity.Position + Vector3.Transform(new Vector3(0f, 0f, -3.75f), racerEntity.Orientation);

                        Vector3 fusionVector = frontPost - collidedBody.Position;
                        Vector3 direction = racerEntity.OrientationMatrix.Left;
                        if (Vector3.Dot(racerEntity.OrientationMatrix.Left, fusionVector) < 0)
                        {
                            //Bend to right
                            direction *= -1;
                        }

                        Vector3 a = collidedBody.Position - racerEntity.Position;
                        Vector3 b = racerEntity.OrientationMatrix.Forward;

                        var tau = Math.Sin(angleBetween(a, b)) / a.Length();
                        float scaleFactor = (float)Math.Abs(2f - tau);

                        racerEntity.ApplyImpulse(racerEntity.Position, direction * (200 * scaleFactor));
                    }


                    if (racerEntity.LinearVelocity.Length() < 50) //TODO: tweak
                    {
                        /////////////////
                        //Bounce in a similar way to when hitting walls

                        //Slow ship down in other components
                        racerEntity.LinearVelocity *= 0.94f;

                        //Get scale of bounce based on speed ship was moving towards the ship.
                        //Minimum bounce until 5 speed, max bounce after 100;
                        float minmaxV = Math.Min(50, Math.Max(10, (Math.Abs(shipVelocity_inShipBounceDirection))));
                        //float bounceScale = minmaxV / 20;//scale velocity down so max bounce is 5 times as strong as bounce at 20mph, and 20mph bounce is unscaled
                        racerEntity.LinearVelocity -= bounceVector * shipVelocity_inShipBounceDirection;

                        //Apply a bounce impulse
                        racerEntity.ApplyImpulse(racerEntity.Position, bounceVector * 25 * minmaxV);
                    }


                    else if (localQuadIsSide && opponentQuadIsSide)
                    {
                        /////////////////
                        //Bounce in a similar way to when hitting walls

                        //Slow ship down in other components
                        racerEntity.LinearVelocity *= 0.94f;

                        //Get scale of bounce based on speed ship was moving towards the ship.
                        //Minimum bounce until 5 speed, max bounce after 100;
                        float minmaxV = Math.Min(100, Math.Max(5, (Math.Abs(shipVelocity_inShipBounceDirection))));
                        //float bounceScale = minmaxV / 20;//scale velocity down so max bounce is 5 times as strong as bounce at 20mph, and 20mph bounce is unscaled
                        racerEntity.LinearVelocity -= bounceVector * shipVelocity_inShipBounceDirection;

                        //Apply a bounce impulse
                        racerEntity.ApplyImpulse(racerEntity.Position, bounceVector * 15 * minmaxV);


                    }

                    else if ((localQuadrant == Quadrant.Front) && opponentQuadrant == Quadrant.Back)
                    {
                        /////////////////
                        //Create ramming effect


                        //var worldCoordinates = racerEntity.Position + Vector3.Transform(backPosLeft, racerEntity.Orientation);

                        racerEntity.LinearVelocity -= bounceVector * realtiveVelInShipBounceDirection;


                        // Dampens both ships' speeds
                        if (localQuadrant == Quadrant.Front) racerEntity.LinearVelocity *= 0.98f;
                        else racerEntity.LinearVelocity *= 0.92f;

                        //Create ramming effect by speeding up front ship and slowing down back ship
                        float skewfactor = 7 / 8;
                        if (localQuadrant == Quadrant.Front) skewfactor = 1 / 9;

                        //physicsBody.LinearVelocity = skewfactor * physicsBody.LinearVelocity + (1 - skewfactor) * collidedBody.LinearVelocity;
                        if (racerEntity.LinearVelocity.Length() != 0)
                        {
                            Vector3 normalisedVel = racerEntity.LinearVelocity;
                            normalisedVel.Normalize();
                            racerEntity.LinearVelocity = normalisedVel * (skewfactor * racerEntity.LinearVelocity.Length() + (1 - skewfactor) * collidedBody.LinearVelocity.Length());
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
                        racerEntity.LinearVelocity *= 0.92f;

                        //Get scale of bounce based on speed ship was moving towards the ship.
                        //Minimum bounce until 5 speed, max bounce after 100;
                        float minmaxV = Math.Min(100, Math.Max(25, (Math.Abs(shipVelocity_inShipBounceDirection))));
                        //float bounceScale = minmaxV / 20;//scale velocity down so max bounce is 5 times as strong as bounce at 20mph, and 20mph bounce is unscaled
                        racerEntity.LinearVelocity -= bounceVector * shipVelocity_inShipBounceDirection;

                        //Apply a bounce impulse
                        racerEntity.ApplyImpulse(racerEntity.Position, bounceVector * 12 * minmaxV);
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
                    if (racerEntity.LinearVelocity.Length() > collidedBody.LinearVelocity.Length())
                    {
                        //Bounce the ship to the left
                        //physicsBody.ApplyImpulse(physicsBody.Position, collidedBody.OrientationMatrix.Left * 2000);
                    }
                    r.lastCollisionPoint = contact.Position;
                    //Initiate controller vibration
                    r.isColliding = true;
                    if (r.GetType() == typeof(RacerHuman))
                    {
                        SoundManager.Collision();
                    }
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

        private void ReactToWallHit(ContactData contact)
        {
            ////Only hit wall once per collision by only ignoring hits which are close in time.
            //if (BeatShift.singleton.currentTime.TotalGameTime.TotalMilliseconds - lastWallHitTime > 100)
            //{
            //    //Console.WriteLine("Creating Bounce");
            //    lastWallHitTime = BeatShift.singleton.currentTime.TotalGameTime.TotalMilliseconds;

            ////Stop all rotation
            //physicsBody.AngularVelocity = new Vector3(0, 0, 0);
            parentRacer.lastCollisionPoint = contact.Position;
            //Initiate controller vibration
            parentRacer.isColliding = true;
            if (parentRacer.GetType() == typeof(RacerHuman))
            {
                SoundManager.Collision();
            }
            //Calculate direction to bounce
            Vector3 v1 = Vector3.Cross(nearestMapPoint.trackUp, contact.Normal);
            Vector3 bounceVector = Vector3.Cross(nearestMapPoint.trackUp, v1);
            bounceVector.Normalize();

            if (bounceVector.Length() < 0.8f)
                bounceVector = Vector3.Zero;

            //Remove ships velocity towards the wall
            //by setting velocity component in direction of bounce to 0
            float shipVelocity_inWallBounceDirection = Vector3.Dot(racerEntity.LinearVelocity, bounceVector);// no need to divide by bounceVector.Length() as normalized
            racerEntity.LinearVelocity -= bounceVector * shipVelocity_inWallBounceDirection;
            //Console.WriteLine("velocityTowardsWall is " + shipVelocity_inWallBounceDirection);

            //Slow ship down in other components
            racerEntity.LinearVelocity *= 0.97f;

            //Get scale of bounce based on speed ship was moving towards the wall.
            //Minimum bounce until 5 speed, max bounce after 100;
            float minmaxV = Math.Min(100, Math.Max(20, (Math.Abs(shipVelocity_inWallBounceDirection))));
            float bounceScale = minmaxV / 20;//scale velocity down so max bounce is 5 times as strong as bounce at 20mph, and 20mph bounce is unscaled

            //Apply a bounce impulse
            applyImpulseInSurfacePlane(bounceVector * 1200 * bounceScale);
            //parentRacer.shipDrawing.drawArrowListPermanent.Add(new D_Arrow { pos = contact.Position, dir = bounceVector * 4f, col = Color.DarkBlue.ToVector3() });
            //}
        }

        float angleBetween(Vector3 a, Vector3 b)
        {
            return (float)Math.Acos(Vector3.Dot(a, b) / (a.Length() * b.Length()));
        }

    }
}
