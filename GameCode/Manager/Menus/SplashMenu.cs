﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using BeatShift.Input;
namespace BeatShift.Menus
{
    class SplashMenu : IMenuPage
    {
        //Video video ;
        VideoPlayer player = new VideoPlayer() ;
        //texture for current frame in video
        Texture2D videoTexture = null;

        public SplashMenu()
        {
            //title = "";
            //video = BeatShift.contentManager.Load<Video>("Videos/splashVideo");
            player = new VideoPlayer();
            player.IsLooped = true;
            //player.Play(video);
        }

        public override void respondToMenuBack()
        {

        }

        public override void setupMenuItems()
        {
            addMenuItem("Press Start to Play", (Action)(delegate
            {
                MenuManager.setCurrentMenu(MenuPage.Main);
            }));
         }

        public override void overrideMenuPositions()
        {
            Offset = new Vector2(0, 50);
            MenuPos = new Vector2(250, 150);
            TextScale = 0.5f;
        }

        public override void otherUpdate()
        {
            if (MenuManager.anyInput.actionTapped(InputAction.Start))
            {
                respondToMenuAccept();
            }

            if (player.IsDisposed != true)
            {
                if (player.State == MediaState.Stopped)
                {
                    player.IsLooped = true;
                    player.Play(GameVideos.splashVideo);
                }
            }

        }

        public override void otherDraw()
        {
            if (player.State != MediaState.Stopped)
                videoTexture = player.GetTexture();

            // Drawing to the rectangle will stretch the 
            // video to fill the screen
            Rectangle screen = new Rectangle(BeatShift.graphics.GraphicsDevice.Viewport.X,
                BeatShift.graphics.GraphicsDevice.Viewport.Y,
                BeatShift.graphics.GraphicsDevice.Viewport.Width,
                BeatShift.graphics.GraphicsDevice.Viewport.Height);

            if (videoTexture != null)
            {
                BeatShift.spriteBatch.Begin();
                BeatShift.spriteBatch.Draw(videoTexture, screen, Color.White);
                BeatShift.spriteBatch.End();
            }

        }

        public override void leavingMenu()
        {
            player.Stop();
            player.Volume.Equals(0);
            player.Dispose();
            base.leavingMenu();
        }



    }
}