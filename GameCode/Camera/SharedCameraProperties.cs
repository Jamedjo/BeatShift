using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BeatShift.Cameras
{
    class SharedCameraProperties
    {
        public Viewport Viewport;

        public Boolean ReverseCamera;

        public Boolean ShipBoosting;
        
        public SharedCameraProperties(Viewport viewport)
        {
            Viewport = viewport;
            ReverseCamera = false;
        }

    }
}
