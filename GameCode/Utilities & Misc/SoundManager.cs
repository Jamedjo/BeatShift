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

namespace BeatShift
{
    
    public class SoundManager
    {
        static Cue mskylar;
        static Cue momicron;
        static Cue mwallCollide;
        static Cue mraceStart;
        static Cue mfinalLap;
        static Cue mraceComplete;
        static Cue mlapComplete;
        static int raiseTime;
        static Boolean toBeRaised;
        static GameTime time;
        static SoundBank soundBank;
        static WaveBank waveBank;
        AudioCategory voiceCategory;
        AudioCategory effectCategory;

        public static event VolumechangeHandler Music;
        public static event VolumechangeHandler Voice;
        public static event VolumechangeHandler Effect;

        public static float getMusicVolume()
        {
            return (Options.MusicVolume * (Options.MasterVolume/100.0f));
        }

        public static float getEffectVolume()
        {
            return (Options.SfxVolume * (Options.MasterVolume / 100.0f));
        }

        public static float getVoiceVolume()
        {
            return (Options.VoiceVolume * (Options.MasterVolume / 100.0f));
        }

        public static void masterVolumeChanged()
        {
            Music(EventArgs.Empty);
            Voice(EventArgs.Empty);
            Effect(EventArgs.Empty);
        }

        public static void musicVolumeChanged()
        {
            Music(EventArgs.Empty);
        }

        public static void voiceVolumeChanged()
        {
            Voice(EventArgs.Empty);
        }

        public static void sfxVolumeChanged()
        {
            Effect(EventArgs.Empty);
        }

        public SoundManager()
        {
            
            //mwallCollide = soundBank.GetCue("Interactive");
            soundBank = new SoundBank(BeatShift.engine, "Content\\XACT\\Voiceover.xsb");
            waveBank = new WaveBank(BeatShift.engine, "Content\\XACT\\SpeechEffects.xwb");
            voiceCategory = BeatShift.engine.GetCategory("Voice");
            effectCategory = BeatShift.engine.GetCategory("Effects");
            time = new GameTime();
            //Load the cues
            mraceStart = soundBank.GetCue("CountDown");
            mraceComplete = soundBank.GetCue("race_complete");
            mlapComplete = soundBank.GetCue("lap_complete"); 
            momicron = soundBank.GetCue("omicron"); 
            mskylar = soundBank.GetCue("skylar"); 
            mfinalLap = soundBank.GetCue("final_lap");
            SoundManager.Effect += new VolumechangeHandler(setEffectVolume);
            SoundManager.Voice += new VolumechangeHandler(setVoiceVolume);
        }

        private void setEffectVolume(EventArgs e)
        {   // Set the category volume.
            float temp = 2.0f * (SoundManager.getEffectVolume() / 100.0f);
            effectCategory.SetVolume(temp);
            System.Diagnostics.Debug.WriteLine(temp);
        }

        private void setVoiceVolume(EventArgs e)
        {   // Set the category volume.
            float temp = 2.0f * (SoundManager.getVoiceVolume() / 100.0f);
            voiceCategory.SetVolume(temp);
            System.Diagnostics.Debug.WriteLine(temp);
        }


        private SoundEffectInstance loadSound(String soundName, ContentManager content)
        {
            return content.Load<SoundEffect>("SoundEffects/" + soundName).CreateInstance();
        }

        public void LoadContent(ContentManager content)
        {
            Console.Write("Loading sounds... ");

            Console.WriteLine("   ...sounds loaded.");
        }

        public static void LapComplete()
        {

            //mlapComplete.Play();
            // TODO:uncomment after and mention on bug
            toBeRaised = true;

        }

        public static void RaceStart(int count)
        {
            //mraceStart.Play();
            // TODO:uncomment after and mention on bug
            soundBank.PlayCue("CountDown");
        }

        public static void RaceComplete()
        {

            //mraceComplete.Play();
            // TODO:uncomment after and mention on bug
        }

        public static void PlayShipName(ShipName shipName)
        {

            switch(shipName)
            {
                case ShipName.Skylar:
                    //mskylar.Play();
                    // TODO:uncomment after and mention on bug
                    break;
                case ShipName.Omicron:
                    //momicron.Play();
                    // TODO:uncomment after and mention on bug
                    break;
            }
        }

        public static Boolean IsPlaying()
        {
            if(mskylar.IsPlaying ||
            momicron.IsPlaying ||
            mfinalLap.IsPlaying ||
            mraceComplete.IsPlaying ||
            mlapComplete.IsPlaying ||
                mwallCollide.IsPlaying ||
                mraceStart.IsPlaying)
            {
                return true;
            }

            return false;
        }

    }


}