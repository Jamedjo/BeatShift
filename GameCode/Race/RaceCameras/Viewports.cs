using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace BeatShift
{
    public static class Viewports
    {

        public readonly static Viewport fullViewport;//should have private set public get. Only used publicly to reset viewports for menu.
        public readonly static Viewport topViewport;
        public readonly static Viewport topRightViewport;
        public readonly static Viewport topLeftViewport;
        public readonly static Viewport bottomViewport;
        public readonly static Viewport bottomRightViewport;
        public readonly static Viewport bottomLeftViewport;

        /// <summary>
        /// Initialize viewports.
        /// Use current viewport as full screen and create split screen viewports for later use.
        /// </summary>
        static Viewports()
        {
            //setup for 2 player splitscreen
            fullViewport = BeatShift.graphics.GraphicsDevice.Viewport;
            topViewport = BeatShift.graphics.GraphicsDevice.Viewport;
            bottomRightViewport = BeatShift.graphics.GraphicsDevice.Viewport;
            bottomLeftViewport = BeatShift.graphics.GraphicsDevice.Viewport;
            topRightViewport = BeatShift.graphics.GraphicsDevice.Viewport;
            topLeftViewport = BeatShift.graphics.GraphicsDevice.Viewport;

            topViewport.Height = (fullViewport.Height / 2);
            bottomViewport = topViewport;
            bottomViewport.Y = topViewport.Height;
            //set size for 4 player split
            bottomRightViewport.Height = (fullViewport.Height / 2);
            bottomRightViewport.Width = (fullViewport.Width / 2);
            bottomLeftViewport.Height = (fullViewport.Height / 2);
            bottomLeftViewport.Width = (fullViewport.Width / 2);
            topRightViewport.Height = (fullViewport.Height / 2);
            topRightViewport.Width = (fullViewport.Width / 2);
            topLeftViewport.Height = (fullViewport.Height / 2);
            topLeftViewport.Width = (fullViewport.Width / 2);
            //set X and Y for 4 player split
            topRightViewport.X = topLeftViewport.Width;
            bottomRightViewport.X = bottomLeftViewport.Width;
            bottomLeftViewport.Y = topViewport.Height;
            bottomRightViewport.Y = topViewport.Height;

        }
    }
}
