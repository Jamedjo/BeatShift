using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BeatShift.Cameras
{
    public class CameraWrapper
    {
        private enum CameraStage { ShipSelect, Racing, PostRace }

        #region accessors

        public Viewport Viewport
        {
            get
            {
                return properties.Viewport;
            }
            set
            {
                properties.Viewport = value;
                resetProjection();
            }
        }

        public Matrix View
        {
            get
            {
                return currentCamera.View;
            }
        }

        public Matrix Projection
        {
            get
            {
                return currentCamera.Projection;
            }
        }

        public Boolean ShouldDrawOwnShip
        {
            get
            {
                return currentCamera.DrawOwnShip;
            }
        }

        public Vector3 ViewVector
        {
            get
            {
                return currentCamera.ViewVector;
            }
        }

        public Boolean ReverseCamera
        {
            get
            {
                return properties.ReverseCamera;
            }

            set
            {
                properties.ReverseCamera = value;
            }
        }

        #endregion

        private int cameraID = 0;
        private ICameraType currentCamera;
       
        private RevolveCamera revolveCamera;

        private CameraStage stage;
        public readonly Racer racer;
        private SharedCameraProperties properties;
        private List<ICameraType> cameraList = new List<ICameraType>();

#if WINDOWS
        private Boolean mouseCameraPressed = false;
        private readonly MouseCamera mouseCamera;
        private Boolean mouseCameraInUse = false;
#endif

        public CameraWrapper(Racer racer)
        {
            stage = CameraStage.ShipSelect;
            this.racer = racer;
            properties = new SharedCameraProperties(Viewports.fullViewport);
            revolveCamera = new RevolveCamera(ref properties);
            currentCamera = revolveCamera;
#if WINDOWS
            mouseCamera = new MouseCamera(ref properties, getShipPosition, getShipOrientation);
#endif
        }

        public Vector3 cameraPosition()
        {
            return currentCamera.CamPosition;
        }

        void resetProjection()
        {
            //Projection = Matrix.CreatePerspectiveFieldOfView(
            //    MathHelper.PiOver4 * 1.5f,
            //    (float)Viewport.Width /
            //    (float)Viewport.Height,
            //    1, 2000);
        }

        //Called when the ship/camera is placed on the map, 
        public void nextStage()
        {
            if (stage == CameraStage.ShipSelect)
            {
                stage++;
                cameraList.Add(new ChaseCamera(ref properties, getShipPosition, getShipOrientation, getShipUp, racer.shipPhysics.getForwardSpeed, new Vector3(0.0f, 4.0f, 16.0f), new Vector3(0.0f, 0.0f, -40.0f)));
                cameraList.Add(new ChaseCamera(ref properties, getShipPosition, getShipOrientation, getShipUp, racer.shipPhysics.getForwardSpeed, new Vector3(0.0f, 2.5f, 12f), new Vector3(0.0f, 0.0f, -30.0f)));
                cameraList.Add(new BumperCamera(ref properties, getShipPosition, getShipOrientation, getShipUp));
                currentCamera = cameraList[cameraID];
                if (changedFieldOfView != 0)
                {
                    setFOV(changedFieldOfView);
                }
            }
            else if (stage == CameraStage.Racing)
            {
                stage++;
            }
        }

        #region ship information wrappers

        protected Vector3 getShipPosition()
        {
            return racer.shipPhysics.ShipPosition;
        }

        protected Quaternion getShipOrientation()
        {
            return racer.shipPhysics.ShipOrientationQuaternion;
        }

        protected Vector3 getShipUp()
        {
            return racer.shipPhysics.ShipTrackUp;
        }
        #endregion

        public void toggleCameraDirection()
        {
            properties.ReverseCamera = !properties.ReverseCamera;
        }

        public void toggleCamera()
        {
            if (stage == CameraStage.Racing)
            {
                cameraID = ++cameraID % cameraList.Count;
                currentCamera = cameraList[cameraID];
            }
        }

        public void setBoost(Boolean boosting)
        {
            properties.ShipBoosting = boosting;
        }

        /// <summary>
        /// Updates all the camera instances, and handle switching to mouse camera if on Windows.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            switch (stage)
            {
                case CameraStage.ShipSelect:
                    revolveCamera.Update(gameTime);
                    break;
                case CameraStage.Racing:
#if WINDOWS
                    KeyboardState keyboardState = Keyboard.GetState();
                    if (keyboardState.IsKeyDown(Keys.M) != mouseCameraPressed)
                    {
                        mouseCameraPressed = !mouseCameraPressed;
                        if (mouseCameraPressed)
                        {
                            mouseCameraInUse = !mouseCameraInUse;
                            if (mouseCameraInUse)
                            {
                                currentCamera = mouseCamera;
                            }
                            else
                            {
                                currentCamera = cameraList[cameraID];
                            }
                        }
                    }
                    if (mouseCameraInUse)
                    {
                        mouseCamera.Update(gameTime);
                    }
#endif
                    foreach (ICameraType cam in cameraList)
                    {
                        cam.Update(gameTime);
                    }
                    break;
                case CameraStage.PostRace:
                    break;
            }

        }

        private float changedFieldOfView= 0;

        public void setFOV(float newFOV)
        {
            changedFieldOfView = newFOV;
            foreach (ICameraType c in cameraList)
            {
                if (c.GetType().Equals(typeof(ChaseCamera)))
                {
                    c.setBaseFieldOfView(newFOV);
                    c.updateProjection();
                }
            }
        }
    }
}
