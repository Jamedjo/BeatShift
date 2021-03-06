﻿#region File Description
//-----------------------------------------------------------------------------
// DebugCommandUI.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using BeatShift.Input;

#endregion

namespace BeatShift.GameDebugTools
{
    /// <summary>
    /// Command Window class for Debug purpose.
    /// </summary>
    /// <remarks>
    /// Debug command UI that runs in the Game.
    /// You can type commands using the keyboard, even on the Xbox
    /// just connect a USB keyboard to it
    /// This works on all 3 platforms (Xbox, Windows, Phone)
    /// 
    /// How to Use:
    /// 1) Add this component to the game.
    /// 2) Register command by RegisterCommand method.
    /// 3) Open/Close Debug window by Tab key.
    /// </remarks>
    public class DebugCommandUI : DrawableGameComponent, IDebugCommandHost
    {
        #region Constants

        /// <summary>
        /// Maximum lines that shows in Debug command window.
        /// </summary>
        const int MaxLineCount = 20;

        /// <summary>
        /// Maximum command history number.
        /// </summary>
        const int MaxCommandHistory = 32;

        /// <summary>
        /// Cursor character.
        /// </summary>
        const string Cursor = "_";

        /// <summary>
        /// Default Prompt string.
        /// </summary>
        public const string DefaultPrompt = "CMD>";

        #endregion

        #region Properties

        /// <summary>
        /// Gets/Sets Prompt string.
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// Is it waiting for key inputs?
        /// </summary>
        public bool Focused { get { return state != State.Closed; } }

        #endregion

        #region Fields

        // Command window states.
        enum State
        {
            Closed,
            Opening,
            Opened,
            Closing
        }

        /// <summary>
        /// CommandInfo class that contains information to run the command.
        /// </summary>
        class CommandInfo
        {
            public CommandInfo(
                string command, string description, DebugCommandExecute callback)
            {
                this.command = command;
                this.description = description;
                this.callback = callback;
            }

            // command name
            public string command;

            // Description of command.
            public string description;

            // delegate for execute the command.
            public DebugCommandExecute callback;
        }

        // Reference to DebugManager.
        private DebugManager debugManager;

        // Current state
        private State state = State.Closed;

        // timer for state transition.
        private float stateTransition;

        // Registered echo listeners.
        List<IDebugEchoListner> listenrs = new List<IDebugEchoListner>();

        // Registered command executioner.
        Stack<IDebugCommandExecutioner> executioners = new Stack<IDebugCommandExecutioner>();

        // Registered commands
        private Dictionary<string, CommandInfo> commandTable =
                                                new Dictionary<string, CommandInfo>();

        // Current command line string and cursor position.
        private string commandLine = String.Empty;
        private int cursorIndex = 0;

        private Queue<string> lines = new Queue<string>();

        // Command history buffer.
        private List<string> commandHistory = new List<string>();

        // Selecting command history index.
        private int commandHistoryIndex;

        #region variables for keyboard input handling.

        // Previous frame keyboard state.
        private KeyboardState prevKeyState;

        // Key that pressed last frame.
        private Keys pressedKey;

        // Timer for key repeating.
        private float keyRepeatTimer;

        // Key repeat duration in seconds for the first key press.
        private float keyRepeatStartDuration = 0.3f;

        // Key repeat duration in seconds after the first key press.
        private float keyRepeatDuration = 0.03f;

