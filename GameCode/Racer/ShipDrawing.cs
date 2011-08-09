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
        //BasicEffect shipPhysicsModelEffect;
        public Boolean isVisible = true;
        float reflectOverride = 0.0f;//Used to turn ship shiny silver at Lvl5

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

        public void Update(GameTime gameTime)
        {
            ShipFbx ship = shipClasses[(int)currentShip];
            if(ship.isAnimated) ship.clipPlayer.Update(gameTime,Matrix.Identity);
        }

        public void playUpClip(int newLevel)
        {
            if (!shipClasses[(int)currentShip].isAnimated) return;
            switch (newLevel)
            {
                case 1://Leveled up from 1 to 2
                    shipClasses[(int)currentShip].clipPlayer.play(1, 9, false);
                    break;
                case 2:
                    shipClasses[(int)currentShip].clipPlayer.play(9, 19, false);
                    break;
                case 3:
                    shipClasses[(int)currentShip].clipPlayer.play(19, 29, false);
                    break;
                case 4://Leveled up from 4 to 5
                    shipClasses[(int)currentShip].clipPlayer.play(29, 39, false);
                    break;

                default:
                    break;
            }
        }

        public void playDownClip(int newLevel)
        {
            if (!shipClasses[(int)currentShip].isAnimated) return;
            switch (newLevel)
            {
                case 0: //Leveled down from 2 to 1
                    shipClasses[(int)currentShip].clipPlayer.play(9, 1, false);//As 50 frames used for 4 animations, frame is ((50/4)*lvl), should have used 40,60 or 80 frames not 50
                    break;
                case 1:
                    shipClasses[(int)currentShip].clipPlayer.play(19, 9, false);
                    break;
                case 2:
                    shipClasses[(int)currentShip].clipPlayer.play(29, 19, false);
                    break;
                case 3: //Leveled down from 5 to 4
                    shipClasses[(int)currentShip].clipPlayer.play(39, 29, false);
                    break;

                default:
                    break;
            }
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
            BeatShift.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;//.LinearWrap;

            Matrix viewMatrix = camera.View;
            Matrix projectionMatrix = camera.Projection;
            BeatShift.graphics.GraphicsDevice.Viewport = camera.Viewport;

            //Set light to colour
            //shipClasses[(int)currentShip].shipRenderer.DiffuseColor = shipColour.ToVector3();
            //shipClasses[(int)currentShip].shipRenderer.DirectionalLight0.DiffuseColor = shipColour.ToVector3();

            //shipClasses[(int)currentShip].shipRenderer.View = viewMatrix;
            //shipClasses[(int)currentShip].shipRenderer.Projection = projectionMatrix;

            //world uses SRT matrix for scale rotation and translation of ship
            Matrix worldMatrix = getShipDrawOrientationMatrix();
            worldMatrix.Translation = getShipPosition();
            //shipClasses[(int)currentShip].shipRenderer.World = worldMatrix;

            if (((camera.ShouldDrawOwnShip || !isThisTheCamerasShip) && GameLoop.getCurrentState() == GameState.LocalGame) || isThisTheCamerasShip)
            {

                if (parentRacer.beatQueue.getLayer() == 4) {
                    reflectOverride = MathHelper.Lerp(reflectOverride, 1.0f,0.05f); }
                else reflectOverride = MathHelper.Lerp(reflectOverride, 0.0f, 0.2f);

                //Draw ship using bShiftEffect.fx as instructed by the fbx file
                ShipFbx shipFbx = shipClasses[(int)currentShip];
                foreach (ModelMesh mesh in shipFbx.model.Meshes)
                {
                    foreach (Effect effect in mesh.Effects)
                    {
                        if (shipFbx.isAnimated)
                        {
                            effect.CurrentTechnique = effect.Techniques["SkinnedShip"];
                            effect.Parameters["Bones"].SetValue(shipFbx.Bones);
                        }

                        effect.Parameters["world_Mx"].SetValue(worldMatrix);
                        effect.Parameters["wvp_Mx"].SetValue(worldMatrix * viewMatrix * projectionMatrix);

                        //Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(worldTransform));
                        //effect.Parameters["wit_Mx"].SetValue(worldInverseTranspose);

                        Matrix viewInverse = Matrix.Invert(viewMatrix);
                        effect.Parameters["viewInv_Mx"].SetValue(viewInverse);


                        effect.Parameters["useAmbient"].SetValue(Globals.useAmbient);
                        effect.Parameters["useLambert"].SetValue(Globals.useLambert);
                        effect.Parameters["useSpecular"].SetValue(Globals.useSpecular);
                        effect.Parameters["drawNormals"].SetValue(Globals.drawNormals);
                        effect.Parameters["reflectOverride"].SetValue(reflectOverride);
                    }
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
