using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeatShift.Cameras;
using Microsoft.Xna.Framework;

namespace BeatShift
{
    public class RacerHuman : Racer
    {
        public CameraWrapper localCamera
        {
            get;
            private set;
        }

        // Index of the viewport this ship is controlled by.
        // Alows things to be drawn which only appear in the player's own screen
        int cameraIndex;

        public RacerHuman(RacerId rID, int shipNumber, RacerType racer_Type, int viewportIndex, Boolean hasPhysics)
            : base(rID, shipNumber, racer_Type)
        {
            cameraIndex = viewportIndex;
            localCamera = new CameraWrapper(this);
        }

        public override void insertShipOnMap(RacerType newRacerType)
        {
            base.insertShipOnMap(newRacerType);
            localCamera.nextStage();
        }

        public override void OtherUpdate(GameTime gameTime)
        {
            localCamera.Update(gameTime);
        }

        // Needs implementing, but need to decide how to maintain multiple cameras first.
        public void toggleCamera()
        {
            localCamera.toggleCamera();
        }

        public void toggleReverseCamera()
        {
            localCamera.toggleCameraDirection();
        }

        public override void setBoost(Boolean boosting)
        {
            localCamera.setBoost(boosting);
        }

    }
}
