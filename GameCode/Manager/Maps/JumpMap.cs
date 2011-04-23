using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BeatShift
{
    public class JumpMap : Map
    {
        


        public override void LoadContent()
        {
            Console.Write("Started loading JumpMap...");

            LoadSkybox("Skyboxes/skybox2", "Skyboxes/j_skybox_upper35");

            modelList.Add(new FbxModel("JumpMap_track", MapContent, MapName.JumpMap, ModelCategory.Track));
            modelList.Add(new FbxModel("JumpMap_walls", MapContent, MapName.JumpMap, ModelCategory.Wall));
            modelList.Add(new FbxModel("JumpMap_scenery", MapContent, MapName.JumpMap, ModelCategory.Scenery));
            mapData = new MapData("JumpMap", 60f, 3f);

            mapTrackTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Diffuse");
            mapTrackAlphaTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Alpha");
            mapTrackNormalTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Normal");
            Console.WriteLine("Loaded JumpMap");
        }
    }
}
