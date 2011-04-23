using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace BeatShift
{
    public static class GameTextures
    {
        public static Texture2D MenuBackground;
        public static Texture2D CountdownReady;
        public static Texture2D Countdown3;
        public static Texture2D Countdown2;
        public static Texture2D Countdown1;
        public static Texture2D CountdownGo;
        public static Texture2D HudBar;
        public static Texture2D BoostBar;
        public static Texture2D HorizontalSplit;
        public static Texture2D VerticalSplit;
        public static Texture2D WrongWaySign;
        public static Texture2D BeatVisualisation;

        public static void load(ContentManager c)
        {

            Console.Write("Loading game textures... ");
            MenuBackground = c.Load<Texture2D>("Images/splash");
            CountdownReady = c.Load<Texture2D>("Images/ready");
            Countdown3 = c.Load<Texture2D>("Images/3");
            Countdown2 = c.Load<Texture2D>("Images/2");
            Countdown1 = c.Load<Texture2D>("Images/1");
            CountdownGo = c.Load<Texture2D>("Images/go");
            HudBar = c.Load<Texture2D>("Images/HudBar");
            BoostBar = c.Load<Texture2D>("Images/BoostBar");
            HorizontalSplit = c.Load<Texture2D>("Images/horizontalsplit");
            VerticalSplit = c.Load<Texture2D>("Images/verticalsplit");
            WrongWaySign = c.Load<Texture2D>("Images/wrongway");
            BeatVisualisation = c.Load<Texture2D>("Visualisation/viz_red");
            Console.WriteLine("   ...textures loaded.");

        }
    }
}
