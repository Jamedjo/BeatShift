using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeatShift.Cameras;
using BeatShift.DebugGraphics;
using BEPUphysics.DataStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BeatShift.Input;
using BeatShift.Util;
using DPSF;
using DPSF.ParticleSystems;
using BeatShift.Util;

namespace BeatShift
{
    public class ShipDrawing
    {
        // Graphics related variables
        List<ShipFbx> shipClasses = new List<ShipFbx>();
        public Color shipColour;
        public ShipName currentShip = ShipName.Skylar;
        //BasicEffect shipPhysicsModelEffect;
        public Boolean isVisible = true;

        //Physics delegate functions
        public Func<Matrix> getShipDrawOrientationMatrix;
        Func<Vector3> getShipPosition;
        public EngineParticleSystem engineGlow;
        public BeatGlowParticleSystem glow;

        //particles
        public CollisionParticleSystem collision;
        public RespawnParticleSystem spawn;

        //Ai test arrows
        public Vector3 aiWallRayArrow = Vector3.Zero;
        public Boolean aiWallRayHit = false;
        public Vector3 aiFrontRayArrow = Vector3.Zero;
        public Boolean aiFrontRayHit = false;

        //test arrows
        public List<D_Arrow> drawArrowListRays = new List<D_Arrow>();
        public List<D_Arrow> drawArrowListPermanent = new List<D_Arrow>();

        private Racer parentRacer;

        public ShipDrawing(Func<Matrix> getDrawOrientationMatrix, Func<Vector3> getPosition, Racer parent)
        {
            parentRacer = parent;
            //parent.raceTiming.previousSpeed();
            setPositionFunctions(getDrawOrientationMatrix, getPosition);
            //System.Diagnostics.Debug.WriteLine(engineGlow.Emitter.ParticlesPerSecond);
            LoadContent();
        }

        public void LoadParticles()
        {
            collision = new CollisionParticleSystem(null);
            parentRacer.globalSystems.AddParticleSystem(collision);
            collision.AutoInitialize(BeatShift.graphics.GraphicsDevice, BeatShift.contentManager, null);

            spawn = new RespawnParticleSystem(null);
            parentRacer.globalSystems.AddParticleSystem(spawn);
            spawn.AutoInitialize(BeatShift.graphics.GraphicsDevice, BeatShift.contentManager, null);

            engineGlow = new EngineParticleSystem(null,parentRacer);
            parentRacer.globalSystems.AddParticleSystem(engineGlow);
            engineGlow.AutoInitialize(BeatShift.graphics.GraphicsDevice, BeatShift.contentManager, null);

            if (currentShip == ShipName.Skylar)
                engineGlow.SetOffset(new Vector3(0, 0.5f, 4.5f));
            else if (currentShip == ShipName.Omicron)
                engineGlow.SetOffset(new Vector3(0, 0.3f, 5.0f));
            else if (currentShip == ShipName.Wraith)
                engineGlow.SetOffset(new Vector3(0, 0.25f, 4.5f));
            else if (currentShip == ShipName.Flux)
                engineGlow.SetOffset(new Vector3(0, 0.7f, 6.25f));
        }

        public void setPositionFunctions(Func<Matrix> getOrientationMatrix, Func<Vector3> getPosition)
        {
            getShipDrawOrientationMatrix = getOrientationMatrix;
            getShipPosition = getPosition;
        }

        void LoadContent()
        {
            //Load ships in the order they appear in PlayerType
            shipClasses.Add(new ShipFbx("Skylar"));
            shipClasses.Add(new ShipFbx("Omicron"));
            shipClasses.Add(new ShipFbx("Wraith"));
            shipClasses.Add(new ShipFbx("Flux"));
        }

        #region Draw