        #endregion

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        public DebugCommandUI(Game game)
            : base(game)
        {
            Prompt = DefaultPrompt;

            // Add this instance as a service.
            Game.Services.AddService(typeof(IDebugCommandHost), this);

            // Draw the command UI on top of everything
            DrawOrder = int.MaxValue;

            // Adding default commands.

            // Help command displays registered command information.
            RegisterCommand("help", "Show Command helps",
            delegate(IDebugCommandHost host, string command, IList<string> args)
            {
                int maxLen = 0;
                foreach (CommandInfo cmd in commandTable.Values)
                    maxLen = Math.Max(maxLen, cmd.command.Length);

                string fmt = String.Format("{{0,-{0}}}    {{1}}", maxLen);

                foreach (CommandInfo cmd in commandTable.Values)
                {
                    Echo(String.Format(fmt, cmd.command, cmd.description));
                }
            });

            // Clear screen command
            RegisterCommand("cls", "Clear Screen",
            delegate(IDebugCommandHost host, string command, IList<string> args)
            {
                lines.Clear();
            });

            //// Echo command
            //RegisterCommand("echo", "Display Messages",
            //delegate(IDebugCommandHost host, string command, IList<string> args)
            //{
            //    Echo(command.Substring(5));
            //});

            // Command to toggle trackNormals
            RegisterCommand("arrows", "Toggle drawing of track normal/tangent arrows",
            delegate(IDebugCommandHost host, string command, IList<string> args)
            {
                Options.DrawTrackNormals = !Options.DrawTrackNormals;
            });

            // Command to toggle drawing of waypoint spheres
            RegisterCommand("waypoints", "Toggle drawing of waypoints",
            delegate(IDebugCommandHost host, string command, IList<string> args)
            {
                Options.DrawWaypoints = !Options.DrawWaypoints;
            });

            // Command to toggle if racers are updated with a parellel ForEach, or serially
            RegisterCommand("parallel", "Toggle racer update between parallel and serial",
            delegate(IDebugCommandHost host, string command, IList<string> args)
            {
                Globals.UpdateRaceWithParallel = !Globals.UpdateRaceWithParallel;
                if (Globals.UpdateRaceWithParallel)
                    Echo("   update set to parallel");
                else
                    Echo("   update set to serial");
            });

            // Command to toggle drawing of map scenery
            RegisterCommand("scenery", "Toggle drawing of map scenery",
            delegate(IDebugCommandHost host, string command, IList<string> args)
            {
                Globals.DisplayScenery = !Globals.DisplayScenery;
            });

            // Command to toggle drawing of skybox
            RegisterCommand("skybox", "Toggle drawing of map scenery",
            delegate(IDebugCommandHost host, string command, IList<string> args)
            {
                Globals.DisplaySkybox = !Globals.DisplaySkybox;
            });

            // Command to toggle drawing of HUD
            RegisterCommand("hud", "Toggle drawing of HUD",
            delegate(IDebugCommandHost host, string command, IList<string> args)
            {
                Globals.DisplayHUD = !Globals.DisplayHUD;
            });

            // Command to toggle screen post processing
            RegisterCommand("post", "Toggle postprocessing effects (Bloom)",
            delegate(IDebugCommandHost host, string command, IList<string> args)
            {
                Globals.PostProcess = !Globals.PostProcess;
            });

            // Command to toggle forcing AI as the input controller.
            RegisterCommand("testai", "Toggle forcing AI as the input controller",
            delegate(IDebugCommandHost host, string command, IList<string> args)
            {
                AiInputManager.testAI = !AiInputManager.testAI;
                if (AiInputManager.testAI)
                    Echo("   input forced to AI");
                else
                    Echo("   input setup uses controllers");
            });

            // Command to toggle drawing and updating of particles.
            RegisterCommand("particles", "Toggle drawing and updating of particles",
            delegate(IDebugCommandHost host, string command, IList<string> args)
            {
                Globals.EnableParticles = !Globals.EnableParticles;
                if (Globals.EnableParticles)
                    Echo("   particles enabled");
                else
                    Echo("   particles disabled");
            });

            RegisterCommand("ambient", "Toggle shader ambient colour",
            delegate(IDebugCommandHost host, string command, IList<string> args)
            {
                Globals.useAmbient = !Globals.useAmbient;
                if (Globals.useAmbient)
                    Echo("   ambient enabled");
                else
                    Echo("   ambient disabled");
            });
            RegisterCommand("lambert", "Toggle shader lambert colour",
            delegate(IDebugCommandHost host, string command, IList<string> args)
            {
                Globals.useLambert = !Globals.useLambert;
                if (Globals.useLambert)
                    Echo("   lambert enabled");
                else
                    Echo("   lambert disabled");
            });
            RegisterCommand("specular", "Toggle shader specular colour",
            delegate(IDebugCommandHost host, string command, IList<string> args)
            {
                Globals.useSpecular = !Globals.useSpecular;
                if (Globals.useSpecular)
                    Echo("   specular enabled");
                else
                    Echo("   specular disabled");
            });
            RegisterCommand("normals", "Get shader to draw normal values instead of texture",
            delegate(IDebugCommandHost host, string command, IList<string> args)
            {
                Globals.drawNormals = !Globals.drawNormals;
                if (Globals.drawNormals)
                    Echo("   drawNormals enabled");
                else
                    Echo("   drawNormals disabled");
            });

            // Command to toggle between test states to create live code changes.
            RegisterCommand("state", "Toggle between custom test states",
            delegate(IDebugCommandHost host, string command, IList<string> args)
            {
                Globals.nextState();
                Echo("Globals.TestState set to state "+Globals.TestState);
            });

            // Command to quickly exit game.
            RegisterCommand("exitgame", "Quickly exit game",
            delegate(IDebugCommandHost host, string command, IList<string> args)
            {
                BeatShift.singleton.Exit();
            });

            //'pos' command
            // Command to 
            RegisterCommand("pos", "set/show test Vector2 positions: 'pos help' for details.",
            delegate(IDebugCommandHost host, string command, IList<string> arguments)
            {
                if (String.Compare(arguments[0], "help", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    host.Echo("pos command allows testVectors to be dynamicaly set");
                    host.Echo("e.g. a GUI element could be set to position Globals.testVector1");
                    host.Echo("     the position is not saved but can be hard coded once finalized");
                    host.Echo("'pos show vectorNumber' to view coordinates of a test vector");
                    host.Echo("'pos vectorNumber x-coordinate y-coordinate' to set test vector");
                }
                else if (String.Compare(arguments[0], "show", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    //show オプションで現在座標を表示する
                    // "pos show testVectorNumber": option to display current coordinates
                    // e.g. "pos show 1"
                    int vectorNumber = Int32.Parse(arguments[1]);
                    Vector2 debugPos = Globals.getVector(vectorNumber);
                    host.Echo(String.Format("Pos={0},{1}", debugPos.X, debugPos.Y));
                }
                else
                {
                    // "pos x座標 y座標"と入力されたものを処理する
                    // Input processed as "pos testVectorNumber x-coordinates y-coordinates"
                    // e.g. "pos 1 59.4f 200f"
                    int vectorNumber = Int32.Parse(arguments[0]);
                    Globals.setVector(vectorNumber, Single.Parse(arguments[1]), Single.Parse(arguments[2]));
                    Vector2 debugPos = Globals.getVector(vectorNumber);
                    host.Echo(String.Format("Pos={0},{1}", debugPos.X, debugPos.Y));
                }
            });

            //Command to reset all debug settings and high-scores for release?


        }

        public bool isClosed()
        {
            if (state.Equals(State.Closed)) return true;
            return false;
        }

        /// <summary>
        /// Initialize component
        /// </summary>
        public override void Initialize()
        {
            debugManager =
                Game.Services.GetService(typeof(DebugManager)) as DebugManager;

            if (debugManager == null)
                throw new InvalidOperationException("Coudn't find DebugManager.");

            base.Initialize();
        }

        #endregion

        #region IDebugCommandHostinterface implemenration

        public void RegisterCommand(
            string command, string description, DebugCommandExecute callback)
        {
            string lowerCommand = command.ToLower();
            if (commandTable.ContainsKey(lowerCommand))
            {
                throw new InvalidOperationException(
                    String.Format("Command \"{0}\" is already registered.", command));
            }

            commandTable.Add(
                lowerCommand, new CommandInfo(command, description, callback));
        }

        public void UnregisterCommand(string command)
        {
            string lowerCommand = command.ToLower();
            if (!commandTable.ContainsKey(lowerCommand))
            {
                throw new InvalidOperationException(
                    String.Format("Command \"{0}\" is not registered.", command));
            }

            commandTable.Remove(command);
        }

        public void ExecuteCommand(string command)
        {
            // Call registered executioner.
            if (executioners.Count != 0)
            {
                executioners.Peek().ExecuteCommand(command);
                return;
            }

            // Run the command.
            char[] spaceChars = new char[] { ' ' };

            Echo(Prompt + command);

            command = command.TrimStart(spaceChars);

            List<string> args = new List<string>(command.Split(spaceChars));
            string cmdText = args[0];
            args.RemoveAt(0);

            CommandInfo cmd;
            if (commandTable.TryGetValue(cmdText.ToLower(), out cmd))
            {
                try
                {
                    // Call registered command delegate.
                    cmd.callback(this, command, args);
                }
                catch (Exception e)
                {
                    // Exception occurred while running command.
                    EchoError("Unhandled Exception occurred");

                    string[] lines = e.Message.Split(new char[] { '\n' });
                    foreach (string line in lines)
                        EchoError(line);
                }
            }
            else
            {
                Echo("Unknown Command");
            }

            // Add to command history.
            commandHistory.Add(command);
            while (commandHistory.Count > MaxCommandHistory)
                commandHistory.RemoveAt(0);

            commandHistoryIndex = commandHistory.Count;
        }

        public void RepeatLastCommand()
        {
            if (commandHistory.Count > 0)
            {
                ExecuteCommand(commandHistory[commandHistory.Count-1]);
            }
        }

        public void RegisterEchoListner(IDebugEchoListner listner)
        {
            listenrs.Add(listner);
        }

        public void UnregisterEchoListner(IDebugEchoListner listner)
        {
            listenrs.Remove(listner);
        }

        public void Echo(DebugCommandMessage messageType, string text)
        {
            lines.Enqueue(text);
            while (lines.Count >= MaxLineCount)
                lines.Dequeue();

            // Call registered listeners.
            foreach (IDebugEchoListner listner in listenrs)
                listner.Echo(messageType, text);
        }

        public void Echo(string text)
        {
            Echo(DebugCommandMessage.Standard, text);
        }

        public void EchoWarning(string text)
        {
            Echo(DebugCommandMessage.Warning, text);
        }

        public void EchoError(string text)
        {
            Echo(DebugCommandMessage.Error, text);
        }

        public void PushExecutioner(IDebugCommandExecutioner executioner)
        {
            executioners.Push(executioner);
        }

        public void PopExecutioner()
        {
            executioners.Pop();
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Show Debug Command window.
        /// </summary>
        public void Show()
        {
            if (state == State.Closed)
            {
                stateTransition = 0.0f;
                state = State.Opening;
            }
        }

        /// <summary>
        /// Hide Debug Command window.
        /// </summary>
        public void Hide()
        {
            if (state == State.Opened)
            {
                stateTransition = 1.0f;
                state = State.Closing;
            }
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            const float OpenSpeed = 8.0f;
            const float CloseSpeed = 8.0f;

            switch (state)
            {
                case State.Closed:
                    if (keyState.IsKeyDown(Keys.Tab))
                        Show();
                    break;
                case State.Opening:
                    stateTransition += dt * OpenSpeed;
                    if (stateTransition > 1.0f)
                    {
                        stateTransition = 1.0f;
                        state = State.Opened;
                    }
                    break;
                case State.Opened:
                    ProcessKeyInputs(dt);
                    break;
                case State.Closing:
                    stateTransition -= dt * CloseSpeed;
                    if (stateTransition < 0.0f)
                    {
                        stateTransition = 0.0f;
                        state = State.Closed;
                    }
                    break;
            }

            prevKeyState = keyState;

            base.Update(gameTime);
        }

        /// <summary>
        /// Hand keyboard input.
        /// </summary>
        /// <param name="dt"></param>
        public void ProcessKeyInputs(float dt)
        {
            KeyboardState keyState = Keyboard.GetState();
            Keys[] keys = keyState.GetPressedKeys();

            bool shift = keyState.IsKeyDown(Keys.LeftShift) ||
                            keyState.IsKeyDown(Keys.RightShift);

            foreach (Keys key in keys)
            {
                if (!IsKeyPressed(key, dt)) continue;

                char ch;
                if (KeyboardUtils.KeyToString(key, shift, out ch))
                {
                    // Handle typical character input.
                    commandLine = commandLine.Insert(cursorIndex, new string(ch, 1));
                    cursorIndex++;
                }
                else
                {
                    switch (key)
                    {
                        case Keys.Back:
                            if (cursorIndex > 0)
                                commandLine = commandLine.Remove(--cursorIndex, 1);
                            break;
                        case Keys.Delete:
                            if (cursorIndex < commandLine.Length)
                                commandLine = commandLine.Remove(cursorIndex, 1);
                            break;
                        case Keys.Left:
                            if (cursorIndex > 0)
                                cursorIndex--;
                            break;
                        case Keys.Right:
                            if (cursorIndex < commandLine.Length)
                                cursorIndex++;
                            break;
                        case Keys.Enter:
                        case Keys.Insert:
                            // Run the command.
                            ExecuteCommand(commandLine);
                            commandLine = string.Empty;
                            cursorIndex = 0;
                            break;
                        case Keys.Up:
                            // Show command history.
                            if (commandHistory.Count > 0)
                            {
                                commandHistoryIndex =
                                    Math.Max(0, commandHistoryIndex - 1);

                                commandLine = commandHistory[commandHistoryIndex];
                                cursorIndex = commandLine.Length;
                            }
                            break;
                        case Keys.Down:
                            // Show command history.
                            if (commandHistory.Count > 0)
                            {
                                commandHistoryIndex = Math.Min(commandHistory.Count - 1,
                                                                commandHistoryIndex + 1);
                                commandLine = commandHistory[commandHistoryIndex];
                                cursorIndex = commandLine.Length;
                            }
                            break;
                        case Keys.Tab:
                            Hide();
                            break;
                    }
                }
            }

        }

        /// <summary>
        /// Pressing check with key repeating.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool IsKeyPressed(Keys key, float dt)
        {
            // Treat it as pressed if given key has not pressed in previous frame.
            if (prevKeyState.IsKeyUp(key))
            {
                keyRepeatTimer = keyRepeatStartDuration;
                pressedKey = key;
                return true;
            }

            // Handling key repeating if given key has pressed in previous frame.
            if (key == pressedKey)
            {
                keyRepeatTimer -= dt;
                if (keyRepeatTimer <= 0.0f)
                {
                    keyRepeatTimer += keyRepeatDuration;
                    return true;
                }
            }

            return false;
        }

        public override void Draw(GameTime gameTime)
        {
            // Do nothing when command window is completely closed.
            if (state == State.Closed)
                return;

            SpriteFont font = debugManager.DebugFont;
            SpriteBatch spriteBatch = debugManager.SpriteBatch;
            Texture2D whiteTexture = debugManager.WhiteTexture;

            // Compute command window size and draw.
            float w = GraphicsDevice.Viewport.Width;
            float h = GraphicsDevice.Viewport.Height;
            float topMargin = h * 0.1f;
            float leftMargin = w * 0.1f;

            Rectangle rect = new Rectangle();
            rect.X = (int)leftMargin;
            rect.Y = (int)topMargin;
            rect.Width = (int)(w * 0.8f);
            rect.Height = (int)(MaxLineCount * font.LineSpacing);

            Matrix mtx = Matrix.CreateTranslation(
                        new Vector3(0, -rect.Height * (1.0f - stateTransition), 0));

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, mtx);

            spriteBatch.Draw(whiteTexture, rect, new Color(0, 0, 0, 200));

            // Draw each lines.
            Vector2 pos = new Vector2(leftMargin, topMargin);
            foreach (string line in lines)
            {
                spriteBatch.DrawString(font, line, pos, Color.White);
                pos.Y += font.LineSpacing;
            }

            // Draw prompt string.
            string leftPart = Prompt + commandLine.Substring(0, cursorIndex);
            Vector2 cursorPos = pos + font.MeasureString(leftPart);
            cursorPos.Y = pos.Y;

            spriteBatch.DrawString(font,
                String.Format("{0}{1}", Prompt, commandLine), pos, Color.White);
            spriteBatch.DrawString(font, Cursor, cursorPos, Color.White);

            spriteBatch.End();
        }

        #endregion

    }
}
