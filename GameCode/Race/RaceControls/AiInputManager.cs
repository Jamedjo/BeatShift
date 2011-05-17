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
using BeatShift.Util.Random;


namespace BeatShift.Input
{
    class AiInputManager : IInputManager
    {
        /// <summary>
        ///  Set to false and the player retakes control
        /// </summary>
        public const Boolean testAI = false;
        public const int numberOfAI = 2;

        private GamePadState currentState;
        private GamePadState lastState;
        private Racer parent;
        private float lastTurn = 0f;

        Ray AiRay = new Ray();
        Ray testRay = new Ray();

        private Beat? nextBeatToPress = null;

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

            if (nextBeatToPress == null)
            {
                nextBeatToPress = parent.beatQueue.nextBeat();
            }
            else if (nextBeatToPress.Value.Time > BeatShift.bgm.songTick())
            {
                b |= nextBeatToPress.Value.Button;
                nextBeatToPress = null;
            }
            
            // Initially no buttons.
            return b;
        }

        const float futureWeight = 0.7f;
        const float wallsWeight = 0.3f;

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
            float randInaccuracy = randTurn();

            float fTrack = futureTrack();
            float nWalls = newWalls();
            float aWalls = avoidWalls();


            //System.Diagnostics.Debug.WriteLine("{0:0.000} {1:0.000} {2:0.000} {3:0.000}", randInaccuracy, fTrack, nWalls, aWalls);
            
            float retVal = futureWeight * fTrack + wallsWeight * nWalls + aWalls + randInaccuracy;
            return Math.Max(-1, Math.Min(1, retVal));
        }


        private float currentRotation = 0f;
        /// <summary>
        /// Set the random turn component of the AI.
        /// </summary>
        private float randTurn()
        {
            float offset = 2.0f;
            float radius = 1.0f;
            float maxRotation = MathHelper.Pi / 7.0f;

            currentRotation += (float) (SimpleRNG.GetUniform() * 2 - 1) * maxRotation;

            float width = (float) Math.Sin(currentRotation) * radius;
            float extra = (float) Math.Cos(currentRotation) * radius;

            return (float) Math.Tanh(width / (offset + extra));
        }


        public Vector3 wallTest { get; private set; }

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

            Vector3 rayOrigin = parent.shipPhysics.ShipPosition;

            float shipWidth = 3f;
            float rayLength = shipWidth + parent.shipPhysics.ShipSpeed / 8;

            RayHit result;

            AiRay.Position = rayOrigin;
            AiRay.Direction = testVector;
            Physics.currentTrackWall.RayCast(AiRay, rayLength, out result);

            float distance = result.T < shipWidth ? rayLength - shipWidth : rayLength - result.T;

            parent.shipDrawing.testWalls = testVector * rayLength;
            if (result.T == 0)
            {
                distance = 0;
                parent.shipDrawing.wallHit = false;
            }
            else
            {
                parent.shipDrawing.wallHit = true;
            }

            t = distance / (rayLength - shipWidth);

            float retVal = t * Math.Sign(direction);

            return float.IsNaN(retVal) ? 0f : retVal;
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

            int offset = 5;

            Matrix shipOrientation = parent.shipPhysics.ShipOrientationMatrix;
            MapPoint lastPoint = parent.shipPhysics.nearestMapPoint;
            MapPoint nextPoint = parent.shipPhysics.mapData.mapPoints[(parent.shipPhysics.nearestMapPoint.getIndex() + offset) % parent.shipPhysics.mapData.mapPoints.Count];

            // partially take current orientation into account, but not too much
            Vector3 guessUp = (shipOrientation.Up + nextPoint.trackUp * 9) / 10;

            Vector3 relativeTrackHeading = Vector3.Normalize(Vector3.Cross(nextPoint.tangent, guessUp));
            Vector3 relativeShipHeading = Vector3.Normalize(Vector3.Cross(shipOrientation.Forward, guessUp));
            Vector3 relativeShipRight = Vector3.Normalize(Vector3.Cross(shipOrientation.Right, guessUp));

            float dotProduct = Vector3.Dot(relativeTrackHeading, relativeShipHeading);
            float direction = Vector3.Dot(relativeTrackHeading, relativeShipRight);

            //t = (float)Math.Sin(Math.Acos(dotProduct));
            t = (float) (Math.Acos(dotProduct)  / (Math.PI / 2));
            return float.IsNaN(t) ? 0f : (float)(Math.Sqrt(t) * Math.Sign(direction));
        }

        private float avoidWallsRayLength = 100f;
        private float avoidWalls()
        {
            AiRay.Direction = parent.shipPhysics.ShipOrientationMatrix.Forward;
            AiRay.Position = parent.shipPhysics.ShipPosition;

            RayHit result;

            Physics.currentTrackWall.RayCast(AiRay, 100f, out result);

            float wallAngle;

            if (result.T != 0)
            {
                wallAngle = (float) Vector3.Dot(Vector3.Normalize(result.Normal), parent.shipPhysics.ShipOrientationMatrix.Left);
                
                if (wallAngle > 0)
                {
                    return -1 * turnWalls(wallAngle, result.T);
                }
                else
                {
                    return turnWalls(wallAngle, result.T);
                }
            }

            return 0;

            AiRay.Direction = (parent.shipPhysics.ShipOrientationMatrix.Forward + parent.shipPhysics.ShipOrientationMatrix.Right * 2) / 3;
            Physics.currentTrackWall.RayCast(AiRay, 30f, out result);
            if(result.T != 0){
                wallAngle = Vector3.Dot(Vector3.Normalize(result.Normal), parent.shipPhysics.ShipOrientationMatrix.Left);
                //if(wallAngle < 
            }
        }

        private float turnWalls(float wallAngle, float wallDistance)
        {
            float angVal = (float)Math.Sqrt(Math.Sin(Math.Acos(wallAngle)));
            float disVal = 1 / wallDistance;
            float retVal = angVal + disVal;
            return float.IsNaN(retVal) ? 0 : Math.Min(1.0f, retVal);
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

            float rayLength = 120f;

            RayHit result;

            testRay.Position = rayOrigin;
            testRay.Direction = testVector;
            Physics.currentTrackWall.RayCast(testRay, rayLength, out result);

            float distance = result.T == 0 ?  0 : rayLength - result.T;

            Physics.currentTrackFloor.RayCast(testRay, rayLength, out result);
            if (distance > result.T)
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

