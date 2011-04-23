using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities;

namespace BeatShift.Cameras
{
    class RevolveCamera : ICameraType
    {
        public RevolveCamera(ref SharedCameraProperties properties)
            : base(ref properties, null, null)
        {
            CameraTarget = Vector3.Up;
        }

        public override void Update(GameTime gameTime)
        {

            //rotate camera around ship
            Vector3 cameraPositionOffset = new Vector3(0.0f, 5.0f, -15f);//Rotate 5 units above ship and 16 away
            cameraPositionOffset = Vector3.Transform(cameraPositionOffset, Matrix.CreateFromAxisAngle(Vector3.Up, (float)gameTime.TotalGameTime.TotalMilliseconds / 1000));
                        CamPosition = CameraTarget + cameraPositionOffset;
            View = Matrix.CreateLookAt(CamPosition, CameraTarget, Vector3.Up);
        }
    }
}
