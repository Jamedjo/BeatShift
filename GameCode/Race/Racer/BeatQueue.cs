using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeatShift.Utilities___Misc;
using Microsoft.Xna.Framework.Input;
using DPSF;
using DPSF.ParticleSystems;

namespace BeatShift
{
    public class BeatQueue
    {
        const float leeway = 100.0f;
        const int averageLength = 10;
        const int latencyLeeway = 300;
        const int penalty = 1;
        float latency = 185;
        int boostBar = 0;
        int myLayer = 0;
        long lastTime = 0;
        private long invinciEndtime = 0;
        Queue<Beat> beats;
        //BeatVisualisation myBar;
        int maxLayer;
        private int[] averageDists;
        private int averageCounter = 0;
        public BeatRingParticleSystem visualisation;
        public BeatQueue() {
            beats = new Queue<Beat>();
            //myBar =  HeadsUpDisplay.beatVisualisation;
            maxLayer = BeatShift.bgm.Layers();
            averageDists = new int[averageLength];
            for(int i=0;i<averageLength;i++) {
                averageDists[i] = (int)latency;
            }
        }

        public Beat? nextBeat()
        {
            if (beats.Count != 0)
            {
                return beats.Peek();
            }
            else
            {
                return null;
            }
        }

        public int GetBoost()
        {
            return boostBar;
        }

        public int getLayer()
        {
            return myLayer;
        }

        public Boolean isLevellingUp { get; set; }

        public Boolean isLevellingDown { get; set; }

        public void Load()
        {
            visualisation = new BeatRingParticleSystem(null);
            BeatShift.particleManager.AddParticleSystem(visualisation);
            visualisation.AutoInitialize(BeatShift.graphics.GraphicsDevice, BeatShift.contentManager, null);
        }

        public void BeatTap(Buttons button)
        {
            Decimal result = new Decimal(0);
            long time = BeatShift.bgm.songTick();
                while (beats.Count>0 && (beats.Peek()).getTimeWithLatency((int)latency) < time + leeway)
                {
                    Beat temp = beats.Peek();
                    if (temp.getTimeWithLatency((int)latency) < (time - leeway))
                    {
                        beats.Dequeue();
                        //System.Diagnostics.Debug.WriteLine(temp.getTimeWithLatency((int)latency) + ": Dequeued because way out");
                    }
                    else
                    {
                        if (temp.Button.Equals(button))
                        {
                            int difference = (int)(temp.getTimeWithLatency((int)latency) - time);
                            difference = Math.Abs(difference);
                            result = (decimal)(difference / leeway);
                            result = 1 - result;
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine("Wrong button");
                        }
                        beats.Dequeue();
                        lastTime = temp.getTimeWithLatency((int)latency);
                        if (myLayer == 0)
                        {
                            AdjustLatency(time);
                        }
                        //System.Diagnostics.Debug.WriteLine(temp.getTimeWithLatency((int)latency) + ": Dequeued with ratio " + result + ". BB @:  \n" +
                                                   // "Distance to next: " + (time - (beats.Peek()).getTimeWithLatency((int)latency)) + "\n" +
                                                    // "Distance to last: " + (time - lastTime));
                    }
                }
            
            if (result == 0)
            {
                if (myLayer == 0)
                {
                    AdjustLatency(time);
                }
                //System.Diagnostics.Debug.WriteLine("PRESSED WITH NO NEAR BEAT. \n" +
                 //                                   "Distance to next: " + (time - (beats.Peek()).getTime((int)latency)) + "\n" +
                   //                                  "Distance to last: " + (time - lastTime));
            }
            
            if (boostBar == 100 && result > 0.9m)
                LevelUp();
            else if ((boostBar < 100) && (result > 0m))
                boostBar +=(int)(result * 5);
            else if ((boostBar > 0) && (result == 0m))
                if(time>invinciEndtime)
                    boostBar -= penalty;

            if (boostBar > 100)
            {
                boostBar = 100;
            }
            else if (boostBar < 0)
            {
                LevelDown();
            }
        }

        private void AdjustLatency(long time)
        {
            long tempoffset;
            if (beats.Count > 0)
            {
                {
                    long difference;
                    long next = (time - (beats.Peek()).getTimeWithLatency((int)latency));
                    long last = (time - lastTime);

                    if (Math.Abs(next) > Math.Abs(last))
                        difference = last;
                    else
                        difference = next;
                    if (!(Math.Abs(difference) < latencyLeeway))
                    {
                        return;
                    }
                    tempoffset = difference + (int)latency;
                }
                int temp = averageDists[averageCounter];
                averageDists[averageCounter++] = (int)tempoffset;
                if (averageCounter >= averageLength)
                {
                    averageCounter = 0;
                }
                latency -= temp / averageLength;
                latency += tempoffset / averageLength;
                /*System.Diagnostics.Debug.WriteLine(temp.getTime(latency) + ": Dequeued with ratio " + result + ". BB @:  \n" +
                                "Distance to next: " + (time - (beats.Peek()).getTime(latency)) + "\n" +
                                 "Distance to last: " + (time - lastTime));*/
                //System.Diagnostics.Debug.WriteLine("New Latency: " + latency);
            }
        }

        public void LevelDown()
        {
            if (myLayer > 0)
            {
                invinciEndtime = BeatShift.bgm.songTick() + 3000;
                myLayer--;
                BeatShift.bgm.MusicDown();
                boostBar = 80;
                visualisation.Clear();
                beats.Clear();
                isLevellingDown = true;
            }
            else
            {
                boostBar = 0;
            }
        }

        public void LevelUp()
        {
            if (myLayer < (maxLayer-1))
            {
                invinciEndtime = BeatShift.bgm.songTick() + 3000;
                myLayer++;
                BeatShift.bgm.MusicUp();
                boostBar = 20;
                visualisation.Clear();
                beats.Clear();
                isLevellingUp = true;
            }
        }

        public void AddBeat(Beat newBeat)
        {
                     
            {
                int duration = 2500;
                int temp2 = (int)(newBeat.getTimeWithLatency((int)latency) - BeatShift.bgm.songTick());
                int elapsed = duration - temp2;
                //System.Console.WriteLine(elapsed + " " + temp2);
                switch (newBeat.Button){
                    case Buttons.A:
                        visualisation.addBeat(Buttons.A, duration,elapsed);
                        //Console.Out.WriteLine("A");
                        break;
                    case Buttons.B:
                        visualisation.addBeat(Buttons.B, duration, elapsed);
                        //Console.Out.WriteLine("B");
                        break;
                    case Buttons.X:
                        visualisation.addBeat(Buttons.X, duration, elapsed);
                        //Console.Out.WriteLine("X");
                        break;
                    case Buttons.Y:
                        visualisation.addBeat(Buttons.Y, duration, elapsed);
                        //Console.Out.WriteLine("Y");
                        break;
                }
                beats.Enqueue(newBeat);
            }
            
            
        }

        public void Update()
        {
            if (beats.Count > 0)
            {
                Beat temp = beats.Peek();
                if (temp.getTimeWithLatency((int)latency) < (BeatShift.bgm.songTick() - leeway))
                {
                    lastTime = temp.getTimeWithLatency((int)latency);
                    beats.Dequeue();

                    //System.Diagnostics.Debug.WriteLine(temp + ": Dequeued because way out");
                    if (lastTime > invinciEndtime)
                    {
                            boostBar -= penalty;
                    }
                }
            }
            if (boostBar < 0)
                LevelDown();
        }

        public void DrainBoost()
        {
            boostBar -= 1;
            if (boostBar < 0)
                boostBar = 0;
        }
    }
}
