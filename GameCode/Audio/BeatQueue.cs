using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeatShift.Utilities___Misc;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
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
        public BeatGlowParticleSystem beatGlow;
        // Variables to tweak difficulties on layers
        private double[] layerBonus = { 15, 10, 6, 4, 1 };
        private double[] layerPenalty = { 0.25, 0.375, 0.5, 0.75, 1.5 };
        private float[] layerLeeway = { 125.0f, 120.0f, 115.0f, 110.0f, 105.0f };
        private float[] layerBoost = { 0.0f, 0.1f, 0.2f, 0.3f, 0.4f };
        private float[] layerSpeedMultiplier = { 1.0f, 1.1f, 1.3f, 1.6f, 1.9f };
        private int combo;
        private Color missColor = Color.Black;
        private Color hitColor = Color.FloralWhite;
        private Color levelupColor = Color.Fuchsia;
        private Color leveldownColor = Color.SaddleBrown;

        public BeatQueue(Racer racer)
        {
            parentRacer = racer;
            beats = new Queue<Beat>();
            maxLayer = BeatShift.bgm.Layers();
        }

        public float getBoostRatio()
        {
            return layerBoost[myLayer];
        }
        public float getSpeedMultiplier()
        {
            return layerSpeedMultiplier[myLayer];
        }

        public double GetBoost() { return ((20*myLayer)+ (boostBar/5)); }

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
            parentRacer.visualizationSystems.AddParticleSystem(visualisation);
            visualisation.AutoInitialize(BeatShift.graphics.GraphicsDevice, BeatShift.contentManager, null);

            //Creates feedback visualization to show when beats are hit
            beatGlow = new BeatGlowParticleSystem(null);
            parentRacer.visualizationSystems.AddParticleSystem(beatGlow);
            beatGlow.AutoInitialize(BeatShift.graphics.GraphicsDevice, BeatShift.contentManager, null);
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
                            switch (temp.Button)
                            {

                                case Buttons.A:
                                    hitColor = Color.Lime;
                                    break;
                                case Buttons.B:
                                    hitColor = Color.Red;
                                    break;
                                case Buttons.X:
                                    hitColor = Color.DodgerBlue;
                                    break;
                                case Buttons.Y:
                                    hitColor = Color.Yellow;
                                    break;
                                default:
                                    hitColor = Color.Purple;
                                    break;
                            }


                        }
                        visualisation.RemoveRecent();
                        beats.Dequeue();
                        lastTime = temp.getTimeWithLatency((int)latency);
                        //System.Diagnostics.Debug.WriteLine(temp.getTimeWithLatency((int)latency) + ": Dequeued with ratio " + result + ". BB @:  \n" +
                                                   // "Distance to next: " + (time - (beats.Peek()).getTimeWithLatency((int)latency)) + "\n" +
                                                    // "Distance to last: " + (time - lastTime));
                    }
                }

                if ((result > 0m))
                {
                    if (boostBar == 100)
                        LevelUp();
                    boostBar += ((double)result * layerBonus[myLayer]);
                    beatGlow.Glow(hitColor, parentRacer.shipPhysics.ShipPosition, parentRacer.shipPhysics.racerEntity.LinearVelocity);
                    combo++;
                }
                else if ((boostBar > 0) && (result == 0m))
                {
                    combo = 0;
                    if (time > invinciEndtime)
                    {
                        boostBar -= layerPenalty[myLayer];
                        combo = 0;
                    }
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
                BeatShift.bgm.MusicDown(myLayer);
                boostBar = 98;
                visualisation.Clear();
                beats.Clear();
                //beatGlow.Glow(leveldownColor, parentRacer.shipPhysics.ShipPosition, parentRacer.shipPhysics.physicsBody.LinearVelocity);
                parentRacer.shipDrawing.engineGlow.setLayer(myLayer);
                parentRacer.messagePopupManager.addPopup(GameTextures.LevelDown, 500);
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
                BeatShift.bgm.MusicUp(myLayer);
                boostBar = 2;
                visualisation.Clear();
                beats.Clear();
                //beatGlow.Glow(levelupColor, parentRacer.shipPhysics.ShipPosition, parentRacer.shipPhysics.physicsBody.LinearVelocity);
                parentRacer.shipDrawing.engineGlow.setLayer(myLayer);
                parentRacer.messagePopupManager.addPopup(GameTextures.LevelUp, 700);
                parentRacer.messagePopupManager.addPopup(GameTextures.BoostOn, 1000);
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
                
                visualisation.addBeat(newBeat.Button, duration, elapsed);

                beats.Enqueue(newBeat);
            }   
        }

        public int getCombo()
        {
            return combo;
        }

        public void Update()
        {
            
            if (visualisation != null)
            {

                visualisation.SetPosition(parentRacer.shipPhysics.ShipPosition, parentRacer.shipPhysics.DrawOrientation);
                beatGlow.setPosition(parentRacer.shipPhysics.ShipPosition, parentRacer.shipPhysics.DrawOrientation);
            }
            if (!parentRacer.raceTiming.hasCompletedRace)
            {
                if (beats.Count > 0)
                {
                    Beat temp = beats.Peek();
                    if (temp.getTimeWithLatency((int)latency) < (BeatShift.bgm.songTick() - layerLeeway[myLayer]))
                    {
                        lastTime = temp.getTimeWithLatency((int)latency);
                        beats.Dequeue();
                        combo = 0;
                        if (lastTime > invinciEndtime)
                        {
                            boostBar -= layerPenalty[myLayer];
                        }
                    }
                }
                if (boostBar < 0)
                    LevelDown();
            }    
        }

        public void DrainBoost()
        {
            boostBar -= 1;
            if (boostBar < 0)
                boostBar = 0;
            else if (boostBar == 0)
                LevelDown();
        }

        
    }
}
