using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BeatShift.Input;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System.Threading;

namespace BeatShift.Menus
{
    class SinglePlayerShipSelect : IMenuPage
    {

        //Stuff for playing videos of ships
        VideoPlayer playerSkylar = new VideoPlayer();
        VideoPlayer playerOmicron = new VideoPlayer();
        VideoPlayer playerWraith = new VideoPlayer();
        VideoPlayer playerFlux = new VideoPlayer();

        Texture2D videoTexture = null;
        // Rectangle for the size of the video
        Rectangle screen = new Rectangle(590, 350, BeatShift.graphics.GraphicsDevice.Viewport.Width / 4, BeatShift.graphics.GraphicsDevice.Viewport.Height / 4);

        bool shipChanged = true;

        public SinglePlayerShipSelect()
        {
            title = "";
        }

        public override void setupMenuItems()
        {
            foreach (ShipName type in Utils.GetValues<ShipName>())
            {
                //changed to not write name
                addMenuItem("", (Action)(delegate
                {
                    startSinglePlayerGame();
                    MenuManager.mainMenuSystem.setCurrentMenu(MenuPage.Loading);
                }));
            }
        }

        public override void enteringMenu()
        {
            //base.enteringMenu();
            Race.setupSelectionRacers(1,true);
            Race.Enabled = true;
            Race.Visible = false;
            playerSkylar = new VideoPlayer();
            playerSkylar.IsLooped = true;
            playerOmicron = new VideoPlayer();
            playerOmicron.IsLooped = true;
            playerWraith = new VideoPlayer();
            playerWraith.IsLooped = true;
            playerFlux = new VideoPlayer();
            playerFlux.IsLooped = true;
            shipChanged = true;

            //playVideosThread = new Thread(playAllVideos);
            //playVideosThread.Start();
            playAllVideos();

            updateShipSelection();
            base.enteringMenu();
        }

        public void playAllVideos()
        {
            playerSkylar.IsMuted = true;
            playerOmicron.IsMuted = true;
            playerWraith.IsMuted = true;
            playerFlux.IsMuted = true;
            playerSkylar.Play(GameVideos.skylarVideo);
            playerOmicron.Play(GameVideos.omicronVideo);
            playerWraith.Play(GameVideos.wraithVideo);
            playerFlux.Play(GameVideos.fluxVideo);
        }

        public void disposeVideos()
        {
            playerSkylar.Stop();
            //playerSkylar.Volume.Equals(0);
            playerSkylar.Dispose();
            playerOmicron.Stop();
            //playerOmicron.Volume.Equals(0);
            playerOmicron.Dispose();
            playerWraith.Stop();
            //playerWraith.Volume.Equals(0);
            playerWraith.Dispose();
            playerFlux.Stop();
            //playerFlux.Volume.Equals(0);
            playerFlux.Dispose();
        }

        public override void leavingMenu()
        {
            Race.Enabled = false;
            Race.Visible = false;
            Thread unloadVideos = new Thread(disposeVideos);
            unloadVideos.Start();
            base.leavingMenu();
        }

        public override void respondToSelectionUp()
        {
            shipChanged = true;
            base.respondToSelectionUp();//Change highlighted item
            updateShipSelection();
        }
        public override void respondToSelectionDown()
        {
            shipChanged = true;
            base.respondToSelectionDown();//Change highlighted item
            updateShipSelection();
        }

        void updateShipSelection()
        {
            Race.humanRacers[0].shipName=(ShipName)getCurrentItem();
        }

        public override bool isPreviousTrigger()
        {
            return MenuManager.anyInput.actionTapped(InputAction.MenuLeft) || MenuManager.anyInput.actionTapped(InputAction.PadLeft);
        }
        public override bool isNextTrigger()
        {
            return MenuManager.anyInput.actionTapped(InputAction.MenuRight) || MenuManager.anyInput.actionTapped(InputAction.PadRight);
        }

        /*public override void otherUpdate()
        {
            if (MenuManager.anyInput.actionPressed(InputAction.MenuUp) || MenuManager.anyInput.actionTapped(InputAction.PadUp))
            {
                //decreaseShipHue(2.5f);
                //Race.humanRacers[0].setColour((int)shipHue);
            }
            if (MenuManager.anyInput.actionPressed(InputAction.MenuDown) || MenuManager.anyInput.actionTapped(InputAction.PadDown))
            {
                //increaseShipHue(2.5f);
                //Race.humanRacers[0].setColour((int)shipHue);
            }
        }*/

