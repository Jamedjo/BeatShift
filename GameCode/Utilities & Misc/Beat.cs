using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace BeatShift
{
    public class Beat
    {
        int time;
        Buttons key;

        public Beat(int ntime, Buttons nKey)
        {
            time = ntime;
            key = nKey;
        }

        public int getTime(int latency)
        {
            return time + latency;
        }

        public Buttons getKey()
        {
            return key;
        }
    }
}
