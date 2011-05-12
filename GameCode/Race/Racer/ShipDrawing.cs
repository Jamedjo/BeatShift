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
using DPSF;
using DPSF.ParticleSystems;

namespace BeatShift
{
    public class ShipDrawing
    {
        // Graphics related variables
        List<ShipFbx> shipClasses = new List<ShipFbx>();
        public Color shipColour;
        public ShipName currentShip = ShipName.Skylar;
        //Model shipPhysicsModel;
        BasicEffect shipPhysicsModelEffect;
        public List<D_Arrow> drawArrowListRays = new List<D_Arrow>();
        public List<D_Arrow> drawArrowListPermanent = new List<D_Arrow>();
        public Boolean isVisible = true;
        public EngineParticleSystem engineGlow;
        public BeatIndicatorParticleSystem indicator;
        public BeatGlowParticleSystem glow;
        //Physics delegate functions
        public Func<Matrix> getShipDrawOrientationMatrix;
        Func<Vector3> getShipPosition;
        public CollisionParticleSystem collision;

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
            indicator = new BeatIndicatorParticleSystem(null);
            BeatShift.particleManager.AddParticleSystem(indicator);
            indicator.AutoInitialize(BeatShift.graphics.GraphicsDevice, BeatShift.contentManager, null);

            collision = new CollisionParticleSystem(null);
            BeatShift.particleManager.AddParticleSystem(collision);
            collision.AutoInitialize(BeatShift.graphics.GraphicsDevice, BeatShift.contentManager, null);

            glow = new BeatGlowParticleSystem(null);
            BeatShift.particleManager.AddParticleSystem(glow);
            glow.AutoInitialize(BeatShift.graphics.GraphicsDevice, BeatShift.contentManager,null);

            engineGlow = new EngineParticleSystem(null);
            BeatShift.particleManager.AddParticleSystem(engineGlow);
            engineGlow.AutoInitialize(BeatShift.graphics.GraphicsDevice, BeatShift.contentManager, null);

            if (currentShip == ShipName.Skylar)
                engineGlow.SetPosition(new Vector3(0, 0.5f, 4.5f));
            else if (currentShip == ShipName.Omicron)
                engineGlow.SetPosition(new Vector3(0, 0.3f, 5.0f));
            else if (currentShip == ShipName.Wraith)
                engineGlow.SetPosition(new Vector3(0, 0.25f, 4.5f));
            else if (currentShip == ShipName.Flux)
                engineGlow.SetPosition(new Vector3(0, 0.7f, 6.25f));
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

            if (engineGlow != null)
            {
                engineGlow.SetWorldViewProjectionMatrices(worldMatrix, viewMatrix, projectionMatrix);
                collision.SetWorldViewProjectionMatrices(worldMatrix, viewMatrix, projectionMatrix);
                //was using camera position (0,0,0) so all particles were facing towards the centre
                //as particles are one sided this made them invisible
                //this line faces them towards the ship which works as a quick hack when they originate from the centre/center
                //needs to be replaced with engineGlow.SetCameraPosition(camera.getPosition()); or somthing that uses the camera's position.
                //Vector3 temp = camera.cameraPosition();
                if (parentRacer.shipPhysics != null)
                {
                    engineGlow.SetCameraPosition(Vector3.Transform(camera.cameraPosition(), Matrix.Invert(worldMatrix)));
                    collision.SetCameraPosition(Vector3.Transform(camera.cameraPosition(), Matrix.Invert(worldMatrix)));
                }
                else
                {
                    engineGlow.SetCameraPosition(camera.cameraPosition());
                    collision.SetCameraPosition(camera.cameraPosition());
                }
                engineGlow.Draw();
                collision.Draw();
                //Vector3.Transform(camera.cameraPosition(),Matrix.Invert(worldMatrix));
                if (isThisTheCamerasShip && !parentRacer.raceTiming.hasCompletedRace)
                {
                    indicator.SetWorldViewProjectionMatrices(worldMatrix, viewMatrix, projectionMatrix);
                    parentRacer.beatQueue.visualisation.SetWorldViewProjectionMatrices(worldMatrix, viewMatrix, projectionMatrix);
                    glow.SetWorldViewProjectionMatrices(worldMatrix, viewMatrix, projectionMatrix);
                    if (parentRacer.shipPhysics != null)
                    {
                        indicator.SetCameraPosition(Vector3.Transform(camera.cameraPosition(), Matrix.Invert(worldMatrix)));
                        parentRacer.beatQueue.visualisation.SetCameraPosition(Vector3.Transform(camera.cameraPosition(), Matrix.Invert(worldMatrix)));
                        glow.SetCameraPosition(Vector3.Transform(camera.cameraPosition(), Matrix.Invert(worldMatrix)));
                    }
                    else
                    {
                        indicator.SetCameraPosition(camera.cameraPosition());
                        parentRacer.beatQueue.visualisation.SetCameraPosition(camera.cameraPosition());
                        glow.SetCameraPosition(camera.cameraPosition());
                    }
                    indicator.Draw();
                    parentRacer.beatQueue.visualisation.Draw();
                    glow.Draw();

                }
            }
            //if (Options.DrawCollisionPoints)
            {
                foreach (D_Arrow arrow in drawArrowListPermanent)
                {
                    DrawVector.drawArrow(camera, arrow.pos, arrow.dir, arrow.col);
                }
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
