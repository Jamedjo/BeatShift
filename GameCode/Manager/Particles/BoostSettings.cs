using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace BeatShift
{
    public class BoostSettings : ParticleSettings
    {
        public BoostSettings(Texture2D colorMap, Texture2D material) : base(colorMap, material)
        {
            velocity = new Vector3(0,0,-1);
            rndvelocity = new Vector3(0.5f, 0.5f, 0.5f);
            oneshot=false;
            maxEmitted = 10; // max is 167772
            minEmitted=10;
            maxEmit=5;
            minEmit=2;
            maxLife=20;
            minLife=7;
            maxRound=50;
            minRound=5;
            minSize=0.2f;
            maxSize=0.3f;
            maxSpeed=0.5f;
            minSpeed=0.2f;
        }
    }
}
