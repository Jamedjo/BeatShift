#define AI_DEBUG_OUT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using BeatShift.DebugGraphics;
using BEPUphysics;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.MathExtensions;
using BEPUphysics.Collidables.MobileCollidables;
using BEPUphysics.Collidables;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.CollisionTests;
using BEPUphysics.CollisionRuleManagement;


namespace BeatShift.Input
{
    class AiInputManager : IInputManager
    {
        /// <summary>
        ///  Set to false and the player retakes control
        /// </summary>
        public const Boolean testAI = false;
        public const int numberOfAI = 8;

        public static Vector3 drawVector;
        public static Vector3 drawLookVector;

        private float randInaccuracy;
        private GameTime lastRandChange;
        private Random randGen;
        private GamePadState currentState;
        private GamePadState lastState;
        private Racer parent;
        private Box aheadBox;

        /// <summary>
        /// An input interface to a virtual controller which acts as an AI racer.
        /// </summary>
        /// <param name="parent">
        /// The racer which this input system will be controlling.
        /// </param>
        public AiInputManager(Racer parent)
        {
            lastState = currentState = new GamePadState();
            this.parent = parent;

            // WHY WILL THIS NOT PRODUCE COLLISIONS
            aheadBox = new Box(parent.shipPhysics.ShipPosition + parent.shipPhysics.ShipOrientationMatrix.Forward * 00, 500, 500, 500, 0.01f);
            aheadBox.Orientation = parent.shipPhysics.ShipOrientationQuaternion;

            aheadBox.CollisionInformation.Events.PairTouched += boxCollide;
            aheadBox.CollisionInformation.Events.InitialCollisionDetected += boxinitialCollide;

            randGen = new Random();
        }

        Boolean IInputManager.actionPressed(InputAction action)
        {
            return currentState.IsButtonDown(InputLayout.getButton(action));
        }

        Boolean IInputManager.actionTapped(InputAction action)
        {
            return (currentState.IsButtonDown(InputLayout.getButton(action))
                    && !lastState.IsButtonDown(InputLayout.getButton(action)));
        }

        float IInputManager.getActionValue(InputAction action)
        {
            Buttons check = InputLayout.getButton(action);
            float value;

            switch (check)
            {
                case Buttons.LeftTrigger:
                    value = currentState.Triggers.Left;
                    break;
                case Buttons.RightTrigger:
                    value = currentState.Triggers.Right;
                    break;

                //Thumbsticks have a vector, with each component ranging from -1 to 1
                //Get value or '-value' so that a positive value is given in the direction being checked.
                case Buttons.LeftThumbstickRight:
                    value = currentState.ThumbSticks.Left.X;
                    break;
                case Buttons.LeftThumbstickLeft:
                    value = -currentState.ThumbSticks.Left.X;
                    break;
                case Buttons.LeftThumbstickUp:
                    value = currentState.ThumbSticks.Left.Y;
                    break;
                case Buttons.LeftThumbstickDown:
                    value = -currentState.ThumbSticks.Left.Y;
                    break;

                case Buttons.RightThumbstickRight:
                    value = currentState.ThumbSticks.Right.X;
                    break;
                case Buttons.RightThumbstickLeft:
                    value = -currentState.ThumbSticks.Right.X;
                    break;
                case Buttons.RightThumbstickUp:
                    value = currentState.ThumbSticks.Right.Y;
                    break;
                case Buttons.RightThumbstickDown:
                    value = -currentState.ThumbSticks.Right.Y;
                    break;

                //Button does not give an analog value
                default:
                    if (currentState.IsButtonDown(check))
                        return 1f;
                    else return 0f;
            }

            //If a negative value in thumbstick direction then return zero for that direction.
            if (value < 0f)
                return 0f;

            return value;

        }

