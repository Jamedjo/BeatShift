using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeatShift
{
    static class MapManager
    {

        /// <summary>
        /// Contains the map of the game world.
        /// </summary>
        public static Map currentMap;//TODO:should not be public!!!
        
        public static Boolean Enabled = false;
        public static Boolean Visible = false;

        /*internal static void Initialize()
        {
            
        }*/

        //Unloads the previous map if needed and then loads the new one and changes to it
        public static void loadMap(MapName map)
        {
            if (currentMap != null && map == (currentMap.currentMapName)) return;

                if (currentMap != null && map != (currentMap.currentMapName) ) MapManager.currentMap.UnloadContent();

                switch (map)
                {
                    case MapName.DesertMap: MapManager.currentMap = new DesertMap(); break;
                    case MapName.LoopMap: MapManager.currentMap = new LoopMap(); break;
                    case MapName.RingMap: MapManager.currentMap = new RingMap(); break;
                    case MapName.JumpMap: MapManager.currentMap = new JumpMap(); break;
                    case MapName.OldHillMap: MapManager.currentMap = new OldHillMap(); break;
                }
                currentMap.currentMapName = map;

                MapManager.currentMap.addMapToPhysics();
            
        }

    }
}
