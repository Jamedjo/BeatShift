using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace BeatShift
{
    public class FbxModel
    {
        public Model model;
        public Matrix[] transforms;
        public MapName mapName;
        public ModelCategory category;
        //String/enum modelName;

        // An object on one or all of the maps loaded when this object is created
        public FbxModel(String name, ContentManager content, MapName mapname, ModelCategory category)
        {
            mapName = mapname;
            this.category = category;

            //Load the model
            model = content.Load<Model>("Models/Maps/" + name);//Takes a long time for large models. estimate 4 seconds per 1MB fbx.

#if !DEBUG
            Utils.fixNullLightDirections(model);
#endif

            GC.Collect();

            //sets scale and position of model from fbx file
            transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
        }
    }
}
