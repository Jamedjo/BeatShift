#region File Description
//-----------------------------------------------------------------------------
// SkinningSample.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SkinnedModel;
#endregion

namespace SkinningSample
{
    /// <summary>
    /// Sample game showing how to display skinned character animation.
    /// </summary>
    public class SkinningSampleGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;

        KeyboardState currentKeyboardState = new KeyboardState();
        GamePadState currentGamePadState = new GamePadState();
        KeyboardState lastKeyboardState = new KeyboardState();
        GamePadState lastGamePadState = new GamePadState();

        Model currentModel;
        //AnimationPlayer animationPlayer;
        ClipPlayer clipPlayer;

        float cameraArc = 0;
        float cameraRotation = 0;
        float cameraDistance = 100;

        #endregion

        #region Initialization


        public SkinningSampleGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if WINDOWS_PHONE
            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);
            
            graphics.IsFullScreen = true;            
#endif
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load the model.
            currentModel = Content.Load<Model>("dude");

            // Look up our custom skinning information.
            SkinningData skinningData = currentModel.Tag as SkinningData;

            if (skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            // Create an clip player, and start decoding an animation clip at 24fps.
            //animationPlayer = new AnimationPlayer(skinningData);
            clipPlayer = new ClipPlayer(skinningData, 24);
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            UpdateCamera(gameTime);

            Matrix ShipMatrix = Matrix.CreateScale(7f);
            clipPlayer.Update(gameTime, ShipMatrix);

            base.Update(gameTime);
        }

        int shipLevel = 1;
        void levelUp()
        {
            shipLevel++;
            Console.WriteLine("Leveled Up to " + shipLevel);
            playUpClip(shipLevel);
        }
        void levelDown()
        {
            shipLevel--;
            Console.WriteLine("Leveled Down to " + shipLevel);
            playDownClip(shipLevel);
        }

        void playUpClip(int newLevel)
        {
            switch (newLevel)
            {
                case 2:
                    clipPlayer.play(1, 9, false);
                    break;
                case 3:
                    clipPlayer.play(9, 19, false);
                    break;
                case 4:
                    clipPlayer.play(19, 29, false);
                    break;
                case 5:
                    clipPlayer.play(29, 39, false);
                    break;

                default:
                    break;
            }
        }

        void playDownClip(int newLevel)
        {
            switch (newLevel)
            {
                case 1:
                    clipPlayer.play(9, 1, false);//As 50 frames used for 4 animations, frame is ((50/4)*lvl), should have used 40,60 or 80 frames not 50
                    break;
                case 2:
                    clipPlayer.play(19, 9, false);
                    break;
                case 3:
                    clipPlayer.play(29, 19, false);
                    break;
                case 4:
                    clipPlayer.play(39, 29, false);
                    break;

                default:
                    break;
            }
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice device = graphics.GraphicsDevice;

            device.Clear(Color.CornflowerBlue);

            Matrix[] bones = clipPlayer.GetSkinTransforms();

            // Compute camera matrices.
            Matrix view = Matrix.CreateTranslation(0, -25,-8) * 
                          Matrix.CreateRotationY(MathHelper.ToRadians(cameraRotation)) *
                          Matrix.CreateRotationX(MathHelper.ToRadians(cameraArc)) *
                          Matrix.CreateLookAt(new Vector3(0, -5, cameraDistance), 
                                              new Vector3(0, 0, 20), Vector3.Up);

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(0.949982712f,
                                                                    device.Viewport.AspectRatio,
                                                                    1,
                                                                    10000);

            //// Render the skinned mesh.
            //foreach (ModelMesh mesh in currentModel.Meshes)
            //{
            //    foreach (SkinnedEffect effect in mesh.Effects)
            //    {
            //        effect.SetBoneTransforms(bones);

            //        effect.View = view;
            //        effect.Projection = projection;

            //        effect.EnableDefaultLighting();

            //        effect.SpecularColor = new Vector3(0.25f);
            //        effect.SpecularPower = 16;
            //    }

            //    mesh.Draw();
            //}

            Matrix worldMatrix = Matrix.Identity;//clipPlayer.getWorldTransform(0);

            foreach (ModelMesh mesh in currentModel.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.CurrentTechnique = effect.Techniques["SkinnedShip"];

                    effect.Parameters["world_Mx"].SetValue(worldMatrix);
                    effect.Parameters["wvp_Mx"].SetValue(worldMatrix * view * projection);
                    //Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(worldTransform));
                    //effect.Parameters["wit_Mx"].SetValue(worldInverseTranspose);
                    Matrix viewInverse = Matrix.Invert(view);
                    effect.Parameters["viewInv_Mx"].SetValue(viewInverse);
                    
                    effect.Parameters["Bones"].SetValue(bones);
                }

                mesh.Draw();
            }

            base.Draw(gameTime);
        }

        
        #endregion

        #region Handle Input


        /// <summary>
        /// Handles input for quitting the game.
        /// </summary>
        private void HandleInput()
        {
            lastKeyboardState = currentKeyboardState;
            lastGamePadState = currentGamePadState;
            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Check for exit.
            if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
                currentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }

            // Check for lvlUp.
            if ((!currentKeyboardState.IsKeyDown(Keys.PageUp)&&lastKeyboardState.IsKeyDown(Keys.PageUp)) ||
                (!(currentGamePadState.DPad.Up == ButtonState.Pressed)&&(lastGamePadState.DPad.Up == ButtonState.Pressed)))
            {
                levelUp();
            }

            // Check for lvlDown.
            if ((!currentKeyboardState.IsKeyDown(Keys.PageDown) && lastKeyboardState.IsKeyDown(Keys.PageDown)) ||
                (!(currentGamePadState.DPad.Down == ButtonState.Pressed) && (lastGamePadState.DPad.Down == ButtonState.Pressed)))
            {
                levelDown();
            }
        }


        /// <summary>
        /// Handles camera input.
        /// </summary>
        private void UpdateCamera(GameTime gameTime)
        {
            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Check for input to rotate the camera up and down around the model.
            if (currentKeyboardState.IsKeyDown(Keys.Up) ||
                currentKeyboardState.IsKeyDown(Keys.W))
            {
                cameraArc += time * 0.1f;
            }
            
            if (currentKeyboardState.IsKeyDown(Keys.Down) ||
                currentKeyboardState.IsKeyDown(Keys.S))
            {
                cameraArc -= time * 0.1f;
            }

            cameraArc += currentGamePadState.ThumbSticks.Right.Y * time * 0.25f;

            // Limit the arc movement.
            if (cameraArc > 90.0f)
                cameraArc = 90.0f;
            else if (cameraArc < -90.0f)
                cameraArc = -90.0f;

            // Check for input to rotate the camera around the model.
            if (currentKeyboardState.IsKeyDown(Keys.Right) ||
                currentKeyboardState.IsKeyDown(Keys.D))
            {
                cameraRotation += time * 0.1f;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Left) ||
                currentKeyboardState.IsKeyDown(Keys.A))
            {
                cameraRotation -= time * 0.1f;
            }

            cameraRotation += currentGamePadState.ThumbSticks.Right.X * time * 0.25f;

            // Check for input to zoom camera in and out.
            if (currentKeyboardState.IsKeyDown(Keys.Z))
                cameraDistance += time * 0.25f;

            if (currentKeyboardState.IsKeyDown(Keys.X))
                cameraDistance -= time * 0.25f;

            cameraDistance += currentGamePadState.Triggers.Left * time * 0.5f;
            cameraDistance -= currentGamePadState.Triggers.Right * time * 0.5f;

            // Limit the camera distance.
            if (cameraDistance > 500.0f)
                cameraDistance = 500.0f;
            else if (cameraDistance < 10.0f)
                cameraDistance = 10.0f;

            if (currentGamePadState.Buttons.RightStick == ButtonState.Pressed ||
                currentKeyboardState.IsKeyDown(Keys.R))
            {
                cameraArc = 0;
                cameraRotation = 0;
                cameraDistance = 100;
            }
        }


        #endregion
    }


    #region Entry Point

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static class Program
    {
        static void Main()
        {
            using (SkinningSampleGame game = new SkinningSampleGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
