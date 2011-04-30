using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BeatShift.Input;

namespace BeatShift.Menus
{
    class SinglePlayerShipSelect : IMenuPage
    {

        float shipHue = 1;

        public SinglePlayerShipSelect()
        {
            title = "Select Ship";
            
        }

        public override void overrideMenuPositions()
        {
            //Set title position
            DrawTitleFromTextCentre = true;
            TitlePos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, 75);

            //Set position of menu items
            MenuPos = new Vector2(220, 400);
            UseTextWidthAsOffset = true;
            Offset = new Vector2(20, 0);//additional horizontal spaceing.
        }

        public override void setupMenuItems()
        {
            foreach(ShipName type in Utils.GetValues<ShipName>())
            {
                addMenuItem(type.ToString(), new Action(startSinglePlayerGame));
            }
        }

        public override void enteringMenu()
        {
            base.enteringMenu();
            Race.setupSelectionRacers(1,true);
            Race.Enabled = true;
            Race.Visible = true;
            updateShipSelection();
            base.enteringMenu();
        }

        public override void leavingMenu()
        {
            Race.Enabled = false;
            Race.Visible = false;
            base.leavingMenu();
        }

        public override void respondToSelectionUp()
        {
            base.respondToSelectionUp();//Change highlighted item
            updateShipSelection();
        }
        public override void respondToSelectionDown()
        {
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

        public override void otherUpdate()
        {
            if (MenuManager.anyInput.actionPressed(InputAction.MenuUp) || MenuManager.anyInput.actionTapped(InputAction.PadUp))
            {
                decreaseShipHue(2.5f);
                Race.humanRacers[0].setColour((int)shipHue);
            }
            if (MenuManager.anyInput.actionPressed(InputAction.MenuDown) || MenuManager.anyInput.actionTapped(InputAction.PadDown))
            {
                increaseShipHue(2.5f);
                Race.humanRacers[0].setColour((int)shipHue);
            }
        }

        private void startSinglePlayerGame()
        {
            SoundManager.PlayShipName(Race.humanRacers[0].shipName);
            if (Options.AddAItoGame)
            {
                Race.setupAIRacers(AiInputManager.numberOfAI);
            }

            //while waiting for physics multithread to load do nothing
            while (MapManager.currentMap.physicsLoadingThread.IsAlive) {}// Console.WriteLine("physics thread alive!!");}

            GameLoop.setActiveControllers(true, 0);

            GameLoop.setGameState(GameState.LocalGame);
            if ((Keyboard.GetState().IsKeyDown(Keys.Enter)) && !AiInputManager.testAI)
            {
                Race.humanRacers[0].racingControls.chosenInput = new KeyInputManager();
                //TODO: Make it so the input type corresponds to the input pressed
                //Could be done by checking each possible input manually, bypassing inputManger if necessary
            }
            
            //BeatShift.bgm.play();
        }

        void increaseShipHue(float byValue)
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
        }
    }
}
