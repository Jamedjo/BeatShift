using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics;
using Microsoft.Xna.Framework;
using BEPUphysics.Vehicle;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.Entities.Prefabs;
using Microsoft.Xna.Framework.Input;
using BEPUphysics.Collidables.MobileCollidables;
using BEPUphysics.PositionUpdating;
using BEPUphysics.Collidables.Events;

namespace BeatShift
{/// <summary>
        /// Handles input and movement of a Vehicle in the game.
        /// Acts as the 'front end' for the bookkeeping and math of the Vehicle within the physics engine.
        /// </summary>
    public class BepuVehicle
    {
            /// <summary>
            /// Speed that the Vehicle tries towreach when moving backward.
            /// </summary>
            public float BackwardSpeed = -1300;

            /// <summary>
            /// Speed that the Vehicle tries to reach when moving forward.
            /// </summary>
            public float ForwardSpeed = 3000;

            /// <summary>
            /// Whether or not to use the Vehicle's input.
            /// </summary>
            public bool IsActive;


            /// <summary>
            /// Maximum turn angle of the wheels.
            /// </summary>
            public float MaximumTurnAngle = (float)Math.PI / 6;

            /// <summary>
            /// Turning speed of the wheels in radians per second.
            /// </summary>
            public float TurnSpeed = MathHelper.Pi;

            /// <summary>
            /// Physics representation of the Vehicle.
            /// </summary>
            public Vehicle Vehicle;


            /// <summary>
            /// Constructs the front end and the internal physics representation of the Vehicle.
            /// </summary>
            /// <param name="position">Position of the Vehicle.</param>
            /// <param name="owningSpace">Space to add the Vehicle to.</param>
            public BepuVehicle(Vector3 position, List<Vector3> physicsHull)
            {
                ConvexHullShape hull = new ConvexHullShape(physicsHull);
                hull.CollisionMargin = 0.6f;
                var bodies = new List<CompoundShapeEntry>()
                {
                    new CompoundShapeEntry(hull, Vector3.Zero, 25f)
                };

                var body = new CompoundBody(bodies, 60f);
                //body.CollisionInformation.LocalPosition = new Vector3(0, .5f, 0);
                body.Position = (position); //At first, just keep it out of the way.

                body.IsAlwaysActive = true;

                //physicsBody.CenterOfMassOffset = new Vector3(0, 0f, 0);//Becareful with this as forces/impulses act from here including raycasts
                body.LinearDamping = 0.5f;//As there is rarely friction must slow ship down every update
                body.AngularDamping = 0.94f;
                body.Material.KineticFriction = 2f;

                body.PositionUpdateMode = PositionUpdateMode.Continuous;


                Vehicle = new Vehicle(body);

                #region RaycastWheelShapes

                //The wheel model used is not aligned initially with how a wheel would normally look, so rotate them.
                Matrix wheelGraphicRotation = Matrix.CreateFromAxisAngle(Vector3.Forward, MathHelper.PiOver2);

                float shipWidth = 5f;//2.2f;//TODO:don't set manually
                float shipLength = 7.5f;//3.6f;//TODO:don't set manually

                float staticFriction = 0.2f;
                float dynamicFriction = 1.5f;
                float suspensionLength = 2.2f;//0.8f; //0.2f to test for 'jerky' collision behaviour.


                Vehicle.AddWheel(new Wheel(
                                     new RaycastWheelShape(.5f, wheelGraphicRotation),
                                     new WheelSuspension(2000, 100f, Vector3.Down, suspensionLength, new Vector3(-shipWidth / 2, 0f, shipLength / 2)),
                                     new WheelDrivingMotor(3f, 30000, 10000),
                                     new WheelBrake(1.5f, 2, .02f),
                                     new WheelSlidingFriction(dynamicFriction, staticFriction)));
                Vehicle.AddWheel(new Wheel(
                                     new RaycastWheelShape(.5f, wheelGraphicRotation),
                                     new WheelSuspension(2000, 100f, Vector3.Down, suspensionLength, new Vector3(-shipWidth / 4, 0f, -shipLength / 2)),
                                     new WheelDrivingMotor(3f, 30000, 10000),
                                     new WheelBrake(1.5f, 2, .02f),
                                     new WheelSlidingFriction(dynamicFriction, staticFriction)));
                Vehicle.AddWheel(new Wheel(
                                     new RaycastWheelShape(.5f, wheelGraphicRotation),
                                     new WheelSuspension(2000, 100f, Vector3.Down, suspensionLength, new Vector3(shipWidth / 2, 0f, shipLength / 2)),
                                     new WheelDrivingMotor(3f, 30000, 10000),
                                     new WheelBrake(1.5f, 2, .02f),
                                     new WheelSlidingFriction(dynamicFriction, staticFriction)));
                Vehicle.AddWheel(new Wheel(
                                     new RaycastWheelShape(.5f, wheelGraphicRotation),
                                     new WheelSuspension(2000, 100f, Vector3.Down, suspensionLength, new Vector3(shipWidth / 4, 0f, -shipLength / 2)),
                                     new WheelDrivingMotor(3f, 30000, 10000),
                                     new WheelBrake(1.5f, 2, .02f),
                                     new WheelSlidingFriction(dynamicFriction, staticFriction)));

                #endregion

                foreach (Wheel wheel in Vehicle.Wheels)
                {
                    //This is a cosmetic setting that makes it looks like the car doesn't have antilock brakes.
                    wheel.Shape.FreezeWheelsWhileBraking = true;

                    //By default, wheels use as many iterations as the space.  By lowering it,
                    //performance can be improved at the cost of a little accuracy.
                    wheel.Suspension.SolverSettings.MaximumIterations = 1;
                    wheel.Brake.SolverSettings.MaximumIterations = 1;
                    wheel.SlidingFriction.SolverSettings.MaximumIterations = 1;
                    wheel.DrivingMotor.SolverSettings.MaximumIterations = 1;
                }


                Physics.space.Add(Vehicle);
                for (int k = 0; k < 4; k++)
                {
                    Vehicle.Wheels[k].Shape.Detector.Tag = "noDisplayObject";
                }
            }

