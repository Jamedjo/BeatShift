using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BeatShift
{
    public enum ButtonImage {LeftThumbStick,DPad,RightThumbStick,Back,Guide,Start,X,A,Y,B,RightBumper,RightTrigger,LeftTrigger,LeftBumper}
    static class ButtonDraw
    {
        static SpriteFont buttons;
        public static void initialize(SpriteFont buttonFont)
        {
            buttons = buttonFont;
        }

        /// <summary>
        /// Call this within SpirteBatch Begin()/End() blocks to draw that button centred at x,y
        /// </summary>
        /// <param name="spriteBatch">Sprite batch to use</param>
        /// <param name="button">ButtonImage enum button to draw</param>
        /// <param name="pos">position within the current viewport</param>
        /// <param name="scale">float scale to draw the text. 1 is 'normal' sized but 0.5f is often better.</param>
        public static void DrawButton(SpriteBatch spriteBatch, ButtonImage button, Vector2 pos,float scale)
        {
            String s="";
            switch (button)
            {
                case ButtonImage.LeftThumbStick:
                    s=" ";
                    break;
                case ButtonImage.DPad:
                    s="!";
                    break;
                case ButtonImage.RightThumbStick:
                    s="\"";
                    break;
                case ButtonImage.Back:
                    s="#";
                    break;
                case ButtonImage.Guide:
                    s="$";
                    break;
                case ButtonImage.Start:
                    s="%";
                    break;
                case ButtonImage.X:
                    s="&";
                    break;
                case ButtonImage.A:
                    s="'";
                    break;
                case ButtonImage.Y:
                    s="(";
                    break;
                case ButtonImage.B:
                    s=")";
                    break;
                case ButtonImage.RightBumper:
                    s="*";
                    break;
                case ButtonImage.RightTrigger:
                    s="+";
                    break;
                case ButtonImage.LeftTrigger:
                    s=",";
                    break;
                case ButtonImage.LeftBumper:
                    s="-";
                    break;
            }

            Vector2 origin = buttons.MeasureString(s) / 2;
            spriteBatch.DrawString(buttons, s, pos, Color.White, 0, origin, scale, SpriteEffects.None, 1);
        }

    }
}
