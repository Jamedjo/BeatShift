﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BeatShift.Input;

namespace BeatShift.Menus
{
    public abstract class IMenuPage
    {

        protected String title="";
        List<MenuItem> menuItems = new List<MenuItem>();
        int currentItem = 0;

        public Vector2 Offset { get; set; }//pixels gap offset between menu items
        public Vector2 MenuPos { get; set; }
        public Vector2 TitleStartPos { get; set; }
        public Vector2 TitlePos { get; set; }
        
        public Boolean UseTextHeightAsOffset { get; set; }//Font may have over-large height, linespaceing is a seperate variable, does not account for scale
        public Boolean UseTextWidthAsOffset { get; set; }
        public float TextScale { get; set; }
        public Boolean DrawTitleFromTextCentre { get; set; }
        public Boolean DrawMenuItemsFromTextCentre { get; set; }

        public float AnimateInDuration { get; set; }
        float animateInElapsedTime = 0;
        float animateInLerp = 0;


        public IMenuPage()
        {
            setDefaultValues();
            setupMenuItems();
        }

        public abstract void setupMenuItems();

        private void setDefaultValues()
        {
            Offset = new Vector2(0, 30);
            MenuPos = new Vector2(160, 150);
            TitleStartPos = new Vector2(-150, 85);
            TitlePos = new Vector2(120, 85);

            UseTextHeightAsOffset = false;
            UseTextWidthAsOffset = false;
            DrawTitleFromTextCentre = false;
            DrawMenuItemsFromTextCentre = false;

            TextScale = 0.5f;

            AnimateInDuration = 160;
            
            overrideMenuPositions();
        }

        public virtual void overrideMenuPositions(){}

        protected void addMenuItem(MenuItem item)
        {
            menuItems.Add(item);
        }
        protected void addMenuItem(string itemname, Action clickedAction)
        {
            addMenuItem(new MenuItem(itemname, clickedAction));
        }
        protected void addMenuItem(string itemname, Action clickedAction, Func<String> valueFunction)
        {
            addMenuItem(new MenuItem(itemname, clickedAction, valueFunction));
        }

        public void Update(GameTime gameTime)
        {
            //Set animateIn time to 0 at start, keep adding to animateInElapsedTime,  when it reaches animateInDuration, lerp = 1
            if (animateInElapsedTime >= AnimateInDuration)
            {
                animateInLerp = 1;
            }
            else
            {
                animateInElapsedTime += gameTime.ElapsedGameTime.Milliseconds;
                animateInLerp = animateInElapsedTime / AnimateInDuration;
            }

            //If values are displayed in menu this will update them using their assigned Func<String>
            for (int i = 0; i < menuItems.Count; i++)
            {
                menuItems[i].update();
            }

            //Update from up/down/ok/back input
            if (isPreviousTrigger())
            {
                if (currentItem == 0) currentItem = menuItems.Count - 1;
                else currentItem--;
                respondToSelectionUp();
            }
            if (isNextTrigger())
            {
                if (currentItem == (menuItems.Count - 1)) currentItem = 0;
                else currentItem++;
                respondToSelectionDown();
            }
            if (MenuManager.anyInput.actionTapped(InputAction.MenuAccept))
            {
                respondToMenuAccept();
            }
            if (MenuManager.anyInput.actionTapped(InputAction.MenuBack) || MenuManager.anyInput.actionTapped(InputAction.BackButton))
            {
                respondToMenuBack();
            }

            otherUpdate();

        }

        public virtual void otherUpdate()
        {
        }

        public virtual Boolean isPreviousTrigger()
        {
            return MenuManager.anyInput.actionTapped(InputAction.MenuUp) || MenuManager.anyInput.actionTapped(InputAction.PadUp);
        }
        public virtual Boolean isNextTrigger()
        {
            return MenuManager.anyInput.actionTapped(InputAction.MenuDown) || MenuManager.anyInput.actionTapped(InputAction.PadDown);
        }

        public void respondToMenuAccept()
        {
            Action a = menuItems[currentItem].clickedAction;
            a();
        }
        public virtual void respondToMenuBack()
        {
            MenuManager.menuBack();
        }
        public virtual void respondToSelectionUp()
        {
        }
        public virtual void respondToSelectionDown()
        {
        }

        protected int getCurrentItem()
        {
            return currentItem;
        }

        public virtual void leavingMenu()
        {
        }
        public virtual void enteringMenu()
        {
            animateInElapsedTime = 0;
        }

        virtual public void DrawSprites(SpriteBatch spriteBatch){
            //Draws within SpriteBatch Begin()/End()Blocks
        }

        public void Draw()
        {
            BeatShift.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            BeatShift.graphics.GraphicsDevice.Viewport = Viewports.fullViewport;
            BeatShift.spriteBatch.Begin();

            DrawSprites(BeatShift.spriteBatch);
            Vector2 titleOrigin=Vector2.Zero;
            if (DrawTitleFromTextCentre) titleOrigin = BeatShift.newfontgreen.MeasureString(title) / 2;
            BeatShift.spriteBatch.DrawString(BeatShift.newfontgreen, title, Vector2.Lerp(TitleStartPos, TitlePos, animateInLerp), Color.White * animateInLerp,0,titleOrigin,1,SpriteEffects.None,1);

            Vector2 textSizeSoFar = Vector2.Zero;
            for(int i=0;i<menuItems.Count;i++)
            {
                Vector2 itemoffset = Offset * i;
                if (UseTextHeightAsOffset) itemoffset.Y += textSizeSoFar.Y;
                if (UseTextWidthAsOffset) itemoffset.X += textSizeSoFar.X;

                String text = menuItems[i].description + menuItems[i].value;
                Vector2 textSize = (BeatShift.newfont.MeasureString(text) * TextScale);
                textSizeSoFar += textSize;

                Vector2 itemOrigin = Vector2.Zero;
                if (DrawMenuItemsFromTextCentre) itemOrigin = BeatShift.newfont.MeasureString(text) / 2;//textSize / 2;

                Color frontColour = Color.White * animateInLerp;
                if (i == currentItem) frontColour = Color.Red;
                BeatShift.spriteBatch.DrawString(BeatShift.newfont, text, MenuPos + itemoffset, frontColour, 0, itemOrigin,TextScale, SpriteEffects.None, 1);
            }
            BeatShift.spriteBatch.End();

            otherDraw();
        }
        public virtual void otherDraw()
        {
        }

    }
}
