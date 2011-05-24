﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BeatShift
{
    /// <summary>
    /// Defines globals for use when debugging
    /// These values should never be saved and should be set using the console. (Press TAB on a keyboard in game).
    /// </summary>
    public static class Globals
    {
        public static bool UpdateRaceWithParallel = true;
        public static bool DisplayHUD = true;
        public static bool DisplayScenery = true;
        public static bool EnableParticles = true;

        //These are to be used to help position HUD items, without having to recompile
        //Before a commit is made, these should be reset to how they were found and chosen coordinates used
        //These can be set using the 'pos' command from the ingame console
        //public static Vector2 testVector1 = new Vector2();
        //public static Vector2 testVector2 = new Vector2();
        //public static Vector2 testVector3 = new Vector2();
        //public static Vector2 testVector4 = new Vector2();
        //public static Vector2 testVector5 = new Vector2();

        //TestStates are to be used to try different bits of code, switching between them in real-time wihtout having to recompile
        //Before a comit is made, all code using these should be finalized so they are no longer used
        public static int TestState = 0;
        private const int numberOfStates = 6;
        public static void nextState()
        {
            if(TestState>=numberOfStates-1) TestState = 0;
            else TestState++;
        }
    }

}
