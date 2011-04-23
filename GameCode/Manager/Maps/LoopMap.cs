using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BeatShift
{
    public class LoopMap : Map
    {
        public override void LoadContent()
        {
            Console.Write("Started loading LoopMap...");

            LoadSkybox("Skyboxes/skybox2", "Skyboxes/j_skybox_upper35");

            modelList.Add(new FbxModel("LoopMap_track", MapContent, MapName.LoopMap, ModelCategory.Track));
            modelList.Add(new FbxModel("LoopMap_walls", MapContent, MapName.LoopMap, ModelCategory.Wall));
            modelList.Add(new FbxModel("LoopMap_scenery", MapContent, MapName.LoopMap, ModelCategory.Scenery));
            mapData = new MapData("LoopMap", 60f, 3f);

            mapTrackTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Diffuse");
            mapTrackAlphaTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Alpha");
            mapTrackNormalTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Normal");

            Console.WriteLine("Loaded LoopMap");
        }
    }
}
