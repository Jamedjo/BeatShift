using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BeatShift.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using DPSF;
namespace BeatShift
{
    public class Racer
    {
        public RacingControls racingControls { get; private set; }
        //public IInputManager {set{racingControls.inputManager=value might be called chosenInput};}
        public Color shipColour { get { return shipDrawing.shipColour; } private set {shipDrawing.shipColour=value;} }
        public ShipName shipName { get { return shipDrawing.currentShip; } set { shipDrawing.currentShip = value; } }
        public ShipPhysics shipPhysics { get; private set; }
        public ShipDrawing shipDrawing { get; private set; }
        public RaceTiming raceTiming { get; private set; }
        public RacerId racerID  { get;  set; }
        public RacerType racerType { get; private set; }
        public BeatQueue beatQueue;
        public RacerPoints racerPoints { get; private set; }
        public SlidingPopup messagePopupManager = new SlidingPopup(new Vector2(0, -100));
        public Cue Hum;
        private bool isBoosting = false;
        public ParticleSystemManager globalSystems;
        public ParticleSystemManager privateSystems;
        // General game related variables
        const float updatePeriod = 50; //update movement 20 times a second (1000/50=20)
        float lastUpdatedTimer = 0;
        

        /// <summary>
        /// Specifies whether the ship is currently being held, for example after being reset onto the track. This stops control input,
        /// but not physics updates, so the ship will settle and can be collided with.
        /// </summary>
        public Boolean isRespawning { get; set; }

        public Vector3 lastCollisionPoint;

        private Boolean mColliding;



        public Boolean isColliding
        {
            get
            {
                return mColliding;
            }
            set
            {
                if (value)
                {
                    //shipDrawing.collision.Collision(Vector3.Transform(lastCollisionPoint, Matrix.Invert(shipDrawing.getShipDrawOrientationMatrix())));
                    shipDrawing.collision.Collision(lastCollisionPoint,shipPhysics.DrawOrientation,shipPhysics.physicsBody.LinearVelocity);
                }
                mColliding = value;
                //return isColliding;   
            }
        }

        /// <summary>
        /// The point in GameTime when the ship is reactivated after resetting.
        /// </summary>
        public TimeSpan respawnTime; //TODO: use this properly

        //The number of the ship starting from 0. Used to set position on starting grid and seed random variables.
        public int shipNumber;

        public Racer(RacerId rID, int ship_Number, RacerType racer_Type)
        {
            shipNumber = ship_Number;
            racerType = racer_Type;
            racerID = rID;
            raceTiming=new RaceTiming(this);
            shipDrawing = new ShipDrawing(new Func<Matrix>(() => Matrix.Identity), new Func<Vector3>(() => Vector3.Zero), this);
            beatQueue = new BeatQueue(this);
            racerPoints = new RacerPoints();
            privateSystems = new ParticleSystemManager();
            globalSystems = new ParticleSystemManager();
            //setColour(1);//Set to red
            //Hum = SoundManager.getEngineHum();
            //if (this.GetType() == typeof(RacerHuman))
            //{
            //    Hum.Play();
            //}
            constructRaceVariables();

            if (racerType == RacerType.AI)
            {
                constructRandomShip(shipNumber);
            }

            //Setup effect to render the ConvexHull of physicsBody with transparency
            //SetupHullRenderer();
        }

        public void Load()
        {
            if( racerType != RacerType.AI )
                beatQueue.Load();
            shipDrawing.LoadParticles();
        }

        public virtual void insertShipOnMap(RacerType newRacerType)
        {
            racerType = newRacerType;
            shipPhysics = new ShipPhysics(this);
            shipDrawing.setPositionFunctions(new Func<Matrix>(() => shipPhysics.DrawOrientationMatrix), new Func<Vector3>(() => shipPhysics.ShipPosition));
            setupRacingControls();
        }

        private void constructRaceVariables()
        {
            // Racing related variables
            raceTiming = new RaceTiming(this);
            isRespawning = false;
        }

