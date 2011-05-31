using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BeatShift.Util;

namespace BeatShift.Input
{
    public class RacingControls
    {
        //const decimal decelfact = 0.001m;
        decimal[] lastTaps;
        decimal[] tapWeights;
        int tapNo;
        public IInputManager chosenInput { get; private set; }
        public PlayerIndex padIndex { get; private set; } //This will (must) remain as the previous padIndex, if later set to AI
        Racer racer;

        // Vibration variables
        float vibrateBoostControl = 0.0f;
        float vibrateCollisionControl = 0.0f;
        float vibrateControl = 0.0f;
        float vibrateLevelControlRight = 0.0f;
        float vibrateLevelControlLeft = 0.0f;
        float jumpHeight = 27.5f;
        float speedOnCollision = 0.0f;
        bool justCollided = false;
        bool wasBoostingPreviousUpdate = false;
        bool justJump = false;
        bool vibrateSequence = false;
        double sequenceNumber;
        int vibrateNumber;

        //TODO: sort topspeed variable
        float topSpeed = 350.0f;

        //Boolean useKeyBoard;//Disable keyboard on xbox so chatpad doesn't work?

        //TODO: Eventually should give a value based on beat accuracy and trigger distance.

        //int boostBar = 0;

        private Boolean previousCameraReverse = false;
        private Boolean previousPadDown = false;

        public RacingControls(Racer myRacer, IInputManager inputManager)//Should also take 'useKeyboard' Boolean
        {
            //playerIndex = index;//Player index should be able to be 'any' or 'keyboard' too
            padIndex = PlayerIndex.One;

            setChosenInput(inputManager);
            
            racer = myRacer;
            tapNo = 0;
            lastTaps = new decimal[4];
            tapWeights = new decimal[4] {1,.3m,.1m,.05m};
            racer.beatQueue.isLevellingDown = false;
            racer.beatQueue.isLevellingUp = false;
            sequenceNumber = 0;
            vibrateNumber = 0;
            vibrateSequence = true;
        }
        public RacingControls(Racer myRacer)
            : this(myRacer, new PadInputManager(PlayerIndex.One))
        {
            //Use first Controler as default.
        }
        public Boolean actionPressed(InputAction action)
        {
            //Change this? If forward and beat is not being kept then no?
            return chosenInput.actionPressed(action);
        }

        public void setChosenInput(IInputManager newInputManager)
        {
            chosenInput = newInputManager;
            if (chosenInput.GetType() == typeof(PadInputManager))
                padIndex = ((PadInputManager)chosenInput).getPlayerIndex();
        }

