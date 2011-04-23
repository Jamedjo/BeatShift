using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BeatShift
{
    public class RingMap : Map
    {
        public override void LoadContent()
        {
            Console.Write("Started loading Ringmap...");

            LoadSkybox("Skyboxes/skybox2", "Skyboxes/j_skybox_upper35");

            modelList.Add(new FbxModel("RingMap_track", MapContent, MapName.RingMap, ModelCategory.Track));
            modelList.Add(new FbxModel("RingMap_walls", MapContent, MapName.RingMap, ModelCategory.Wall));
            modelList.Add(new FbxModel("RingMap_scenery", MapContent, MapName.RingMap, ModelCategory.Scenery));
            mapData = new MapData("RingMap", 60f, 3f);

            mapTrackTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Diffuse");
            mapTrackAlphaTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Alpha");
            mapTrackNormalTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Normal");

            Console.WriteLine("Loaded Ringmap");
        }
    }
}
