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

namespace BeatShift
{
    public enum MapName { None, All, CityMap, SpaceMap, DesertMap }
    public enum ModelCategory { Scenery, Wall, Track } //In draw order

    public abstract class Map
    {
        protected MapData mapData;//List of all MapPoints/Waypoints
        public MapData CurrentMapData { get { return mapData; } }

        protected ContentManager MapContent;

        protected List<FbxModel> modelList = new List<FbxModel>();
        protected FbxModel sphere;
        Effect bshiftEffect;

        protected Texture2D mapTrackTexture;
        protected Texture2D mapTrackAlphaTexture;
        protected Texture2D mapTrackNormalTexture;
        
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
            sphere = new FbxModel("Sphere", MapContent, MapName.All, ModelCategory.Scenery);
            //Order the modelList so transparency is drawn in correct order
            modelList = modelList.OrderBy((m) => m.category).ToList();
            //load shader
            bshiftEffect = MapContent.Load<Effect>("Shaders/BShiftShader");

        }

        //Add physics to current map
        public void addMapToPhysics()
        {
            Physics.reset();

            Console.Write("adding physics to map... ");
            foreach (FbxModel mod in modelList)
            {
                if (mod.mapName == currentMapName || mod.mapName == MapName.All)
                {
                    Physics.addMapToPhysics(mod.model, mod.category);
                }
            }
            Console.WriteLine("Loaded");
        }

        #endregion

        #region Loading related methods

        // Load the model of the map.
        public abstract void LoadContent();

        #endregion

        //Unload the models of the map
        public void UnloadContent()
        {
            Console.Write("Unloading Map data... ");
            MapContent.Unload();
            Console.WriteLine("Unloaded Map");
        }

        #region Drawing Related Methods

        // Used to modify the effects in a mesh
        void setupBShiftEffect(Matrix view, Matrix proj, Matrix worldTransform)//, Vector3 viewVector)
        {
            bshiftEffect.Parameters["world_Mx"].SetValue(worldTransform);
            bshiftEffect.Parameters["view_Mx"].SetValue(view);
            //bshiftEffect.Parameters["ViewVector"].SetValue(viewVector);
            bshiftEffect.Parameters["wvp_Mx"].SetValue(worldTransform * view * proj);

            Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(worldTransform));
            bshiftEffect.Parameters["wit_Mx"].SetValue(worldInverseTranspose);

            
            if (currentMapName == MapName.SpaceMap)
                bshiftEffect.Parameters["ambientColour"].SetValue(Color.DodgerBlue.ToVector4());
            else if (currentMapName == MapName.CityMap)
                bshiftEffect.Parameters["ambientColour"].SetValue(Color.PapayaWhip.ToVector4());
            else bshiftEffect.Parameters["ambientColour"].SetValue(Color.White.ToVector4());

