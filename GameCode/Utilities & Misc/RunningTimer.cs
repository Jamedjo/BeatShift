using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BeatShift
{
    /// <summary>
    /// Timer which only keeps track of time while game is not paused.
    /// Manges all subclasses created centrally, by static update being called in GameLoop
    /// </summary>
    public class RunningTimer
    {
        private static List<RunningTimer> existingTimers = new List<RunningTimer>();

        public static void Update(GameTime gameTime)
        {
            foreach (RunningTimer timer in existingTimers)
            {
                timer.UpdateTimer(gameTime);
            }
        }

        // Non Static code below

        private bool isStopped = true;
        TimeSpan elapsed;

        RunningTimer()
        {
            elapsed = new TimeSpan();
            existingTimers.Add(this);
        }

        ~RunningTimer()
        {
            existingTimers.Remove(this);
        }

        void UpdateTimer(GameTime gameTime)
        {
            if(!isStopped) elapsed.Add(gameTime.ElapsedGameTime); //TODO: this is a messy method as we are adding time between global updates not local ones and so may include a paused time period.
        }

        public void Start()
        {
            isStopped = false;
        }
        public void stop()
        {
            isStopped = true;
        }
        public void SetStopWatchToZeroAndStop()
        {
            isStopped = true;
            elapsed = new TimeSpan();

        }
        public void SetStopWatchToZeroAndStart()
        {
            isStopped = false;
            elapsed = new TimeSpan();
        }

        public TimeSpan Elapsed()
        {
            return (new TimeSpan()).Add(elapsed);
        }

    }
}