        void IInputManager.Update(GameTime gameTime)
        {
            //foreach (CollidablePairHandler p in aheadBox.CollisionInformation.Pairs)
            //    {
            //        foreach (ContactInformation c in p.Contacts)
            //        {
            //            System.Diagnostics.Debug.WriteLine("{0} {1}", (c.Contact.Position - ship.ShipPosition).Length(), c.Contact.Normal);
            //        }
            //    }
            if (lastRandChange != null)
            {
                if (gameTime.TotalGameTime.Subtract(lastRandChange.TotalGameTime).Seconds == 1)
                {
                    lastRandChange = gameTime;
                    randInaccuracy = (float)randGen.NextDouble() - 0.5f;
                    if (randInaccuracy > 0)
                    {
                        randInaccuracy = randInaccuracy * randInaccuracy;
                    }
                    else
                    {
                        randInaccuracy = randInaccuracy * randInaccuracy * -1;
                    }
                }
            }
            else
            {
                lastRandChange = gameTime;
                randInaccuracy = (float)randGen.NextDouble() - 0.5f;
                if (randInaccuracy > 0)
                {
                    randInaccuracy = randInaccuracy * randInaccuracy;
                }
                else
                {
                    randInaccuracy = randInaccuracy * randInaccuracy * -1;
                }
            }

            aheadBox.Position = parent.shipPhysics.ShipPosition + parent.shipPhysics.ShipOrientationMatrix.Forward * 0;
            aheadBox.Orientation = parent.shipPhysics.ShipOrientationQuaternion;

            Vector2 leftThumbStick = Vector2.Zero;
            Vector2 rightThumbStick = Vector2.Zero;

            Buttons pressedButtons;

            float acceleration;

            pressedButtons = setButtons();

            leftThumbStick.X = setTurn();

            acceleration = setAcceleration();

            GamePadThumbSticks sticks = new GamePadThumbSticks(leftThumbStick, rightThumbStick);
            GamePadButtons buttons = new GamePadButtons(pressedButtons);
            GamePadTriggers triggers;
            if (acceleration >= 0f)
            {
                triggers = new GamePadTriggers(0f, acceleration);
            }
            else
            {
                triggers = new GamePadTriggers(-acceleration, 0f);
            }
            GamePadDPad dpad = new GamePadDPad();

            lastState = currentState;
            currentState = new GamePadState(sticks, triggers, buttons, dpad);
            
        }

        /// <summary>
        /// Decide which buttons (if any the AI should be pressing.
        /// </summary>
        /// <returns>
        /// A Buttons object with appropriate flags set.
        /// </returns>
        private Buttons setButtons()
        {
            Buttons b = new Buttons();

            // Initially no buttons.
            return b;
        }

        /// <summary>
        /// Calculate how much the AI should be turning. This is likely to fail if the AI is not
        /// going the correct direction for whatever reason.
        /// </summary>
        /// <returns>
        /// A float describing the X componont of the left thumbstick of the AI's virtual
        /// controller.
        /// </returns>
        private float setTurn()
        {
            float aWalls = avoidWalls();
            float fTrack = futureTrack();
            float nWalls = newWalls();
            return 0.0f * aWalls + 0.6f * fTrack + 0.3f * nWalls + randInaccuracy;
        }