        public void setupRacingControls(IInputManager inputM)
        {
            // AI control tests
            if (AiInputManager.testAI)
            {
                racingControls = new RacingControls(this, new AiInputManager(this));
            }
            else
            {
                racingControls = new RacingControls(this, inputM);
            }
            //racingControls = new RacingControls(this, new AiInputManager(this));
        }
        private void setupRacingControls()
        {
            if (racingControls != null) return;
            //Controller related variables
            if ((racerType == RacerType.LocalHuman) & !AiInputManager.testAI)
                racingControls = new RacingControls(this);//sets input to game pad 1 use (this,game,input) to set input
            else if ((racerType == RacerType.AI) || AiInputManager.testAI)
                racingControls = new RacingControls(this, new AiInputManager(this));
            else
                racingControls = new RacingControls(this, new NullInputManager());
        }

        /// <summary>
        /// Sets the colour of the ship.
        /// </summary>
        /// <param name="hue">The new hue to use</param>
        public void setColour(int hue)
        {
            double val = 0.55;
            double sat = 0.95;
            shipColour = ColorConvert.getRGB(hue, sat, val);
        }

        public void constructRandomShip(int seed)
        {
            //Set random ship and hue
            setColour((new Random(seed * 11 + 7)).Next(256));//set hue to random hue between 0 and 255
            shipName = Utils.GetValues<ShipName>().ToArray()[(new Random(seed * 7 + 17)).Next(Utils.GetValues<ShipName>().Count())];
        }

        public void Update(GameTime gameTime)
        {
            beatQueue.Update();
            //clear debug arrow list
            shipDrawing.drawArrowListRays.Clear();
           /* float temp;
            if (shipPhysics.ShipSpeed > 300)
            {
                temp = 5;
            }
            else
            {
                temp = shipPhysics.ShipSpeed * 5 / 300;
            }*/
            //Hum.SetVariable("EngineSpeed",temp);
            //if playerType==PlayerType.Remote then return.

            //Only update ? times per second- based on updatePeriod.
            lastUpdatedTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (lastUpdatedTimer >= updatePeriod)
            {
                lastUpdatedTimer -= updatePeriod;

                //Update code here (steady 20fps)
                //Adjust speed from accellerators/breaks
                if (racerType == RacerType.LocalHuman)
                {
                    racingControls.Update(gameTime);
                }
                else if (racerType == RacerType.AI)
                {
                    racingControls.Update(gameTime);
                    // Disable pushing the AI round the track while doing AI stuff.
                    //shipPhysics.UpdateFromAI();
                }

            }

            shipPhysics.couteractDrift();

            if ((racerType == RacerType.AI) || (racerType == RacerType.LocalHuman))
            {
                shipPhysics.checkIfOverturned(gameTime);              

                shipPhysics.UpdateWithRayCasts(gameTime);

            }

            shipPhysics.counteractPitching();

            // Update the way points and therefore the current lap
            shipPhysics.UpdateWaypoints(); //if (shipPhysics != null) shipPhysics.UpdateWaypoints();

            // Finishes the racer if required
            if (raceTiming.isRacing) raceTiming.Update();
            if (shipDrawing.engineGlow != null)
            {
                shipDrawing.engineGlow.setVelocity(shipPhysics.physicsBody.LinearVelocity);
                shipDrawing.engineGlow.SetPosition(shipPhysics.ShipPosition, shipPhysics.DrawOrientation);
                shipDrawing.spawn.setPosition(shipPhysics.ShipPosition, shipPhysics.DrawOrientation);
            }
            globalSystems.UpdateAllParticleSystems((float)gameTime.ElapsedGameTime.TotalSeconds);
            privateSystems.UpdateAllParticleSystems((float)gameTime.ElapsedGameTime.TotalSeconds);

            int lvl = (beatQueue == null) ? 0 : beatQueue.getLayer();
            racerPoints.Update(gameTime, isBoosting, lvl);
            racerPoints.pointsPopupManager.Update(gameTime);
            messagePopupManager.Update(gameTime);

            OtherUpdate(gameTime);
        }

        public virtual void OtherUpdate(GameTime gameTime)
        {
            //Additional update code for child classes
        }

        public virtual void setBoost(Boolean boosting) {
            isBoosting = boosting;
        }

    }



    /// <summary>
    /// Defines how the racer is conrolled, locally via input, locally via AI, or remotely.
    /// If the ship is not being controlled (e.g. it is shown in a menu) it can be set to 'None'.
    /// LocalHuman can be pad/keyboard/co-op, remote might also be a remote AI.
    /// </summary>
    public enum RacerType { LocalHuman, Remote, AI, None }

    public enum ShipName { Skylar, Omicron, Wraith, Flux }
}