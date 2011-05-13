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

            LoadSkybox("Skyboxes/skybox2", "Skyboxes/j_skybox_upper35");

            modelList.Add(new FbxModel("DesertMap_track", MapContent, MapName.DesertMap, ModelCategory.Track));
            modelList.Add(new FbxModel("DesertMap_walls", MapContent, MapName.DesertMap, ModelCategory.Wall));
            modelList.Add(new FbxModel("DesertMap_scenery", MapContent, MapName.DesertMap, ModelCategory.Scenery));
            mapData= new MapData("DesertMap", 60f, 4f);

            //getting ready for all the scenery objects to be individual FBXs with UV maps
            //modelList.Add(new FbxModel("Desert\banner_pole", MapContent, MapName.DesertMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Desert\camera_stand", MapContent, MapName.DesertMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Desert\circle_sign", MapContent, MapName.DesertMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Desert\diamond_sign", MapContent, MapName.DesertMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Desert\double_board", MapContent, MapName.DesertMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Desert\hanger", MapContent, MapName.DesertMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Desert\lampost", MapContent, MapName.DesertMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Desert\large_darrow", MapContent, MapName.DesertMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Desert\light_stand", MapContent, MapName.DesertMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Desert\med1_board", MapContent, MapName.DesertMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Desert\med2_board", MapContent, MapName.DesertMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Desert\small_board", MapContent, MapName.DesertMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Desert\small_darrow", MapContent, MapName.DesertMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Desert\square_sign", MapContent, MapName.DesertMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Desert\start_board", MapContent, MapName.DesertMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Desert\support", MapContent, MapName.DesertMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Desert\transmitter", MapContent, MapName.DesertMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Desert\wide_board", MapContent, MapName.DesertMap, ModelCategory.Scenery));
            //modelList.Add(new FbxModel("Desert\wind_turbine", MapContent, MapName.DesertMap, ModelCategory.Scenery));

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