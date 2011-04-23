using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BeatShift
{
    public class OldHillMap : Map
    {
        public override void LoadContent()
        {
            Console.Write("Started loading OldHillMap...");

            LoadSkybox("Skyboxes/skybox2", "Skyboxes/j_skybox_upper35");

            modelList.Add(new FbxModel("OldHillMap_track", MapContent, MapName.OldHillMap, ModelCategory.Track));
            modelList.Add(new FbxModel("OldHillMap_walls", MapContent, MapName.OldHillMap, ModelCategory.Wall));
            modelList.Add(new FbxModel("OldHillMap_scenery", MapContent, MapName.OldHillMap, ModelCategory.Scenery));
            mapData = new MapData("OldHillMap", 60f, 3f);

            mapTrackTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Diffuse");
            mapTrackAlphaTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Alpha");
            mapTrackNormalTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Normal");

            Console.WriteLine("Loaded OldHillMap");
        }
    }
}
