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

            modelList.Add(new FbxModel("CityMap_track", MapContent, MapName.CityMap, ModelCategory.Track));
            modelList.Add(new FbxModel("CityMap_walls", MapContent, MapName.CityMap, ModelCategory.Wall));
            modelList.Add(new FbxModel("CityMap_scenery", MapContent, MapName.CityMap, ModelCategory.Scenery));
            mapData = new MapData("CityMap", 60f, 4f);

            //getting ready for all the scenery objects to be individual FBXs with UV maps
            //modelList.Add(new FbxModel("City\bridge_a", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\bridge_b", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\bridge_c", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\bridge_d", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\energy_line", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\float_struct", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\girder", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\hanging_billboard", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\lighthouse", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\segmented_walkway", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\sign_set_a", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\sign_set_b", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\start_centre", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\start_left", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\start_right", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\struct_a", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\struct_b", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\struct_c", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\t_light", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\the_don", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\tower_a", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\tower_b", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\tower_c", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\tower_d", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\tower_e", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\tower_f", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\tower_g", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\tower_h", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\tower_i", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\tower_j", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\tower_k", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\walkway_a", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\walkway_b", MapContent, MapName.CityMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("City\wires", MapContent, MapName.CityMap, ModelCategory.Scenery));

            mapTrackTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Diffuse");
            mapTrackAlphaTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Alpha");
            mapTrackNormalTexture = BeatShift.contentManager.Load<Texture2D>("Textures/Track2_Normal");

            Console.WriteLine("loaded city");
        }

    }
}