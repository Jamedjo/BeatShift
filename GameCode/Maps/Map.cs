using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using BEPUphysics;
using BEPUphysics.DataStructures;
using BeatShift.DebugGraphics;
using BeatShift.Cameras;
using ParallelTasks;

namespace BeatShift
{
    public enum MapName { None, All, CityMap, SpaceMap, DesertMap }
    public enum ModelCategory { SceneryFx, SceneryBasic, Wall, Track, InvisibleWall, GlowScenery } //In draw order

    public abstract class Map
    {
        protected MapData mapData;//List of all MapPoints/Waypoints
        public MapData CurrentMapData { get { return mapData; } }

        protected ContentManager MapContent;

        protected List<FbxModel> modelList = new List<FbxModel>();
        protected FbxModel sphere;

        //protected Texture2D mapTrackTexture;
        //protected Texture2D mapTrackAlphaTexture;
        //protected Texture2D mapTrackNormalTexture;

        public MapName currentMapName;

        public double[] timeTrialRanks = new double[4];

        public Texture2D skyboxTexture;
        public Model skyboxModel;

        #region Map Initialisation and Setting

        public Map()
        {
            MapContent = new ContentManager(BeatShift.singleton.Services);
            MapContent.RootDirectory = "Content";

            //load map content (specific to each map)
            LoadContent();

            //load common things for all maps
            sphere = new FbxModel("Sphere", MapContent, MapName.All, ModelCategory.SceneryBasic);
            //Order the modelList so transparency is drawn in correct order
            modelList = modelList.OrderBy((m) => m.category).ToList();
        }

        //Add physics to current map
        public void addMapToPhysics()
        {
            Physics.reset();

             Parallel.ForEach(modelList, mod =>
            {
                if (mod.mapName == currentMapName || mod.mapName == MapName.All)
                {
                    Physics.addMapToPhysics(mod.model, mod.category);
                }
            });
        }

        #endregion

        #region Loading related methods

        // Load the model of the map.
        public abstract void LoadContent();

        protected void LoadSkybox(string modelName)//, string textureName)
        {
            skyboxModel = BeatShift.contentManager.Load<Model>(modelName);
            //skyboxTexture = BeatShift.contentManager.Load<Texture2D>(textureName);

            foreach (ModelMesh mesh in skyboxModel.Meshes)
            {
                foreach (BasicEffect beffect in mesh.Effects)
                {
                    //beffect.Texture = skyboxTexture;//may only need to do this loop on map change
                    beffect.TextureEnabled = true;
                    //beffect.EnableDefaultLighting();//todo: turn off?
                }
            }
        }

        //Unload the models of the map
        public void UnloadContent()
        {
            // Console.Write("Unloading Map data... ");
            MapContent.Unload();
            // Console.WriteLine("Unloaded Map");
        }
        #endregion

        #region Drawing Related Methods

        public void Draw(GameTime gameTime, CameraWrapper camera)
        {
            BeatShift.graphics.GraphicsDevice.Viewport = camera.Viewport;

            if (Options.DrawTrackNormals) //Draw track normals.
            {
                drawTrackNormals(gameTime, camera);
            }

            //DrawMap
            BeatShift.graphics.GraphicsDevice.BlendState = BlendState.Opaque;
            foreach (FbxModel modelObject in modelList)
            {
                if (modelObject.mapName == currentMapName || modelObject.mapName == MapName.All)
                {
                    drawModel(modelObject, gameTime, camera);
                }
            }

        }

        public void DrawGlow(GameTime gameTime, CameraWrapper camera)
        {
            BeatShift.graphics.GraphicsDevice.Viewport = camera.Viewport;

            foreach (FbxModel modelObject in modelList)
            {
                if (modelObject.mapName == currentMapName || modelObject.mapName == MapName.All)
                {
                    if(modelObject.category.Equals(ModelCategory.GlowScenery) )
                    {
                        drawWithBShiftEffect(modelObject.model, modelObject.transforms, camera);
                    }
                }
            }

        }

        void drawModel(FbxModel modelObject, GameTime gameTime, CameraWrapper camera)
        {
            //Don't draw invisible things
            if (modelObject.category == ModelCategory.InvisibleWall) return;
            if (modelObject.category == ModelCategory.GlowScenery) return;
            //Don't draw when settings turn scenery display off
            if ((!Globals.DisplayScenery) && (modelObject.category.Equals(ModelCategory.SceneryFx)||modelObject.category.Equals(ModelCategory.SceneryBasic)) ) return;//If scenery should not be displayed, don't draw scenry

            //Draw both sides of track by changing cull mode
            if (modelObject.category == ModelCategory.Track) BeatShift.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            
            if ((currentMapName.Equals(MapName.SpaceMap) && modelObject.category == ModelCategory.Wall) || modelObject.category==ModelCategory.SceneryBasic)
            {
                drawWithBasicEffect(modelObject, camera);
            }
            else
            {
                drawWithBShiftEffect(modelObject.model, modelObject.transforms, camera);
            }
            if (modelObject.category == ModelCategory.Track) BeatShift.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        }

