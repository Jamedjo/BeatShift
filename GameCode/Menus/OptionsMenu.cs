using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BeatShift.Menus
{
    class OptionsMenu : IMenuPage
    {
        public bool debugOptions = true;
        public OptionsMenu()
        {
            title = "OPTIONS";
        }

        public override void overrideMenuPositions()
        {
            DrawMenuItemsFromTextCentre = true;
            //DrawTitleFromTextCentre = true;
            //TitlePos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, 75);
            MenuPos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, 250);
            //TitleStartPos = new Vector2(BeatShift.graphics.GraphicsDevice.Viewport.Width / 2, -100);
            //UseTextWidthAsOffset = true;
            Offset = new Vector2(0, 35);//additional vertical spaceing.
            TextScale = 0.5f;
        }

        public override void setupMenuItems()
        {
            addMenuItem("Add AI: ", (Action)(delegate
            {
                Options.AddAItoGame = !Options.AddAItoGame;
            })
            ,
            (Func<string>)delegate { return Options.AddAItoGame.ToString(); }
            );

            addMenuItem("Number Of AI: ", (Action)(delegate
            {
                if (Options.NumberAI > 3)
                    Options.NumberAI = 0;
                else
                    Options.NumberAI ++;
            })
            ,
            (Func<string>)delegate { return Options.NumberAI.ToString(); }
            );

            if (debugOptions == true)
            {
                addMenuItem("Draw Waypoints: ", (Action)(delegate
                {
                    Options.DrawWaypoints = !Options.DrawWaypoints;
                })
                ,
                (Func<string>)delegate { return Options.DrawWaypoints.ToString(); }
                );

                addMenuItem("Draw Track Normals: ", (Action)(delegate
                {
                    Options.DrawTrackNormals = !Options.DrawTrackNormals;
                })
                ,
                (Func<string>)delegate { return Options.DrawTrackNormals.ToString(); }
                );

                //addMenuItem("Input Method: ", (Action)(delegate
                //{

                //})
                //,
                //(Func<string>)delegate { return ""; }
                //);

                //addMenuItem("Beat Offset: ", (Action)(delegate
                //{

                //})
                //,
                //(Func<string>)delegate { return ""; }
                //);

                addMenuItem("Keyboard as Pad 2: ", (Action)(delegate
                {
                    Options.UseKeyboardAsPad2 = !Options.UseKeyboardAsPad2;
                    BeatShift.shipSelect.resetInputsAndScreens();
                })
                ,
                (Func<string>)delegate { return Options.UseKeyboardAsPad2.ToString(); }
                );

                addMenuItem("Draw Ship Physics: ", (Action)(delegate
                {
                    Options.DrawShipBoundingBoxes = !Options.DrawShipBoundingBoxes;
                })
                ,
                (Func<string>)delegate { return Options.DrawShipBoundingBoxes.ToString(); }
                );

                addMenuItem("Controller Vibration: ", (Action)(delegate
                {
                    Options.ControllerVibration = !Options.ControllerVibration;
                })
                ,
                (Func<string>)delegate { return Options.ControllerVibration.ToString(); }
                );
            }
            else
            {
                Options.DrawWaypoints = false;
                Options.DrawTrackNormals = false;
                Options.ControllerVibration = true;
                Options.DrawShipBoundingBoxes = false;
                Options.UseKeyboardAsPad2 = false;
            }

            addMenuItem("Master Volume: ", (Action)(delegate
            {
                if (Options.MasterVolume > 90)
                    Options.MasterVolume = 0;
                else
                    Options.MasterVolume += 10;
            })
            ,
            (Func<string>)delegate { return Options.MasterVolume.ToString() + "%"; }
            );
            addMenuItem("Music Volume: ", (Action)(delegate
            {
                if (Options.MusicVolume > 90)
                    Options.MusicVolume = 0;
                else
                    Options.MusicVolume += 10;
            })
            ,
            (Func<string>)delegate { return Options.MusicVolume.ToString() + "%"; }
            );

            addMenuItem("Voice Volume: ", (Action)(delegate
            {
                if (Options.VoiceVolume > 90)
                    Options.VoiceVolume = 0;
                else
                    Options.VoiceVolume += 10;
            })
            ,
            (Func<string>)delegate { return Options.VoiceVolume.ToString() + "%"; }
            );

            addMenuItem("Effect Volume: ", (Action)(delegate
            {
                if (Options.SfxVolume > 90)
                    Options.SfxVolume = 0;
                else
                    Options.SfxVolume += 10;
            })
            ,
            (Func<string>)delegate { return Options.SfxVolume.ToString() + "%"; }
            );

        }

        public override void leavingMenu()
        {
            // This ought to display a message to the user telling them that the options are being saved.
            // System.Diagnostic.Debug.WriteLine("Start saving options");
            Options.saveOptions();
            // System.Diagnostic.Debug.WriteLine("Options saving complete.");
            base.leavingMenu();
        }

    }
}