        private void startSinglePlayerGame()
        {
            Race.humanRacers[0].setupRacingControls(((AnyInputManager)MenuManager.anyInput).getLastInputTapped());
            SoundManager.PlayShipName(Race.humanRacers[0].shipName);
            GameLoop.setActiveControllers(true, 0);
            //BeatShift.bgm.play();
        }

        /*void increaseShipHue(float byValue)
        {
            shipHue += byValue;
            shipHue %= 360;
        }
        void decreaseShipHue(float byValue)
        {
            shipHue -= byValue;
            if (shipHue < 0)
            {
                shipHue %= 360;//modulo takes sign of shipHue so still negative
                shipHue = 360 + shipHue;
            }
        }*/

        public void muteVideos()
        {
            playerSkylar.Volume.Equals(0);
            playerOmicron.Volume.Equals(0);
            playerWraith.Volume.Equals(0);
            playerFlux.Volume.Equals(0);
        }

        public override void DrawSprites(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(GameTextures.ShipSelectBasis, new Rectangle(0, 0, BeatShift.graphics.GraphicsDevice.Viewport.Width, BeatShift.graphics.GraphicsDevice.Viewport.Height), Color.White);

            if ((ShipName)getCurrentItem() == ShipName.Skylar)
            {
                if (shipChanged == true)
                    shipChanged = false;

                videoTexture = playerSkylar.GetTexture();
                spriteBatch.Draw(videoTexture, screen, Color.White);
                //draw select box and write correct name of ship
                spriteBatch.Draw(GameTextures.ShipSelectBox, new Rectangle(415, 230, 90, 90), Color.White);
                spriteBatch.DrawString(BeatShift.newfont, "Skylar", new Vector2(390, 420), Color.White);
            }
            if ((ShipName)getCurrentItem() == ShipName.Omicron)
            {
                if (shipChanged == true)
                    shipChanged = false;
                videoTexture = playerOmicron.GetTexture();
                BeatShift.spriteBatch.Draw(videoTexture, screen, Color.White);
                //draw select box and write correct name of ship
                spriteBatch.Draw(GameTextures.ShipSelectBox, new Rectangle(555, 230, 90, 90), Color.White);
                spriteBatch.DrawString(BeatShift.newfont, "Omicron", new Vector2(375, 420), Color.White);
            }
            if ((ShipName)getCurrentItem() == ShipName.Wraith)
            {
                if (shipChanged == true)
                    shipChanged = false;
                videoTexture = playerWraith.GetTexture();
                BeatShift.spriteBatch.Draw(videoTexture, screen, Color.White);
                //draw select box and write correct name of ship
                spriteBatch.Draw(GameTextures.ShipSelectBox, new Rectangle(685, 230, 90, 90), Color.White);
                spriteBatch.DrawString(BeatShift.newfont, "Wraith", new Vector2(390, 420), Color.White);
            }
            if ((ShipName)getCurrentItem() == ShipName.Flux)
            {
                if (shipChanged == true)
                    shipChanged = false;
                videoTexture = playerFlux.GetTexture();
                BeatShift.spriteBatch.Draw(videoTexture, screen, Color.White);
                //draw select box and write correct name of ship
                spriteBatch.Draw(GameTextures.ShipSelectBox, new Rectangle(825, 230, 90, 90), Color.White);
                spriteBatch.DrawString(BeatShift.newfont, "Flux", new Vector2(410, 420), Color.White);
            }

            spriteBatch.Draw(GameTextures.Skylar, new Rectangle(420, BeatShift.graphics.GraphicsDevice.Viewport.Height / 3 + 5, BeatShift.graphics.GraphicsDevice.Viewport.Width/16, BeatShift.graphics.GraphicsDevice.Viewport.Height/12), Color.White);
            spriteBatch.Draw(GameTextures.Omicron, new Rectangle(560, BeatShift.graphics.GraphicsDevice.Viewport.Height / 3 + 5, BeatShift.graphics.GraphicsDevice.Viewport.Width / 16, BeatShift.graphics.GraphicsDevice.Viewport.Height / 12), Color.White);
            spriteBatch.Draw(GameTextures.Wraith, new Rectangle(690, BeatShift.graphics.GraphicsDevice.Viewport.Height / 3 + 5, BeatShift.graphics.GraphicsDevice.Viewport.Width / 16, BeatShift.graphics.GraphicsDevice.Viewport.Height / 12), Color.White);
            spriteBatch.Draw(GameTextures.Flux, new Rectangle(830, BeatShift.graphics.GraphicsDevice.Viewport.Height / 3 + 5, BeatShift.graphics.GraphicsDevice.Viewport.Width / 16, BeatShift.graphics.GraphicsDevice.Viewport.Height / 12), Color.White);
        
        }
    }
}
