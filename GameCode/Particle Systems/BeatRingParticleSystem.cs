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
using Microsoft.Xna.Framework.Input;
#endregion

namespace DPSF.ParticleSystems
{
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
    public class BeatRingParticleSystem : DefaultSprite3DBillboardParticleSystem
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cGame">Handle to the Game object being used. Pass in null for this 
        /// parameter if not using a Game object.</param>
        public BeatRingParticleSystem(Game cGame) : base(cGame) { }

        //===========================================================
        // Structures and Variables
        //===========================================================

        //-----------------------------------------------------------
        // TODO: Place any Particle System properties here
        //-----------------------------------------------------------
        float mfSizeMin = 10;
        float mfSizeMax = 50;
        float normLifetime = 2.0f;
        float overrun = 0.2f;
        float extradist=0;
        float notedist = 12.0f;
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
            InitializeSpriteParticleSystem(cGraphicsDevice, cContentManager, 20, 100,
                                                 "Particles/textures/ring006");

            // Set the Name of the Particle System
            Name = "Beat Ring Particle System";

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
            ParticleInitializationFunction = InitialiseParticleProperties;
            //ParticleInitializationFunction = InitializeParticleProperties;

            // Setup the Initial Properties of the Particles.
            // These are only applied if using InitializeParticleUsingInitialProperties 
            // as the Particle Initialization Function.
            InitialProperties.LifetimeMin = normLifetime+overrun;
            InitialProperties.LifetimeMax = normLifetime + overrun;
            InitialProperties.PositionMin = Vector3.Zero;
            InitialProperties.PositionMax = Vector3.Zero;
            InitialProperties.VelocityMin = new Vector3(0, 0, 10);
            InitialProperties.VelocityMax = new Vector3(0, 0, 10);
            InitialProperties.RotationMin = 0;
            InitialProperties.RotationMax = 0;
            InitialProperties.RotationalVelocityMin = 0;
            InitialProperties.RotationalVelocityMax = 0;
            InitialProperties.StartWidthMin = 2;
            InitialProperties.StartWidthMax = 2;
            InitialProperties.StartHeightMin = 2;
            InitialProperties.StartHeightMax = 2;
            InitialProperties.EndWidthMin = 2;
            InitialProperties.EndWidthMax = 2;
            InitialProperties.EndHeightMin = 2;
            InitialProperties.EndHeightMax = 2;
            InitialProperties.StartColorMin = Color.Green;
            InitialProperties.StartColorMax = Color.Green;

            // Remove all Events first so that none are added twice if this function is called again
            ParticleEvents.RemoveAllEvents();
            ParticleSystemEvents.RemoveAllEvents();

            // Allow the Particle's Position, Rotation, Width and Height, Color, and Transparency to be updated each frame
            ParticleEvents.AddEveryTimeEvent(LerpParticleDistance);
            ParticleEvents.AddEveryTimeEvent(UpdateParticleRotationUsingRotationalVelocity);
            //ParticleEvents.AddEveryTimeEvent(UpdateParticleWidthAndHeightUsingLerp);
            //ParticleEvents.AddEveryTimeEvent(UpdateParticleColorUsingLerp);

