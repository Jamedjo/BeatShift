using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics.Entities;

namespace BeatShift.Cameras
{
    abstract class ICameraType
    {
        /// <summary>
        /// The view matrix of the camera instance.
        /// </summary>
        public Matrix View
        {
            get;
            protected set;
        }

        /// <summary>
        ///  The projection matrix of the camera instance.
        /// </summary>
        public Matrix Projection
        {
            get;
            protected set;
        }

        /// <summary>
        /// The current position of the camera instance.
        /// </summary>
        public Vector3 CamPosition
        {
            get;
            protected set;
        }

        /// <summary>
        /// The coordinates the camera instance if looking at.
        /// </summary>
        public Vector3 CameraTarget
        {
            get;
            protected set;
        }

        /// <summary>
        /// If false, the players own ship will not be drawn. Needed for allowing the camera to
        /// be placed inside the model, or in a position where part of the ship may pass through
        /// the camera's field of view.
        /// </summary>
        public Boolean DrawOwnShip
        {
            get;
            protected set;
        }

        /// <summary>
        /// Returns the unit vector describing the vector the camera is looking.
        /// </summary>
        public Vector3 ViewVector
        {
            get
            {
                Vector3 vv = Vector3.Transform(CameraTarget - CamPosition, Matrix.CreateRotationY(0));
                vv.Normalize();
                return vv;
            }
        }
        
        protected Quaternion camRotation = Quaternion.Identity;
        protected Vector3 camLastUp = Vector3.Up;
        protected float fieldOfView = MathHelper.PiOver4;

        /// <summary>
        /// This must be shared between all camera instances for each player, or things will not
        /// function correctly. Should have been passed in by ref to the constructor, and passed
        /// up to the base constructor by ref if appropriate.
        /// </summary>
        protected readonly SharedCameraProperties properties;

#region Delegates

        /// <summary>
        /// Delegate for accessing position of object to be looked at.
        /// </summary>
        /// <returns>
        /// A Vector3 describing the position in world space to be looked at
        /// </returns>
        public delegate Vector3 FGetPosition();
        /// <summary>
        /// Delegate for accessing the Quaternion describing the orientation of the object to be
        /// looked at, relative to the world.
        /// </summary>
        /// <returns>
        /// A Quaternion describing the orientation of the object in world space.
        /// </returns>
        public delegate Quaternion FGetOrientation();

        /// <summary>
        /// Delegate method to get the position vector in world space of the object or point to be
        /// looked at.
        /// </summary>
        protected readonly FGetPosition getPosition;
        /// <summary>
        /// Delegate method to get the orientation quaternion of the object relative to world space.
        /// </summary>
        protected readonly FGetOrientation getOrientation;

#endregion

        public ICameraType(ref SharedCameraProperties properties, FGetPosition getPosition, FGetOrientation getOrientation)
        {
            View = Matrix.CreateLookAt(new Vector3(0, 6, 60), Vector3.Zero, camLastUp);
            Projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, properties.Viewport.AspectRatio, 1, 2500);
            this.properties = properties;
            this.getPosition = getPosition;
            this.getOrientation = getOrientation;
            DrawOwnShip = true;
        }

        /// <summary>
        /// Usually only needed to be called when the viewport of the camera is changed.
        /// </summary>
        public void updateProjection()
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, properties.Viewport.AspectRatio, 1, 2000);
        }

        public abstract void Update(GameTime gameTime);
    }
}
