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
    public class DesertMap : Map
    {
        public override void LoadContent()
        {
            Console.Write("Started loading Desert map...");

            LoadSkybox("Skyboxes/skyboxDesert", "Skyboxes/j_skybox_upper35");

            modelList.Add(new FbxModel("DesertMap_track", MapContent, MapName.DesertMap, ModelCategory.Track));
            modelList.Add(new FbxModel("DesertMap_walls", MapContent, MapName.DesertMap, ModelCategory.Wall));
            modelList.Add(new FbxModel("DesertMap_invisiblewalls", MapContent, MapName.DesertMap, ModelCategory.InvisibleWall));
            modelList.Add(new FbxModel("DesertMap_scenery", MapContent, MapName.DesertMap, ModelCategory.Scenery));
            mapData= new MapData("DesertMap", 60f, 4f);

            mapTrackTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Diffuse");
            mapTrackAlphaTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Alpha");
            mapTrackNormalTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Normal");

            timeTrialRanks[0] = 21000;
            timeTrialRanks[1] = 24000;
            timeTrialRanks[2] = 27000;
            timeTrialRanks[3] = 31000;

            Console.WriteLine("loaded desert");
        }

    }
}