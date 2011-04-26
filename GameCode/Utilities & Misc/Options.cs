using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Collections.Generic;

namespace BeatShift
{
    public static class Options
    {

        #region Public accessors

        /// <summary>
        /// Controls whether the keyboard is used as the second player.
        /// </summary>
        public static Boolean UseKeyboardAsPad2
        {
            get
            {
                return useKeyboardAsPad2;
            }

            set
            {
                useKeyboardAsPad2 = value;
                optsDoc.Root.Descendants("UseKeyboardAsPad2").First().Value = value.ToString();
            }
        }

        /// <summary>
        /// Controls whether waypoints are drawn on the track.
        /// </summary>
        public static Boolean DrawWaypoints
        {
            get
            {
                return drawWaypoints;
            }

            set
            {
                drawWaypoints = value;
                optsDoc.Root.Descendants("DrawWayPoints").First().Value = value.ToString();
            }
        }

        /// <summary>
        /// Controls whether track normals are drawn.
        /// </summary>
        public static Boolean DrawTrackNormals
        {
            get
            {
                return drawTrackNormals;
            }

            set
            {
                drawTrackNormals = value;
                optsDoc.Root.Descendants("DrawTrackNormals").First().Value = value.ToString();
            }
        }

        /// <summary>
        /// Controls whether AI is added to races.
        /// </summary>
        public static Boolean AddAItoGame
        {
            get
            {
                return addAItoGame;
            }

            set
            {
                addAItoGame = value;
                optsDoc.Root.Descendants("AddAI").First().Value = value.ToString();
            }
        }

        /// <summary>
        /// Controls whether bounding boxes of all ships are drawn.
        /// </summary>
        public static Boolean DrawShipBoundingBoxes
        {
            get
            {
                return drawShipBoundingBoxes;
            }

            set
            {
                drawShipBoundingBoxes = value;
                optsDoc.Root.Descendants("DrawShipBoundingBoxes").First().Value = value.ToString();
            }
        }

        /// <summary>
        /// Controls whether controller vibration is on or off
        /// </summary>
        public static Boolean ControllerVibration
        {
            get
            {
                return controllerVibration;
            }

            set
            {
                controllerVibration = value;
                optsDoc.Root.Descendants("ControllerVibration").First().Value = value.ToString();
            }
        }

        /// <summary>
        /// Used to scale volume of all in game sounds.
        /// </summary>
        public static byte MasterVolume
        {
            get
            {
                return masterVolume;
            }

            set
            {
                if(value <= 100)
                {
                    masterVolume = value;
                    optsDoc.Root.Descendants("MasterVolume").First().Value = value.ToString();
                    SoundManager.masterVolumeChanged();
                }
                else
                {
                    throw new ArgumentOutOfRangeException("MasterVolume", "Value must be <= 100");
                }
            }
        }

        /// <summary>
        /// Controls the relative volume of music in game.
        /// </summary>
        public static byte MusicVolume
        {
            get
            {
                return musicVolume;
            }

            set
            {
                if(value <= 100)
                {
                    musicVolume = value;
                    optsDoc.Root.Descendants("MusicVolume").First().Value = value.ToString();
                    SoundManager.musicVolumeChanged();
                }
                else
                {
                    throw new ArgumentOutOfRangeException("MusicVolume", "Value must be <= 100");
                }
            }
        }

        /// <summary>
        /// Controls the relative volume of voice effects in game.
        /// </summary>
        public static byte VoiceVolume
        {
            get
            {
                return voiceVolume;
            }

            set
            {
                if (value <= 100)
                {
                    voiceVolume = value;
                    optsDoc.Root.Descendants("VoiceVolume").First().Value = value.ToString();
                    SoundManager.voiceVolumeChanged();
                }
                else
                {
                    throw new ArgumentOutOfRangeException("VoiceVolume", "Value must be <= 100");
                }
            }
        }

        /// <summary>
        /// Controls the relative volume of sound effects in game.
        /// </summary>
        public static byte SfxVolume
        {
            get
            {
                return sfxVolume;
            }

            set
            {
                if(value <= 100)
                {
                    sfxVolume = value;
                    optsDoc.Root.Descendants("SfxVolume").First().Value = value.ToString();
                    SoundManager.sfxVolumeChanged();
                }
                else
                {
                    throw new ArgumentOutOfRangeException("SfxVolume", "Value must be <= 100");
                }
            }
        }

        #endregion

        #region Private variables

        // XDocument containing the XML Options tree.
        private static XDocument optsDoc;

        // Private local variables accessed through getters and setters to maintain the XDocument state.
        private static Boolean useKeyboardAsPad2;
        private static Boolean addAItoGame;
        private static Boolean drawTrackNormals;
        private static Boolean drawWaypoints;
        private static Boolean drawShipBoundingBoxes;
        private static Boolean controllerVibration;
        private static byte masterVolume;
        private static byte musicVolume;
        private static byte voiceVolume;
        private static byte sfxVolume;

        #endregion

