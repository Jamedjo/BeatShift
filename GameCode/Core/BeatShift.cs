using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;
using BEPUphysics;
using BeatShift.Menus;
using BeatShift.GameDebugTools;
using BeatShift.Util.Random;
using DPSF;

namespace BeatShift
{

    public delegate void VolumechangeHandler(EventArgs e);
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class BeatShift : Microsoft.Xna.Framework.Game
    {

        public readonly static String Title = "BeatShift";

        #region variables

        // Race related variables
        public static ShipSelect shipSelect;

        // General graphics related variables
        public static GraphicsDeviceManager graphics;
        public static ContentManager contentManager;
        
        // HUD graphics related variables
        public static SpriteBatch spriteBatch { get; protected set; } //should not be public?
        public static SpriteFont font { get; protected set; }
        public static SpriteFont newfont { get; protected set; }
        public static SpriteFont newfontgreen { get; protected set; }
        public static SpriteFont buttonsFont { get; protected set; }
        public static SpriteFont blueNumbersFont { get; protected set; }

        // Music related variables
        public static SoundTrack bgm;
        public static SoundManager sfx;
        public static AudioEngine engine;

        // Particle related variables
        public static BoostSettings settingsb;
        public static ParticleEmitter emitter;
        public static Effect pEffect;

        // General variables
        private static StorageDevice storage = null;
        public static StorageDevice Storage {get{ return storage; }}
        private static Boolean shouldExitGame = false;
        public static NetworkedGame networkedGame;
        public static BeatShift singleton;
        public static GamerServicesComponent gamerServices { get; private set; }
        public static ParticleSystemManager particleManager;
        public GameTime currentTime;

        #endregion

        #region Initialization

        /// <summary>
        /// Construct game object
        /// </summary>
        public BeatShift()
        {
            singleton = this;
            graphics = new GraphicsDeviceManager(this);
            //graphics.PreferredBackBufferWidth = 1280;
            //graphics.PreferredBackBufferHeight = 800;
            // TODO: uncomment the above at your peril, performance profiling needed
            particleManager = new ParticleSystemManager();
            Content.RootDirectory = "Content";
            contentManager = Content;

            //Turn on antialiasing
            graphics.PreferMultiSampling = true;
            //Components.Add(new GamerServicesComponent(this));
            //TODO: xbox-live
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //set up content
            Content.RootDirectory = "Content";

            // Now with static constuctor so auto initalized on first access.
            //Viewports.setupViewports();

            SimpleRNG.SetSeedFromSystemTime();

            DebugSystem.Initialize(this, "FontA");
            DebugSystem.Instance.FpsCounter.Visible = true;
            DebugSystem.Instance.TimeRuler.Visible = true;

            MenuManager.Initialize();

           // MapManager.Initialize();

            shipSelect = new ShipSelect();

            networkedGame = new NetworkedGame(this);
            networkedGame.Enabled = false;
            networkedGame.Visible = false;
            networkedGame.Initialize();

            Physics.Initialize();

            GameLoop.setGameStateAndResetPlayers(GameState.Menu);
            engine = new AudioEngine("Content/XACT/BeatShift.xgs");
            bgm = new SoundTrack(140);
            bgm.LoadContent(Content,"bgm2");
            sfx = new SoundManager();
            sfx.LoadContent(Content);
   
            Texture2D map = Content.Load<Texture2D>("Particles/defcol");
            Texture2D paticle = Content.Load<Texture2D>("Particles/try");
            pEffect = Content.Load<Effect>("Particles/Particle");
            settingsb = new BoostSettings(map, paticle);

            base.Initialize();

            gamerServices = new GamerServicesComponent(this);
            Components.Add(gamerServices);


            //gamerServices.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {

            // Guarantees that the storage device will be not be null before continuing
            //while(Storage == null)
            //{
            Console.Write("Getting Storage...");
            // To avoid GuideAlreadyVisibleException
            while(Guide.IsVisible) { }
            StorageDevice.BeginShowSelector(getStorage, "Get initial StorageDevice");
            Console.Write("   ..."+Storage+"... ");
            Console.Write("Call Started...");
            //}

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            GameTextures.load(Content);
            //MapManager.tempMap.LoadContent(Content);
            font = Content.Load<SpriteFont>("FontA");
            newfont = Content.Load<SpriteFont>("fontfile");
            newfontgreen = Content.Load<SpriteFont>("fontfile-green");
            blueNumbersFont = Content.Load<SpriteFont>("bluenumbersfont");
            buttonsFont = Content.Load<SpriteFont>("xboxControllerSpriteFont");
            ButtonDraw.initialize(buttonsFont);
        }

        private void getStorage(IAsyncResult result)
        {
            Console.Write("...Call complete...");
            storage = StorageDevice.EndShowSelector(result);
            if(storage != null)
                Options.Initialize(Storage);
            else
                StorageDevice.BeginShowSelector(getStorage, "Recursive get StorageDevice");
            Console.WriteLine("...storage methods completed.");
        }


        #endregion

        #region Finalize

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        #endregion

        #region Update

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            DebugSystem.Instance.TimeRuler.StartFrame();
            DebugSystem.Instance.TimeRuler.BeginMark("SomeCode", Color.Blue);
            currentTime = gameTime;
            GameLoop.Update(gameTime);
            engine.Update();
            base.Update(gameTime);

            DebugSystem.Instance.TimeRuler.EndMark("SomeCode");

            particleManager.UpdateAllParticleSystems((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        #endregion

        #region Draw

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GameLoop.Draw(gameTime);
            base.Draw(gameTime);
        }

        #endregion

        #region Methods

        #endregion
    }
}