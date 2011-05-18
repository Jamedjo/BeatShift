using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework.Input;
using BEPUphysics;

namespace BeatShift.Cameras
{
    class ChaseCamera : ICameraType
    {
        private readonly Vector3 relativeFocalPoint;
        private readonly Vector3 relativePosition;
        
        public delegate Vector3 FGetUp();
        public delegate float FGetSpeed();

        private readonly FGetUp getUp;
        private readonly FGetSpeed getSpeed;

        private Matrix oldRot;

        public ChaseCamera(ref SharedCameraProperties properties, FGetPosition getPosition, FGetOrientation getOrientation, FGetUp getUpVector, FGetSpeed getSpeed, Vector3 relativePosition, Vector3 relativeFocalPoint)
            : base(ref properties, getPosition, getOrientation)
        {
            getUp = getUpVector;
            this.getSpeed = getSpeed;
            this.relativeFocalPoint = relativeFocalPoint;
            this.relativePosition = relativePosition;
            oldRot = Matrix.CreateFromQuaternion(getOrientation());
        }

        public override void Update(GameTime gameTime)
        {
            //Use track up as camera up, would allow ship banking to be visible, make crashes less violent
            camLastUp = Vector3.Lerp(camLastUp, getUp(), 0.1f);

            //Matrix rot = Matrix.CreateFromQuaternion(getOrientation());

            //float yaw = (float)Math.Acos(Vector3.Dot(rot.Up, oldRot.Up));
            //float pitch = (float)Math.Acos(Vector3.Dot(rot.Right, oldRot.Right));
            //float roll = (float)Math.Acos(Vector3.Dot(rot.Forward, oldRot.Forward));

            //oldRot = rot;

            //Quaternion smoothedRotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);

            //camRotation = Quaternion.Lerp(camRotation, camRotation * smoothedRotation, 0.1f);
            camRotation = Quaternion.Lerp(camRotation, getOrientation(), 0.075f);

            Vector3 preTransformPosition = relativePosition;
            //preTransformPosition += getSpeedOffset();

            CamPosition = Vector3.Transform(preTransformPosition, camRotation);
            CamPosition += getPosition();

            CameraTarget = Vector3.Transform(relativeFocalPoint, camRotation);
            CameraTarget += getPosition();

            // Cast a ray from 'behind' (relative to the ray) to ensure camera stays above the
            // ground. Move CamPosition if needed.
            //float minFloorDistance = 1.5f;
            //Vector3 adjustPositionDirection = Vector3.Normalize(Vector3.Cross((CameraTarget - CamPosition), Matrix.CreateFromQuaternion(getOrientation()).Left));

            //RayHit outRay;
            //Physics.currentTrackFloor.RayCast(new Ray(CamPosition - (minFloorDistance * adjustPositionDirection), adjustPositionDirection), 20f, out outRay);

            //CamPosition += outRay.T * adjustPositionDirection;

//#if WINDOWS
//            if (outRay.T != 0)
//                System.Diagnostics.Debug.WriteLine(outRay.T);
//#endif

            View = Matrix.CreateLookAt(CamPosition, CameraTarget, camLastUp);

            // Alters field of view based on speed
            //alterFieldOfView();

            // Alters field of view only while boost pressed
            // No check that the ship actually is/can though
            if(properties.ShipBoosting)
            {
                fieldOfView = MathHelper.Lerp(fieldOfView, 1.25f * baseFieldOfView, 0.05f);
                updateProjection();
            }
            else if(fieldOfView != baseFieldOfView)
            {
                fieldOfView = MathHelper.Lerp(fieldOfView, baseFieldOfView, 0.05f);
                updateProjection();
            }

            if(properties.ReverseCamera)
            {
                View = View * Matrix.CreateFromAxisAngle(View.Up, (float) Math.PI);
            }
        }

        private void alterFieldOfView()
        {
            float scaleValue = Math.Min(getSpeed() / 100, MathHelper.E - 1.0f);
            if (scaleValue < 0.0f)
            {
                scaleValue = 1.0f;
            }
            else
            {
                scaleValue += 1.0f;
            }
            fieldOfView = baseFieldOfView + ((float) Math.Log(scaleValue) * MathHelper.PiOver4 / 2.0f);
            updateProjection();
        }

        /// <summary>
        /// Gets the vector by which to offset the chase camer's position.
        /// Currently naively based on current speed, needs changing to some 'smarter' system.
        /// </summary>
        /// <returns></returns>
        private Vector3 getSpeedOffset()
        {
            return new Vector3(0.0f, 0.0f, getSpeed() / 50.0f);
        }
    }
}
