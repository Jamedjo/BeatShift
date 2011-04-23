using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using BeatShift.Cameras;


namespace BeatShift
{
    public class ParticleEmitter
    {
        VertexPositionTexture[] verts;
        Vector3[] vertexDirectionArray;
        Color[] vertexColorArray;
        VertexBuffer particleVertexBuffer;

        Func<Matrix> matrixFunction;

        int lifeLeft;

        int numParticlesPerRound = 10;
        int maxParticles;
        static Random rnd = new Random(987);
        int roundTime = 10;
        int timeSinceLastRound = 0;

        GraphicsDevice graphicsDevice;

        ParticleSettings settings; //= new ParticleSettings();

        Effect particleEffect;

        Texture2D particleColorsTexture;

        int endOfLive = 0;
        int endOfDead = 0;
        float[] size;
        Quaternion rotation;
        Vector3 rndSpeed;
        Vector3 Localspeed;
        //ParticleSystem particle;
        //MainGame mainGame;
        int nextEmit;
        bool emitting;

        public bool IsDead
        {
            get { return endOfDead == maxParticles; }
        }

        public ParticleEmitter(Func<Matrix> functionToReturnWorldMatrix, ParticleSettings particlesettings, Effect particleEffect)
        {
            matrixFunction = functionToReturnWorldMatrix;
            this.settings = particlesettings;
            this.particleEffect = particleEffect;
            this.graphicsDevice = BeatShift.graphics.GraphicsDevice;
            this.particleColorsTexture = settings.colors;
            maxParticles = settings.minEmitted + rnd.Next(0,settings.maxEmitted-settings.minEmitted);
            numParticlesPerRound = settings.minEmit + rnd.Next(0, settings.maxEmit - settings.minEmit);
            lifeLeft = settings.minLife + rnd.Next(0, settings.maxLife - settings.minLife);
            this.particleEffect.Parameters["theTexture"].SetValue(settings.material);
            InitialiseParticleVertices();
        }

        private void InitialiseParticleVertices()
        {
            verts = new VertexPositionTexture[maxParticles * 4];
            vertexDirectionArray = new Vector3[maxParticles];
            size = new float[maxParticles];
            vertexColorArray = new Color[maxParticles];

            Color[] colors = new Color[particleColorsTexture.Width * particleColorsTexture.Height];
            particleColorsTexture.GetData(colors);

            for (int i = 0; i < maxParticles; ++i)
            {
                size[i] = (float)rnd.NextDouble() * settings.maxSize;

                verts[i * 4] = new VertexPositionTexture(Vector3.Zero, new Vector2(0, 0));
                verts[(i * 4) + 1] = new VertexPositionTexture(Vector3.Zero, new Vector2(0, 1));
                verts[(i * 4) + 2] = new VertexPositionTexture(Vector3.Zero, new Vector2(1, 0));
                verts[(i * 4) + 3] = new VertexPositionTexture(Vector3.Zero, new Vector2(1, 1));

                Vector3 direction = new Vector3(
                    (float)rnd.NextDouble() * rdir() * settings.rndvelocity.X + settings.velocity.X,
                    (float)rnd.NextDouble() * rdir() * settings.rndvelocity.Y + settings.velocity.Y,
                    (float)rnd.NextDouble() * rdir() * settings.rndvelocity.Z + settings.velocity.Z);

                direction.Normalize();

                direction *= settings.minSpeed + (float)rnd.NextDouble() * (settings.maxSpeed - settings.minSpeed);

                vertexDirectionArray[i] = direction;

                vertexColorArray[i] = colors[(
                                        rnd.Next(0, particleColorsTexture.Height) * particleColorsTexture.Width) +
                                        rnd.Next(0, particleColorsTexture.Width)];


            }
            particleVertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionTexture), verts.Length, BufferUsage.None);

        }

        private int rdir()
        {
            if (rnd.Next(0, 2) != 0)
            {
                return -1;
            }
            return 1;
        }

        public void Update(GameTime gameTime)
        {
            if (lifeLeft > 0)
                lifeLeft -= gameTime.ElapsedGameTime.Milliseconds;

            timeSinceLastRound += gameTime.ElapsedGameTime.Milliseconds;
            if (timeSinceLastRound > roundTime)
            {
                timeSinceLastRound -= roundTime;

                if (endOfLive < maxParticles)
                {
                    endOfLive += numParticlesPerRound;
                    if (endOfLive > maxParticles)
                        endOfLive = maxParticles;
                }
                if (lifeLeft <= 0)
                {
                    if (endOfDead < maxParticles)
                    {
                        endOfDead += numParticlesPerRound;
                        if (endOfDead > maxParticles)
                            endOfDead = maxParticles;
                    }
                }
            }
            for (int i = endOfDead; i < endOfLive; ++i)
            {
                verts[i * 4].Position += vertexDirectionArray[i];
                verts[(i * 4) + 1].Position += vertexDirectionArray[i];
                verts[(i * 4) + 2].Position += vertexDirectionArray[i];
                verts[(i * 4) + 3].Position += vertexDirectionArray[i];
            }
        }

        public void Draw(CameraWrapper camera)
        {
            //graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.BlendState = BlendState.AlphaBlend;
            particleEffect.Parameters["World"].SetValue(matrixFunction());//Matrix.Identity);//
            //particleEffect.Parameters["xAllowedRotDir"].SetValue(new Vector3(1, 0, 0));

            graphicsDevice.SetVertexBuffer(particleVertexBuffer);

            if (endOfLive - endOfDead > 0)
            {
                for (int i = endOfDead; i < endOfLive; ++i)
                {
                    //particleEffect.Parameters["vp"].SetValue(
                    //camera.View * camera.Projection);
                    //camera.View.Up;
                    particleEffect.Parameters["View"].SetValue(
                    camera.View);
                    particleEffect.Parameters["Projection"].SetValue(camera.Projection);
                    //particleEffect.Parameters["xCamPos"].SetValue(camera.View.Translation);
                    particleEffect.Parameters["BillboardSize"].SetValue(
                        size[i]);
                    particleEffect.Parameters["particleColor"].SetValue(
                    vertexColorArray[i].ToVector4());
                    // Draw particles
                    foreach (EffectPass pass in particleEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        graphicsDevice.DrawUserPrimitives<VertexPositionTexture>(
                        PrimitiveType.TriangleStrip,
                        verts, i * 4, 2);
                    }
                }
            }

            graphicsDevice.BlendState = BlendState.Opaque;
        }
    }
}