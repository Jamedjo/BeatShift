using System;
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
        ////Video video ;
        //VideoPlayer player = new VideoPlayer() ;
        ////texture for current frame in video
        //Texture2D videoTexture = null;
        public static int counter = 0;

        public SplashMenu()
        {
            ////title = "";
            ////video = BeatShift.contentManager.Load<Video>("Videos/splashVideo");
            //player = new VideoPlayer();
            //player.IsLooped = true;
            ////player.Play(video);
        }

        public override void respondToMenuBack()
        {

        }

        public override void setupMenuItems()
        {
            addMenuItem("Press Start to Play", (Action)(delegate
            {
                MenuManager.mainMenuSystem.setCurrentMenu(MenuPage.Main);
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

        //    if (player.IsDisposed != true)
        //    {
        //        if (player.State == MediaState.Stopped)
        //        {
        //     `       player.IsLooped = true;
        //            player.Play(GameVideos.splashVideo);
        //        }
        //    }

        }

        public override void otherDraw()
        {
            //if (player.State != MediaState.Stopped)
            //    videoTexture = player.GetTexture();

            //// Drawing to the rectangle will stretch the 
            //// video to fill the screen
            Rectangle screen = new Rectangle(BeatShift.graphics.GraphicsDevice.Viewport.X,
                BeatShift.graphics.GraphicsDevice.Viewport.Y,
                BeatShift.graphics.GraphicsDevice.Viewport.Width,
                BeatShift.graphics.GraphicsDevice.Viewport.Height);

            //if (videoTexture != null)
            //{
            //    BeatShift.spriteBatch.Begin();
            //    BeatShift.spriteBatch.Draw(videoTexture, screen, Color.White);
            //}
            counter++;

            BeatShift.spriteBatch.Begin();
            //BeatShift.spriteBatch.DrawString(BeatShift.newfont, "Please press start", new Vector2(350, 300), Color.White);
            BeatShift.spriteBatch.Draw(GameTextures.MenuBackgroundBlue, screen, Color.White);

            if (counter < 300)
                BeatShift.spriteBatch.Draw(GameTextures.CityRender, screen, Color.White);
            else if (counter < 600)
                BeatShift.spriteBatch.Draw(GameTextures.DesertRender, screen, Color.White);
            else if (counter < 900)
                BeatShift.spriteBatch.Draw(GameTextures.SpaceRender, screen, Color.White);
            else
                counter = 0;


            BeatShift.spriteBatch.Draw(GameTextures.Start, new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - GameTextures.Start.Width / 2, 9* (BeatShift.graphics.GraphicsDevice.Viewport.Height / 10)), Color.White);
            BeatShift.spriteBatch.Draw(GameTextures.Logo, new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2 - GameTextures.Logo.Width / 2, -10), Color.White);
            BeatShift.spriteBatch.End();

        }

        public override void leavingMenu()
        {
            //player.Stop();
            //player.Volume.Equals(0);
            //player.Dispose();
            base.leavingMenu();
        }



    }
}