        public void Update(GameTime gameTime)
        {
            //System.Diagnostics.Debug.WriteLine("Update controls");
            chosenInput.Update(gameTime);

            // These two events should only be possible from human players, so cast to RacerHuman
            if(chosenInput.actionTapped(InputAction.CameraToggle)
              || chosenInput.actionTapped(InputAction.PadUp))
            {
                //System.Diagnostics.Debug.WriteLine("Camera toggle");
                ((RacerHuman)racer).toggleCamera();
            }
            if(chosenInput.actionPressed(InputAction.CameraReverse) != previousCameraReverse)
            {
                ((RacerHuman)racer).toggleReverseCamera();
                previousCameraReverse = !previousCameraReverse;
            }
            else if (chosenInput.actionPressed(InputAction.PadDown) != previousPadDown)
            {
                ((RacerHuman)racer).toggleReverseCamera();
                previousPadDown = !previousPadDown;
            }

            #region Vibrations

            #region LEVEL CHANGE

            if (racer.beatQueue.isLevellingDown )
            {
                if (sequenceNumber < Math.PI * 40)
                {
                    vibrateLevelControlRight = 0.4f*(float)Math.Sin(sequenceNumber / 20);
                    if (vibrateLevelControlRight < 0)
                    {
                        vibrateLevelControlLeft = -vibrateLevelControlRight;
                        vibrateLevelControlRight = 0;
                    }
                    sequenceNumber = sequenceNumber + 5;
                }
                else
                {
                    racer.beatQueue.isLevellingDown = false;
                    sequenceNumber = 0;
                    vibrateLevelControlLeft = 0;
                    vibrateLevelControlRight = 0;
                }
                //if (vibrateSequence)
                //{
                //    if (vibrateLevelControl < 0.3f)
                //        vibrateLevelControl = vibrateLevelControl + 0.02f;
                //    else
                //        vibrateSequence = false;
                //}
                //else
                //{
                //    if (vibrateLevelControl > 0.03f)
                //        vibrateLevelControl = vibrateLevelControl - 0.02f;
                //    else
                //    {
                //        vibrateLevelControl = 0;
                //        vibrateSequence = true;
                //        sequenceNumber++;
                //    }
                //}
                //if (sequenceNumber == 2)
                //{
                //    racer.beatQueue.isLevellingDown = false;
                //    sequenceNumber = 0;
                //}
            }

            #endregion

            #region COLLISIONS

            // checking for ship jump, saving peak height
            if (racer.shipPhysics.shipRayToTrackTime > jumpHeight)
            {
                jumpHeight = racer.shipPhysics.shipRayToTrackTime;
                justJump = true;
            }
            // checking for ship landing
            else if ((racer.shipPhysics.shipRayToTrackTime < 22) && justJump)
            {
                // TODO: VALUES STILL NEED TWEAKING
                vibrateCollisionControl = vibrateCollisionControl + ((jumpHeight - 27.5f) / 15);
                // reset jump threshold
                jumpHeight = 27.5f;
                justJump = false;
                justCollided = true;
            }

            // checking for wall collisions
            if (racer.isColliding && !justCollided)
            {
                //Console.WriteLine(racer.raceTiming.previousSpeed);
                speedOnCollision = racer.raceTiming.previousSpeed;
                vibrateCollisionControl = vibrateCollisionControl + 0.1f;
                justCollided = true;
                vibrateNumber = 0;
            }
            else if (justCollided)
            {
                if (speedOnCollision > racer.raceTiming.previousSpeed && vibrateNumber < 25)
                {
                    vibrateCollisionControl = vibrateCollisionControl + ((speedOnCollision - racer.raceTiming.previousSpeed) / (topSpeed/2));
                    speedOnCollision = racer.raceTiming.previousSpeed;
                }
                else
                {
                    if (vibrateCollisionControl > 0.05f)
                        vibrateCollisionControl = vibrateCollisionControl - 0.05f;
                    else
                    {
                        //Console.WriteLine(racer.raceTiming.previousSpeed);
                        vibrateCollisionControl = 0.0f;
                        justCollided = false;
                        racer.isColliding = false;
                    }
                }
                vibrateNumber++;
            }

            #endregion

            #region BOOST

#if DEBUG
            //Debug boost to get super speed
            if(Keyboard.GetState().IsKeyDown(Keys.X)) Boost(3f);
#endif

            // vibrations from boost
            if (((chosenInput.actionPressed(InputAction.Boost)) || (racer.beatQueue.isLevellingUp)) 
                && (racer.beatQueue.GetBoost() > 0) && (!racer.raceTiming.hasCompletedRace) && (racer.beatQueue.getLayer() > 0))
            {
                racer.setBoost(true);
                wasBoostingPreviousUpdate = true;
                racer.shipDrawing.engineGlow.boostType(1);
                float boostIncrease = 0.01f;

                if (racer.beatQueue.isLevellingUp)
                    Boost(racer.beatQueue.getBoostRatio());
                else
                {
                    racer.beatQueue.DrainBoost();
                    Boost(racer.beatQueue.getBoostRatio());
                    boostIncrease = 0.03f;
                }

                // max vibrate is currently 0.3f (can be as high as 1.0f)
                if (vibrateBoostControl < 0.3f)
                {
                    // boost increase
                    vibrateBoostControl = vibrateBoostControl + boostIncrease;
                }
                else
                    racer.beatQueue.isLevellingUp = false;

            }
            else if (wasBoostingPreviousUpdate)
            {
                racer.setBoost(false);
                racer.shipDrawing.engineGlow.boostType(0);

                // boost decay
                if (vibrateBoostControl > 0.015f)
                    vibrateBoostControl = vibrateBoostControl - 0.015f;
                else
                {
                    vibrateBoostControl = 0.0f;
                    wasBoostingPreviousUpdate = false;
                }
            }

            #endregion

            #region CALCULATIONS

            // check vibration values are capped at 1.0
            vibrateControl = vibrateCollisionControl + vibrateBoostControl;
            if (vibrateControl > 1.0f)
                vibrateControl = 1.0f;

            //check pad is being used and vibration option is set to true
            if (chosenInput.GetType() == typeof(PadInputManager) && (Options.ControllerVibration) && (!racer.raceTiming.hasCompletedRace))
                GamePad.SetVibration(padIndex, vibrateControl+vibrateLevelControlLeft, vibrateControl+vibrateLevelControlRight);

            #endregion

            #endregion

            if ((racer.raceTiming.isRacing == true || racer.raceTiming.hasCompletedRace==true) && racer.isRespawning == false)
            {
                //General Input
                UpdateLocalFromInput();
                BoostControl();
            }
        }
        
