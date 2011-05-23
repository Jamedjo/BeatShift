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
            //Getting information from the GraphicsDevice might be very slow on Xbox
            int viewportHeight = BeatShift.graphics.GraphicsDevice.Viewport.Height;
            int viewportWidth = BeatShift.graphics.GraphicsDevice.Viewport.Width;

            if (racer.racerType == RacerType.None)
                return;//No HUD when just viewing ship model.
            BeatShift.graphics.GraphicsDevice.Viewport = camera.Viewport;
            //Draw speed text //must be after mesh.draw

            int vOffset = camera.Viewport.Height - 60;

            //MainGame.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;//Set display states

            int bleedHeight = (int)(viewportHeight * 0.1);
            int bleedWidth = (int)(viewportWidth * 0.1);


            if (racer.raceTiming.hasCompletedRace && GameLoop.raceComplete != true)
            {
                ////////////////////////
                ///// FINAL RESULTS ////
                ////////////////////////
                if (Race.currentRaceType.getRaceTypeString().Equals("LappedRace"))
                {
                    //BeatShift.spriteBatch.Begin();//(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                    //DrawMessage("Finished!", 325, vOffset / 2);
                    DrawMessageColour(BeatShift.newfont, "Finished!", viewportWidth / 4, viewportHeight / 2 + 40, 0.6f, Color.PapayaWhip);
                    //DrawMessage("Final Time: " + racer.raceTiming.getFinalTotalTime(), 300, vOffset / 2 + 40);
                }
                if (Race.currentRaceType.getRaceTypeString().Equals("EliminationRace"))
                {
                    BeatShift.spriteBatch.Draw(GameTextures.Eliminated, new Rectangle(viewportWidth / 2 - GameTextures.Eliminated.Width / 2, 3 * viewportHeight / 3, GameTextures.Eliminated.Width, GameTextures.Eliminated.Height), Color.PapayaWhip);
                    //DrawMessage("ELIMINATED!", 325, vOffset / 2);
                }
                if (Race.currentRaceType.getRaceTypeString().Equals("TimeTrialRace"))
                {
                    //BeatShift.spriteBatch.Begin();
                    DrawMessageColour(BeatShift.newfont, "Best Lap: " + racer.raceTiming.getBestLapTime(), viewportWidth / 4, viewportHeight / 2, 0.5f, Color.PapayaWhip);
                    //DrawMessage("Best lap time: " + racer.raceTiming.getBestLapTime(), 325, vOffset / 2);
                }

            }
            else
            {
                //Draw messages to when points are given
                ////////////////////
                //// Points Msg ////
                ////////////////////
                racer.racerPoints.pointsPopupManager.Draw(new Vector2(80f, viewportHeight - 140));
                //DrawMessageColour(BeatShift.newfont, racer.racerPoints.getTotalPoints().ToString(), viewportWidth - 125, 195, 0.3f, Color.Goldenrod);

                //////////////////
                //// Messages ////
                //////////////////
                racer.messagePopupManager.Draw(new Vector2(180f, viewportHeight - 140));
                //DrawMessageColour(BeatShift.newfont, racer.racerPoints.getTotalPoints().ToString(),viewportWidth - 125, 195, 0.4f, Color.Goldenrod);

                /////////////////////
                ///// HUD BAR ///////
                /////////////////////
                int h = viewportWidth / (GameTextures.HudBar.Width / GameTextures.HudBar.Height);

                if (viewportWidth > 700)
                {
                    BeatShift.spriteBatch.Draw(GameTextures.HudBar, new Rectangle(0, viewportHeight - GameTextures.HudBar.Height, GameTextures.HudBar.Width, GameTextures.HudBar.Height), Color.White);
                }
                else
                {
                    BeatShift.spriteBatch.Draw(GameTextures.HudBarSmall, new Rectangle(0, viewportHeight - GameTextures.HudBarSmall.Height, GameTextures.HudBarSmall.Width, GameTextures.HudBarSmall.Height), Color.White);
                }
                ////////////////////////////
                ///// ORANGE BOOST BAR /////
                ////////////////////////////

                racer.raceTiming.previousBoost = MathHelper.Lerp(racer.raceTiming.previousBoost, (float)racer.racingControls.getBoostValue() / 100, 0.1f);
                racer.raceTiming.previousLapProgress = MathHelper.Lerp(racer.raceTiming.previousLapProgress, (float)racer.shipPhysics.getLapPercentage() / 100, 0.05f);

                var chosenLine = (viewportWidth > 700) ? GameTextures.BoostBarLine : GameTextures.BoostBarLineSmall;
                int srcWidth = (int)((chosenLine.Width) * racer.raceTiming.previousBoost);

                if (viewportWidth > 700)
                {
                    Rectangle orange_src2 = new Rectangle(chosenLine.Width - srcWidth, 0, chosenLine.Width, chosenLine.Height);
                    Rectangle orange_dest = new Rectangle(36, viewportHeight - chosenLine.Height, chosenLine.Width, chosenLine.Height);
                    BeatShift.spriteBatch.Draw(chosenLine, orange_dest, orange_src2, Color.White);
                }
                else
                {
                    Rectangle orange_src2 = new Rectangle(chosenLine.Width - srcWidth, 0, chosenLine.Width, chosenLine.Height);
                    Rectangle orange_dest = new Rectangle(21, viewportHeight - chosenLine.Height, chosenLine.Width, chosenLine.Height);
                    BeatShift.spriteBatch.Draw(chosenLine, orange_dest, orange_src2, Color.White);
                }

                ////////////////////////
                /////// LEVEL //////////
                ////////////////////////

                if (viewportWidth > 700)
                {
                    DrawMessageColour(BeatShift.volterfont, "LEVEL", viewportWidth - 240, vOffset - 53, 0.5f, Color.PapayaWhip);
                    DrawMessageColour(BeatShift.volterfont, (racer.beatQueue.getLayer() + 1).ToString(), viewportWidth - 230, vOffset - 35, 1f, Color.PapayaWhip);
                }
                else
                {
                    DrawMessageColour(BeatShift.volterfont, "LEVEL", viewportWidth - 143, vOffset - 12, 0.4f, Color.PapayaWhip);
                    DrawMessageColour(BeatShift.volterfont, (racer.beatQueue.getLayer() + 1).ToString(), viewportWidth - 136, vOffset - 3, 0.8f, Color.PapayaWhip);
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
                if (viewportWidth > 700)
                {
                    DrawMessageColour(BeatShift.volterfont, "MPH", viewportWidth - 126, vOffset - 42, 0.5f, Color.Black);
                    DrawMessageColour(BeatShift.newfont, racer.raceTiming.speedToDisplay, viewportWidth - 185, vOffset - 48, 0.4f, Color.Black);
                }
                else
                {
                    DrawMessageColour(BeatShift.volterfont, "MPH", viewportWidth - 75, vOffset - 14, 0.5f, Color.Black);
                    DrawMessageColour(BeatShift.newfont, racer.raceTiming.speedToDisplay, viewportWidth - 110, vOffset - 8, 0.4f, Color.Black);
                }

                //////////////////////////////
                /////// TOP RIGHT BOARD //////
                //////////////////////////////

                double scaleFactorHeight = (double)viewportHeight / 720;
                double scaleFactorWidth = scaleFactorHeight;// (double)viewportWidth / 1280;

                int newBoardWidth = (int)(GameTextures.TopRightBoard.Width * scaleFactorWidth);
                int newBoardHeight = (int)(GameTextures.TopRightBoard.Height * scaleFactorHeight);

                if (Race.currentRaceType.getRaceTypeString().Equals("EliminationRace"))
                {
                    {
                        Rectangle d = new Rectangle(viewportWidth - GameTextures.EliminationBar.Width, 0, GameTextures.EliminationBar.Width, GameTextures.EliminationBar.Height);
                        BeatShift.spriteBatch.Draw(GameTextures.EliminationBar, d, Color.White);

                        if ( racer.raceTiming.currentRanking == Race.currentRacers.Count)
                            DrawMessageColour(BeatShift.newfont, "DANGER!", viewportWidth - 190, 55, 0.75f, Color.White);
                        else
                            DrawMessageColour(BeatShift.newfont, "SAFE", viewportWidth - 180, 55, 0.75f, Color.White);

                        //if (racer.raceTiming.currentRanking == 1)
                        //{
                        //    DrawMessageColour(BeatShift.newfont, racer.raceTiming.currentRanking.ToString(), viewportWidth - 160, 55, 0.75f, Color.PapayaWhip);
                        //    DrawMessageColour(BeatShift.newfont, calculateRankSuffix(racer.raceTiming.currentRanking), viewportWidth - 140, 55, 0.75f, Color.PapayaWhip);
                        //}
                        //else
                        //{
                        //    DrawMessageColour(BeatShift.newfont, racer.raceTiming.currentRanking.ToString(), viewportWidth - 160, 55, 0.75f, Color.PapayaWhip);
                        //    DrawMessageColour(BeatShift.newfont, calculateRankSuffix(racer.raceTiming.currentRanking), viewportWidth - 135, 55, 0.75f, Color.PapayaWhip);
                        //}
                    }
                }
                else if (Race.currentRaceType.getRaceTypeString().Equals("PointsRace"))
                {
                    if (viewportWidth > 700)
                    {
                        Rectangle d = new Rectangle(viewportWidth - GameTextures.PointsHUD.Width, 0, GameTextures.PointsHUD.Width, GameTextures.PointsHUD.Height);
                        BeatShift.spriteBatch.Draw(GameTextures.PointsHUD, d, Color.White);
                    }
                    else
                    {
                        Rectangle d = new Rectangle(viewportWidth - GameTextures.PointsHUD.Width, 0, GameTextures.PointsHUD.Width, GameTextures.PointsHUD.Height);
                        BeatShift.spriteBatch.Draw(GameTextures.PointsHUD, d, Color.White);
                    }
                }
                else
                {
                    if (viewportWidth > 700)
                    {
                        Rectangle d = new Rectangle(viewportWidth - GameTextures.TopRightBoard.Width, 0, GameTextures.TopRightBoard.Width, GameTextures.TopRightBoard.Height);
                        BeatShift.spriteBatch.Draw(GameTextures.TopRightBoard, d, Color.White);
                    }
                    else
                    {
                        Rectangle d = new Rectangle(viewportWidth - GameTextures.TopRightBoardSmall.Width, 0, GameTextures.TopRightBoardSmall.Width, GameTextures.TopRightBoardSmall.Height);
                        BeatShift.spriteBatch.Draw(GameTextures.TopRightBoardSmall, d, Color.White);
                    }
                }


                //////////////////////////////
                //////// TOTAL POINTS ////////
                //////////////////////////////

                if (Race.currentRaceType.displayTotalPoints)
                {
                    DrawMessageColour(BeatShift.newfont, racer.racerPoints.getTotalPoints().ToString(), viewportWidth - GameTextures.PointsHUD.Width/2 -65, 65, 1f, Color.PapayaWhip);
                }

                //////////////////////////////
                ////////// TOTAL LAPS ////////
                //////////////////////////////

                if (Race.currentRaceType.displayCurrentLapOutofTotalLaps)
                {
                    if (viewportWidth > 700)
                    {
                        if (racer.raceTiming.isRacing == true)
                            DrawMessageColour(BeatShift.newfont, (racer.raceTiming.currentLap + 1) + "/" + Race.currentRaceType.maxLaps, viewportWidth - 125, 33, 0.5f, Color.PapayaWhip);
                        else
                            DrawMessageColour(BeatShift.newfont, (racer.raceTiming.currentLap) + "/" + Race.currentRaceType.maxLaps, viewportWidth - 125, 33, 0.5f, Color.PapayaWhip);
                        DrawMessageColour(BeatShift.volterfont, "LAPS", viewportWidth - 74, 39, 1f, Color.DimGray);
                    }
                    else
                    {
                        if (racer.raceTiming.isRacing == true)
                            DrawMessageColour(BeatShift.volterfont, (racer.raceTiming.currentLap + 1) + "/" + Race.currentRaceType.maxLaps, viewportWidth - 82, 25, 0.5f, Color.PapayaWhip);
                        else
                            DrawMessageColour(BeatShift.volterfont, (racer.raceTiming.currentLap) + "/" + Race.currentRaceType.maxLaps, viewportWidth - 82, 25, 0.5f, Color.PapayaWhip);
                        DrawMessageColour(BeatShift.volterfont, "LAPS", viewportWidth - 54, 25, 0.5f, Color.PapayaWhip);
                    }
                }

                //////////////////////////////
                ///////// CURRENT LAP ////////
                //////////////////////////////

                if (Race.currentRaceType.displayCurrentLapTime)
                {
                    if (viewportWidth > 700)
                    {
                        DrawMessageColour(BeatShift.newfont, racer.raceTiming.getCurrentLapTime(), viewportWidth - 125, 70, 0.4f, Color.PapayaWhip);
                    }
                    else
                    {
                        DrawMessageColour(BeatShift.newfont, racer.raceTiming.getCurrentLapTime(), viewportWidth - 125, 36, 0.4f, Color.PapayaWhip);
                    }
                }
                /////////////////////////////
                ////// WRONG WAY SIGN ///////
                /////////////////////////////

                if (racer.shipPhysics.wrongWay == true)
                {
                    int newWarningWidth = (int)(GameTextures.WrongWaySign.Width * scaleFactorWidth);
                    int newWarningHeight = (int)(GameTextures.WrongWaySign.Height * scaleFactorHeight);
                    BeatShift.spriteBatch.Draw(GameTextures.WrongWaySign, new Rectangle(viewportWidth / 2 - newWarningWidth / 2, viewportHeight / 2 - newWarningHeight / 2, newWarningWidth, newWarningHeight), Color.White);
                }

                /////////////////////////////
                ////// RESETTING SIGN ///////
                /////////////////////////////

                if (racer.isRespawning && racer.shipPhysics.millisecsLeftTillReset < 2000)
                {
                    int newWarningWidth = (int)(GameTextures.ResettingSign.Width * scaleFactorWidth);
                    int newWarningHeight = (int)(GameTextures.ResettingSign.Height * scaleFactorHeight);
                    BeatShift.spriteBatch.Draw(GameTextures.ResettingSign, new Rectangle(viewportWidth / 2 - newWarningWidth / 2, viewportHeight / 2 - newWarningHeight / 2, newWarningWidth, newWarningHeight), Color.White);
                }

                /////////////////////////////
                ////// COUNTDOWN SIGN ///////
                /////////////////////////////

                if (Race.currentRaceType.countDownRunning)
                    BeatShift.spriteBatch.Draw(Race.currentRaceType.countdownState, new Vector2(viewportWidth / 2 - Race.currentRaceType.countdownState.Width / 2, viewportHeight / 2 - Race.currentRaceType.countdownState.Height / 2), Color.White);

                // Other info
                //DrawMessage("Progress: " + racer.getLapPercentage() + "%", 10, vOffset + 26);
                //DrawMessage("Accuracy: " + racer.racingControls.getLastPress(), 10, vOffset - 30);
                //DrawMessage("Dist from trac: " + racer.shipPhysics.shipRayToTrackTime, 10, vOffset - 30);
                //DrawNewMessage(" !\"#$%'()*+,-./0123456789:;<=>?@ABCDEFGHIJ", 0, 30);
                //DrawNewMessage("KLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrrstuvwxyz{|}~", 0, 90);
            

                //////////////////////////////
                ////////// BEST LAP //////////
                //////////////////////////////

                if (Race.currentRaceType.displayCurrentBestLap)
                {
                    if (viewportWidth > 700)
                    {
                        DrawMessageColour(BeatShift.newfont, racer.raceTiming.getBestLapTime(), viewportWidth - 125, 95, 0.4f, Color.Goldenrod);
                    }

                    //////////////////////////
                    ////////// RANK //////////
                    //////////////////////////

                    if (Race.currentRaceType.displayCurrentRank && racer.raceTiming.currentRanking == 0)
                    {
                        if (viewportWidth > 700)
                        {
                            DrawMessageColour(BeatShift.newfont, "-", viewportWidth - 190, 22, 0.75f, Color.PapayaWhip);
                        }
                        else
                        {
                            DrawMessageColour(BeatShift.newfont, "-", viewportWidth - 187, 22, 0.75f, Color.PapayaWhip);
                        }
                    }
                    else if (Race.currentRaceType.displayCurrentRank)
                    {
                        if (viewportWidth > 700)
                        {
                            DrawMessageColour(BeatShift.newfont, racer.raceTiming.currentRanking.ToString(), viewportWidth - 190, 22, 0.75f, Color.PapayaWhip);
                        }
                        else
                        {
                            DrawMessageColour(BeatShift.newfont, racer.raceTiming.currentRanking.ToString(), viewportWidth - 187, 22, 0.75f, Color.PapayaWhip);
                        }
                        //DrawMessage(BeatShift.newfont, calculateRankSuffix(racer.raceTiming.currentRanking), viewportWidth - 178 + extraSuffixOffset(racer.raceTiming.currentRanking), 26, 0.25f);
                    }
                }

            }
        }

        public static void DrawMessage(string message, int x, int y)
        {
            //spriteBatch.DrawString("Speed: " + shipSpeed, new Vector2(10, 10), Color.OrangeRed, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
            BeatShift.spriteBatch.DrawString(BeatShift.newfont, message, new Vector2(x + 1, y + 1), Color.Black);//,0,new Vector2(0,0),0.5f,null,0f);
            BeatShift.spriteBatch.DrawString(BeatShift.newfont, message, new Vector2(x, y), Color.White);
        }
        public static void DrawMessage(SpriteFont font, string message, int x, int y, float scale)
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
            //Getting information from the GraphicsDevice might be very slow on Xbox
            int viewportHeight = BeatShift.graphics.GraphicsDevice.Viewport.Height;
            int viewportWidth = BeatShift.graphics.GraphicsDevice.Viewport.Width;
            BeatShift.spriteBatch.Draw(GameTextures.HorizontalSplit, new Rectangle(viewportWidth, viewportHeight / 2 - GameTextures.HorizontalSplit.Height / 2, viewportWidth / 2, GameTextures.HorizontalSplit.Height), Color.White);
            BeatShift.spriteBatch.Draw(GameTextures.VerticalSplit, new Rectangle(viewportWidth / 2 - GameTextures.VerticalSplit.Width / 2, 0, GameTextures.VerticalSplit.Width, viewportHeight), Color.White);
        }

        public static void DrawSplitBarsFourPlayer()
        {
            //BeatShift.spriteBatch.Begin();
            //Getting information from the GraphicsDevice might be very slow on Xbox
            int viewportHeight = BeatShift.graphics.GraphicsDevice.Viewport.Height;
            int viewportWidth = BeatShift.graphics.GraphicsDevice.Viewport.Width;
            BeatShift.spriteBatch.Draw(GameTextures.HorizontalSplit, new Rectangle(0, viewportHeight / 2 - GameTextures.HorizontalSplit.Height / 2, viewportWidth, GameTextures.HorizontalSplit.Height), Color.White);
            BeatShift.spriteBatch.Draw(GameTextures.VerticalSplit, new Rectangle(viewportWidth / 2 - GameTextures.VerticalSplit.Width / 2, 0, GameTextures.VerticalSplit.Width, viewportHeight), Color.White);
            BeatShift.spriteBatch.Draw(GameTextures.Crest, new Rectangle(viewportWidth / 2 - GameTextures.Crest.Width / 2, viewportHeight / 2 - GameTextures.Crest.Height / 2, GameTextures.Crest.Width, GameTextures.Crest.Height), Color.White);
        }

        //public static void DrawSplitBars()
        //{
        //    BeatShift.graphics.GraphicsDevice.Viewport = Viewports.fullViewport;
        //    BeatShift.spriteBatch.Begin();
        //    //Getting information from the GraphicsDevice might be very slow on Xbox
        //    int viewportHeight = viewportHeight;
        //    int viewportWidth = BeatShift.graphics.GraphicsDevice.Viewport.Width;
        //    BeatShift.spriteBatch.Draw(GameTextures.HorizontalSplit, new Rectangle(0, viewportHeight / 2 - GameTextures.HorizontalSplit.Height / 2, viewportWidth, GameTextures.HorizontalSplit.Height), Color.White);
        //    BeatShift.spriteBatch.Draw(GameTextures.VerticalSplit, new Rectangle(viewportWidth / 2 - GameTextures.VerticalSplit.Width / 2, 0, GameTextures.VerticalSplit.Width, viewportHeight), Color.White);
        //    BeatShift.spriteBatch.Draw(GameTextures.Crest, new Rectangle(viewportWidth / 2 - GameTextures.Crest.Width / 2, viewportHeight / 2 - GameTextures.Crest.Height / 2, GameTextures.Crest.Width, GameTextures.Crest.Height), Color.White);
        //    BeatShift.spriteBatch.End();
        //}

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

    public class SlidingPopup //These draw from the bottom left
    {
        List<SlideInstance> popups = new List<SlideInstance>();
        List<SlideInstance> temp = new List<SlideInstance>();
        Vector2 motionVector;
        float buttonScale;

        public SlidingPopup(Vector2 MotionVector)
        {
            motionVector = MotionVector;
            buttonScale = 0f;//ButtonScale;
        }

        public void addPopup(Texture2D popup, int duration)//Must becarefull with add/update order so updates are not one frame out?
        {
            popups.Add(new SlideInstance(popup, duration));
        }
        public void addPopup(Texture2D popup,String Message, int duration)//Must becarefull with add/update order so updates are not one frame out?
        {
            popups.Add(new SlideInstance(popup,Message, duration));
        }
        //public void addBeat(ButtonImage button, GameTime currentTime, int expectedHitTime)
        //{
        //    beats.Add(new BeatInstance(button, currentTime.TotalGameTime.Milliseconds, expectedHitTime));
        //}

        //Remove from list when finished, fade just before.

        public void Update(GameTime gameTime)
        {
            foreach (SlideInstance beat in popups)
            {
                beat.elapsedDuration += gameTime.ElapsedGameTime.Milliseconds;
            }
            //beats.RemoveAll(beat => (beat.elapsedDuration >= beat.duration));//wont work on xbox
            temp.Clear();
            for (int i = 0; i < popups.Count; i++)
            {
                if (popups[i].elapsedDuration < popups[i].duration) temp.Add(popups[i]);
            }
            popups.Clear();
            for (int j = 0; j < temp.Count; j++)
            {
                popups.Add(temp[j]);
            }
        }

        public void Draw(Vector2 position)
        {
            BlendState b = BeatShift.graphics.GraphicsDevice.BlendState;
            BeatShift.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            foreach (SlideInstance instance in popups)
            {
                float lerpval = (float)instance.elapsedDuration / instance.duration;
                Vector2 pos = Vector2.Lerp(position, position+motionVector, lerpval);
                BeatShift.spriteBatch.Draw(instance.popup, pos, Color.White);// buttonScale);
                if (instance.hasMessage)
                    BeatShift.spriteBatch.DrawString(BeatShift.newfont, instance.message+" pts", pos+new Vector2(16, 4), Color.Black, 0f, Vector2.Zero, 0.3f, SpriteEffects.None, 1);
            }
            BeatShift.graphics.GraphicsDevice.BlendState = b;

        }

        private class SlideInstance
        {
            public Texture2D popup;
            //float startTime;
            //float expectedHitTime;
            public int duration;
            public int elapsedDuration;

            public bool hasMessage;
            public String message;

            public SlideInstance(Texture2D Popup, int timeDuration)
            {
                popup = Popup;
                duration = timeDuration;
                elapsedDuration = 0;
                hasMessage = false;
            }


            public SlideInstance(Texture2D Popup,String Message, int timeDuration)
            {
                popup = Popup;
                duration = timeDuration;
                elapsedDuration = 0;
                hasMessage = true;
                message = Message;
            }
        }
    }

   
}

