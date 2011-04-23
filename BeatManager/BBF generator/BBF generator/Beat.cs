using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BBF_generator
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

        public int getTime()
        {
            return time;
        }

        public char getKey()
        {
            return key;
        }
    }
}
