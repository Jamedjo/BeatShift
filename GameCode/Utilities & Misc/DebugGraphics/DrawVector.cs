using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BeatShift.Cameras;

namespace BeatShift.DebugGraphics
{
    static class DrawVector
    {

        static VertexPositionColor[] arrowVertices = new VertexPositionColor[5];
        static short[] arrowIndices;
        static BasicEffect arrowEffect;
        static VertexBuffer arrowVertexBuffer;

        static DrawVector()
        {
            BuildArrow();
        }

        //Build an up arrow used to draw a vector length 5
        private static void BuildArrow()
        {
            float r = 0.5f;// arrow radius
            arrowEffect = new BasicEffect(BeatShift.graphics.GraphicsDevice);
            arrowVertices[0] = new VertexPositionColor(Vector3.Forward * 5f, Color.White);
            arrowVertices[1] = new VertexPositionColor(new Vector3(-r, r, 0), Color.White);
            arrowVertices[2] = new VertexPositionColor(new Vector3(r, r, 0), Color.White);
            arrowVertices[3] = new VertexPositionColor(new Vector3(r, -r, 0), Color.White);
            arrowVertices[4] = new VertexPositionColor(new Vector3(-r, -r, 0), Color.White);
            arrowIndices = new short[18] { 1, 2, 3, 3, 4, 1, 1, 0, 2, 2, 0, 3, 3, 0, 4, 4, 0, 1 };
            arrowVertexBuffer = new VertexBuffer(BeatShift.graphics.GraphicsDevice, typeof(VertexPositionColor), arrowVertices.Length, BufferUsage.None);
            arrowVertexBuffer.SetData(arrowVertices);
        }

        public static void drawArrow(CameraWrapper camera, Vector3 position, Vector3 D, Vector3 colour)
        {
            GraphicsDevice gDevice = BeatShift.graphics.GraphicsDevice;
            arrowVertices[0] = new VertexPositionColor(Vector3.Forward * D.Length(), Color.White);
            arrowVertexBuffer.SetData(arrowVertices);
            //arrowEffect.Alpha = 0.5f;
            //gDevice.BlendState = BlendState.AlphaBlend;
            gDevice.SetVertexBuffer(arrowVertexBuffer);
            arrowEffect.World = Matrix.CreateWorld(position, D, Vector3.Up);
            arrowEffect.View = camera.View;
            arrowEffect.Projection = camera.Projection;
            arrowEffect.VertexColorEnabled = true;
            arrowEffect.DiffuseColor = colour;
            foreach (EffectPass pass in arrowEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, arrowVertices, 0, arrowVertices.Length, arrowIndices, 0, arrowIndices.Length / 3);
            }
        }
    }
}