        /// <summary>
        /// Initializes the static Options class so it contains the users stored options
        /// </summary>
        /// <param name="storageDevice">The StorageDevice which is used by the game. Normally
        /// this would be accessed statically through BeatShift but it doesn't appear to be 
        /// reliably accessible since that object may not have completed its initialization.</param> 
        public static void Initialize(StorageDevice storageDevice)
        {
            System.Diagnostics.Debug.WriteLine("BeatShift.Storage: " + BeatShift.Storage != null);
            Boolean optsFileExists;
            {
                StorageContainer container = null;

                while(container == null)
                {
                    IAsyncResult result = BeatShift.Storage.BeginOpenContainer(BeatShift.Title, null, null);
                    result.AsyncWaitHandle.WaitOne();
                    container = BeatShift.Storage.EndOpenContainer(result);
                    result.AsyncWaitHandle.Close();
                }

                Stream optsStream;
                optsFileExists = container.FileExists("Options.xml");

                if(optsFileExists)
                {
                    optsStream = container.OpenFile("Options.xml", FileMode.Open, FileAccess.Read);
                }
                else
                {
                    optsStream = TitleContainer.OpenStream("Options.xml");
                }

                StreamReader optsStreamReader = new StreamReader(optsStream);
                optsDoc = XDocument.Load(optsStreamReader);
                optsStream.Dispose();
                optsStream.Close();
                container.Dispose();
            }

            try
            {
                drawWaypoints = (from item in optsDoc.Root.Descendants() where item.Name == "DrawWayPoints" select Boolean.Parse(item.Value)).First();
                useKeyboardAsPad2 = (from item in optsDoc.Root.Descendants() where item.Name == "UseKeyboardAsPad2" select Boolean.Parse(item.Value)).First();
                drawTrackNormals = (from item in optsDoc.Root.Descendants() where item.Name == "DrawTrackNormals" select Boolean.Parse(item.Value)).First();
                addAItoGame = (from item in optsDoc.Root.Descendants() where item.Name == "AddAI" select Boolean.Parse(item.Value)).First();
                drawShipBoundingBoxes = (from item in optsDoc.Root.Descendants() where item.Name == "DrawShipBoundingBoxes" select Boolean.Parse(item.Value)).First();
                controllerVibration = (from item in optsDoc.Root.Descendants() where item.Name == "ControllerVibration" select Boolean.Parse(item.Value)).First();
                try
                {
                    masterVolume = (from item in optsDoc.Root.Descendants() where item.Name == "MasterVolume" select byte.Parse(item.Value)).First();
                    if(masterVolume > 100)
                        masterVolume = 100;
                }
                catch(OverflowException OException)
                {
                    System.Diagnostics.Debug.WriteLine(OException.StackTrace);
                    masterVolume = 100;
                }
                try
                {
                    musicVolume = (from item in optsDoc.Root.Descendants() where item.Name == "MusicVolume" select byte.Parse(item.Value)).First();
                    if(musicVolume > 100)
                        musicVolume = 100;
                }
                catch(OverflowException OException)
                {
                    System.Diagnostics.Debug.WriteLine(OException.StackTrace);
                    musicVolume = 100;
                }
                try
                {
                    sfxVolume = (from item in optsDoc.Root.Descendants() where item.Name == "VoiceVolume" select byte.Parse(item.Value)).First();
                    if (voiceVolume > 100)
                        voiceVolume = 100;
                }
                catch (OverflowException OException)
                {
                    System.Diagnostics.Debug.WriteLine(OException.StackTrace);
                    voiceVolume = 100;
                }

                try
                {
                    sfxVolume = (from item in optsDoc.Root.Descendants() where item.Name == "SfxVolume" select byte.Parse(item.Value)).First();
                    if(sfxVolume > 100)
                        sfxVolume = 100;
                }
                catch(OverflowException OException)
                {
                    System.Diagnostics.Debug.WriteLine(OException.StackTrace);
                    sfxVolume = 100;
                }

                SoundManager.masterVolumeChanged();

                if(!optsFileExists)
                {
                    saveOptions();
                }
            }
            catch(InvalidOperationException invOpException)
            {
                StorageContainer container = null;

                while(container == null)
                {
                    IAsyncResult result = BeatShift.Storage.BeginOpenContainer(BeatShift.Title, null, null);
                    result.AsyncWaitHandle.WaitOne();
                    container = BeatShift.Storage.EndOpenContainer(result);
                    result.AsyncWaitHandle.Close();
                }

                optsFileExists = container.FileExists("Options.xml");

                if(optsFileExists)
                {
                    // If this throws an exception (Which it probably has, delete the file manually, the exception tells you
                    // where it is.
                    container.DeleteFile("Options.xml");
                }
                else
                {
                    throw new InvalidOperationException("Options.xml in TitleStorage is invalid", invOpException);
                }
                container.Dispose();
                Initialize(storageDevice);
            }
        }

        public static void saveOptions()
        {
            {
                StorageContainer container = null;

                while(container == null)
                {
                    IAsyncResult result = BeatShift.Storage.BeginOpenContainer(BeatShift.Title, null, null);
                    result.AsyncWaitHandle.WaitOne();
                    container = BeatShift.Storage.EndOpenContainer(result);
                    result.AsyncWaitHandle.Close();
                }

                Stream optsStream = container.OpenFile("Options.xml", FileMode.Create, FileAccess.Write);
                XmlWriter optsStreamWriter = XmlWriter.Create(optsStream);
                optsDoc.WriteTo(optsStreamWriter);
                optsStreamWriter.Flush();
                optsStream.Dispose();
                optsStream.Close();
                container.Dispose();
            }

        }

    }
}