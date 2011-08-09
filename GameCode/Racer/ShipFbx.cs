using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using SkinnedModel;

namespace BeatShift
{
    class ShipFbx
    {

        public Model model;
        public ClipPlayer clipPlayer;
        public Matrix[] Bones { get { if (!isAnimated) { return bones; } else { return clipPlayer.GetSkinTransforms(); } } }
        private Matrix[] bones;
        public bool isAnimated = true;

        public ShipFbx(String modelName)
        {
            model = BeatShift.contentManager.Load<Model>("Models/Ships/" + modelName);


            SkinningData skd = GetSkinningDataFromTag();
            if (isAnimated) {
                clipPlayer = new ClipPlayer(skd, 24);
            }
            else
            {
                bones = new Matrix[model.Bones.Count];
                model.CopyAbsoluteBoneTransformsTo(bones);
            }

            GC.Collect();

            //Override the default texture
            //Texture2D grayTexture;
            //grayTexture = content.Load<Texture2D>("tex5");
            //shipRenderer.Texture = grayTexture;
            //shipRenderer.TextureEnabled = true;
        }

        private SkinningData GetSkinningDataFromTag()
        {
            //Get data from custom model processor.
            Dictionary<string, object> tagData = model.Tag as Dictionary<string, object>;
            if (tagData == null)
            {
                throw new Exception();
            }

            SkinningData skd;

            try
            {
                skd = tagData["SkinningData"] as SkinningData;
                if (skd == null)
                {
                    //throw new InvalidOperationException("This model does not contain a SkinningData tag.");

                    isAnimated = false;
                }
            }
            catch (KeyNotFoundException e)
            {
                isAnimated = false;
                skd = null;
            }
            return skd;
        }

    }
}
