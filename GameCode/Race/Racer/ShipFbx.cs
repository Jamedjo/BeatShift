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
        public Matrix[] transforms;

        public ShipFbx(String modelName)
        {
            model = BeatShift.contentManager.Load<Model>("Models/Ships/" + modelName);
            GC.Collect();
            //var normalmap = BeatShift.contentManager.Load<Texture2D>("Models/skylarnormal.png");

            transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            //Override the default texture
            //Texture2D grayTexture;
            //grayTexture = content.Load<Texture2D>("tex5");
            //shipRenderer.Texture = grayTexture;
            //shipRenderer.TextureEnabled = true;
        }

    }
}
