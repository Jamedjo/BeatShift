using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeatShift.Utilities___Misc;
using Microsoft.Xna.Framework.Input;

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
        int lastTime = 0;
        private long invinciEndtime = 0;
        Queue<Beat> beats;
        BeatVisualisation myBar;
        int maxLayer;
        private int[] averageDists;
        private int averageCounter = 0;
        public BeatQueue() {
            beats = new Queue<Beat>();
            myBar =  HeadsUpDisplay.beatVisualisation;
            maxLayer = BeatShift.bgm.Layers();
            averageDists = new int[averageLength];
            for(int i=0;i<averageLength;i++) {
                averageDists[i] = (int)latency;
            }
        }

        public Buttons nextBeatButton()
        {
            return beats.Peek().getKey();
        }

        public long nextBeatTime()
        {
            return beats.Peek().getTime(0);
        }

        public int GetBoost()
        {
            return boostBar;
        }


        public int getLayer()
        {
            return myLayer;
        }

        public void BeatTap(char button)
        {
            Decimal result = new Decimal(0);
            long time = BeatShift.bgm.songTick();
                while (beats.Count>0 && (beats.Peek()).getTime((int)latency) < time + leeway)
                {
                    Beat temp = beats.Peek();
                    if (temp.getTime((int)latency) < (time - leeway))
                    {
                        beats.Dequeue();
                        //System.Diagnostics.Debug.WriteLine(temp.getTime() + ": Dequeued because way out");
                    }
                    else
                    {
                        if (temp.getKey().Equals(button))
                        {
                            int difference = (int)(temp.getTime((int)latency) - time);
                            difference = Math.Abs(difference);
                            result = (decimal)(difference / leeway);
                            result = 1 - result;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Wrong button");
                        }
                        beats.Dequeue();
                        lastTime = temp.getTime((int)latency);
                        if (myLayer == 0)
                        {
                            AdjustLatency(time);
                        }
                        /*System.Diagnostics.Debug.WriteLine(temp.getTime(latency) + ": Dequeued with ratio " + result + ". BB @:  \n" +
                                                    "Distance to next: " + (time - (beats.Peek()).getTime(latency)) + "\n" +
                                                     "Distance to last: " + (time - lastTime));*/
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
                    long next = (time - (beats.Peek()).getTime((int)latency));
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
                System.Diagnostics.Debug.WriteLine("New Latency: " + latency);
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
                myBar.Clear();
                beats.Clear();
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
                myBar.Clear();
                beats.Clear();
            }
        }

        public void AddBeat(Beat newBeat)
        {
                     
            {
                int duration = 2500;
                int temp2 = (int)(newBeat.getTime((int)latency) - BeatShift.bgm.songTick());
                int elapsed = duration - temp2;
                //System.Console.WriteLine(elapsed + " " + temp2);
                switch (newBeat.getKey()){
                    case Buttons.A:
                        myBar.addBeat(ButtonImage.A, duration,elapsed);
                        //Console.Out.WriteLine("A");
                        break;
                    case Buttons.B:
                        myBar.addBeat(ButtonImage.B, duration, elapsed);
                        //Console.Out.WriteLine("B");
                        break;
                    case Buttons.X:
                        myBar.addBeat(ButtonImage.X, duration, elapsed);
                        //Console.Out.WriteLine("X");
                        break;
                    case Buttons.Y:
                        myBar.addBeat(ButtonImage.Y, duration, elapsed);
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
                if (temp.getTime((int)latency) < (BeatShift.bgm.songTick() - leeway))
                {
                    lastTime = temp.getTime((int)latency);
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
