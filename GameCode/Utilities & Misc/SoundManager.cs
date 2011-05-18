using BeatShift.Util;
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
using BeatShift.Util;

namespace BeatShift
{
    
    public class SoundManager
    {
        static GameTime time;
        static SoundBank soundBank;
        static SoundBank effectSound;
        static WaveBank waveBank;
        static WaveBank effectBank;
        AudioCategory voiceCategory;
        AudioCategory effectCategory;
        public static string trackToLoad;
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
            soundBank = new SoundBank(BeatShift.engine, "Content\\XACT\\Voiceover.xsb");
            effectSound = new SoundBank(BeatShift.engine, "Content\\XACT\\SoundEffects.xsb");
            waveBank = new WaveBank(BeatShift.engine, "Content\\XACT\\SpeechEffects.xwb");
            effectBank = new WaveBank(BeatShift.engine, "Content\\XACT\\SoundEffects.xwb");
            voiceCategory = BeatShift.engine.GetCategory("Voice");
            effectCategory = BeatShift.engine.GetCategory("Effects");
            time = new GameTime();       
            
            
            
            
            SoundManager.Effect += new VolumechangeHandler(setEffectVolume);
            SoundManager.Voice += new VolumechangeHandler(setVoiceVolume);
        }

        private void setEffectVolume(EventArgs e)
        {   // Set the category volume.
            float temp = 2.0f * (SoundManager.getEffectVolume() / 100.0f);
            effectCategory.SetVolume(temp);
            // System.Diagnostic.Debug.WriteLine(temp);
        }

        private void setVoiceVolume(EventArgs e)
        {   // Set the category volume.
            float temp = 2.0f * (SoundManager.getVoiceVolume() / 100.0f);
            voiceCategory.SetVolume(temp);
            // System.Diagnostic.Debug.WriteLine(temp);
        }


        private SoundEffectInstance loadSound(String soundName, ContentManager content)
        {
            return content.Load<SoundEffect>("SoundEffects/" + soundName).CreateInstance();
        }

        public void LoadContent(ContentManager content)
        {
            // Console.Write("Loading sounds... ");

            // Console.WriteLine("   ...sounds loaded.");
        }

        public static void Collision()
        {
            effectSound.PlayCue("Collision");
        }

        public static void levelUp()
        {
            effectSound.PlayCue("levelup");
        }

        public static void levelDown()
        {
            effectSound.PlayCue("leveldown");
        }

        public static void MissedNote()
        {
            effectSound.PlayCue("MissedNote");
        }

        public static void FinalLap()
        {
            soundBank.PlayCue("final_lap");
        }

        public static void LapComplete()
        {
            effectSound.PlayCue("LapComplete"); 
        }

        public static void RaceStart(int count)
        {
            soundBank.PlayCue("CountDown");                
        }

        public static Cue getEngineHum()
        {
            return effectSound.GetCue("Hum");
        }

        public static void RaceComplete()
        {

            soundBank.PlayCue("race_complete");
        }

        public static void PlayShipName(ShipName shipName)
        {

            switch(shipName)
            {
                case ShipName.Skylar:
                    soundBank.PlayCue("skylar"); 
                    break;
                case ShipName.Omicron:
                    soundBank.PlayCue("omicron"); 
                    break;
            }
        }

    }


}