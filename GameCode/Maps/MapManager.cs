using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Xml.Serialization;
//using Microsoft.Xna.Framework.Storage;
//using System.IO;

namespace BeatShift
{
    static class MapManager
    {
        /// <summary>
        /// Contains the map of the game world.
        /// </summary>
        /// 
        public static MapName mapToLoad;
        public static Map currentMap;//TODO:should not be public!!!
        
        public static Boolean Enabled = false;
        public static Boolean Visible = false;

        /*internal static void Initialize()
        {
            
        }*/

        //[Serializable]
        //public struct HighScoreData
        //{
        //    public string playerName;
        //    public string time;
            //public int Count ;

            //public HighScoreData( int count )
            //{
            //    playerName = new string[count];
            //    time = new string[count];
            //    Count = count;
        //}

        //Unloads the previous map if needed and then loads the new one and changes to it
        public static void loadMap()
        {
            if (currentMap != null && mapToLoad == (currentMap.currentMapName)) return;

                if (currentMap != null && mapToLoad != (currentMap.currentMapName) ) MapManager.currentMap.UnloadContent();

                switch (mapToLoad)
                {
                    case MapName.DesertMap: MapManager.currentMap = new DesertMap(); break;
                    case MapName.CityMap: MapManager.currentMap = new CityMap(); break;
                    case MapName.SpaceMap: MapManager.currentMap = new SpaceMap(); break;
                }
                currentMap.currentMapName = mapToLoad;

                MapManager.currentMap.addMapToPhysics();
                GC.Collect();            
        }
        //private static void saveHighScoreData(StorageDevice device)
        //{
        //    HighScoreData data = new HighScoreData();
        //    data.playerName = "Sinico";
        //    data.time = "00:20:12";

        //    IAsyncResult result = device.BeginOpenContainer("StorageTest", null, null);

        //    result.AsyncWaitHandle.WaitOne();

        //    StorageContainer container = device.EndOpenContainer(result);

        //    result.AsyncWaitHandle.Close();

        //    string filename = "highscores.sav";

        //    if(container.FileExists(filename))
        //        container.DeleteFile(filename);

        //    Stream stream = container.CreateFile(filename);


        //}
    }
}
