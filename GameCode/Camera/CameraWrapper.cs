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

        public Matrix VisualizationViewM
        {
            get
            {
                if (beatVisCamera == null) return View;
                return beatVisCamera.View;
            }
        }
        public Matrix VisualizationProjection
        {
            get
            {
                if (beatVisCamera == null) return Projection;
                return beatVisCamera.Projection;
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
        private VisCamera beatVisCamera;

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

        private void updateBeatVis()
        {
            beatVisCamera.Update(null);
            beatVisCamera.updateProjection();
        }

        public Vector3 cameraPosition()
        {
            return currentCamera.CamPosition;
        }

        void resetProjection()
        {
            //for each camera in list, updateProjection() ?, then update beatVisProjection?

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

                //If many HUMAN racers at this stage, then screen space limited, so adjust FOV.
                float chaseFOV = ICameraType.defaultFieldOfView;
                Vector3 chasePosition = new Vector3(0.0f, 4.0f, 16.0f);
                Vector3 chaseFocalPoint = new Vector3(0.0f, 1.0f, -40.0f);
                Vector3 chasePosition2 = new Vector3(0.0f, 2.5f, 12f);
                Vector3 chaseFocalPoint2 = new Vector3(0.0f, 1.0f, -30.0f);
                float beatVisHeightOffset = 0f;
                if (Race.humanRacers.Count > 1)
                {
                    chaseFOV = MathHelper.PiOver2;

                    chasePosition = new Vector3(0.0f, 2.5f, 12f);
                    chaseFocalPoint = new Vector3(0.0f, 6.0f, 0.0f);

                    chasePosition2 = new Vector3(0.0f, 2f, 10.0f);
                    chaseFocalPoint2 = new Vector3(0.0f, 5.0f, 0f);

                    beatVisHeightOffset = 2f;
                }

                beatVisCamera = new VisCamera(ref properties, getShipPosition, getShipOrientation, getShipUp, beatVisHeightOffset);
                updateBeatVis();

                cameraList.Add(new ChaseCamera(ref properties, getShipPosition, getShipOrientation, getShipUp, racer.shipPhysics.getForwardSpeed, chasePosition, chaseFocalPoint, chaseFOV));
                cameraList.Add(new ChaseCamera(ref properties, getShipPosition, getShipOrientation, getShipUp, racer.shipPhysics.getForwardSpeed, chasePosition2, chaseFocalPoint2, chaseFOV));
                cameraList.Add(new BumperCamera(ref properties, getShipPosition, getShipOrientation, getShipUp));
                currentCamera = cameraList[cameraID];
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
            return racer.shipPhysics.DrawOrientation;
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

                    //Update beatVisProjection //?TODO:?with v.small FOV change?
                    beatVisCamera.Update(null);

                    break;
                case CameraStage.PostRace:
                    break;
            }

        }

    }
}
