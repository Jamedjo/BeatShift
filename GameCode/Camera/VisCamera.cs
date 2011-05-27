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
    class VisCamera : ICameraType
    {
        private readonly Vector3 relativeFocalPoint;
        private readonly Vector3 relativePosition;

        public delegate Vector3 FGetUp();
        private readonly FGetUp getUp;

        private Matrix oldRot;

        public VisCamera(ref SharedCameraProperties properties, FGetPosition getPosition, FGetOrientation getOrientation, FGetUp getUpVector, float stopHeightOffset)
            : base(ref properties, getPosition, getOrientation)
        {
            this.baseFieldOfView = MathHelper.PiOver4;
            getUp = getUpVector;
            this.relativeFocalPoint = new Vector3(0.0f, 2.0f+stopHeightOffset, -20.0f);
            this.relativePosition = new Vector3(0.0f, 4.0f, 16.0f);
            oldRot = Matrix.CreateFromQuaternion(getOrientation());
        }

        /// <summary>
        /// Updates the lerped position/orientation/view
        /// </summary>
        /// <param name="gameTime">null/not used</param>
        public override void Update(GameTime gameTime)
        {
            //Use track up as camera up, would allow ship banking to be visible, make crashes less violent
            camLastUp = Vector3.Lerp(camLastUp, getUp(), 0.1f);

            camRotation = Quaternion.Lerp(camRotation, getOrientation(), 0.075f);

            Vector3 preTransformPosition = relativePosition;
            
            CamPosition = Vector3.Transform(preTransformPosition, camRotation);
            CamPosition += getPosition();

            CameraTarget = Vector3.Transform(relativeFocalPoint, camRotation);
            CameraTarget += getPosition();

            View = Matrix.CreateLookAt(CamPosition, CameraTarget, camLastUp);

        }
    }
}
