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
            Console.Write("Started loading Desert map...");

            LoadSkybox("Skyboxes/skybox2", "Skyboxes/j_skybox_upper35");

            modelList.Add(new FbxModel("SpaceMap_track", MapContent, MapName.SpaceMap, ModelCategory.Track));
            modelList.Add(new FbxModel("SpaceMap_walls", MapContent, MapName.SpaceMap, ModelCategory.Wall));
            modelList.Add(new FbxModel("SpaceMap_scenery", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            mapData = new MapData("SpaceMap", 60f, 4f);

            //getting ready for all the scenery objects to be individual FBXs with UV maps
            //modelList.Add(new FbxModel("Space\arrow", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\box_board", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\cam_large", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\cam_mid", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\cam_small", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\double_board", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\h_ring", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\large_double_arrow", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\large_single_arrow", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\light_network", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\light_obj", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\point_multi_arrow", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\point_offset_arrow", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\point_trail_arrow", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\sat_board", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\sat_start", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\satellite1", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\satellite2", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\satellite3", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\single_board", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\space_station", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\speaker_tower", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\speakers_large", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\speakers_small", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\std_multi_arrow", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\std_offset_arrow", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\std_trail_arrow", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\tracker", MapContent, MapName.SpaceMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Space\view_ship", MapContent, MapName.SpaceMap, ModelCategory.Scenery));

            mapTrackTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Diffuse");
            mapTrackAlphaTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Alpha");
            mapTrackNormalTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Normal");

            Console.WriteLine("loaded space");
        }

    }
}