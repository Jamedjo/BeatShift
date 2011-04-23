#if WINDOWS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BeatShift.Cameras
{
    /// <summary>
    /// Implementation of a free flying camera, using example code from:
    /// http://www.riemers.net/eng/Tutorials/XNA/Csharp/Series4/Mouse_camera.php
    /// </summary>
    class MouseCamera : ICameraType
    {
        private Vector2 mouseRotation = new Vector2(MathHelper.PiOver2, MathHelper.Pi / 10.0f);
        const float rotationSpeed = 0.3f;
        const float moveSpeed = 80.0f;
        private readonly MouseState originalMouseState;

        public MouseCamera(ref SharedCameraProperties properties, FGetPosition getPosition, FGetOrientation getOrientation)
            : base(ref properties, getPosition, getOrientation)
        {
            Mouse.SetPosition(properties.Viewport.Width / 2, properties.Viewport.Height / 2);
            originalMouseState = Mouse.GetState();
        }

        /// <summary>
        /// Calculates the camera's rotation matrix, then calculates the point to look at and the the
        /// appropriate up vector, then uses these to calculate the view matrix.
        /// </summary>
        private void UpdateViewMatrix()
        {
            Matrix cameraRotation = Matrix.CreateFromYawPitchRoll(mouseRotation.X, mouseRotation.Y, 0.0f);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = CamPosition + cameraRotatedTarget;

            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);
            Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);

            View = Matrix.CreateLookAt(CamPosition, cameraFinalTarget, cameraRotatedUpVector);
        }

        /// <summary>
        /// Add the Vector3 offset to the camera's position, after rotating it appropriately.
        /// </summary>
        /// <param name="vectorToAdd">
        /// The (unrotated) vector to move the camera position by.
        /// </param>
        private void AddToCameraPosition(Vector3 vectorToAdd)
        {
            Matrix cameraRotation = Matrix.CreateFromYawPitchRoll(mouseRotation.X, mouseRotation.Y, 0.0f);
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            CamPosition += moveSpeed * rotatedVector;
            UpdateViewMatrix();
        }

        /// <summary>
        /// Reads in the current input state, and update the position and view of the camera.
        /// </summary>
        /// <param name="timeElapsed">
        /// A scaling factor derived from the time since last update. Used to ensure the camera moves steadily.
        /// </param>
        private void processInput(float timeElapsed)
        {
            Vector3 moveVector = new Vector3(0, 0, 0);
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.U))
                moveVector += new Vector3(0, 0, -1);
            if (keyState.IsKeyDown(Keys.J))
                moveVector += new Vector3(0, 0, 1);
            if (keyState.IsKeyDown(Keys.K))
                moveVector += new Vector3(1, 0, 0);
            if (keyState.IsKeyDown(Keys.H))
                moveVector += new Vector3(-1, 0, 0);
            if (keyState.IsKeyDown(Keys.Y))
                moveVector += new Vector3(0, 1, 0);
            if (keyState.IsKeyDown(Keys.I))
                moveVector += new Vector3(0, -1, 0);

            MouseState currentMouseState = Mouse.GetState();
            if (currentMouseState != originalMouseState)
            {
                float xDifference = currentMouseState.X - originalMouseState.X;
                float yDifference = currentMouseState.Y - originalMouseState.Y;
                mouseRotation.X -= rotationSpeed * xDifference * timeElapsed;
                mouseRotation.Y -= rotationSpeed * yDifference * timeElapsed;
                Mouse.SetPosition(properties.Viewport.Width / 2, properties.Viewport.Height / 2);
            }

            // Calculate the new position of the camera, and update the view matrix.
            AddToCameraPosition(moveVector * timeElapsed);

            // If the 'P' key is pressed, reset the position of the free floating camera to the
            // centre of the ship. Overwrites previously calculated position
            if (keyState.IsKeyDown(Keys.P))
            {
                CamPosition = getPosition();
            }
        }

        public override void Update(GameTime gameTime)
        {
            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            processInput(timeDifference);
        }
    }
}
#endif