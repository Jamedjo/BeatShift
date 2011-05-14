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
        // Music latency
        float latency = 0;

        double boostBar = 0;
        int myLayer = 0;
        int maxLayer;
        long lastTime = 0;
        private long invinciEndtime = 0;

        Queue<Beat> beats;
        Racer parentRacer;
        public BeatRingParticleSystem visualisation;

        // Variables to tweak difficulties on layers
        private double[] layerBonus = { 15, 10, 6, 4, 1 };
        private double[] layerPenalty = { 0.25, 0.375, 0.5, 0.75, 1.5 };
        private float[] layerLeeway = { 125.0f, 120.0f, 115.0f, 105.0f, 95.0f };

        public BeatQueue(Racer racer)
        {
            parentRacer = racer;
            beats = new Queue<Beat>();
            maxLayer = BeatShift.bgm.Layers();
        }

        public double GetBoost() { return boostBar; }

        public int getLayer() { return myLayer; }

        public Boolean isLevellingUp { get; set; }

        public Boolean isLevellingDown { get; set; }

        public Beat? nextBeat()
        {
            if (beats.Count != 0)
                return beats.Peek();
            else
                return null;
        }

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
                while (beats.Count>0 && (beats.Peek()).getTimeWithLatency((int)latency) < time + layerLeeway[myLayer])
                {
                    Beat temp = beats.Peek();
                    if (temp.getTimeWithLatency((int)latency) < (time - layerLeeway[myLayer]))
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
                            result = (decimal)(difference / layerLeeway[myLayer]);
                            result = 1 - result;
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine("Wrong button");
                        }
                        beats.Dequeue();
                        lastTime = temp.getTimeWithLatency((int)latency);
                        //System.Diagnostics.Debug.WriteLine(temp.getTimeWithLatency((int)latency) + ": Dequeued with ratio " + result + ". BB @:  \n" +
                                                   // "Distance to next: " + (time - (beats.Peek()).getTimeWithLatency((int)latency)) + "\n" +
                                                    // "Distance to last: " + (time - lastTime));
                    }
                }
            
            if (boostBar == 100 && result > 0.9m)
                LevelUp();
            else if ((boostBar < 100) && (result > 0m))
                boostBar += ((double)result * layerBonus[myLayer]);
            else if ((boostBar > 0) && (result == 0m))
                if (time > invinciEndtime)
                {
                    boostBar -= layerPenalty[myLayer];
                    //SoundManager.MissedNote();
                }
            if (boostBar > 100)
                boostBar = 100;
            else if (boostBar < 0)
                LevelDown();
        }

        public void LevelDown()
        {
            if (myLayer > 0)
            {
                invinciEndtime = BeatShift.bgm.songTick() + 3000;
                myLayer--;
                BeatShift.bgm.MusicDown();
                boostBar = 50;
                visualisation.Clear();
                beats.Clear();
                isLevellingDown = true;
            }
            else
                boostBar = 0;
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
            if (!parentRacer.raceTiming.hasCompletedRace)
            {
                if (beats.Count > 0)
                {
                    Beat temp = beats.Peek();
                    if (temp.getTimeWithLatency((int)latency) < (BeatShift.bgm.songTick() - layerLeeway[myLayer]))
                    {
                        lastTime = temp.getTimeWithLatency((int)latency);
                        beats.Dequeue();

                        //System.Diagnostics.Debug.WriteLine(temp + ": Dequeued because way out");
                        if (lastTime > invinciEndtime)
                        {
                            boostBar -= layerPenalty[myLayer];
                           // SoundManager.MissedNote();
                        }
                    }
                }
                if (boostBar < 0)
                    LevelDown();
            }
            boostBar = 100;
                 
        }

        public void DrainBoost()
        {
            boostBar -= 1;
            if (boostBar < 0)
                boostBar = 0;
        }

        
    }
}