        private void boxinitialCollide<EntityCollidable>(EntityCollidable sender, Collidable info, CollidablePairHandler pair)
        {
            Collidable candidate = (pair.BroadPhaseOverlap.EntryA == aheadBox.CollisionInformation ? pair.BroadPhaseOverlap.EntryB : pair.BroadPhaseOverlap.EntryA) as Collidable;
            Contact c;

            if (candidate.Equals(Physics.currentTrackWall))
            {
                foreach (ContactInformation contactInformation in pair.Contacts)
                {
                    c = contactInformation.Contact;
                    System.Diagnostics.Debug.WriteLine("{0} {1}", (c.Position - parent.shipPhysics.ShipPosition).Length(), c.Normal);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("{0}", candidate.Shape);
            }
        }

        private void boxCollide<EntityCollidable>(EntityCollidable sender, Collidable other, CollidablePairHandler pair)
        {
            Collidable candidate = (pair.BroadPhaseOverlap.EntryA == aheadBox.CollisionInformation ? pair.BroadPhaseOverlap.EntryB : pair.BroadPhaseOverlap.EntryA) as Collidable;
            Contact c;

            if (candidate.Equals(Physics.currentTrackWall))
            {
                foreach (ContactInformation contactInformation in pair.Contacts)
                {
                    c = contactInformation.Contact;
                    System.Diagnostics.Debug.WriteLine("{0} {1}", (c.Position - parent.shipPhysics.ShipPosition).Length(), c.Normal);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("{0}", candidate.Shape);
            }
        }

        private float newWalls()
        {
            float t = 0;

            if (parent.shipPhysics.getForwardSpeed() == 0)
            {
                return 0f;
            }

            Matrix shipOrientation = parent.shipPhysics.ShipOrientationMatrix;

            if (float.IsNaN(shipOrientation.Forward.X))
                return 0;

            MapPoint lastPoint = parent.shipPhysics.nearestMapPoint;
            MapPoint nextPoint = parent.shipPhysics.mapData.mapPoints[(lastPoint.getIndex() + 1) % parent.shipPhysics.mapData.mapPoints.Count];

            // partially take current orientation into account, but not too much
            Vector3 guessUp = (shipOrientation.Up + nextPoint.trackUp * 9) / 10;

            Vector3 relativeTrackHeading = Vector3.Normalize(Vector3.Cross(nextPoint.tangent, guessUp));
            Vector3 relativeShipRight = Vector3.Normalize(Vector3.Cross(lastPoint.roadSurface, guessUp));

            float direction = Vector3.Dot(relativeTrackHeading, relativeShipRight);

            

            Vector3 testVector = parent.shipPhysics.nearestMapPoint.roadSurface * -1 * Math.Sign(direction);

            RayHit result;

            Vector3 rayOrigin = parent.shipPhysics.ShipPosition;

            float shipWidth = 3f;
            float rayLength = shipWidth + parent.shipPhysics.ShipSpeed / 8;

            Physics.currentTrackWall.RayCast(new Ray(rayOrigin, testVector), rayLength, out result);

            float distance = result.T < shipWidth ? rayLength - shipWidth : rayLength - result.T;

            if (result.T == 0)
                distance = 0;

            t = distance / (rayLength - shipWidth);
            
            
            drawVector = rayLength * testVector;

            return t * Math.Sign(direction);
        }


        /// <summary>
        /// A turning system which solely tries to avoid hitting walls. Uses raycasting to
        /// determine wall distances.
        /// </summary>
        /// <returns>
        /// A turning value.
        /// </returns>
        private float avoidWalls()
        {
            float t = 0;

            
            Matrix shipOrientation = parent.shipPhysics.ShipOrientationMatrix;

            Vector3 leftOuterVector = Vector3.Transform(shipOrientation.Forward, Matrix.CreateFromAxisAngle(parent.shipPhysics.ShipTrackUp, MathHelper.Pi / 3));
            Vector3 leftInnerVector = Vector3.Transform(shipOrientation.Forward, Matrix.CreateFromAxisAngle(parent.shipPhysics.ShipTrackUp, MathHelper.Pi / 6));
            Vector3 rightOuterVector = Vector3.Transform(shipOrientation.Forward, Matrix.CreateFromAxisAngle(parent.shipPhysics.ShipTrackUp, -1 * MathHelper.Pi / 3));
            Vector3 rightInnerVector = Vector3.Transform(shipOrientation.Forward, Matrix.CreateFromAxisAngle(parent.shipPhysics.ShipTrackUp, -1 * MathHelper.Pi / 6));

            RayHit result;

            float rayLength = 50f;
            Vector3 rayOrigin = parent.shipPhysics.ShipPosition - shipOrientation.Up * 2;

            Physics.currentTrackWall.RayCast(new Ray(rayOrigin, leftOuterVector), rayLength, out result);
            float leftOuter = result.T;

            Physics.currentTrackWall.RayCast(new Ray(rayOrigin, leftInnerVector), rayLength, out result);
            float leftInner = result.T;

            Physics.currentTrackWall.RayCast(new Ray(rayOrigin, rightOuterVector), rayLength, out result);
            float rightOuter = result.T;

            Physics.currentTrackWall.RayCast(new Ray(rayOrigin, rightInnerVector), rayLength, out result);
            float rightInner = result.T;

            float leftM = 0;
            float leftC = 0;
            float rightM = 0;
            float rightC = 0;

            float ellipticElongation = 2f;
            
            float angleOfImpactL;
            float angleOfImpactR;
            float ellipticDistanceL;
            float ellipticDistanceR;
            float scaledImpactDistanceL = float.MaxValue;
            float scaledImpactDistanceR = float.MaxValue;

            if (leftOuter != 0 && leftInner != 0)
            {
                float loX = (float)Math.Cos(Math.PI / 6) * leftOuter;
                float loY = (float)Math.Sin(Math.PI / 6) * leftOuter;

                float liX = (float)Math.Cos(Math.PI / 3) * leftInner;
                float liY = (float)Math.Sin(Math.PI / 3) * leftInner;

                float m = (liY - loY) / (liX - loX);
                leftC = loY - (m * (loX - 4));
                leftM = -m;
                if (leftM < 0)
                {
                    leftM = float.MaxValue;
                    leftC = float.PositiveInfinity;
                }
                else
                {
                    angleOfImpactL = (float)Math.Atan(1 / leftM);
                    ellipticDistanceL = (float)(ellipticElongation / (ellipticElongation * Math.Cos(angleOfImpactL) + Math.Sin(angleOfImpactL)));
                    scaledImpactDistanceL = ellipticDistanceL * leftC;
                }
            }
            else
            {
                leftM = float.MaxValue;
                leftC = float.PositiveInfinity;
            }

            if (rightOuter != 0 && rightInner != 0)
            {
                float roX = (float)Math.Cos(Math.PI / 6) * rightOuter;
                float roY = (float)Math.Sin(Math.PI / 6) * rightOuter;

                float riX = (float)Math.Cos(Math.PI / 3) * rightInner;
                float riY = (float)Math.Sin(Math.PI / 3) * rightInner;

                float m = (riY - roY) / (riX - roX);
                rightC = roY - (m * (roX + 4));
                rightM = -m;
                if (rightM < 0)
                {
                    rightM = float.MaxValue;
                    rightC = float.PositiveInfinity;
                }
                else
                {
                    angleOfImpactR = (float) Math.Atan(1 / rightM);
                    ellipticDistanceR = (float) (ellipticElongation / (ellipticElongation * Math.Cos(angleOfImpactR) + Math.Sin(angleOfImpactR)));
                    scaledImpactDistanceR = ellipticDistanceR * rightC;
                }
            }
            else
            {
                rightM = float.MaxValue;
                rightC = float.PositiveInfinity;
            }

            
            float maxDistance = 150f;

            if (scaledImpactDistanceL < scaledImpactDistanceR)
            {
                if (scaledImpactDistanceL < maxDistance)
                {
                    t = (maxDistance - scaledImpactDistanceL) / maxDistance;
                }
                //System.Diagnostics.Debug.WriteLine("l {0:000.0000}", scaledImpactDistanceL);
            }
            else
            {
                if (scaledImpactDistanceR < maxDistance)
                {
                    t = -(maxDistance - scaledImpactDistanceR) / maxDistance;
                }
                //System.Diagnostics.Debug.WriteLine("r {0:000.0000}", scaledImpactDistanceR);
            }

            return t;
        }

        /// <summary>
        /// A very simplistic turning system. Has no notion of avoiding crashes.
        /// </summary>
        /// <returns>
        /// A turning value.
        /// </returns>
        private float futureTrack()
        {
            float t;

            Matrix shipOrientation = parent.shipPhysics.ShipOrientationMatrix;
            MapPoint lastPoint = parent.shipPhysics.nearestMapPoint;
            MapPoint nextPoint = parent.shipPhysics.mapData.mapPoints[(parent.shipPhysics.nearestMapPoint.getIndex() + 2) % parent.shipPhysics.mapData.mapPoints.Count];

            // partially take current orientation into account, but not too much
            Vector3 guessUp = (shipOrientation.Up + nextPoint.trackUp * 9) / 10;

            Vector3 relativeTrackHeading = Vector3.Normalize(Vector3.Cross(nextPoint.tangent, guessUp));
            Vector3 relativeShipHeading = Vector3.Normalize(Vector3.Cross(shipOrientation.Forward, guessUp));
            Vector3 relativeShipRight = Vector3.Normalize(Vector3.Cross(shipOrientation.Right, guessUp));

            float dotProduct = Vector3.Dot(relativeTrackHeading, relativeShipHeading);
            float direction = Vector3.Dot(relativeTrackHeading, relativeShipRight);

            //t = (float)Math.Sin(Math.Acos(dotProduct));
            t = (float) (Math.Acos(dotProduct)  / (Math.PI / 2));
            if ((!(t < 0)) && (!(t > 0)) && (t != 0)) return 0; //Need to check for NaN, could have coded this better.
            return (float) (Math.Sqrt(t) * Math.Sign(direction));
        }

        /// <summary>
        /// Calculate whether the AI should be accelerating or braking, and the magnitude it
        /// should be doing either.
        /// </summary>
        /// <returns>
        /// A float in the range -1.0f to 1.0f, describing whether the AI is accelerating or 
        /// braking by the sign of the returned value (positive is acceleration, negative
        /// decceleration). The value describes the value of the appropriate trigger on the AI's
        /// virtual controller.
        /// </returns>
        private float setAcceleration()
        {
            // Initially, just throw it round.
            // Half speed so it's easy to follow.
            float a;

            Matrix shipOrientation = parent.shipPhysics.ShipOrientationMatrix;

            Vector3 testVector = shipOrientation.Forward;
            Vector3 rayOrigin = parent.shipPhysics.ShipPosition;

            RayHit result;

            float rayLength = 120f;

            Physics.currentTrackWall.RayCast(new Ray(rayOrigin, testVector), rayLength, out result);

            float distance = result.T == 0 ?  0 : rayLength - result.T;
            
            drawLookVector = result.T * testVector;

            Physics.currentTrackFloor.RayCast(new Ray(rayOrigin, testVector), rayLength, out result);
            if (distance < result.T)
            {
                a = 1f;
            }
            else
            {
                a = (1 - (distance / rayLength)) * (1 - (distance / rayLength));
            }
            
            return a;
        }
    }
}

