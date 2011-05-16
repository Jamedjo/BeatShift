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
    public class SpaceMap : Map
    {
        public override void LoadContent()
        {
            Console.Write("Started loading Space map...");

            LoadSkybox("Skyboxes/skybox2", "Skyboxes/j_skybox_upper35");

            modelList.Add(new FbxModel("SpaceMap_track", MapContent, MapName.SpaceMap, ModelCategory.Track));
            modelList.Add(new FbxModel("SpaceMap_walls", MapContent, MapName.SpaceMap, ModelCategory.Wall));
            modelList.Add(new FbxModel("SpaceMap_scenery", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            mapData = new MapData("SpaceMap", 60f, 4f);

            mapTrackTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Diffuse");
            mapTrackAlphaTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Alpha");
            mapTrackNormalTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Normal");

            timeTrialRanks[0] = 40000;
            timeTrialRanks[1] = 50000;
            timeTrialRanks[2] = 60000;
            timeTrialRanks[3] = 80000;

            Console.WriteLine("...Loaded Space map");
        }
    }
}