            /// <summary>
            /// Gives the Vehicle control over the camera and movement input.
            /// </summary>
            public void Activate()
            {
                if (!IsActive)
                {
                    IsActive = true;
                    //Put the Vehicle where the camera is.
                    Vehicle.Body.LinearVelocity = Vector3.Zero;
                    Vehicle.Body.AngularVelocity = Vector3.Zero;
                    Vehicle.Body.Orientation = Quaternion.Identity;
                }
            }

            /// <summary>
            /// Returns input control to the camera.
            /// </summary>
            public void Deactivate()
            {
                if (IsActive)
                {
                    IsActive = false;
                }
            }


            /// <summary>
            /// Handles the input and movement of the character.
            /// </summary>
            /// <param name="dt">Time since last frame in simulation seconds.</param>
            /// <param name="keyboardInput">Keyboard state.</param>
            /// <param name="gamePadInput">Gamepad state.</param>
            public void Update(float dt,Vector3 worldDirection, KeyboardState keyboardInput, GamePadState gamePadInput)
            {
                //Set gravity/world direction for wheel.
                List<Wheel>  wheels = Vehicle.Wheels;
                for (int i = 0; i < wheels.Count; i++)
                {
                    wheels[i].Suspension.WorldDirection = worldDirection;
                }

                //Simulate gravity
                //Vehicle.Body.ApplyImpulse(Vehicle.Body.Position, worldDirection * 9.81f);

                if (IsActive)
                {
//#if XBOX360
//                float speed = gamePadInput.Triggers.Right * ForwardSpeed + gamePadInput.Triggers.Left * BackwardSpeed;
//                Vehicle.Wheels[1].DrivingMotor.TargetSpeed = speed;
//                Vehicle.Wheels[3].DrivingMotor.TargetSpeed = speed;

//                if (gamePadInput.IsButtonDown(Buttons.LeftStick))
//                    foreach (Wheel wheel in Vehicle.Wheels)
//                    {
//                        wheel.Brake.IsBraking = true;
//                    }
//                else
//                    foreach (Wheel wheel in Vehicle.Wheels)
//                    {
//                        wheel.Brake.IsBraking = false;
//                    }
//                Vehicle.Wheels[1].Shape.SteeringAngle = (gamePadInput.ThumbSticks.Left.X * MaximumTurnAngle);
//                Vehicle.Wheels[3].Shape.SteeringAngle = (gamePadInput.ThumbSticks.Left.X * MaximumTurnAngle);
//#else

//                    if (keyboardInput.IsKeyDown(Keys.E))
//                    {
//                        //Drive
//                        Vehicle.Wheels[1].DrivingMotor.TargetSpeed = ForwardSpeed;
//                        Vehicle.Wheels[3].DrivingMotor.TargetSpeed = ForwardSpeed;
//                    }
//                    else if (keyboardInput.IsKeyDown(Keys.D))
//                    {
//                        //Reverse
//                        Vehicle.Wheels[1].DrivingMotor.TargetSpeed = BackwardSpeed;
//                        Vehicle.Wheels[3].DrivingMotor.TargetSpeed = BackwardSpeed;
//                    }
//                    else
//                    {
//                        //Idle
//                        Vehicle.Wheels[1].DrivingMotor.TargetSpeed = 0;
//                        Vehicle.Wheels[3].DrivingMotor.TargetSpeed = 0;
//                    }
//                    if (keyboardInput.IsKeyDown(Keys.Space))
//                    {
//                        //Brake
//                        foreach (Wheel wheel in Vehicle.Wheels)
//                        {
//                            wheel.Brake.IsBraking = true;
//                        }
//                    }
//                    else
                    {
                        //Release brake
                        foreach (Wheel wheel in Vehicle.Wheels)
                        {
                            wheel.Brake.IsBraking = false;
                        }
                    }
                    ////Use smooth steering; while held down, move towards maximum.
                    ////When not pressing any buttons, smoothly return to facing forward.
                    //float angle;
                    //bool steered = false;
                    //if (keyboardInput.IsKeyDown(Keys.S))
                    //{
                    //    steered = true;
                    //    angle = Math.Max(Vehicle.Wheels[1].Shape.SteeringAngle - TurnSpeed * dt, -MaximumTurnAngle);
                    //    Vehicle.Wheels[1].Shape.SteeringAngle = angle;
                    //    Vehicle.Wheels[3].Shape.SteeringAngle = angle;
                    //}
                    //if (keyboardInput.IsKeyDown(Keys.F))
                    //{
                    //    steered = true;
                    //    angle = Math.Min(Vehicle.Wheels[1].Shape.SteeringAngle + TurnSpeed * dt, MaximumTurnAngle);
                    //    Vehicle.Wheels[1].Shape.SteeringAngle = angle;
                    //    Vehicle.Wheels[3].Shape.SteeringAngle = angle;
                    //}
                    //if (!steered)
                    //{
                    //    //Neither key was pressed, so de-steer.
                    //    if (Vehicle.Wheels[1].Shape.SteeringAngle > 0)
                    //    {
                    //        angle = Math.Max(Vehicle.Wheels[1].Shape.SteeringAngle - TurnSpeed * dt, 0);
                    //        Vehicle.Wheels[1].Shape.SteeringAngle = angle;
                    //        Vehicle.Wheels[3].Shape.SteeringAngle = angle;
                    //    }
                    //    else
                    //    {
                    //        angle = Math.Min(Vehicle.Wheels[1].Shape.SteeringAngle + TurnSpeed * dt, 0);
                    //        Vehicle.Wheels[1].Shape.SteeringAngle = angle;
                    //        Vehicle.Wheels[3].Shape.SteeringAngle = angle;
                    //    }
                    //}


//#endif
                }
                else
                {
                    //Parking brake
                    foreach (Wheel wheel in Vehicle.Wheels)
                    {
                        wheel.Brake.IsBraking = true;
                    }
                    //Don't want the car to keep trying to drive.
                    Vehicle.Wheels[1].DrivingMotor.TargetSpeed = 0;
                    Vehicle.Wheels[3].DrivingMotor.TargetSpeed = 0;
                }
            }
        
    }
}
