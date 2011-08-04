using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework;
using System.Xml;

namespace BeatShift
{
    public struct HighScoreEntry
    {
        public readonly String name;
        public readonly long value;

        public HighScoreEntry(String name, long value)
            : this()
        {
            this.name = name;
            this.value = value;
        }
    }

    public enum HighScoreType { TimeMode, PointMode }

    public static class HighScore
    {
        
        private static XDocument scoreDoc;

        /// <summary>
        /// Returns the previous highscore list for a track and mode.
        /// </summary>
        /// <param name="trackID">The track to get highscores for</param>
        /// <param name="mode">Either 0 or 1, for TIME_MODE or  POINT_MODE respectively</param>
        public static List<HighScoreEntry> getHighScores(MapName trackID, HighScoreType mode)
        {
             return getHighScores(trackID, mode, new List<HighScoreEntry>());
        }

        /// <summary>
        /// Get the highscores for a track and mode, incorporating changes to be made.
        /// </summary>
        /// <param name="trackID">The track to get highscores for</param>
        /// <param name="mode">Either 0 or 1, for TIME_MODE or  POINT_MODE respectively</param>
        public static List<HighScoreEntry> getHighScores(MapName trackID, HighScoreType mode, List<HighScoreEntry> thisRaceValues)
        {
            List<HighScoreEntry> scoreTable = new List<HighScoreEntry>(10);

            if (!(trackID == MapName.CityMap || trackID == MapName.DesertMap || trackID == MapName.SpaceMap))
            {
                throw new ArgumentException("Mode must be a valid track identifier from HighScore.", "trackID");
            }
            if (!(mode == HighScoreType.TimeMode || mode == HighScoreType.PointMode))
            {
                throw new ArgumentException("Mode must be a valid mode identifier from HighScore.", "mode");
            }
            switch (trackID)
            {
                case MapName.CityMap: init(0, mode); break;
                case MapName.DesertMap: init(1, mode); break;
                case MapName.SpaceMap: init(2, mode); break;
            }

            var tempTable = (from item in scoreDoc.Descendants("entry") select new HighScoreEntry(item.Descendants("name").First().Value, long.Parse(item.Descendants("value").First().Value)));
            List<HighScoreEntry> sortedInput;
            
            int oldID = 0, newID = 0; 
            List<HighScoreEntry> results = new List<HighScoreEntry>(10);
            if (mode == HighScoreType.TimeMode)
            {
                scoreTable = tempTable.OrderBy(x => x.value).ToList();
                sortedInput = thisRaceValues.OrderBy(x => x.value).ToList();
                scoreDoc = new XDocument();
                XElement rootNode = new XElement("highScores");
                scoreDoc.Add(rootNode);
                for (int i = 0; i < 10; i++)
                {
                    XElement entry = new XElement("entry");
                    XElement name = new XElement("name");
                    XElement value = new XElement("value");
                    if ((newID >= sortedInput.Count) || (scoreTable[oldID].value < sortedInput[newID].value))
                    {
                        results.Add(scoreTable[oldID]);

                        name.SetValue(scoreTable[oldID].name);
                        value.SetValue(scoreTable[oldID].value);
                        oldID++;
                    }
                    else 
                    {
                        results.Add(sortedInput[newID]);
                        
                        name.SetValue(sortedInput[newID].name);
                        value.SetValue(sortedInput[newID].value);
                        newID++;
                    }
                    rootNode.Add(entry);
                    entry.Add(name);
                    entry.Add(value);
                }
            }
            else if (mode == HighScoreType.PointMode)
            {
                scoreTable = tempTable.OrderByDescending(x => x.value).ToList();
                sortedInput = thisRaceValues.OrderByDescending(x => x.value).ToList();
                scoreDoc = new XDocument();
                XElement rootNode = new XElement("highScores");
                scoreDoc.Add(rootNode);
                for (int i = 0; i < 10; i++)
                {
                    XElement entry = new XElement("entry");
                    XElement name = new XElement("name");
                    XElement value = new XElement("value");
                    if ((newID >= sortedInput.Count) || (scoreTable[oldID].value > sortedInput[newID].value))
                    {
                        results.Add(scoreTable[oldID]);

                        name.SetValue(scoreTable[oldID].name);
                        value.SetValue(scoreTable[oldID].value);
                        oldID++;
                    }
                    else 
                    {
                        results.Add(sortedInput[newID]);
                        
                        name.SetValue(sortedInput[newID].name);
                        value.SetValue(sortedInput[newID].value);
                        newID++;
                    }
                    rootNode.Add(entry);
                    entry.Add(name);
                    entry.Add(value);
                }
            }
            

            //for (int i = 0; i < 10; i++)
            //{
            //    System.Diagnostics.Debug.WriteLine("n: " + results[i].name + "   s: " + results[i].value);
            //}

            switch (trackID)
            {
                case MapName.CityMap: saveScores(0, mode); break;
                case MapName.DesertMap: saveScores(1, mode); break;
                case MapName.SpaceMap: saveScores(2, mode); break;
            }

            return results;
        }

        private static void init(int trackID, HighScoreType mode)
        {
            
                StorageContainer container = null;
                Boolean fileExists;

                while (container == null)
                {
                    IAsyncResult result = BeatShift.Storage.BeginOpenContainer(BeatShift.Title, null, null);
                    result.AsyncWaitHandle.WaitOne();
                    container = BeatShift.Storage.EndOpenContainer(result);
                    result.AsyncWaitHandle.Close();
                }

                Stream scoreStream;
                fileExists = container.FileExists("HighScore_" + trackID + "_" + ((int)mode) + ".xml");

                if (fileExists)
                {
                    scoreStream = container.OpenFile("HighScore_" + trackID + "_" + ((int)mode) + ".xml", FileMode.Open, FileAccess.Read);
                }
                else
                {
                    scoreStream = TitleContainer.OpenStream("HighScore_" + trackID + "_" + ((int)mode) + ".xml");
                }

                StreamReader scoreStreamReader = new StreamReader(scoreStream);
                scoreDoc = XDocument.Load(scoreStreamReader);
                
                scoreStream.Dispose();
                scoreStream.Close();
                container.Dispose();
        } 

        public static void saveScores(int trackID, HighScoreType mode)
        {
            {

                System.Diagnostics.Debug.WriteLine(scoreDoc);


                StorageContainer container = null;

                while (container == null)
                {
                    IAsyncResult result = BeatShift.Storage.BeginOpenContainer(BeatShift.Title, null, null);
                    result.AsyncWaitHandle.WaitOne();
                    container = BeatShift.Storage.EndOpenContainer(result);
                    result.AsyncWaitHandle.Close();
                }



                Stream scoreStream = container.OpenFile("HighScore_" + trackID + "_" + ((int)mode) + ".xml", FileMode.Create, FileAccess.Write);
                XmlWriter optsStreamWriter = XmlWriter.Create(scoreStream);
                scoreDoc.WriteTo(optsStreamWriter);
                optsStreamWriter.Flush();
                scoreStream.Dispose();
                scoreStream.Close();
                container.Dispose();
            }

        }

    }
}
