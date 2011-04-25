﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BeatShift.Cameras;
using BeatShift.Utilities___Misc;

namespace BeatShift
{
    public static class HeadsUpDisplay
    {
        public static String[] Ranks = { "th", "st", "nd", "rd", "th", "th", "th", "th", "th", "th", "th" }; //increase for more players
        public static int updatePeriod = 92;
        



        public static BeatVisualisation beatVisualisation = new BeatVisualisation(new Vector2(-60,450), new Vector2(400,450), 0.5f);

        //for displayingw rong way sign
        
        public static int counter = 0;

        public static void Update(GameTime gameTime)
        {
            beatVisualisation.Update(gameTime);
        }

        /// <summary>
        /// Draws a heads up display on the current camera in its viewport, as long as the ship is not PlayerType.None
        /// </summary>
        public static void DrawHUD(CameraWrapper camera, Racer racer, GameTime gameTime)//(Matrix viewMatrix, Matrix projectionMatrix)
        {
            if (racer.racerType == RacerType.None)
                return;//No HUD when just viewing ship model.
            BeatShift.graphics.GraphicsDevice.Viewport = camera.Viewport;
            //Draw speed text //must be after mesh.draw

            int vOffset = camera.Viewport.Height - 60;

            //MainGame.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;//Set display states
            BeatShift.spriteBatch.Begin();//(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            int h = BeatShift.graphics.GraphicsDevice.Viewport.Width / (GameTextures.HudBar.Width / GameTextures.HudBar.Height);
           
            BeatShift.spriteBatch.Draw(GameTextures.HudBar, new Rectangle(0, BeatShift.graphics.GraphicsDevice.Viewport.Height - h, BeatShift.graphics.GraphicsDevice.Viewport.Width, h), Color.White);

            racer.raceTiming.previousBoost = MathHelper.Lerp(racer.raceTiming.previousBoost, (float)racer.racingControls.getBoostValue() / 100, 0.05f);
            racer.raceTiming.previousLapProgress = MathHelper.Lerp(racer.raceTiming.previousLapProgress, (float)racer.shipPhysics.getLapPercentage() / 100, 0.05f);
            

            //source rectangle size should be whole height and perecent of width. Positioned at 0,0
            //destination rectange size should be same as source size, but positioned image height above the bottom of the scren
            int srcWidth = (int)(GameTextures.BoostBarLine.Width * racer.raceTiming.previousLapProgress);
            double scaleFactor = (double) BeatShift.graphics.GraphicsDevice.Viewport.Width / (double) GameTextures.BoostBarLine.Width;

            int destY_Offset = (BeatShift.graphics.GraphicsDevice.Viewport.Height) - GameTextures.HudBar.Height + (int)(scaleFactor * (138));

            
            int destHeight = (int)(scaleFactor * GameTextures.BoostBarLine.Height);
            int destWidth = (int)((GameTextures.BoostBarLine.Width * racer.raceTiming.previousLapProgress) * scaleFactor);

            Rectangle src = new Rectangle(GameTextures.BoostBarLine.Width - srcWidth, 0,srcWidth, GameTextures.BoostBarLine.Height);
            Rectangle dest = new Rectangle((int)(25*scaleFactor),destY_Offset, destWidth, GameTextures.BoostBarLine.Height);
            BeatShift.spriteBatch.Draw(GameTextures.BoostBarLine, dest, src, Color.White);

            BeatShift.spriteBatch.Draw(GameTextures.BoostBar, new Rectangle(((int)(BeatShift.graphics.GraphicsDevice.Viewport.Width * racer.raceTiming.previousLapProgress) - BeatShift.graphics.GraphicsDevice.Viewport.Width), (BeatShift.graphics.GraphicsDevice.Viewport.Height - h) - 12, BeatShift.graphics.GraphicsDevice.Viewport.Width, h), Color.Yellow);
            //BeatShift.spriteBatch.Draw(GameTextures.BoostBar, new Rectangle(((int)(BeatShift.graphics.GraphicsDevice.Viewport.Width * previousBoost) - BeatShift.graphics.GraphicsDevice.Viewport.Width), BeatShift.graphics.GraphicsDevice.Viewport.Height - h, BeatShift.graphics.GraphicsDevice.Viewport.Width, h), Color.White);
            
            
            if (racer.raceTiming.hasCompletedRace)
            {
                DrawMessage("Finished!", 325, vOffset/2);
                DrawMessage("Final Time: " + racer.raceTiming.getFinalTotalTime(), 300, vOffset/2 + 40);
            }
            else
            {

                racer.raceTiming.previousSpeed = MathHelper.Lerp(racer.raceTiming.previousSpeed, (Math.Abs((int)racer.shipPhysics.getForwardSpeed())), 0.05f);

                // Speed info, updates every update period
                //racer.raceTiming.lastUpdatedTimer += gameTime.ElapsedGameTime.Milliseconds;
                //if (racer.raceTiming.lastUpdatedTimer >= updatePeriod)
                //{
                //    racer.raceTiming.lastUpdatedTimer -= updatePeriod;
                //    try
                //    {
                //        racer.raceTiming.speedToDisplay = String.Format("{0:0000}", (Math.Abs((int)racer.shipPhysics.getForwardSpeed())));
                //    }
                //    catch (Exception e) {}
                //}

                racer.raceTiming.speedToDisplay = String.Format("{0:0000}", racer.raceTiming.previousSpeed);

                DrawMessage(BeatShift.blueNumbersFont, racer.raceTiming.speedToDisplay, 575, vOffset - 48, 0.6f);
                DrawMessage(BeatShift.blueNumbersFont, ":", 710, vOffset - 48, 0.6f);

                // Lap info
                //if (Race.currentRaceType.displayCurrentLapOutofTotalLaps)
                //{
                //    DrawMessage(BeatShift.newfont, (racer.raceTiming.currentLap + 1) + "/" + Race.currentRaceType.maxLaps, 680, 20, 0.4f);
                //    DrawMessage(BeatShift.newfontgreen, "LAPS", 720, 22, 0.4f);
                //}
                if (Race.currentRaceType.displayCurrentLapTime)
                {
                    DrawMessage(BeatShift.newfont, racer.raceTiming.getCurrentLapTime(), 680, 43, 0.4f);
                }
                //if (Race.currentRaceType.displayCurrentBestLap)
                //{
                //    DrawMessage(BeatShift.newfont, racer.raceTiming.getBestLapTime(), 680, 66, 0.4f);
                //}
                //if (Race.currentRaceType.displayCurrentRank)
                //{
                //    DrawMessage(BeatShift.newfontgreen, racer.raceTiming.currentRanking.ToString(), 10, 10, 1f);
                //    DrawMessage(BeatShift.newfontgreen, calculateRankSuffix(racer.raceTiming.currentRanking), 35 + extraSuffixOffset(racer.raceTiming.currentRanking), 40, 0.4f);
                //}
                double scaleFactorHeight = (double)BeatShift.graphics.GraphicsDevice.Viewport.Height / 720;
                double scaleFactorWidth = (double)BeatShift.graphics.GraphicsDevice.Viewport.Width / 1280;

                int newBoardWidth = (int)(GameTextures.TopRightBoard.Width * scaleFactorWidth);
                int newBoardHeight = (int)(GameTextures.TopRightBoard.Height * scaleFactorHeight);

                var d = new Rectangle(BeatShift.graphics.GraphicsDevice.Viewport.Width - newBoardWidth, 0, newBoardWidth, newBoardHeight);
                //BeatShift.spriteBatch.Draw(GameTextures.TopRightBoard, d, Color.White);

                foreach (Racer r in Race.humanRacers)
                {
                    if (r.isRespawning)
                    {
                        int newWarningWidth = (int)(GameTextures.ResettingSign.Width * scaleFactorWidth);
                        int newWarningHeight = (int)(GameTextures.ResettingSign.Height * scaleFactorHeight);
                        BeatShift.spriteBatch.Draw(GameTextures.ResettingSign, new Rectangle(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - newWarningWidth / 2, BeatShift.graphics.GraphicsDevice.Viewport.Height / 2 - newWarningHeight / 2, newWarningWidth, newWarningHeight), Color.White);
                    }

                }


                //if wrong way
                if (racer.shipPhysics.wrongWay == true)
                {
                    racer.raceTiming.displayWrongWay = true;
                    //DrawMessage("Wrong Way!", 300, vOffset / 2);
                }
                //uses counter to keep sign shown for a few seconds
                if (racer.raceTiming.displayWrongWay == true && counter < 500)
                {
                    int newWarningWidth = (int)(GameTextures.WrongWaySign.Width * scaleFactorWidth);
                    int newWarningHeight = (int)(GameTextures.WrongWaySign.Height * scaleFactorHeight);
                    BeatShift.spriteBatch.Draw(GameTextures.WrongWaySign, new Rectangle(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - newWarningWidth / 2, BeatShift.graphics.GraphicsDevice.Viewport.Height / 2 - newWarningHeight / 2, newWarningWidth, newWarningHeight), Color.White);
                    counter++;
                }
                else if (counter >= 500) { /*DrawMessage("Right Way", 250, vOffset / 2);*/ counter = 0; racer.raceTiming.displayWrongWay = false; }

                // Other info
                //DrawMessage("Progress: " + racer.getLapPercentage() + "%", 10, vOffset + 26);
                
                //DrawMessage("Accuracy: " + racer.racingControls.getLastPress(), 10, vOffset - 30);


                //DrawMessage("Dist from trac: " + racer.shipPhysics.shipRayToTrackTime, 10, vOffset - 30);

                if (Race.currentRaceType.countDownRunning)
                    BeatShift.spriteBatch.Draw(Race.currentRaceType.countdownState, new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - Race.currentRaceType.countdownState.Width / 2, BeatShift.graphics.GraphicsDevice.Viewport.Height / 2 - Race.currentRaceType.countdownState.Height / 2), Color.White);
                //DrawNewMessage(" !\"#$%'()*+,-./0123456789:;<=>?@ABCDEFGHIJ", 0, 30);
                //DrawNewMessage("KLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrrstuvwxyz{|}~", 0, 90);
            }
            BeatShift.spriteBatch.End();

