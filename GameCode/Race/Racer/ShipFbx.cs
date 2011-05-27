using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace BeatShift
{
    class ShipFbx
    {

        public Model model;
        public BasicEffect shipRenderer;
        public Matrix[] transforms;

        public ShipFbx(String modelName)
        {
            model = BeatShift.contentManager.Load<Model>("Models/Ships/" + modelName);
            GC.Collect();
            //var normalmap = BeatShift.contentManager.Load<Texture2D>("Models/skylarnormal.png");

            transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            //Extract the first effect used in the first mesh. This will control things like the texture.
            shipRenderer = (BasicEffect)model.Meshes[0].Effects[0]; //new BasicEffect(mainGame.graphics.GraphicsDevice);
            shipRenderer.EnableDefaultLighting();
            shipRenderer.PreferPerPixelLighting = true;

            //Set specular settings so ship doesn't appear over-lit
            shipRenderer.SpecularColor = new Vector3(0.2f, 0.2f, 0.2f);
            shipRenderer.SpecularPower = 12f;

            //Override the default texture
            //Texture2D grayTexture;
            //grayTexture = content.Load<Texture2D>("tex5");
            //shipRenderer.Texture = grayTexture;
            //shipRenderer.TextureEnabled = true;
        }

    }
}