        private void UpdateLocalFromInput()
        {
          //  if (racer.shipPhysics.shipNearGround())
          //  {

                if (actionPressed(InputAction.Backwards))
                {
                    racer.shipPhysics.physicsBody.ApplyImpulse(racer.shipPhysics.physicsBody.Position, racer.shipPhysics.physicsBody.OrientationMatrix.Backward * 100);
                }

                //axis*angle*factor*analogueInputFactor
                //Apply left/right rotation multiplied by analogue steering values
                //keyboard keys have analogue values of 0f when up and 1f when pressed.

                //Only apply rotation if the ship doesn't already have a large rotational force
                Vector3 a = Vector3.Up;
                Vector3 b = racer.shipPhysics.bepuV.Vehicle.Body.BufferedStates.Entity.AngularVelocity;
                float angularSize = Vector3.Dot(a, b);

                // Change the impulse direction if we are in reverse
                int reversingMultiplier = 1;
                //if (new Random().Next(60) == 0) Console.WriteLine(angularSize);

                // CAUTION: We change reverse direction once we're reversing more than 35f. Plays better when hitting walls head on but weird.
                float backwardsComponent = Vector3.Dot(racer.shipPhysics.bepuV.Vehicle.Body.LinearVelocity, racer.shipPhysics.bepuV.Vehicle.Body.WorldTransform.Backward);
                if (backwardsComponent > 20f)
                    reversingMultiplier = -1;

                if (!(backwardsComponent < 20f && backwardsComponent > 0f))
                {
                    if (angularSize < 3.5f)
                    {
                        Vector3 leftVector = racer.shipPhysics.bepuV.Vehicle.Body.OrientationMatrix.Up * reversingMultiplier * 105f * (1 + (Math.Abs(angularSize) * 0.12f)) * chosenInput.getActionValue(InputAction.Left);
                        Physics.ApplyAngularImpulse(ref leftVector, racer.shipPhysics.bepuV.Vehicle.Body);
                    }

                    if (angularSize > -3.5f)
                    {
                        Vector3 rightVector = racer.shipPhysics.bepuV.Vehicle.Body.OrientationMatrix.Up * reversingMultiplier * -105f * (1 + (Math.Abs(angularSize) * 0.12f)) * chosenInput.getActionValue(InputAction.Right);
                        Physics.ApplyAngularImpulse(ref rightVector, racer.shipPhysics.bepuV.Vehicle.Body);
                    }
                }
        }

        public void applyForwardMotionFromAnalogue()
        {
            
            float forwardsValue = chosenInput.getActionValue(InputAction.Forwards);
            if ((forwardsValue < 0.05) && chosenInput.actionPressed(InputAction.Boost)) forwardsValue = 1f;//If boost pressed keep accelerating

            float baseSpeed = racer.beatQueue.getSpeedMultiplier();

            //Apply impulse
            racer.shipPhysics.bepuV.Vehicle.Body.ApplyImpulse(racer.shipPhysics.bepuV.Vehicle.Body.Position, racer.shipPhysics.bepuV.Vehicle.Body.OrientationMatrix.Forward * 230 * forwardsValue * baseSpeed);
        }

        public void Boost(double accuracy)
        {
            racer.shipPhysics.physicsBody.ApplyImpulse(racer.shipPhysics.physicsBody.Position, racer.shipPhysics.physicsBody.OrientationMatrix.Forward * 1200 * (float)accuracy);
        }

        public void AverageMove(double average)
        {
            racer.shipPhysics.physicsBody.ApplyImpulse(racer.shipPhysics.physicsBody.Position, racer.shipPhysics.physicsBody.OrientationMatrix.Forward * 70 * (float)average);
        }
        
        public decimal getLastPress()
        {
            return (lastTaps[tapNo]);
        }

        public double getBoostValue()
        {
            return racer.beatQueue.GetBoost();
        }

        void BoostControl()
        {
            applyForwardMotionFromAnalogue();
                
            if (chosenInput.actionTapped(InputAction.Green))
            {
                racer.beatQueue.BeatTap(Buttons.A);
            } else if (chosenInput.actionTapped(InputAction.Red))
            {
                racer.beatQueue.BeatTap(Buttons.B);
            } else if (chosenInput.actionTapped(InputAction.Blue))
            {
                racer.beatQueue.BeatTap(Buttons.X);
            } else if (chosenInput.actionTapped(InputAction.Yellow))
            {
                racer.beatQueue.BeatTap(Buttons.Y);
            }
        }
    }
}
