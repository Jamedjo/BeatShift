#region File Description
//===================================================================
// DefaultSpriteParticleSystemTemplate.cs
// 
// This file provides the template for creating a new Sprite Particle
// System that inherits from the Default Sprite Particle System.
//
// The spots that should be modified are marked with TODO statements.
//
// Copyright Daniel Schroeder 2008
//===================================================================
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace DPSF.ParticleSystems
{

    public class EngineSpriteParticle : DefaultSpriteParticle
    {
        public int particleType;

        /// <summary>
        /// Resets the Particles variables to default values
        /// </summary>
        public override void Reset()
        {
            base.Reset();

            particleType = 0;
        }

        /// <summary>
        /// Deep copy the ParticleToCopy's values into this Particle
        /// </summary>
        /// <param name="ParticleToCopy">The Particle whose values should be Copied</param>
        public override void CopyFrom(DPSFParticle ParticleToCopy)
        {
            // Cast the Particle from its base type to its actual type
            EngineSpriteParticle cParticleToCopy = (EngineSpriteParticle)ParticleToCopy;
            base.CopyFrom(cParticleToCopy);

            //-----------------------------------------------------------
            // TODO: Copy your Particle properties from the given Particle here
            //-----------------------------------------------------------
            particleType = cParticleToCopy.particleType;
        }
    }

    //-----------------------------------------------------------
    // TODO: Rename/Refactor the Particle System class
    //-----------------------------------------------------------
    /// <summary>
    /// Create a new Particle System class that inherits from a
    /// Default DPSF Particle System
    /// </summary>
#if (WINDOWS)
    [Serializable]
#endif
    public class EngineParticleSystem : DefaultSprite3DBillboardParticleSystem
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cGame">Handle to the Game object being used. Pass in null for this 
        /// parameter if not using a Game object.</param>
        public EngineParticleSystem(Game cGame) : base(cGame) { }

        //===========================================================
        // Structures and Variables
        //===========================================================

        //-----------------------------------------------------------
        // TODO: Place any Particle System properties here
        //-----------------------------------------------------------
        float mfSizeMin = 10;
        float mfSizeMax = 50;
        int layer=0;
        protected Color[] defaultColor = { Color.Red, Color.Orange, Color.Gold, Color.DarkBlue, Color.White };//Color.AntiqueWhite;
        protected Color[] defaultEnd = {Color.Yellow,Color.Red,Color.Orange,Color.OrangeRed,Color.Blue};
        protected Color[] boostColor = { Color.Lime, Color.DeepSkyBlue, Color.Lime, Color.Crimson, Color.Black};
        protected Color[] boostFadeEnd = { Color.AntiqueWhite, Color.LightSeaGreen, Color.Blue, Color.AntiqueWhite, Color.Red};
        private float lifeTime;
        private float maxLifeTime = 0.6f;
        private const int particleRate = 300;
        protected Vector3 offSet;
        protected Vector3 velocity;
        int type;
        //===========================================================
        // Overridden Particle System Functions
        //===========================================================

        //-----------------------------------------------------------
        // TODO: Place any overridden Particle System functions here
        //-----------------------------------------------------------

        //===========================================================
        // Initialization Functions
        //===========================================================

        /// <summary>
        /// Function to Initialize the Particle System with default values.
        /// Particle system properties should not be set until after this is called, as 
        /// they are likely to be reset to their default values.
        /// </summary>
        /// <param name="cGraphicsDevice">The Graphics Device the Particle System should use</param>
        /// <param name="cContentManager">The Content Manager the Particle System should use to load resources</param>
        /// <param name="cSpriteBatch">The Sprite Batch that the Sprite Particle System should use to draw its particles.
        /// If this is not initializing a Sprite particle system, or you want the particle system to use its own Sprite Batch,
        /// pass in null.</param>
        public override void AutoInitialize(GraphicsDevice cGraphicsDevice, ContentManager cContentManager, SpriteBatch cSpriteBatch)
        {
            //-----------------------------------------------------------
            // TODO: Change any Initialization parameters desired and the Name
            //-----------------------------------------------------------
            // Initialize the Particle System before doing anything else
            InitializeSpriteParticleSystem(cGraphicsDevice, cContentManager, particleRate * 2, particleRate * 100, "Particles/Textures/Particle004");

            // Set the Name of the Particle System
            Name = "EngineParticleSystem";

            // Finish loading the Particle System in a separate function call, so if
            // we want to reset the Particle System later we don't need to completely 
            // re-initialize it, we can just call this function to reset it.
            LoadParticleSystem();
        }

        /// <summary>
        /// Load the Particle System Events and any other settings
        /// </summary>
        public void LoadParticleSystem()
        {
            //-----------------------------------------------------------
            // TODO: Setup the Particle System to achieve the desired result.
            // You may change all of the code in this function. It is just
            // provided to show you how to setup a simple particle system.
            //-----------------------------------------------------------

            // Set the Function to use to Initialize new Particles.
            // The Default Templates include a Particle Initialization Function called
            // InitializeParticleUsingInitialProperties, which initializes new Particles
            // according to the settings in the InitialProperties object (see further below).
            // You can also create your own Particle Initialization Functions as well, as shown with
            // the InitializeParticleProperties function below.
            //ParticleInitializationFunction = InitializeParticleUsingInitialProperties;
            ParticleInitializationFunction = InitializeParticleProperties;

            // Setup the Initial Properties of the Particles.
            // These are only applied if using InitializeParticleUsingInitialProperties 
            // as the Particle Initialization Function.
            InitialProperties.LifetimeMin = 0.04f;
            InitialProperties.LifetimeMax = 0.9f;
            InitialProperties.PositionMin = Vector3.Zero;
            InitialProperties.PositionMax = Vector3.Zero;
            InitialProperties.VelocityMin = new Vector3(-0.05f, -0.05f, 0);
            InitialProperties.VelocityMax = new Vector3(0.05f, 0.05f, 1);
            InitialProperties.RotationMin = 0.0f;
            InitialProperties.RotationMax = 0.0f;
            InitialProperties.RotationalVelocityMin = 0.0f;
            InitialProperties.RotationalVelocityMax = 0.0f;
            InitialProperties.StartWidthMin = 1;
            InitialProperties.StartWidthMax = 1;
            InitialProperties.StartHeightMin = 1;
            InitialProperties.StartHeightMax = 1;
            InitialProperties.EndWidthMin = 1;
            InitialProperties.EndWidthMax = 1;
            InitialProperties.EndHeightMin = 1;
            InitialProperties.EndHeightMax = 1;
            InitialProperties.StartColorMin = defaultColor[layer];
            InitialProperties.StartColorMax = defaultColor[layer];
            InitialProperties.EndColorMin = defaultEnd[layer];
            InitialProperties.EndColorMax = defaultEnd[layer];

            // Remove all Events first so that none are added twice if this function is called again
            ParticleEvents.RemoveAllEvents();
            ParticleSystemEvents.RemoveAllEvents();

            // Allow the Particle's Position, Rotation, Width and Height, Color, and Transparency to be updated each frame
            ParticleEvents.AddEveryTimeEvent(UpdateParticlePositionUsingVelocity);
            //ParticleEvents.AddEveryTimeEvent(UpdateParticleRotationUsingRotationalVelocity);
            //ParticleEvents.AddEveryTimeEvent(UpdateParticleWidthAndHeightUsingLerp);
            ParticleEvents.AddEveryTimeEvent(UpdateParticleColorUsingLerp);

            // This function must be executed after the Color Lerp function as the Color Lerp will overwrite the Color's
            // Transparency value, so we give this function an Execution Order of 100 to make sure it is executed last.
            ParticleEvents.AddEveryTimeEvent(UpdateParticleTransparencyToFadeOutUsingLerp, 100);

            // Set the Particle System's Emitter to toggle on and off every 0.5 seconds
            //ParticleSystemEvents.LifetimeData.EndOfLifeOption = CParticleSystemEvents.EParticleSystemEndOfLifeOptions.Repeat;
            //ParticleSystemEvents.LifetimeData.Lifetime = 1.0f;
            //ParticleSystemEvents.AddTimedEvent(0.0f, UpdateParticleSystemEmitParticlesAutomaticallyOn);
            //ParticleSystemEvents.AddTimedEvent(0.5f, UpdateParticleSystemEmitParticlesAutomaticallyOff);

            // Setup the Emitter
            Emitter.ParticlesPerSecond = particleRate;
            Emitter.PositionData.Position = new Vector3(0, 0, 0);
        }

        /// <summary>
        /// Example of how to create a Particle Initialization Function
        /// </summary>
        /// <param name="cParticle">The Particle to be Initialized</param>
        public void InitializeParticleProperties(DefaultSprite3DBillboardParticle cParticle)
        {
            Quaternion cBackup = Emitter.OrientationData.Orientation;
            Emitter.OrientationData.Orientation = Quaternion.Identity;
            InitializeParticleUsingInitialProperties(cParticle);
            Emitter.OrientationData.Orientation = cBackup;

            cParticle.Position =offSet;
            cParticle.Position = Vector3.Transform(cParticle.Position, Emitter.OrientationData.Orientation);
            cParticle.Position += Emitter.PositionData.Position;

            cParticle.Velocity = Vector3.Transform(cParticle.Velocity, Emitter.OrientationData.Orientation);
            switch (type)
            {
                case 0:
                    cParticle.StartColor = defaultColor[layer];
                    cParticle.EndColor = defaultEnd[layer];
                    break;
                case 1:
                    cParticle.StartColor = boostColor[layer];
                    cParticle.EndColor = boostFadeEnd[layer];
                    break;
                default:
                    cParticle.StartColor = defaultColor[layer];
                    cParticle.EndColor = defaultEnd[layer];
                    break;
            }
        }

        //===========================================================
        // Particle Update Functions
        //===========================================================

        //-----------------------------------------------------------
        // TODO: Place your Particle Update functions here, using the 
        // same function prototype as below (i.e. public void FunctionName(DPSFParticle, float))
        //-----------------------------------------------------------

        /// <summary>
        /// Example of how to create a Particle Event Function
        /// </summary>
        /// <param name="cParticle">The Particle to update</param>
        /// <param name="fElapsedTimeInSeconds">How long it has been since the last update</param>
        protected void UpdateParticleFunctionExample(EngineSpriteParticle cParticle, float fElapsedTimeInSeconds)
        {
            // Place code to update the Particle here
            // Example: cParticle.Position += cParticle.Velocity * fElapsedTimeInSeconds;
        }

        //===========================================================
        // Particle System Update Functions
        //===========================================================

        //-----------------------------------------------------------
        // TODO: Place your Particle System Update functions here, using 
        // the same function prototype as below (i.e. public void FunctionName(float))
        //-----------------------------------------------------------

        /// <summary>
        /// Example of how to create a Particle System Event Function
        /// </summary>
        /// <param name="fElapsedTimeInSeconds">How long it has been since the last update</param>
        protected void UpdateParticleSystemFunctionExample(float fElapsedTimeInSeconds)
        {
            // Place code to update the Particle System here
            // Example: Emitter.EmitParticles = true;
            // Example: SetTexture("TextureAssetName");
        }

        //===========================================================
        // Other Particle System Functions
        //===========================================================

        //-----------------------------------------------------------
        // TODO: Place any other functions here
        //-----------------------------------------------------------

        public void setVelocity(Vector3 currentVelocity)
        {
            velocity = currentVelocity;
        }

        public void SetOffset(Vector3 newOffset)
        {
            offSet = newOffset;
        }



        public void SetPosition(Vector3 newPosition, Quaternion orientation)
        {
            Emitter.PositionData.Position = newPosition;
            Emitter.OrientationData.Orientation = orientation;
        }

        public void boostType(int newtype)
        {
            type = newtype;
        }

        internal void setLayer(int myLayer)
        {
            layer = myLayer;
        }
    }
}
