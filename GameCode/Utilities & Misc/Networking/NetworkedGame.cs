using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using System.Diagnostics;

namespace BeatShift
{
    /// <summary>
    /// Defines the type of session to start when startSession is called. 
    /// </summary>
    public enum SessionType { None, Create, Join }

    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class NetworkedGame : Microsoft.Xna.Framework.DrawableGameComponent
    {

        SessionType sessionType = SessionType.None;
        Boolean started = false;

        // A large amount of this has pretty much been lifted from the invite
        // example code, just to get it all working.
        const int maxGamers = 4;
        const int maxLocalGamers = 2;

        //int outCount = 0;

        NetworkSession networkSession = null;


        PacketWriter packetWriter = new PacketWriter();
        PacketReader packetReader = new PacketReader();

        string errorMessage = "";

        BeatShift mainGame;


        public NetworkedGame(BeatShift game)
            : base(game)
        {
            mainGame = game;

            // Add the game object to the components list
            // This seems to be done in every example, so I'm adding it here.
            // I have a feeling it allows us to use Live.
            //mainGame.Components.Add(new GamerServicesComponent(mainGame));
            //http://msdn.microsoft.com/en-us/library/bb975692.aspx


            // Listen for invite notification events. This handler will be called
            // whenever the user accepts an invite message, or if they select the
            // "Join Session In Progress" option from their Guide friends screen.
            NetworkSession.InviteAccepted += InviteAcceptedEventHandler;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            Console.Write("Initializing networking (GamerServicesDispatcher)... ");
            //This block of code needs to be uncommented somewhere
            //Unfortunately it is very slow (5s) on my PC and delays
            //Startup significantly.
            try
            {
                GamerServicesDispatcher.WindowHandle = mainGame.Window.Handle;
                if (!GamerServicesDispatcher.IsInitialized)
                    GamerServicesDispatcher.Initialize(mainGame.Services);
            }
            catch (Exception e)
            {
                //DO SOMTHING SENSIBLE HERE.
                //triggered if live services not running
                Debug.WriteLine("Unable to initialize GamerServicesDispatcher.");
            }
            Console.WriteLine("   ...done.");

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (networkSession == null)
            {
                // Not in a network session, so display menu.

                Race.Enabled = false;
                MapManager.Enabled = false;
                Physics.Enabled = false;
                UpdateLobbyScreen();
            }
            else
            {
                Race.Enabled = true;
                MapManager.Enabled = true;
                Physics.Enabled = true;

                UpdateNetworkSession(gameTime);
            }

            base.Update(gameTime);
        }

        void UpdateNetworkSession(GameTime gameTime)
        {
            GamerServicesDispatcher.Update();

            // TODO: Work out how to do this right...
            BroadcastLocalShips();

            networkSession.Update();

            if (networkSession == null)
            {
                // If here, then network session has ended.
                //Go back to join/new session menu
                //mainGame.setGameState(MainGame.GameState.Menu);
                return;
            }

            foreach (LocalNetworkGamer gamer in networkSession.LocalGamers)
            {
                ReadIncomingPackets(gamer);
            }

        }

        void BroadcastLocalShips()
        {
            foreach (LocalNetworkGamer gamer in networkSession.LocalGamers)
            {
                Racer racer = gamer.Tag as Racer;

                packetWriter.Write(racer.shipPhysics.ShipPosition);
                packetWriter.Write(racer.shipPhysics.ShipOrientationMatrix.M11);//TODO: almost certainly wrong
                packetWriter.Write(racer.shipPhysics.ShipSpeed);//Could be wrong

                gamer.SendData(packetWriter, SendDataOptions.InOrder);
            }
        }

        void returnToMenu()
        {
            if (networkSession != null) networkSession.Dispose();
            networkSession = null;
            started = false;
            sessionType = SessionType.None;
            GameLoop.setGameStateAndResetPlayers(GameState.Menu);
        }

        void UpdateLobbyScreen()
        {
            if (mainGame.IsActive)
            {
                if (Gamer.SignedInGamers.Count == 0)
                {
                    // If there are no profiles signed in and guide has been dismissed, return to menu.
                    if (!Guide.IsVisible)
                    {
                        returnToMenu();
                    }
                }
                else if (started == false)
                {
                    started = true;
                    if (sessionType == SessionType.Create)
                    {
                        // Create a new session?
                        CreateSession();
                    }
                    else if (sessionType == SessionType.Join)
                    {
                        // Join an existing session?
                        JoinSession();
                    }
                }
            }
        }

        /// <summary>
        /// Displays guide if no LIVE accounts logged in. Then either creates or joins a session.
        /// </summary>
        public void startSession(SessionType sessiontype)
        {
            sessionType = sessiontype;
            started = false;
            if (Gamer.SignedInGamers.Count == 0)
            {
                // If there are no profiles signed in, we cannot proceed.
                // Show the Guide so the user can sign in.
                Guide.ShowSignIn(maxLocalGamers, false);//shouldn't onlineOnly be true?
            }
        }

        /// <summary>
        /// Currently draws the lobby when needed and ensures scene(map and ships) is visible when in game.
        /// Should later have seperate lobby class, and only set scene visible when the game starts.
        /// </summary>
        /// <param name="gameTime">The game time to pass on.</param>
        public override void Draw(GameTime gameTime)
        {

            if (networkSession == null)
            {
                // Not in network session, draw menu
                GraphicsDevice.Clear(Color.LightSkyBlue);
                DrawLobbyScreen();

                MapManager.Visible = false;
                Race.Visible = false;
            }
            else
            {
                Race.Visible = true;
                MapManager.Visible = true;
            }

            base.Draw(gameTime);
        }

        void DrawLobbyScreen()
        {
            string message = string.Empty;

            if (!string.IsNullOrEmpty(errorMessage))
                message += "Error:\n" + errorMessage.Replace(". ", ".\n") + "\n\n";

            message += "Lobby Area";

            BeatShift.spriteBatch.Begin();

            BeatShift.spriteBatch.DrawString(BeatShift.font, message, new Vector2(161, 161), Color.Black);
            BeatShift.spriteBatch.DrawString(BeatShift.font, message, new Vector2(160, 160), Color.White);

            BeatShift.spriteBatch.End();

        }

        /// <summary>
        /// Helper draws notification messages before calling blocking network methods.
        /// </summary>
        void DrawMessage(string message)
        {

            GraphicsDevice.Clear(Color.CornflowerBlue);

            BeatShift.spriteBatch.Begin();

            BeatShift.spriteBatch.DrawString(BeatShift.font, message, new Vector2(161, 161), Color.Black);
            BeatShift.spriteBatch.DrawString(BeatShift.font, message, new Vector2(160, 160), Color.White);

            BeatShift.spriteBatch.End();
        }



        #region Networking

        /// <summary>
        /// Starts hosting a new network session.
        /// </summary>
        void CreateSession()
        {
            DrawMessage("Creating session...");

            try
            {
                networkSession = NetworkSession.Create(NetworkSessionType.PlayerMatch,
                                                       maxLocalGamers, maxGamers);

                HookSessionEvents();
            }
            catch (Exception error)
            {
                errorMessage = error.Message;
            }
        }

        /// <summary>
        /// Joins an existing network session.
        /// </summary>
        void JoinSession()
        {
            DrawMessage("Joining session...");

            try
            {
                // Search for sessions.
                using (AvailableNetworkSessionCollection availableSessions =
                            NetworkSession.Find(NetworkSessionType.PlayerMatch,
                                                maxLocalGamers, null))
                {
                    if (availableSessions.Count == 0)
                    {
                        errorMessage = "No network sessions found.";
                        return;
                    }

                    // Join the first session we found.
                    networkSession = NetworkSession.Join(availableSessions[0]);

                    HookSessionEvents();
                }
            }
            catch (Exception error)
            {
                errorMessage = error.Message;
            }
        }

        /// <summary>
        /// This event handler will be called whenever the game recieves an invite
        /// notification. This can occur when the user accepts an invite that was
        /// sent to them by a friend (pull mode), or if they choose the "Join
        /// Session In Progress" option in their friends screen (push mode).
        /// The handler should leave the current session (if any), then join the
        /// session referred to by the invite. It is not necessary to prompt the
        /// user before doing this, as the Guide will already have taken care of
        /// the necessary confirmations before the invite was delivered to you.
        /// </summary>
        void InviteAcceptedEventHandler(object sender, InviteAcceptedEventArgs e)
        {
            DrawMessage("Joining session from invite...");

            // Leave the current network session.
            if (networkSession != null)
            {
                networkSession.Dispose();
                networkSession = null;
            }

            try
            {
                // Join a new session in response to the invite.
                networkSession = NetworkSession.JoinInvited(maxLocalGamers);

                HookSessionEvents();
            }
            catch (Exception error)
            {
                errorMessage = error.Message;
            }
        }

        /// <summary>
        /// After creating or joining a network session, we must subscribe to
        /// some events so we will be notified when the session changes state.
        /// </summary>
        void HookSessionEvents()
        {
            networkSession.GamerJoined += GamerJoinedEventHandler;
            networkSession.SessionEnded += SessionEndedEventHandler;
        }

        /// <summary>
        /// Helper for reading incoming network packets.
        /// </summary>
        void ReadIncomingPackets(LocalNetworkGamer gamer)
        {
            // Keep reading as long as incoming packets are available.
            while (gamer.IsDataAvailable)
            {
                NetworkGamer sender;

                // Read a single packet from the network.
                gamer.ReceiveData(packetReader, out sender);

                // Discard packets sent by local gamers: we already know their state!
                if (sender.IsLocal)
                    continue;

                // Look up the ship associated with whoever sent this packet.
                Racer racer = sender.Tag as Racer;

                // Read the state of this ship from the network packet.
                try
                {
                    racer.shipPhysics.ShipPosition = packetReader.ReadVector3();
                    //tempShip.ShipYaw = packetReader.ReadSingle();
                    //tempShip.shipSpeed = packetReader.ReadSingle();//EndOfStreamException was unhandled.
                    //roll??
                }
                catch (EndOfStreamException e)
                {
                    //Do something here
                    returnToMenu();
                }

            }
        }

        /// <summary>
        /// This event handler will be called whenever a new gamer joins the session.
        /// We use it to allocate a Ship object, and associate it with the new gamer.
        /// </summary>
        void GamerJoinedEventHandler(object sender, GamerJoinedEventArgs e)
        {
            //int gamerIndex = networkSession.AllGamers.IndexOf(e.Gamer);
            RacerType pType = RacerType.Remote;
            if (e.Gamer.IsLocal) pType = RacerType.LocalHuman;
            //TODO: uncomment this code and make it work//e.Gamer.Tag = Race.addPlayer(pType);
        }


        /// <summary>
        /// Event handler notifies us when the network session has ended.
        /// </summary>
        void SessionEndedEventHandler(object sender, NetworkSessionEndedEventArgs e)
        {
            errorMessage = e.EndReason.ToString();//TODO:need to pass this error to menu

            returnToMenu();


        }

        #endregion


    }
}
