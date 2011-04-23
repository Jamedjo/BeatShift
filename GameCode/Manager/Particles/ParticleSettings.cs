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
    public class ParticleSettings
    {
        public Texture2D material;
        public Texture2D colors;
        public Vector3 velocity = new Vector3(0,0,0);
        public Vector3 rndvelocity= new Vector3(1,1,1);
        public bool oneshot=true;
        public bool colorChange=false;
        public int maxEmitted=30000;
        public int minEmitted=20000;
        public int maxEmit=50;
        public int minEmit=5;
        public int maxLife=200;
        public int minLife=70;
        public int maxRound=50;
        public int minRound=5;
        public float minSize=1;
        public float maxSize=1.2f;
        public float maxSpeed=0.5f;
        public float minSpeed=0.2f;
        public float growth;

        public ParticleSettings(Texture2D colorMap, Texture2D material)
        {
            colors = colorMap;
            this.material = material;
        }

    }
}
