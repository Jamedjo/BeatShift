using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.IO;


namespace BeatShift
{
    /// <summary>
    /// This class handles background music and beat tracking.
    /// </summary>
    public class SoundTrack
    {
        const float leeway = 100.0f;
        int latency = 0;
        Boolean shouldPlay = false;
        Boolean recentBeat = false;
        Stopwatch tick;
        decimal mpb;
        private Boolean loaded = false;
        Queue<Beat>[] beats;
        Queue<Beat>[] originalBeats;
        Queue<Beat> activeBeats;
        int songLength;
        SoundBank soundBank;
        WaveBank waveBank;
        WaveBank effectWave;
        AudioCategory musicCategory;
        Cue track;
        private int currentLayer = 0;
        public void LoadBFF(StreamReader file) {
            string temp = file.ReadLine();
            //TODO: Do stuff with metadata
            temp = file.ReadLine();
            int layers = Convert.ToInt16(temp.Substring(1, temp.Length - 2));
            beats = new Queue<Beat>[layers]; 
            originalBeats = new Queue<Beat>[layers]; 
            activeBeats = new Queue<Beat>();
            for (int i = 0; i < layers; i++)
            {
                originalBeats[i] = new Queue<Beat>();
            }
            temp = file.ReadLine();
            songLength = Convert.ToInt32(temp.Substring(1, temp.Length - 2));
            while((temp=file.ReadLine())!=null)
            {
                System.Diagnostics.Debug.WriteLine(temp);
                string[] bits = temp.Split(' ');
                int time = Convert.ToInt32(bits[0].Substring(1, bits[0].Length - 2));
                string temp2 = bits[1].Substring(1, bits[1].Length - 2);
                string[] layerSet = temp2.Split(',');
                string temp3 = bits[2].Substring(1, bits[2].Length - 2);
                string[] buttonSet = temp3.Split(',');
                for(int ii = 0; ii<layerSet.Length; ii++) {
                    int layer = Convert.ToInt32(layerSet[ii]) - 1;
                    if (layer >= 0 && layer < layers)
                    {
                        originalBeats[layer].Enqueue(new Beat(time, convertButton(buttonSet[ii][0])));
                    }
                }
            }
            ResetBeats();
        }

        private Buttons convertButton(char buttonChar)
        {
            switch (buttonChar)
            {
                case 'A': return Buttons.A;
                case 'B': return Buttons.B;
                case 'X': return Buttons.X;
                case 'Y': return Buttons.Y;
            }

            // Default.
            return Buttons.A;
        }


        public void ResetBeats()
        {
            for (int i = 0; i < originalBeats.Length; i++)
            {
                beats[i] = new Queue<Beat>(originalBeats[i]);
            }
        }

        public void MusicUp()
        {
            if (currentLayer < originalBeats.Length - 1)
            {
                currentLayer++;
                BeatShift.engine.SetGlobalVariable("Layer", (currentLayer + 0.1f));
            }
        }

        public void MusicDown()
        {
            if (currentLayer > 0)
            {
                currentLayer--;
                BeatShift.engine.SetGlobalVariable("Layer", currentLayer);
            }
        }

        public SoundTrack(int bpm)           
        {
             tick = new Stopwatch();
             mpb = 60000.0m/bpm;
             soundBank = new SoundBank(BeatShift.engine, "Content\\XACT\\Map1.xsb");
             waveBank = new WaveBank(BeatShift.engine, "Content\\XACT\\Map1.xwb",0,32);
            effectWave = new WaveBank(BeatShift.engine,"Content\\XACT\\SoundEffects.xwb");
             musicCategory = BeatShift.engine.GetCategory("Music");
             track = soundBank.GetCue("Interactive");
            //track.
             while (!track.IsPrepared)
             {
                 System.Diagnostics.Debug.WriteLine("Not yet prepped");
             }
             SoundManager.Music += new VolumechangeHandler(setVolume);
        }

                
        private void setVolume(EventArgs e)
        {   // Set the category volume.
            float temp = 2.0f * (SoundManager.getMusicVolume() / 100.0f);
            musicCategory.SetVolume(temp);
            System.Diagnostics.Debug.WriteLine(temp);
        }


