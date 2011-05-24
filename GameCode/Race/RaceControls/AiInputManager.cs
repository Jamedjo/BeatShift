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
using BeatShift.Util;
using BeatShift.Cameras;
using Microsoft.Xna.Framework.Graphics;


namespace BeatShift.Input
{
    class AiInputManager : IInputManager
    {
        /// <summary>
        ///  Set to false and the player retakes control
        /// </summary>
        public static bool testAI = false;//DO NOT CHANGE THIS TO TRUE, USE THE CONSOLE BEFORE A RACE
        public int numberOfAI = Options.NumberAI;

        private GamePadState currentState;
        private GamePadState lastState;
        private Racer parent;
        private float lastTurn = 0f;

        //values used to determine turn
        private float turnVal = 0;
        private float randInaccuracy = 0;
        private float fTrack3 = 0;
        private float fTrack5 = 0;
        private float sideWalls = 0;
        private float frontWalls = 0;

        //values used to determine acceleration
        private float accelVal = 0;
        private float closerBy = 0;
        private float closerByFraction = 0;

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

            pressedButtons = setButtons();

            turnVal = setTurn();
            leftThumbStick.X = turnVal;

            accelVal = setAcceleration();

            GamePadThumbSticks sticks = new GamePadThumbSticks(leftThumbStick, rightThumbStick);
            GamePadButtons buttons = new GamePadButtons(pressedButtons);
            GamePadTriggers triggers;
            if (accelVal >= 0f)
            {
                triggers = new GamePadTriggers(0f, accelVal);
            }
            else
            {
                triggers = new GamePadTriggers(-accelVal, 0f);
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

        /// <summary>
        /// Calculate how much the AI should be turning.
        /// </summary>
        /// <returns>
        /// A float describing the X componont of the left thumbstick of the AI's virtual
        /// controller.
        /// </returns>
        private float setTurn()
        {
            randInaccuracy = randTurn();

            fTrack3 = futureTrack(3);
            fTrack5 = futureTrack(5); 
            sideWalls = avoidSideWalls();
            frontWalls = avoidWallsInFront();


            //System.Diagnostics.Debug.WriteLine("{0:0.000} {1:0.000} {2:0.000} {3:0.000}", randInaccuracy, fTrack, nWalls, aWalls);
            
            //                         0.7*fTrack    +0.3*nWalls  + aWalls  + randInaccuracy
            //float retVal = futureWeight * fTrack + wallsWeight * nWalls + aWalls + randInaccuracy;


            //Steer towards the centre of the track (3 waypoints ahead), adjusting for long term centre (5 ahead)
            //TODO: Change the 0.7 and 0.3 based on a 'planning' skill
            float steerCentre = 0.7f * fTrack3 + 0.3f * fTrack5;
            float mistakes = randInaccuracy * 0.2f;//Adjust this value with skill
            float balancedDescision = sideWalls * 1.4f + steerCentre + mistakes;

            //If previous calculations continue ship in the same direction.
            if (Math.Abs(balancedDescision) < 0.2f)
            {
                //Ensure that steering is adjusted to avoid walls in front of ship.
                balancedDescision += frontWalls;
            }

            float retVal=0;
            //if (Globals.TestState == 0)
                retVal = balancedDescision;
            //if (Globals.TestState == 1)
            //    retVal = sideWalls*3f;
            //if (Globals.TestState == 2)
            //    retVal = frontWalls;
            //if (Globals.TestState == 3)
            //    retVal = steerCentre;
            //if (Globals.TestState == 4)
            //    retVal = sideWalls*3f + steerCentre;
            //if (Globals.TestState == 5)
            //    retVal = sideWalls*1f + steerCentre;
            

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

        /// <summary>
        /// Casts a ray sideways from the ship to determine how near the wall is and how to respond.
        /// </summary>
        /// <returns>Returns a value to represent how urgently the ship must turn to avoid a wall</returns>
        private float avoidSideWalls()
        {
            float t = 0;

            if (parent.shipPhysics.getForwardSpeed() == 0)
            {
                return 0f;
            }

            Matrix shipOrientation = parent.shipPhysics.ShipOrientationMatrix;

            if (float.IsNaN(shipOrientation.Forward.X))
                return 0;

            //MapPoint lastPoint = parent.shipPhysics.nearestMapPoint;
            //MapPoint nextPoint = parent.shipPhysics.mapData.nextPoint(lastPoint);
            MapPoint nearPoint = parent.shipPhysics.nearestMapPoint;

            // partially take current orientation into account, but not too much
            //Vector3 guessUp = (shipOrientation.Up + nextPoint.trackUp * 9) / 10;

            //Vector3 relativeTrackHeading = Vector3.Normalize(Vector3.Cross(nextPoint.roadSurface, guessUp));
            //Vector3 relativeShipRight = Vector3.Normalize(Vector3.Cross(lastPoint.tangent, guessUp));

            //float direction = Vector3.Dot(relativeTrackHeading, relativeShipRight);
            
            // Find if left/right of centre of track by determining which side of
            // the plane defined by the roadSurface vector the ship is on
                //If the Vector from the waypoint to the ship is within 90 degrees of roadSurface
                //it is on the side the track the roadSurface arrow faces
            float direction = Vector3.Dot(nearPoint.roadSurface, (parent.shipPhysics.ShipPosition - nearPoint.position));

            Vector3 testVector = parent.shipPhysics.nearestMapPoint.roadSurface * Math.Sign(direction);

            Vector3 rayOrigin = parent.shipPhysics.ShipPosition;

            float shipWidth = 3f;
            float rayLength = shipWidth + parent.shipPhysics.ShipSpeed / 8;

            RayHit result;

            AiRay.Position = rayOrigin;
            AiRay.Direction = testVector;
            Physics.currentTrackWall.RayCast(AiRay, rayLength, out result);

            float distance = result.T < shipWidth ? rayLength - shipWidth : rayLength - result.T;

            //Setup arrows for drawing
            parent.shipDrawing.aiWallRayArrow = testVector * rayLength;
            if (result.T == 0)
            {
                distance = 0;
                parent.shipDrawing.aiWallRayHit = false;
            }
            else
            {
                parent.shipDrawing.aiWallRayHit = true;
            }

            t = distance / (rayLength - shipWidth);

            float retVal = t * -1 * Math.Sign(direction);

            return float.IsNaN(retVal) ? 0f : retVal;
        }

        /// <summary>
        /// A very simplistic turning system. Has no notion of avoiding crashes.
        /// Returns an amount to turn to get the ship to aim 'offset' waypoints ahead
        /// </summary>
        /// <returns>
        /// A turning value.
        /// </returns>
        private float futureTrack(int offset)
        {
            float t;

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

        /// <summary>
        /// Avoid walls in front of the ship by turning away from them.
        /// </summary>
        /// <returns></returns>
        private float avoidWallsInFront()
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

            parent.shipDrawing.aiFrontRayArrow = testVector * rayLength;
            parent.shipDrawing.aiFrontRayHit = (result.T == 0)? false : true;

            if (distance > result.T)
            {
                closerBy = 0;
                closerByFraction = 0;
                a = 1f;
            }
            else
            {
                closerBy = rayLength - distance;
                closerByFraction = closerBy / rayLength;
                a = closerByFraction * closerByFraction;
            }
            
            return a;
        }

        private static Vector2 hudPosition = new Vector2(50f, 50f);
        private static Vector2 hudPosition2 = new Vector2(52f, 52f);
        public void DrawAiHUD(CameraWrapper camera, GameTime gameTime)
        {
            String message = "AI-HUD:\nAcceleration: " + accelVal + "\n  -closerBy: " + closerBy + "\n  -cB_Fraction: " + closerByFraction + "\n\nTurn: " + turnVal + "\n  -randInaccuracy: " + randInaccuracy + "\n  -fTrack3: " + fTrack3 + "\n  -fTrack5: " + fTrack5 + "\n  -sideWalls: " + sideWalls + "\n  -frontWalls: " + frontWalls;
            BeatShift.spriteBatch.DrawString(BeatShift.newfont, message, hudPosition, Color.White, 0f, Vector2.Zero, 0.65f, SpriteEffects.None, 1);
            BeatShift.spriteBatch.DrawString(BeatShift.newfont, message, hudPosition2, Color.Black, 0f, Vector2.Zero, 0.65f, SpriteEffects.None, 1);
        }
    }
}