        public void Draw(GameTime gameTime, CameraWrapper camera, Boolean isThisTheCamerasShip)
        {

            //TODO: 
            //if playerType == PlayerType.None and this is not the camera belonging to this ship
            //Then return, we only want to draw ship selection ships to thier own camera

            //Set display states
            BeatShift.graphics.GraphicsDevice.BlendState = BlendState.Opaque;
            BeatShift.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            BeatShift.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            Matrix viewMatrix = camera.View;
            Matrix projectionMatrix = camera.Projection;
            BeatShift.graphics.GraphicsDevice.Viewport = camera.Viewport;

            //Set light to colour
            //shipClasses[(int)currentShip].shipRenderer.DiffuseColor = shipColour.ToVector3();
            //shipClasses[(int)currentShip].shipRenderer.DirectionalLight0.DiffuseColor = shipColour.ToVector3();

            shipClasses[(int)currentShip].shipRenderer.View = viewMatrix;
            shipClasses[(int)currentShip].shipRenderer.Projection = projectionMatrix;

            //world uses SRT matrix for scale rotation and translation of ship
            Matrix worldMatrix = getShipDrawOrientationMatrix();
            worldMatrix.Translation = getShipPosition();
            shipClasses[(int)currentShip].shipRenderer.World = worldMatrix;

            if (((camera.ShouldDrawOwnShip || !isThisTheCamerasShip) && GameLoop.getCurrentState() == GameState.LocalGame) || isThisTheCamerasShip)
            {
                foreach (ModelMesh mesh in shipClasses[(int)currentShip].model.Meshes)
                {
                    mesh.Draw();
                }
            }
            
            //if (Options.DrawShipBoundingBoxes)
            //{

            //    BeatShift.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            //    shipPhysicsModelEffect.View = viewMatrix;
            //    shipPhysicsModelEffect.Projection = projectionMatrix;
            //    shipPhysicsModelEffect.World = worldMatrix;
            //    foreach (ModelMesh mesh in shipPhysicsModel.Meshes)
            //    {
            //        mesh.Draw();
            //    }
            //}

            if (Options.DrawShipBoundingBoxes)
            {
                foreach (D_Arrow arrow in drawArrowListRays)
                {
                    DrawVector.drawArrow(camera, arrow.pos, arrow.dir, arrow.col);
                }
                drawArrowListRays.Clear();
            }

            if (Globals.EnableParticles)
            {
                if (engineGlow != null)
                {
                    parentRacer.globalSystems.SetWorldViewProjectionMatricesForAllParticleSystems(Matrix.Identity, viewMatrix, projectionMatrix);
                    parentRacer.globalSystems.SetCameraPositionForAllParticleSystems(camera.cameraPosition());
                    parentRacer.globalSystems.DrawAllParticleSystems();
                    if (isThisTheCamerasShip && !parentRacer.raceTiming.hasCompletedRace)
                    {
                        //Draw visualizations with camera's view matrix, but with the visualization projection matrix
                        parentRacer.visualizationSystems.SetWorldViewProjectionMatricesForAllParticleSystems(Matrix.Identity, camera.VisualizationViewM, camera.VisualizationProjection);
                        parentRacer.visualizationSystems.SetCameraPositionForAllParticleSystems(camera.cameraPosition());
                        parentRacer.visualizationSystems.DrawAllParticleSystems();
                    }
                }
            }
            //if (Options.DrawCollisionPoints)  
            {
                foreach (D_Arrow arrow in drawArrowListPermanent)
                {
                    DrawVector.drawArrow(camera, arrow.pos, arrow.dir, arrow.col);
                }
            }
            if (isThisTheCamerasShip&&AiInputManager.testAI)
            {
                DrawVector.drawArrow(camera, getShipPosition(), aiWallRayArrow, aiWallRayHit ? new Vector3(255, 0, 0) : new Vector3(0, 0, 255));
                DrawVector.drawArrow(camera, getShipPosition(), aiFrontRayArrow, aiFrontRayHit ? new Vector3(200, 0, 50) : new Vector3(0, 50, 200));
            }
            

        }

        //void SetupHullRenderer()
        //{
        //    shipPhysicsModelEffect = (BasicEffect)shipPhysicsModel.Meshes[0].Effects[0];
        //    shipPhysicsModelEffect.EnableDefaultLighting();
        //    shipPhysicsModelEffect.PreferPerPixelLighting = true;
        //    shipPhysicsModelEffect.Alpha = 0.4f;
        //    shipPhysicsModelEffect.DiffuseColor = Color.LawnGreen.ToVector3();
        //}

        #endregion
    }
}