        public void LoadContent(ContentManager content, string track)
        {
            System.IO.Stream stream = TitleContainer.OpenStream(BeatShift.contentManager.RootDirectory + "/BFF/try.bff");
            LoadBFF(new StreamReader(stream));
        }

        /// <summary>
        /// Stops playing the music and beat tracking.
        /// </summary>
        public void stop()
        {
            tick.Reset();
            ResetBeats();
            currentLayer=0;
            BeatShift.engine.SetGlobalVariable("Layer", (currentLayer + 0.1f));
            track.Stop(AudioStopOptions.Immediate);
            shouldPlay = false;
        }

        public int Layers()
        {
            return originalBeats.Length;
        }


        /// <summary>
        /// Begin playing the music and beat tracking.
        /// </summary>
        public void play()
        {
            tick.Reset();
            ResetBeats();
            if (!track.IsPlaying)
            {
                track.Play();
            }
            while(!track.IsPlaying) {
                System.Diagnostics.Debug.WriteLine("Not yet playing");
            }
            tick.Start();
            shouldPlay = true;
        }
        /*
        /// <summary>
        /// Calculate where in the beat we are.  Returns a value from 0 to 1, 1 being on the beat, and 0 being perfectly between two beats
        /// </summary>
        public decimal beatTime()
        {
            Decimal result = new Decimal(0);
            while ((activeBeats.Peek()).getTime()+latency < tick.ElapsedMilliseconds + leeway)
            {
                Beat temp = activeBeats.Peek();
                if(temp.getTime()+latency<(tick.ElapsedMilliseconds-leeway)){
                    activeBeats.Dequeue();
                    //System.Diagnostics.Debug.WriteLine(temp.getTime() + ": Dequeued because way out. Off by: " + (tick.ElapsedMilliseconds-temp.getTime()));
                } else {
                    int difference = (int)(temp.getTime()+latency - tick.ElapsedMilliseconds);
                    difference = Math.Abs(difference);
                    result = (decimal)(difference/leeway);
                    activeBeats.Dequeue();
                    //System.Diagnostics.Debug.WriteLine(temp.getTime() + ": Dequeued with ratio " + result);
                    return result;
                }
            }
            if (result == 0)
            {
                System.Diagnostics.Debug.WriteLine("PRESSED WITH NO NEAR BEAT.  Distance to nearest: " + (tick.ElapsedMilliseconds - (activeBeats.Peek()).getTime()));
            }
            return result;
            
        }
        */

        public long songTick()
        {
            return tick.ElapsedMilliseconds;
        }

        /// <summary>
        /// Calculate where in the beat we are.  Returns a value from 0 to 1, 1 being on the beat, and 0 being perfectly between two beats
        /// </summary>
        public decimal trueBeatTime()
        {
            Decimal offset = new Decimal();
            offset = tick.ElapsedMilliseconds / mpb;
            //Console.Out.WriteLine("Offset: " + offset);
            //Console.Out.WriteLine("Rounded: " + Math.Round(offset, 0));
            decimal ratio = offset - Math.Round(tick.ElapsedMilliseconds / mpb);
            
            if (ratio > 0)
            {
                ratio = 0.5m - ratio;
            }
            else
            {
                ratio += 0.5m;
            }
            //Console.Out.WriteLine("Ratio: " + ratio);
            ratio *= 2;
            return Math.Abs(ratio*ratio);
        }

        public void Update()
        {
            //Adding bets into racers beatqueues.
            for(int i = 0; i<beats.Length;i++) {
            while ((beats[i].Count != 0) && (tick.ElapsedMilliseconds > (beats[i].Peek().Time - 2000)))
            {
                Beat beat = beats[i].Dequeue();
                foreach (Racer r in Race.humanRacers)
                {
                    if (r.beatQueue.getLayer() == i)
                    {
                        r.beatQueue.AddBeat(beat);
                    }
                }
                
                beats[i].Enqueue(new Beat(beat.Time + songLength,beat.Button));
            }
            }/*
                Beat temp = activeBeats.Peek();
                if (temp.getTime() < (tick.ElapsedMilliseconds - leeway))
                {
                    activeBeats.Dequeue();
                    //System.Diagnostics.Debug.WriteLine(temp + ": Dequeued because way out");
                    //TODO: Decrease Boost Bar here!!
                }*/
        }

    }
}
