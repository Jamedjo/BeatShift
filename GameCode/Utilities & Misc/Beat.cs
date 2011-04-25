using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace BeatShift
{
    public struct Beat
    {
        public readonly long Time;

        public readonly Buttons Button;

        public Beat(long time, Buttons button)
        {
            Time = time;
            Button = button;
        }

        public long getTimeWithLatency(long latency)
        {
            return Time + latency;
        }
    }
}
