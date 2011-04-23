using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BeatShift.Cameras
{
    class BumperCamera : ICameraType
    {
        public delegate Vector3 FGetUp();

        private readonly FGetUp getUp;

        private readonly static Vector3 offset = new Vector3(0.0f, 1.0f, -3.0f);
        private readonly static Vector3 focalPoint = new Vector3(0.0f, 0.0f, -30.0f);

        public BumperCamera(ref SharedCameraProperties properties, FGetPosition getPosition, FGetOrientation getOrientation, FGetUp getUpVector)
            : base(ref properties, getPosition, getOrientation)
        {
            DrawOwnShip = false;

            getUp = getUpVector;
        }

        public override void Update(GameTime gameTime)
        {
            //camLastUp = Vector3.Lerp(camLastUp, getUp(), 0.1f);
            camLastUp = getUp();

            //camRotation = Quaternion.Lerp(camRotation, getQuaternionOrientation(), 0.1f);
            camRotation = getOrientation();

            CamPosition = Vector3.Transform(offset, camRotation);
            CamPosition += getPosition();

            CameraTarget = getPosition() + Vector3.Transform(focalPoint, camRotation);

            View = Matrix.CreateLookAt(CamPosition, CameraTarget, camLastUp);

            if(properties.ReverseCamera)
            {
                View = View * Matrix.CreateFromAxisAngle(View.Up, (float) Math.PI);
            }
        }

    }
}
