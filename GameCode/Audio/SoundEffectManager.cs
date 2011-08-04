//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Audio;
//using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Media;
//using System.Threading;

//namespace BeatShift
//{
//    public class SoundEffectManager
//    {
//        SoundEffectInstance mskylar;
//        SoundEffectInstance momicron;
//        List<SoundEffectInstance> mwallCollide;
//        List<SoundEffectInstance> mraceStart;
//        SoundEffectInstance mfinalLap;
//        SoundEffectInstance mraceComplete;
//        SoundEffectInstance mlapComplete;
//        int raiseTime;
//        Boolean toBeRaised;
//        GameTime time;

//        public SoundEffectManager()
//        {
//            mraceStart = new List<SoundEffectInstance>();
//            mwallCollide = new List<SoundEffectInstance>();
//            time = new GameTime();
//        }

//        private SoundEffectInstance loadSound(String soundName, ContentManager content)
//        {
//            return content.Load<SoundEffect>("SoundEffects/" + soundName).CreateInstance();
//        }

//        public void LoadContent(ContentManager content)
//        {
//            mraceStart.Add(loadSound("3", content));
//            mraceStart.Add(loadSound("2", content));
//            mraceStart.Add(loadSound("1", content));
//            mraceStart.Add(loadSound("go", content));
//            mraceComplete = loadSound("race_complete", content);
//            mlapComplete = loadSound("lap_complete", content);
//            momicron = loadSound("omicronsnd", content);
//            mskylar = loadSound("skylarsnd", content);
//            mfinalLap = loadSound("final_lap", content);
//        }

//        public void LapComplete()
//        {
//            //hack-fix to not play sound on zero volume. this should be changed so that volumes are all managed and updated by a volume class.
//            if(Options.MasterVolume == 0)
//                return;

//            mlapComplete.Play();
//            BeatShift.bgm.Dim();
//            toBeRaised = true;

//        }

//        public void RaceStart(int count)
//        {
//            //hack-fix to not play sound on zero volume. this should be changed so that volumes are all managed and updated by a volume class.
//            if(Options.MasterVolume == 0)
//                return;

//            mraceStart[3 - count].Play();
//            BeatShift.bgm.Dim();
//        }

//        public void RaceComplete()
//        {
//            //hack-fix to not play sound on zero volume. this should be changed so that volumes are all managed and updated by a volume class.
//            if(Options.MasterVolume == 0)
//                return;

//            mraceComplete.Play();
//            BeatShift.bgm.Dim();
//        }

//        public void PlayShipName(ShipName shipName)
//        {
//            //hack-fix to not play sound on zero volume. this should be changed so that volumes are all managed and updated by a volume class.
//            if(Options.MasterVolume == 0)
//                return;

//            switch(shipName)
//            {
//                case ShipName.Skylar:
//                    mskylar.Play();
//                    break;
//                case ShipName.Omicron:
//                    momicron.Play();
//                    break;
//            }
//            BeatShift.bgm.Dim();
//        }

//        public Boolean IsPlaying()
//        {
//            if(mskylar.State == SoundState.Playing ||
//            momicron.State == SoundState.Playing ||
//            mfinalLap.State == SoundState.Playing ||
//            mraceComplete.State == SoundState.Playing ||
//            mlapComplete.State == SoundState.Playing)
//            {
//                return true;
//            }

//            foreach(SoundEffectInstance sound in mwallCollide)
//            {
//                if(sound.State == SoundState.Playing)
//                {
//                    return true;
//                }
//            }

//            foreach(SoundEffectInstance sound in mraceStart)
//            {
//                if(sound.State == SoundState.Playing)
//                {
//                    return true;
//                }
//            }

//            return false;
//        }

//    }


//}