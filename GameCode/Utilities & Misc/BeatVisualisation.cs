using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using BeatShift.Cameras;


namespace BeatShift.Utilities___Misc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class BeatVisualisation
    {
        
        //Graphics stuff to draw beat ring
              static VertexPositionColor[] quadVertices = new VertexPositionColor[5];
        static short[] quadIndices;
        static BasicEffect visEffect;
        static VertexBuffer quadVertexBuffer;
        static BeatVisualisation()
        {
            BuildQuad();
        }

        //Stuff to track beats
            List<BeatInstance> beats = new List<BeatInstance>();
            Vector2 startPosition;
            Vector2 hitPosition;
            float buttonScale;

            public BeatVisualisation(Vector2 StartPosition, Vector2 HitPosition, float ButtonScale)
            {
                startPosition = StartPosition;
                hitPosition = HitPosition;
                buttonScale = ButtonScale;
            }

            public void addBeat(ButtonImage button, int duration, int elapsed)//Must becarefull with add/update order so updates are not one frame out?
            {
                beats.Add(new BeatInstance(button, duration, elapsed));
            }
            //public void addBeat(ButtonImage button, GameTime currentTime, int expectedHitTime)
            //{
            //    beats.Add(new BeatInstance(button, currentTime.TotalGameTime.Milliseconds, expectedHitTime));
            //}

            //Remove from list when finished, fade just before.

            public void Clear()
            {
                beats.Clear();
            }

            public void Update(GameTime gameTime)
            {
                foreach (BeatInstance beat in beats)
                {
                    beat.elapsedDuration += gameTime.ElapsedGameTime.Milliseconds;
                }
                //beats.RemoveAll(beat => (beat.elapsedDuration >= beat.duration));//wont work on xbox
                beats = beats.Where(beat => (beat.elapsedDuration < beat.duration)).ToList();
            }

            public void Draw(CameraWrapper camera, ShipPhysics shipPhysics)
            {
                BeatShift.spriteBatch.Begin();
                BeatShift.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

                foreach (BeatInstance beat in beats)
                {
                    if (beat.elapsedDuration < beat.duration)
                    {
                        float lerpval = (float)beat.elapsedDuration / beat.duration;
                        
                        //Draw old beats
                        //ButtonDraw.DrawButton(BeatShift.spriteBatch, beat.button, Vector2.Lerp(startPosition, hitPosition, lerpval), buttonScale);

                        //Draw Visualisation Rings
                        drawQuad(camera,shipPhysics.ShipOrientationMatrix.Up,shipPhysics.ShipPosition,lerpval);
                    }
                }

                BeatShift.spriteBatch.End();
            }

            private class BeatInstance
            {
                public ButtonImage button;
                //float startTime;
                //float expectedHitTime;
                public int duration;
                public int elapsedDuration;

                public BeatInstance(ButtonImage Button, int timeDuration, int elapsedSoFar)
                {
                    button = Button;
                    duration = timeDuration;
                    elapsedDuration = elapsedSoFar;
                }
            }

        //Build an up quad for 3d drawing
        private static void BuildQuad()
        {
            float r = 0.5f;// quad radius
            visEffect = new BasicEffect(BeatShift.graphics.GraphicsDevice);
            quadVertices[0] = new VertexPositionColor(new Vector3(-r, r, 0), Color.White);
            quadVertices[1] = new VertexPositionColor(new Vector3(r, r, 0), Color.White);
            quadVertices[2] = new VertexPositionColor(new Vector3(r, -r, 0), Color.White);
            quadVertices[3] = new VertexPositionColor(new Vector3(-r, -r, 0), Color.White);
            quadIndices = new short[6] { 1, 2, 3, 3, 4, 1};
            quadVertexBuffer = new VertexBuffer(BeatShift.graphics.GraphicsDevice, typeof(VertexPositionColor), quadVertices.Length, BufferUsage.None);
            quadVertexBuffer.SetData(quadVertices);
        }

        public static void drawQuad(CameraWrapper camera, Vector3 upDirection, Vector3 position, float size)
        {
            GraphicsDevice gDevice = BeatShift.graphics.GraphicsDevice;
            quadVertices[0] = new VertexPositionColor(Vector3.Forward * size, Color.White);
            quadVertexBuffer.SetData(quadVertices);
            visEffect.Alpha = 0.8f; //TODO: times by size or log size or somthing to get fade
            gDevice.BlendState = BlendState.AlphaBlend;
            gDevice.SetVertexBuffer(quadVertexBuffer);
            visEffect.World = Matrix.CreateWorld(position, upDirection, Vector3.Up);
            visEffect.View = camera.View;
            visEffect.Projection = camera.Projection;
            visEffect.VertexColorEnabled = true;
            //visEffect.DiffuseColor = colour;
            foreach (EffectPass pass in visEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, quadVertices, 0, quadVertices.Length, quadIndices, 0, quadIndices.Length / 3);
            }
        }


        }
}
