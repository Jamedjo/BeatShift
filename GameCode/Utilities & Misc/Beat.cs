using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeatShift
{
    public class Beat
    {
        int time;
        char key;

        public Beat(int ntime, char nKey)
        {
            time = ntime;
            key = nKey;
        }

        public int getTime(int latency)
        {
            return time + latency;
        }

        public char getKey()
        {
            return key;
        }
    }
}