            bshiftEffect.Parameters["ambientIntensity"].SetValue(0.2f);
        }

        // Used to modify the effects in a mesh
        void setupEffect(BasicEffect effect, Matrix view, Matrix proj, Matrix transforms)
        {
            effect.View = view;
            effect.Projection = proj;
            effect.World = transforms;

            effect.EnableDefaultLighting();
            effect.PreferPerPixelLighting = true;

        }

        // Draw the map withing the game.
        public void Draw(GameTime gameTime, CameraWrapper camera)
        {
            BeatShift.graphics.GraphicsDevice.Viewport = camera.Viewport;

            BeatShift.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            DrawSkybox(camera);

            //Set display states
            BeatShift.graphics.GraphicsDevice.BlendState = BlendState.Opaque;
            BeatShift.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            BeatShift.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;


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

        void drawModel(FbxModel modelObject, GameTime gameTime, CameraWrapper camera)
        {
            RasterizerState cull = BeatShift.graphics.GraphicsDevice.RasterizerState;
            if (modelObject.category == ModelCategory.Track)
            {
                BeatShift.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                drawWithBShiftEffect(modelObject.model, modelObject.transforms, camera);
            }
            else
            {
                drawWithBasicEffect(modelObject, camera);
            }
            BeatShift.graphics.GraphicsDevice.RasterizerState = cull;
        }

        void drawWithBasicEffect(FbxModel fbxModel, CameraWrapper camera)
        {
            Matrix viewMatrix = camera.View;
            Matrix projectionMatrix = camera.Projection;
            foreach (ModelMesh mesh in fbxModel.model.Meshes)
            {
                foreach (BasicEffect beffect in mesh.Effects)
                {
                    setupEffect(beffect, viewMatrix, projectionMatrix, fbxModel.transforms[mesh.ParentBone.Index]);
                    //if (fbxModel.category == ModelCategory.Wall)
                    //{
                    //    Vector3 colour = new Vector3(240, 100, 255)*0.2f;
                    //    beffect.LightingEnabled = true;
                    //    beffect.EmissiveColor = colour;
                    //    beffect.AmbientLightColor = colour;
                    //}
                }
                mesh.Draw();
            }
        }

        void drawWithBShiftEffect(Model model, Matrix[] transforms, CameraWrapper camera)
        {
            Vector3 viewVector = camera.ViewVector;
            Matrix viewMatrix = camera.View;
            Matrix projectionMatrix = camera.Projection;
            bshiftEffect.Parameters["diffuseTex"].SetValue(mapTrackTexture);
            bshiftEffect.Parameters["alphaTex"].SetValue(mapTrackAlphaTexture);
            bshiftEffect.Parameters["normalTex"].SetValue(mapTrackNormalTexture);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart mmPart in mesh.MeshParts)
                {
                    mmPart.Effect = bshiftEffect;
                    setupBShiftEffect(viewMatrix, projectionMatrix, transforms[mesh.ParentBone.Index]);//, viewVector);
                }
                mesh.Draw();
            }
        }

        public void drawSpheres(GameTime gameTime, CameraWrapper camera)
        {
            BeatShift.graphics.GraphicsDevice.Viewport = camera.Viewport;

            //Set display states
            BeatShift.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            BeatShift.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            BeatShift.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

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
                    setupEffect(effect, viewMatrix, projectionMatrix, sphere.transforms[mesh.ParentBone.Index]);
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

        void drawTrackNormals(GameTime gameTime, CameraWrapper camera)
        {
            MapPoint point;
            for (int i = CurrentMapData.mapPoints.Count - 1; i >= 0; i--)
            {
                point = CurrentMapData.mapPoints[i];

                Vector3 drawPosition = point.position;
                DrawVector.drawArrow(camera, drawPosition, point.roadSurface*9f, Color.Yellow.ToVector3());
                DrawVector.drawArrow(camera, drawPosition, point.tangent * 12f, Color.Blue.ToVector3());
                DrawVector.drawArrow(camera, drawPosition, point.trackUp * 12f, Color.Red.ToVector3());
            }
        }

        protected void LoadSkybox(string modelName, string textureName)
        {
            skyboxModel = BeatShift.contentManager.Load<Model>(modelName);
            //skyboxTexture = BeatShift.contentManager.Load<Texture2D>(textureName);

            foreach(ModelMesh mesh in skyboxModel.Meshes)
            {
                foreach (BasicEffect beffect in mesh.Effects)
                {
                    //beffect.Texture = skyboxTexture;//may only need to do this loop on map change
                    beffect.TextureEnabled = true;
                    //beffect.EnableDefaultLighting();//todo: turn off?
                }
            }
        }

        //todo call method
        public void DrawSkybox(CameraWrapper camera)
        {
        // set scale
            Matrix[] skyboxTransforms = new Matrix[skyboxModel.Bones.Count];
            skyboxModel.CopyAbsoluteBoneTransformsTo(skyboxTransforms);
            Matrix viewMatrix = camera.View;
            Matrix projectionMatrix = camera.Projection;


            foreach (ModelMesh mesh in skyboxModel.Meshes)
            {
                // TODO: put int he right place

                foreach (BasicEffect currentEffect in mesh.Effects)
                {
                    Matrix translation = Matrix.CreateTranslation(camera.racer.shipPhysics.physicsBody.Position);
                    currentEffect.View = viewMatrix;
                    currentEffect.Projection = projectionMatrix;

                     currentEffect.World = skyboxTransforms[mesh.ParentBone.Index] * translation;//transforms;
                }

                mesh.Draw();
            }
        }

        #endregion
    }
}