            // Setup the Emitter
            Emitter.ParticlesPerSecond = 50;
            Emitter.PositionData.Position = new Vector3(0, 5, -20f);
            Emitter.EmitParticlesAutomatically = false;
            ParticleInitializationFunction = InitialiseIndicatorParticle;
            AddParticle();
            ParticleInitializationFunction = InitialiseParticleProperties;
            extradist =notedist*overrun / (normLifetime + overrun);
        }

        /// <summary>
        /// Example of how to create a Particle Initialization Function
        /// </summary>
        /// <param name="cParticle">The Particle to be Initialized</param>
        /// 
        public void InitialiseIndicatorParticle(DefaultSpriteParticle cParticle)
        {
            InitializeParticleUsingInitialProperties(cParticle);
            cParticle.Lifetime = 0.0f;
            cParticle.RotationalVelocity = 5f;
            cParticle.Color = Color.White;
        }

        public void InitialiseParticleProperties(DefaultSpriteParticle cParticle)
        {
            //-----------------------------------------------------------
            // TODO: Initialize all of the Particle's properties here.
            // If you plan on simply using the default InitializeParticleUsingInitialProperties
            // Particle Initialization Function (see the LoadParticleSystem() function above), 
            // then you may delete this function all together.
            //-----------------------------------------------------------

            //Quaternion cBackup = Emitter.OrientationData.Orientation;
            //Emitter.OrientationData.Orientation = Quaternion.Identity;
            InitializeParticleUsingInitialProperties(cParticle);
            //Emitter.OrientationData.Orientation = cBackup;
            // Set the Particle's Lifetime (how long it should exist for)
            cParticle.Lifetime = normLifetime+overrun;

            // Set the Particle's initial Position to be wherever the Emitter is
            cParticle.Position = Vector3.Transform(cParticle.Position, Emitter.OrientationData.Orientation);
            //cParticle.Position += Emitter.PositionData.Position;
            
            // Set the Particle's Velocity
            //Vector3 sVelocityMin = new Vector3(-50, 50, -50);
            //Vector3 sVelocityMax = new Vector3(50, 100, 50);
            //cParticle.Velocity = DPSFHelper.RandomVectorBetweenTwoVectors(sVelocityMin, sVelocityMax);


            
            // Adjust the Particle's Velocity direction according to the Emitter's Orientation
            //cParticle.Velocity = Vector3.Transform(cParticle.Velocity, Emitter.OrientationData.Orientation);

            // Give the Particle a random Size
            // Since we have Size Lerp enabled we must also set the Start and End Size
            //cParticle.Width = cParticle.StartWidth = cParticle.EndWidth =
              //  cParticle.Height = cParticle.StartHeight = cParticle.EndHeight = RandomNumber.Next((int)mfSizeMin, (int)mfSizeMax);

            // Give the Particle a random Color
            // Since we have Color Lerp enabled we must also set the Start and End Color
            //cParticle.Color = cParticle.StartColor = cParticle.EndColor = DPSFHelper.RandomColor();
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
        protected void LerpParticleDistance(DefaultSpriteParticle cParticle, float fElapsedTimeInSeconds)
        {
            // Place code to update the Particle here
            // Example: cParticle.Position += cParticle.Velocity * fElapsedTimeInSeconds;
            if (cParticle.Lifetime != 0)
            {
                cParticle.Position.X = 0;
                cParticle.Position.Y = (3-extradist) + 12 * (1.0f - cParticle.NormalizedElapsedTime);
                cParticle.Position.Z = 0;
                cParticle.Position = Vector3.Transform(cParticle.Position, Emitter.OrientationData.Orientation);
                cParticle.Position += Emitter.PositionData.Position;
            }
            else
            {
                cParticle.Position.X = 0;
                cParticle.Position.Y = 3;
                cParticle.Position.Z = 0;
                cParticle.Position = Vector3.Transform(cParticle.Position, Emitter.OrientationData.Orientation);
                cParticle.Position += Emitter.PositionData.Position;
            }

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
        public void SetPosition(Vector3 position, Quaternion orientation)
        {
            Emitter.PositionData.Position = position;
            Emitter.OrientationData.Orientation = orientation;
        }
        
        public void Clear()
        {
            this.RemoveAllParticles();
            ParticleInitializationFunction = InitialiseIndicatorParticle;
            AddParticle();
            ParticleInitializationFunction = InitialiseParticleProperties;
        }

        public void RemoveRecent()
        {
            
            DefaultSprite3DBillboardParticle cparticle = ActiveParticles.Last.Value;
            ActiveParticles.RemoveLast();
            ActiveParticles.RemoveLast();
            ActiveParticles.AddLast(cparticle);
        }


        public void addBeat(Buttons noteType, float duration, float elapsedAmount)
        {            
            //lastElapsed = elapsedAmount;
            switch (noteType)
            {

                case Buttons.A:
                    InitialProperties.StartColorMin = Color.Lime;
                    InitialProperties.StartColorMax = Color.Lime;
                    InitialProperties.RotationMin = 5 * MathHelper.PiOver4;
                    InitialProperties.RotationMax = 5 * MathHelper.PiOver4;
                    break;
                case Buttons.B:
                    InitialProperties.StartColorMin = Color.Red;
                    InitialProperties.StartColorMax = Color.Red;
                    InitialProperties.RotationMin = 7 * MathHelper.PiOver4;
                    InitialProperties.RotationMax = 7 * MathHelper.PiOver4;
                    break;
                case Buttons.X:
                    InitialProperties.StartColorMin = Color.DodgerBlue;
                    InitialProperties.StartColorMax = Color.DodgerBlue;
                    InitialProperties.RotationMin = 3 * MathHelper.PiOver4;
                    InitialProperties.RotationMax = 3 * MathHelper.PiOver4;
                    break;
                case Buttons.Y:
                    InitialProperties.StartColorMin = Color.Yellow;
                    InitialProperties.StartColorMax = Color.Yellow;
                    InitialProperties.RotationMin = MathHelper.PiOver4;
                    InitialProperties.RotationMax = MathHelper.PiOver4;
                    break;
                default:
                    InitialProperties.StartColorMin = Color.Purple;
                    InitialProperties.StartColorMax = Color.Purple;
                    break;
            }
            AddParticles(1);
        }
    }
}