            beatVisualisation.Draw(camera,racer.shipPhysics);
        }

        public static void DrawMessage(string message, int x, int y)
        {
            //spriteBatch.DrawString("Speed: " + shipSpeed, new Vector2(10, 10), Color.OrangeRed, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
            BeatShift.spriteBatch.DrawString(BeatShift.font, message, new Vector2(x + 1, y + 1), Color.Black);//,0,new Vector2(0,0),0.5f,null,0f);
            BeatShift.spriteBatch.DrawString(BeatShift.font, message, new Vector2(x, y), Color.White);
        }
        public static void DrawMessage(SpriteFont font, string message, int x, int y,float scale)
        {
            BeatShift.spriteBatch.DrawString(font, message, new Vector2(x, y), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 1); ;
        }

        public static void DrawSplitBars()
        {
            BeatShift.graphics.GraphicsDevice.Viewport = Viewports.fullViewport;
            BeatShift.spriteBatch.Begin();
            BeatShift.spriteBatch.Draw(GameTextures.HorizontalSplit, new Rectangle(0, BeatShift.graphics.GraphicsDevice.Viewport.Height / 2 - GameTextures.HorizontalSplit.Height / 2, BeatShift.graphics.GraphicsDevice.Viewport.Width, GameTextures.HorizontalSplit.Height), Color.White);
            BeatShift.spriteBatch.Draw(GameTextures.VerticalSplit, new Rectangle(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - GameTextures.VerticalSplit.Width / 2, 0, GameTextures.VerticalSplit.Width, BeatShift.graphics.GraphicsDevice.Viewport.Height), Color.White);

            BeatShift.spriteBatch.End();
        }

        public static String calculateRankSuffix(int target)
        { 
            if (!(target > 10 && target < 20))
                return Ranks[target % 10];
            else
                return "th";
        }

        public static int extraSuffixOffset(int target)
        {
            if (target >= 10)
                return 25;
            else
                return 0;
        }



    }
    
   
    }

