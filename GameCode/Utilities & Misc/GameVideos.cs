using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace BeatShift
{
    public static class GameVideos
    {
        public static Video splashVideo;
        public static Video skylarVideo;
        public static Video omicronVideo;
        public static Video wraithVideo;
        public static Video fluxVideo;

        public static void load(ContentManager c)
        {
            splashVideo = BeatShift.contentManager.Load<Video>("Videos/splashVideo");
            skylarVideo = BeatShift.contentManager.Load<Video>("Videos/SkylarVideo");
            omicronVideo = BeatShift.contentManager.Load<Video>("Videos/OmicronVideo");
            wraithVideo = BeatShift.contentManager.Load<Video>("Videos/WraithVideo");
            fluxVideo = BeatShift.contentManager.Load<Video>("Videos/FluxVideo");
        }
    }
}
