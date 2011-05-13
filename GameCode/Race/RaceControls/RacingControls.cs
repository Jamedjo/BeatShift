using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BeatShift.Input
{
    public class RacingControls
    {
        //const decimal decelfact = 0.001m;
        decimal[] lastTaps;
        decimal[] tapWeights;
        int tapNo;
        public IInputManager chosenInput;
        Racer racer;

        // Vibration variables
        float vibrateBoostControl = 0.0f;
        float vibrateCollisionControl = 0.0f;
        float vibrateControl = 0.0f;
        float vibrateLevelControl = 0.0f;
        float jumpHeight = 27.5f;
        float speedOnCollision = 0.0f;
        bool justCollided = false;
        bool justBoost = false;
        bool justJump = false;
        bool vibrateSequence = false;
        int sequenceNumber;
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
            chosenInput = inputManager;
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
                if( vibrateSequence )
                {
                    if (vibrateLevelControl < 0.3f)
                        vibrateLevelControl = vibrateLevelControl + 0.02f;
                    else
                        vibrateSequence = false ;
                }
                else
                {
                    if (vibrateLevelControl > 0.03f)
                        vibrateLevelControl = vibrateLevelControl - 0.02f;
                    else
                    {
                        vibrateLevelControl = 0;
                        vibrateSequence = true ;
                        sequenceNumber++;
                    }
                }
                if (sequenceNumber == 2)
                {
                    racer.beatQueue.isLevellingDown = false;
                    sequenceNumber = 0;
                }
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

            // vibrations from boost
            if (((chosenInput.actionPressed(InputAction.Boost)) || (racer.beatQueue.isLevellingUp)) 
                && (racer.beatQueue.GetBoost() > 0) && (!racer.raceTiming.hasCompletedRace) && (racer.beatQueue.getLayer() > 0))
            {
                racer.setBoost(true);
                justBoost = true;
                racer.shipDrawing.engineGlow.BoostOn();
                float boostIncrease = 0.01f;

                if (racer.beatQueue.isLevellingUp)
                    Boost(0.1);
                else
                {
                    racer.beatQueue.DrainBoost();
                    Boost(0.1);
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
            else if (justBoost)
            {
                racer.setBoost(false);
                racer.shipDrawing.engineGlow.BoostOff();

                // boost decay
                if (vibrateBoostControl > 0.015f)
                    vibrateBoostControl = vibrateBoostControl - 0.015f;
                else
                {
                    vibrateBoostControl = 0.0f;
                    justBoost = false;
                }
            }

            #endregion

            #region CALCULATIONS

            // check vibration values are capped at 1.0
            vibrateControl = vibrateCollisionControl + vibrateBoostControl + vibrateLevelControl;
            if (vibrateControl > 1.0f)
                vibrateControl = 1.0f;

            //check pad is being used and vibration option is set to true
            if (chosenInput.GetType() == typeof(PadInputManager) && (Options.ControllerVibration == true))
                GamePad.SetVibration(((PadInputManager)chosenInput).getPlayerIndex(), vibrateControl, vibrateControl);

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
                Vector3 b = racer.shipPhysics.physicsBody.BufferedStates.Entity.AngularVelocity;
                float angularSize = Vector3.Dot(a, b);

                // Change the impulse direction if we are in reverse
                int reversingMultiplier = 1;
                //if (new Random().Next(60) == 0) Console.WriteLine(angularSize);

                // CAUTION: We change reverse direction once we're reversing more than 35f. Plays better when hitting walls head on but weird.
                if (Vector3.Dot(racer.shipPhysics.physicsBody.LinearVelocity, racer.shipPhysics.physicsBody.WorldTransform.Backward) > 35f)
                    reversingMultiplier = -1;

                if (angularSize < 3.5f)
                {
                    Vector3 leftVector = racer.shipPhysics.physicsBody.OrientationMatrix.Up * reversingMultiplier * 75f * (1 + (Math.Abs(angularSize) * 0.12f)) * chosenInput.getActionValue(InputAction.Left);
                    Physics.ApplyAngularImpulse(ref leftVector, ref racer.shipPhysics.physicsBody);
                }

                if (angularSize > -3.5f)
                {
                    Vector3 rightVector = racer.shipPhysics.physicsBody.OrientationMatrix.Up * reversingMultiplier * -75f * (1 + (Math.Abs(angularSize) * 0.12f)) * chosenInput.getActionValue(InputAction.Right);
                    Physics.ApplyAngularImpulse(ref rightVector, ref racer.shipPhysics.physicsBody);
                }
            //}
        }

        public void applyForwardMotionFromAnalogue()
        {
            racer.shipPhysics.physicsBody.ApplyImpulse(racer.shipPhysics.physicsBody.Position, racer.shipPhysics.physicsBody.OrientationMatrix.Forward * 240 * chosenInput.getActionValue(InputAction.Forwards)); //TODO: should be 280?
        }

        public void Boost(double accuracy)
        {
            racer.shipPhysics.physicsBody.ApplyImpulse(racer.shipPhysics.physicsBody.Position, racer.shipPhysics.physicsBody.OrientationMatrix.Forward * 1200 * (float)accuracy);
        }

        public void AverageMove(double average)
        {
            racer.shipPhysics.physicsBody.ApplyImpulse(racer.shipPhysics.physicsBody.Position, racer.shipPhysics.physicsBody.OrientationMatrix.Forward * 70 * (float)average);
        }

        /*void AverageControl()
        {
            //if (currentPadState.IsConnected)
            {
                if (chosenInput.actionTapped(InputAction.Forwards))
                {
                    //lastTaps[tapNo++] = BeatShift.bgm.beatTime();
                    tapNo = tapNo % 4;
                }
            }

            decimal average = 0;
            for(int i = 0; i<4; i++)
            {
                int t = i-tapNo;
                if (t>=0) {
                    t=t%4;
                } else if (t< 0) {
                    t=4+t;
                }
                average += lastTaps[i]*tapWeights[t];
            }

                if (lastTaps[(tapNo+3)%4] > 0)
                {
                    lastTaps[(tapNo + 3) % 4] = lastTaps[(tapNo + 3) % 4] - decelfact;
                }
                else if (lastTaps[(tapNo + 2) % 4] > 0)
                {
                    lastTaps[(tapNo + 2) % 4] = lastTaps[(tapNo + 2) % 4] - decelfact;
                }
                else if (lastTaps[(tapNo + 1) % 4] > 0)
                {
                    lastTaps[(tapNo + 1) % 4] = lastTaps[(tapNo + 1) % 4] - decelfact;
                }
                else if (lastTaps[tapNo] > 0)
                {
                    lastTaps[tapNo] = lastTaps[tapNo] - decelfact;
                }
                for (int i = 0; i < 4; i++)
                {
                    if (lastTaps[i] < 0)
                    {
                        lastTaps[i] = 0;
                    }
                }
            AverageMove((double)average);
        }*/
        
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
                
            // Increase vibration if the player is tapping the A button.
            // Subtract vibration otherwise, even if the player holds down A
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

            //if (chosenInput.actionTapped(InputAction.Boost))//Should be actionPressed&notAlreadyBoosting
            //{
            //    //Create particle boost.
            //    BeatShift.emitter = new ParticleEmitter((Func<Matrix>)delegate { return Race.humanRacers[0].shipPhysics.ShipOrientationMatrix /*+ Vector3.Transform(new Vector3(0f, -1f, -2f), Race.humanRacers[0].shipPhysics.ShipOrientationMatrix)*/; }, BeatShift.settingsb, BeatShift.pEffect);
            //}
        }
    }
}