        void drawWithBShiftEffect(Model model, Matrix[] transforms, CameraWrapper camera)
        {
            Matrix view = camera.View;
            Matrix projectionMatrix = camera.Projection;

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart mmPart in mesh.MeshParts)
                {
                    // mmPart.Effect = bshiftEffect;
                    Effect effect = mmPart.Effect;
                    Matrix worldTransform = transforms[mesh.ParentBone.Index];
                    effect.Parameters["world_Mx"].SetValue(worldTransform);
                    effect.Parameters["wvp_Mx"].SetValue(worldTransform * view * projectionMatrix);

                    //Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(worldTransform));
                    //effect.Parameters["wit_Mx"].SetValue(worldInverseTranspose);

                    Matrix viewInverse = Matrix.Invert(view);
                    effect.Parameters["viewInv_Mx"].SetValue(viewInverse);

                    effect.Parameters["useAmbient"].SetValue(Globals.useAmbient);
                    effect.Parameters["useLambert"].SetValue(Globals.useLambert);
                    effect.Parameters["useSpecular"].SetValue(Globals.useSpecular);
                    effect.Parameters["drawNormals"].SetValue(Globals.drawNormals);
                }
                mesh.Draw();
            }
        }

        void drawWithBasicEffect(FbxModel fbxModel, CameraWrapper camera)
        {
            Matrix viewMatrix = camera.View;
            Matrix projectionMatrix = camera.Projection;
            foreach (ModelMesh mesh in fbxModel.model.Meshes)
            {
                foreach (BasicEffect beffect in mesh.Effects)
                {   
                    beffect.View = viewMatrix;
                    beffect.Projection = projectionMatrix;
                    beffect.World = fbxModel.transforms[mesh.ParentBone.Index];

                    beffect.EnableDefaultLighting();
                    beffect.PreferPerPixelLighting = true;
                }
                mesh.Draw();
            }
        }

        public void drawSpheres(GameTime gameTime, CameraWrapper camera)
        {
            BeatShift.graphics.GraphicsDevice.Viewport = camera.Viewport;
            drawSpheresOnSingleCamera(gameTime, camera);
        }

        void drawSpheresOnSingleCamera(GameTime gameTime, CameraWrapper camera)
        {
            //MainGame.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            //MainGame.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            Matrix viewMatrix = camera.View;
            Matrix projectionMatrix = camera.Projection;
            foreach (ModelMesh mesh in sphere.model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    //boxEffect = new BasicEffect(BeatShift.graphics.GraphicsDevice);
                    effect.View = viewMatrix;
                    effect.Projection = projectionMatrix;
                    effect.World = sphere.transforms[mesh.ParentBone.Index];

                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;

                    effect.DiffuseColor = Color.OrangeRed.ToVector3();
                    effect.Alpha = 0.2f;

                    MapPoint point;
                    for (int i = CurrentMapData.mapPoints.Count - 1; i >= 0; i--)//draw in reverse order so transparency works when going the right way round the track
                    {
                        point = CurrentMapData.mapPoints[i];
                        effect.World = Matrix.CreateScale(point.getWidth()) * Matrix.CreateTranslation(point.position);
                        effect.DiffuseColor = point.getColour();
                        mesh.Draw();
                    }
                }
            }
        }

        Vector3 red = Color.Red.ToVector3();
        Vector3 yellow = Color.Yellow.ToVector3();
        Vector3 blue = Color.Blue.ToVector3();
        void drawTrackNormals(GameTime gameTime, CameraWrapper camera)
        {
            MapPoint point;
            for (int i = CurrentMapData.mapPoints.Count - 1; i >= 0; i--)
            {
                point = CurrentMapData.mapPoints[i];

                Vector3 drawPosition = point.position;
                DrawVector.drawArrow(camera, drawPosition, point.roadSurface * 9f, yellow);
                DrawVector.drawArrow(camera, drawPosition, point.tangent * 12f, blue);
                DrawVector.drawArrow(camera, drawPosition, point.trackUp * 12f, red);
            }
        }

        public void DrawSkybox(GameTime gameTime, CameraWrapper camera)
        {
            BeatShift.graphics.GraphicsDevice.Viewport = camera.Viewport;

            if (!Globals.DisplaySkybox) return;

            Matrix scale = Matrix.CreateScale(7f);
            Matrix[] skyboxTransforms = new Matrix[skyboxModel.Bones.Count];
            skyboxModel.CopyAbsoluteBoneTransformsTo(skyboxTransforms);
            Matrix viewMatrix = camera.View;
            Matrix projectionMatrix = camera.Projection;
            //Vector3 upMovement = new Vector3(0f, 50f, 0f);

            foreach (ModelMesh mesh in skyboxModel.Meshes)
            {
                foreach (BasicEffect currentEffect in mesh.Effects)
                {
                    Matrix translation = Matrix.CreateTranslation(camera.racer.shipPhysics.ShipPosition);//+upMovement);
                    currentEffect.View = viewMatrix;
                    currentEffect.Projection = projectionMatrix;

                    currentEffect.World = scale * skyboxTransforms[mesh.ParentBone.Index] * translation;//transforms;
                }

                mesh.Draw();
            }
        }

        #endregion
    }
}
