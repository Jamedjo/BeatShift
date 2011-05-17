using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework;
using System.Xml;

namespace BeatShift.Util
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

    public static class HighScore
    {

        #region Constants

        const int TIME_MODE = 0;
        const int POINT_MODE = 1;
        
        const int CITY = 0;
        const int SPACE = 1;
        const int DESERT = 2;

        #endregion


        static XDocument scoreDoc;

        public static List<HighScoreEntry> getHighScores(int trackID, int mode, List<HighScoreEntry> thisRaceValues)
        {
            List<HighScoreEntry> scoreTable = new List<HighScoreEntry>(10);

            if (!(trackID == DESERT || trackID == SPACE || trackID == CITY))
            {
                throw new ArgumentException("Mode must be a valid track identifier from HighScore.", "trackID");
            }
            if (!(mode == TIME_MODE || mode == POINT_MODE))
            {
                throw new ArgumentException("Mode must be a valid mode identifier from HighScore.", "mode");
            }
            init(trackID, mode);

            var tempTable = (from item in scoreDoc.Descendants("entry") select new HighScoreEntry(item.Descendants("name").First().Value, long.Parse(item.Descendants("value").First().Value)));
            List<HighScoreEntry> sortedInput;
            
            int oldID = 0, newID = 0; 
            List<HighScoreEntry> results = new List<HighScoreEntry>(10);
            if (mode == TIME_MODE)
            {
                scoreTable = tempTable.OrderBy(x => x.value).ToList();
                sortedInput = thisRaceValues.OrderBy(x => x.value).ToList();
                for (int i = 0; i < 10; i++)
                {
                    if (newID >= sortedInput.Count)
                    {
                        results[i] = scoreTable[oldID++];
                    }
                    else
                    {
                        if (sortedInput[newID].value < scoreTable[oldID].value)
                        {
                            results[i] = sortedInput[newID++];
                        }
                        else
                        {
                            results[i] = scoreTable[oldID++];
                        }
                    }
                }
            }
            else if (mode == POINT_MODE)
            {
                scoreTable = tempTable.OrderBy(x => x.value).ToList();
                sortedInput = thisRaceValues.OrderBy(x => x.value).ToList();
                for (int i = 0; i < 10; i++)
                {
                    if (newID >= sortedInput.Count)
                    {
                        results[i] = scoreTable[oldID++];
                    }
                    else
                    {
                        if (scoreTable[oldID].value < sortedInput[newID].value)
                        {
                            results[i] = sortedInput[newID++];
                        }
                        else
                        {
                            results[i] = scoreTable[oldID++];
                        }
                    }
                }
            }

            return results;
        }

        private static void init(int trackID, int mode)
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
                fileExists = container.FileExists("HighScore_" + trackID + "_" + mode + ".xml");

                if (fileExists)
                {
                    scoreStream = container.OpenFile("HighScore_" + trackID + "_" + mode + ".xml", FileMode.Open, FileAccess.Read);
                }
                else
                {
                    scoreStream = TitleContainer.OpenStream("HighScore_" + trackID + "_" + mode + ".xml");
                    saveScores(trackID, mode);
                }

                StreamReader scoreStreamReader = new StreamReader(scoreStream);
                scoreDoc = XDocument.Load(scoreStreamReader);
                scoreStream.Dispose();
                scoreStream.Close();
                container.Dispose();
            
        } 

        public static void saveScores(int trackID, int mode)
        {
            {
                StorageContainer container = null;

                while (container == null)
                {
                    IAsyncResult result = BeatShift.Storage.BeginOpenContainer(BeatShift.Title, null, null);
                    result.AsyncWaitHandle.WaitOne();
                    container = BeatShift.Storage.EndOpenContainer(result);
                    result.AsyncWaitHandle.Close();
                }

                Stream scoreStream = container.OpenFile("HighScore_" + trackID + "_" + mode + ".xml", FileMode.Create, FileAccess.Write);
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
