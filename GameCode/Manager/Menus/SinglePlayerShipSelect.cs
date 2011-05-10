using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BeatShift.Input;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace BeatShift.Menus
{
    class SinglePlayerShipSelect : IMenuPage
    {

        float shipHue = 1;

        //Stuff for playing videos of ships
        VideoPlayer player = new VideoPlayer();
        Texture2D videoTexture = null;
        // Rectangle for the size of the video
        Rectangle screen = new Rectangle(590, 350, BeatShift.graphics.GraphicsDevice.Viewport.Width / 4, BeatShift.graphics.GraphicsDevice.Viewport.Height / 4);

        bool shipChanged = true;

        public SinglePlayerShipSelect()
        {
            title = "";
        }

        /*public override void overrideMenuPositions()
        {
            //Set title position
            DrawTitleFromTextCentre = true;
            TitlePos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, 75);

            //Set position of menu items
            MenuPos = new Vector2(220, 400);
            UseTextWidthAsOffset = true;
            Offset = new Vector2(20, 0);//additional horizontal spaceing.
        }*/

        public override void setupMenuItems()
        {
            foreach (ShipName type in Utils.GetValues<ShipName>())
            {
                //changed to not write name
                addMenuItem("", (Action)(delegate
                {
                    startSinglePlayerGame();
                    MenuManager.setCurrentMenu(MenuPage.Loading);
                }));
            }
        }

        public override void enteringMenu()
        {
            //base.enteringMenu();
            Race.setupSelectionRacers(1,true);
            Race.Enabled = true;
            Race.Visible = false;
            player = new VideoPlayer();
            player.IsLooped = true;
            shipChanged = true;
            updateShipSelection();
            base.enteringMenu();
        }

        public override void leavingMenu()
        {
            Race.Enabled = false;
            Race.Visible = false;
            player.Stop();
            player.Volume.Equals(0);
            player.Dispose();
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
            SoundManager.PlayShipName(Race.humanRacers[0].shipName);
            if (Options.AddAItoGame)
            {
                Race.setupAIRacers(AiInputManager.numberOfAI);
            }

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

        public override void DrawSprites(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(GameTextures.ShipSelectBasis, new Rectangle(0, 0, BeatShift.graphics.GraphicsDevice.Viewport.Width, BeatShift.graphics.GraphicsDevice.Viewport.Height), Color.White);

            if ((ShipName)getCurrentItem() == ShipName.Skylar)
            {
                //if just changed to this ship stop old and vid play new
                if (shipChanged == true)
                {
                    shipChanged = false;
                    player.Stop();
                    player.Play(GameVideos.skylarVideo);
                }
                videoTexture = player.GetTexture();
                spriteBatch.Draw(videoTexture, screen, Color.White);
                //draw select box and write correct name of ship
                spriteBatch.Draw(GameTextures.ShipSelectBox, new Rectangle(415, 230, 90, 90), Color.White);
                spriteBatch.DrawString(BeatShift.newfont, "Skylar", new Vector2(390, 420), Color.White);
            }
            if ((ShipName)getCurrentItem() == ShipName.Omicron)
            {
                //if just changed to this ship stop old vid and play new
                if (shipChanged == true)
                {
                    shipChanged = false;
                    player.Stop();
                    player.Play(GameVideos.omicronVideo);
                }
                videoTexture = player.GetTexture();
                BeatShift.spriteBatch.Draw(videoTexture, screen, Color.White);
                //draw select box and write correct name of ship
                spriteBatch.Draw(GameTextures.ShipSelectBox, new Rectangle(555, 230, 90, 90), Color.White);
                spriteBatch.DrawString(BeatShift.newfont, "Omicron", new Vector2(375, 420), Color.White);
            }
            if ((ShipName)getCurrentItem() == ShipName.Wraith)
            {
                //if just changed to this ship stop old vid and play new
                if (shipChanged == true)
                {
                    shipChanged = false;
                    player.Stop();
                    player.Play(GameVideos.wraithVideo);
                }
                videoTexture = player.GetTexture();
                BeatShift.spriteBatch.Draw(videoTexture, screen, Color.White);
                //draw select box and write correct name of ship
                spriteBatch.Draw(GameTextures.ShipSelectBox, new Rectangle(685, 230, 90, 90), Color.White);
                spriteBatch.DrawString(BeatShift.newfont, "Wraith", new Vector2(390, 420), Color.White);
            }
            if ((ShipName)getCurrentItem() == ShipName.Flux)
            {
                //if just changed to this ship stop old vid and play new
                if (shipChanged == true)
                {
                    shipChanged = false;
                    player.Stop();
                    player.Play(GameVideos.fluxVideo);
                }
                videoTexture = player.GetTexture();
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
