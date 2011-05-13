using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace BeatShift
{
    public class CityMap : Map
    {
        public override void LoadContent()
        {
            Console.Write("Started loading City map...");

            LoadSkybox("Skyboxes/skybox2", "Skyboxes/j_skybox_upper35");

            //TODO: Update to city
            modelList.Add(new FbxModel("CityMap_track", MapContent, MapName.CityMap, ModelCategory.Track));
            modelList.Add(new FbxModel("CityMap_walls", MapContent, MapName.CityMap, ModelCategory.Wall));
            modelList.Add(new FbxModel("CityMap_scenery", MapContent, MapName.CityMap, ModelCategory.Scenery));
            mapData = new MapData("CityMap", 60f, 4f);

            mapTrackTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Diffuse");
            mapTrackAlphaTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Alpha");
            mapTrackNormalTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Normal");

            timeTrialRanks[0] = 21000;
            timeTrialRanks[1] = 24000;
            timeTrialRanks[2] = 27000;
            timeTrialRanks[3] = 31000;

            Console.WriteLine("...Loaded City map");
        }

    }
}