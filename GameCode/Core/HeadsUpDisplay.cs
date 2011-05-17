using System;
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
        public static String[] Ranks = { "TH", "ST", "ND", "RD", "TH", "TH", "TH", "TH", "TH", "TH", "TH" }; //increase for more players
        public static int updatePeriod = 92;

        public static void Update(GameTime gameTime)
        {
            
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

            int bleedHeight = (int)(BeatShift.graphics.GraphicsDevice.Viewport.Height * 0.1);
            int bleedWidth = (int)(BeatShift.graphics.GraphicsDevice.Viewport.Width * 0.1);


            if (racer.raceTiming.hasCompletedRace && GameLoop.raceComplete != true)
            {
                ////////////////////////
                ///// FINAL RESULTS ////
                ////////////////////////
                if (Race.currentRaceType.getRaceTypeString().Equals("LappedRace"))
                {
                    //BeatShift.spriteBatch.Begin();//(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                    DrawMessage("Finished!", 325, vOffset / 2);
                    DrawMessage("Final Time: " + racer.raceTiming.getFinalTotalTime(), 300, vOffset / 2 + 40);
                }
                if (Race.currentRaceType.getRaceTypeString().Equals("EliminationRace"))
                {
                    BeatShift.spriteBatch.Draw(GameTextures.Eliminated, new Rectangle(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - GameTextures.Eliminated.Width / 2, 3 * BeatShift.graphics.GraphicsDevice.Viewport.Height / 3, GameTextures.Eliminated.Width, GameTextures.Eliminated.Height), Color.White);
                    //DrawMessage("ELIMINATED!", 325, vOffset / 2);
                }
                if (Race.currentRaceType.getRaceTypeString().Equals("TimeTrialRace"))
                {
                    //BeatShift.spriteBatch.Begin();
                    DrawMessage("Best lap time: " + racer.raceTiming.getBestLapTime(), 325, vOffset / 2);
                }

            }
            else
            {



                //////////////////////////////
                //////// TOTAL POINTS ////////
                //////////////////////////////

                if (Race.currentRaceType.displayTotalPoints)
                {
                    DrawMessageColour(BeatShift.newfont, racer.racerPoints.getPoints().ToString(), BeatShift.graphics.GraphicsDevice.Viewport.Width - 125, 195, 0.4f, Color.Goldenrod);
                }


                /////////////////////
                ///// HUD BAR ///////
                /////////////////////
                int h = BeatShift.graphics.GraphicsDevice.Viewport.Width / (GameTextures.HudBar.Width / GameTextures.HudBar.Height);

                if (BeatShift.graphics.GraphicsDevice.Viewport.Width > 700)
                {
                    BeatShift.spriteBatch.Draw(GameTextures.HudBar, new Rectangle(0, BeatShift.graphics.GraphicsDevice.Viewport.Height - GameTextures.HudBar.Height, GameTextures.HudBar.Width, GameTextures.HudBar.Height), Color.White);
                }
                else
                {
                    BeatShift.spriteBatch.Draw(GameTextures.HudBarSmall, new Rectangle(0, BeatShift.graphics.GraphicsDevice.Viewport.Height - GameTextures.HudBarSmall.Height, GameTextures.HudBarSmall.Width, GameTextures.HudBarSmall.Height), Color.White);
                }
                ////////////////////////////
                ///// ORANGE BOOST BAR /////
                ////////////////////////////

                racer.raceTiming.previousBoost = MathHelper.Lerp(racer.raceTiming.previousBoost, (float)racer.racingControls.getBoostValue() / 100, 0.05f);
                racer.raceTiming.previousLapProgress = MathHelper.Lerp(racer.raceTiming.previousLapProgress, (float)racer.shipPhysics.getLapPercentage() / 100, 0.05f);

                var chosenLine = (BeatShift.graphics.GraphicsDevice.Viewport.Width > 700) ? GameTextures.BoostBarLine : GameTextures.BoostBarLineSmall;
                int srcWidth = (int)((chosenLine.Width) * racer.raceTiming.previousBoost);

                if (BeatShift.graphics.GraphicsDevice.Viewport.Width > 700)
                {
                    Rectangle orange_src2 = new Rectangle(chosenLine.Width - srcWidth, 0, chosenLine.Width, chosenLine.Height);
                    Rectangle orange_dest = new Rectangle(36, BeatShift.graphics.GraphicsDevice.Viewport.Height - chosenLine.Height, chosenLine.Width, chosenLine.Height);
                    BeatShift.spriteBatch.Draw(chosenLine, orange_dest, orange_src2, Color.White);
                }
                else
                {
                    Rectangle orange_src2 = new Rectangle(chosenLine.Width - srcWidth, 0, chosenLine.Width, chosenLine.Height);
                    Rectangle orange_dest = new Rectangle(21, BeatShift.graphics.GraphicsDevice.Viewport.Height - chosenLine.Height, chosenLine.Width, chosenLine.Height);
                    BeatShift.spriteBatch.Draw(chosenLine, orange_dest, orange_src2, Color.White);
                }

                ////////////////////////
                /////// LEVEL //////////
                ////////////////////////

                if (BeatShift.graphics.GraphicsDevice.Viewport.Width > 700)
                {
                    DrawMessageColour(BeatShift.volterfont, "LEVEL", BeatShift.graphics.GraphicsDevice.Viewport.Width - 240, vOffset - 53, 0.5f, Color.PapayaWhip);
                    DrawMessageColour(BeatShift.volterfont, (racer.beatQueue.getLayer() + 1).ToString(), BeatShift.graphics.GraphicsDevice.Viewport.Width - 230, vOffset - 35, 1f, Color.PapayaWhip);
                }
                else
                {
                    DrawMessageColour(BeatShift.volterfont, "LEVEL", BeatShift.graphics.GraphicsDevice.Viewport.Width - 143, vOffset - 12, 0.4f, Color.PapayaWhip);
                    DrawMessageColour(BeatShift.volterfont, (racer.beatQueue.getLayer() + 1).ToString(), BeatShift.graphics.GraphicsDevice.Viewport.Width - 136, vOffset - 3, 0.8f, Color.PapayaWhip);
                }

                ////////////////////////
                /////// SPEED //////////
                ////////////////////////
                try
                {
                    racer.raceTiming.previousSpeed = MathHelper.Lerp(racer.raceTiming.previousSpeed, (Math.Abs((int)racer.shipPhysics.getForwardSpeed())), 0.05f);
                }
                catch (OverflowException e)
                {
                    racer.raceTiming.previousSpeed = 0;
                }
                racer.raceTiming.speedToDisplay = String.Format("{0:0000}", racer.raceTiming.previousSpeed);
                if (BeatShift.graphics.GraphicsDevice.Viewport.Width > 700)
                {
                    DrawMessageColour(BeatShift.volterfont, "MPH", BeatShift.graphics.GraphicsDevice.Viewport.Width - 126, vOffset - 42, 0.5f, Color.Black);
                    DrawMessageColour(BeatShift.newfont, racer.raceTiming.speedToDisplay, BeatShift.graphics.GraphicsDevice.Viewport.Width - 185, vOffset - 48, 0.4f, Color.Black);
                }
                else
                {
                    DrawMessageColour(BeatShift.volterfont, "MPH", BeatShift.graphics.GraphicsDevice.Viewport.Width - 75, vOffset - 14, 0.5f, Color.Black);
                    DrawMessageColour(BeatShift.newfont, racer.raceTiming.speedToDisplay, BeatShift.graphics.GraphicsDevice.Viewport.Width - 110, vOffset - 8, 0.4f, Color.Black);
                }

                //////////////////////////////
                /////// TOP RIGHT BOARD //////
                //////////////////////////////

                double scaleFactorHeight = (double)BeatShift.graphics.GraphicsDevice.Viewport.Height / 720;
                double scaleFactorWidth = scaleFactorHeight;// (double)BeatShift.graphics.GraphicsDevice.Viewport.Width / 1280;

                int newBoardWidth = (int)(GameTextures.TopRightBoard.Width * scaleFactorWidth);
                int newBoardHeight = (int)(GameTextures.TopRightBoard.Height * scaleFactorHeight);

                if (BeatShift.graphics.GraphicsDevice.Viewport.Width > 700)
                {
                    Rectangle d = new Rectangle(BeatShift.graphics.GraphicsDevice.Viewport.Width - GameTextures.TopRightBoard.Width, 0, GameTextures.TopRightBoard.Width, GameTextures.TopRightBoard.Height);
                    BeatShift.spriteBatch.Draw(GameTextures.TopRightBoard, d, Color.White);
                }
                else
                {
                    Rectangle d = new Rectangle(BeatShift.graphics.GraphicsDevice.Viewport.Width - GameTextures.TopRightBoardSmall.Width, 0, GameTextures.TopRightBoardSmall.Width, GameTextures.TopRightBoardSmall.Height);
                    BeatShift.spriteBatch.Draw(GameTextures.TopRightBoardSmall, d, Color.White);
                }
                //////////////////////////////
                ////////// TOTAL LAPS ////////
                //////////////////////////////

                if (Race.currentRaceType.displayCurrentLapOutofTotalLaps)
                {
                    if (BeatShift.graphics.GraphicsDevice.Viewport.Width > 700)
                    {
                        if (racer.raceTiming.isRacing == true)
                            DrawMessageColour(BeatShift.newfont, (racer.raceTiming.currentLap + 1) + "/" + Race.currentRaceType.maxLaps, BeatShift.graphics.GraphicsDevice.Viewport.Width - 125, 33, 0.5f, Color.PapayaWhip);
                        else
                            DrawMessageColour(BeatShift.newfont, (racer.raceTiming.currentLap) + "/" + Race.currentRaceType.maxLaps, BeatShift.graphics.GraphicsDevice.Viewport.Width - 125, 33, 0.5f, Color.PapayaWhip);
                        DrawMessageColour(BeatShift.volterfont, "LAPS", BeatShift.graphics.GraphicsDevice.Viewport.Width - 74, 39, 1f, Color.DimGray);
                    }
                    else
                    {
                        if (racer.raceTiming.isRacing == true)
                            DrawMessageColour(BeatShift.volterfont, (racer.raceTiming.currentLap + 1) + "/" + Race.currentRaceType.maxLaps, BeatShift.graphics.GraphicsDevice.Viewport.Width - 82, 25, 0.5f, Color.PapayaWhip);
                        else
                            DrawMessageColour(BeatShift.volterfont, (racer.raceTiming.currentLap) + "/" + Race.currentRaceType.maxLaps, BeatShift.graphics.GraphicsDevice.Viewport.Width - 82, 25, 0.5f, Color.PapayaWhip);
                        DrawMessageColour(BeatShift.volterfont, "LAPS", BeatShift.graphics.GraphicsDevice.Viewport.Width - 54, 25, 0.5f, Color.PapayaWhip);
                    }
                }

                //////////////////////////////
                ///////// CURRENT LAP ////////
                //////////////////////////////

                if (Race.currentRaceType.displayCurrentLapTime)
                {
                    if (BeatShift.graphics.GraphicsDevice.Viewport.Width > 700)
                    {
                        DrawMessageColour(BeatShift.newfont, racer.raceTiming.getCurrentLapTime(), BeatShift.graphics.GraphicsDevice.Viewport.Width - 125, 70, 0.4f, Color.PapayaWhip);
                    }
                    else
                    {
                        DrawMessageColour(BeatShift.newfont, racer.raceTiming.getCurrentLapTime(), BeatShift.graphics.GraphicsDevice.Viewport.Width - 125, 36, 0.4f, Color.PapayaWhip);
                    }
                }

                //////////////////////////////
                ////////// BEST LAP //////////
                //////////////////////////////

                if (Race.currentRaceType.displayCurrentBestLap)
                {
                    if (BeatShift.graphics.GraphicsDevice.Viewport.Width > 700)
                    {
                        DrawMessageColour(BeatShift.newfont, racer.raceTiming.getBestLapTime(), BeatShift.graphics.GraphicsDevice.Viewport.Width - 125, 95, 0.4f, Color.Goldenrod);
                    }

                    //////////////////////////
                    ////////// RANK //////////
                    //////////////////////////

                    if (Race.currentRaceType.displayCurrentRank && racer.raceTiming.currentRanking == 0)
                    {
                        if (BeatShift.graphics.GraphicsDevice.Viewport.Width > 700)
                        {
                            DrawMessageColour(BeatShift.newfont, "-", BeatShift.graphics.GraphicsDevice.Viewport.Width - 190, 22, 0.75f, Color.PapayaWhip);
                        }
                        else
                        {
                            DrawMessageColour(BeatShift.newfont, "-", BeatShift.graphics.GraphicsDevice.Viewport.Width - 187, 22, 0.75f, Color.PapayaWhip);
                        }
                    }
                    else if (Race.currentRaceType.displayCurrentRank)
                    {
                        if (BeatShift.graphics.GraphicsDevice.Viewport.Width > 700)
                        {
                            DrawMessageColour(BeatShift.newfont, racer.raceTiming.currentRanking.ToString(), BeatShift.graphics.GraphicsDevice.Viewport.Width - 190, 22, 0.75f, Color.PapayaWhip);
                        }
                        else
                        {
                            DrawMessageColour(BeatShift.newfont, racer.raceTiming.currentRanking.ToString(), BeatShift.graphics.GraphicsDevice.Viewport.Width - 187, 22, 0.75f, Color.PapayaWhip);
                        }
                        //DrawMessage(BeatShift.newfont, calculateRankSuffix(racer.raceTiming.currentRanking), BeatShift.graphics.GraphicsDevice.Viewport.Width - 178 + extraSuffixOffset(racer.raceTiming.currentRanking), 26, 0.25f);
                    }

                    /////////////////////////////
                    ////// RESETTING SIGN ///////
                    /////////////////////////////


                    if (racer.isRespawning && racer.shipPhysics.millisecsLeftTillReset < 2000)
                    {
                        int newWarningWidth = (int)(GameTextures.ResettingSign.Width * scaleFactorWidth);
                        int newWarningHeight = (int)(GameTextures.ResettingSign.Height * scaleFactorHeight);
                        BeatShift.spriteBatch.Draw(GameTextures.ResettingSign, new Rectangle(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - newWarningWidth / 2, BeatShift.graphics.GraphicsDevice.Viewport.Height / 2 - newWarningHeight / 2, newWarningWidth, newWarningHeight), Color.White);
                    }


                    /////////////////////////////
                    ////// COUNTDOWN SIGN ///////
                    /////////////////////////////

                    if (Race.currentRaceType.countDownRunning)
                        BeatShift.spriteBatch.Draw(Race.currentRaceType.countdownState, new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - Race.currentRaceType.countdownState.Width / 2, BeatShift.graphics.GraphicsDevice.Viewport.Height / 2 - Race.currentRaceType.countdownState.Height / 2), Color.White);

                    // Other info
                    //DrawMessage("Progress: " + racer.getLapPercentage() + "%", 10, vOffset + 26);
                    //DrawMessage("Accuracy: " + racer.racingControls.getLastPress(), 10, vOffset - 30);
                    //DrawMessage("Dist from trac: " + racer.shipPhysics.shipRayToTrackTime, 10, vOffset - 30);
                    //DrawNewMessage(" !\"#$%'()*+,-./0123456789:;<=>?@ABCDEFGHIJ", 0, 30);
                    //DrawNewMessage("KLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrrstuvwxyz{|}~", 0, 90);
                }


            }
        }

        public static void DrawMessage(string message, int x, int y)
        {
            //spriteBatch.DrawString("Speed: " + shipSpeed, new Vector2(10, 10), Color.OrangeRed, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
            BeatShift.spriteBatch.DrawString(BeatShift.newfont, message, new Vector2(x + 1, y + 1), Color.Black);//,0,new Vector2(0,0),0.5f,null,0f);
            BeatShift.spriteBatch.DrawString(BeatShift.newfont, message, new Vector2(x, y), Color.White);
        }
        public static void DrawMessage(SpriteFont font, string message, int x, int y,float scale)
        {
            BeatShift.spriteBatch.DrawString(font, message, new Vector2(x, y), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 1); ;
        }

        public static void DrawMessageColour(SpriteFont font, string message, int x, int y, float scale, Color col)
        {
            BeatShift.spriteBatch.DrawString(font, message, new Vector2(x, y), col, 0f, Vector2.Zero, scale, SpriteEffects.None, 1); ;
        }

            public static void DrawSplitBarsTwoPlayer()
            {
                //BeatShift.spriteBatch.Begin();
                BeatShift.spriteBatch.Draw(GameTextures.VerticalSplit, new Rectangle(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - GameTextures.VerticalSplit.Width / 2, 0, GameTextures.VerticalSplit.Width, BeatShift.graphics.GraphicsDevice.Viewport.Height), Color.White);
            }

            public static void DrawSplitBarsThreePlayer()
            {
                //BeatShift.spriteBatch.Begin();
                BeatShift.spriteBatch.Draw(GameTextures.HorizontalSplit, new Rectangle(BeatShift.graphics.GraphicsDevice.Viewport.Width, BeatShift.graphics.GraphicsDevice.Viewport.Height / 2 - GameTextures.HorizontalSplit.Height / 2, BeatShift.graphics.GraphicsDevice.Viewport.Width/2, GameTextures.HorizontalSplit.Height), Color.White);
                BeatShift.spriteBatch.Draw(GameTextures.VerticalSplit, new Rectangle(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - GameTextures.VerticalSplit.Width / 2, 0, GameTextures.VerticalSplit.Width, BeatShift.graphics.GraphicsDevice.Viewport.Height), Color.White);
            }

            public static void DrawSplitBarsFourPlayer()
            {
                //BeatShift.spriteBatch.Begin();
                BeatShift.spriteBatch.Draw(GameTextures.HorizontalSplit, new Rectangle(0, BeatShift.graphics.GraphicsDevice.Viewport.Height / 2 - GameTextures.HorizontalSplit.Height / 2, BeatShift.graphics.GraphicsDevice.Viewport.Width, GameTextures.HorizontalSplit.Height), Color.White);
                BeatShift.spriteBatch.Draw(GameTextures.VerticalSplit, new Rectangle(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - GameTextures.VerticalSplit.Width / 2, 0, GameTextures.VerticalSplit.Width, BeatShift.graphics.GraphicsDevice.Viewport.Height), Color.White);
                BeatShift.spriteBatch.Draw(GameTextures.Crest, new Rectangle(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - GameTextures.Crest.Width / 2, BeatShift.graphics.GraphicsDevice.Viewport.Height / 2 - GameTextures.Crest.Height / 2, GameTextures.Crest.Width, GameTextures.Crest.Height), Color.White);
            }

        public static void DrawSplitBars()
        {
            BeatShift.graphics.GraphicsDevice.Viewport = Viewports.fullViewport;
            BeatShift.spriteBatch.Begin();
            BeatShift.spriteBatch.Draw(GameTextures.HorizontalSplit, new Rectangle(0, BeatShift.graphics.GraphicsDevice.Viewport.Height / 2 - GameTextures.HorizontalSplit.Height / 2, BeatShift.graphics.GraphicsDevice.Viewport.Width, GameTextures.HorizontalSplit.Height), Color.White);
            BeatShift.spriteBatch.Draw(GameTextures.VerticalSplit, new Rectangle(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - GameTextures.VerticalSplit.Width / 2, 0, GameTextures.VerticalSplit.Width, BeatShift.graphics.GraphicsDevice.Viewport.Height), Color.White);
            BeatShift.spriteBatch.Draw(GameTextures.Crest, new Rectangle(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - GameTextures.Crest.Width / 2, BeatShift.graphics.GraphicsDevice.Viewport.Height / 2 - GameTextures.Crest.Height / 2, GameTextures.Crest.Width, GameTextures.Crest.Height), Color.White);
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

