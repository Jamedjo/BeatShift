using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BeatShift
{
    /// <summary>
    /// Collection of static utility classes to get stuff done.
    /// </summary>
    static class Utils
    {

        /// <summary>
        /// Takes an enumerated type and returns the values so they can be used in a foreach loop.
        /// This method uses reflection, so its slow.
        /// http://forums.create.msdn.com/forums/p/1610/157478.aspx 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetValues<T>()
        {
            return (from x in typeof(T).GetFields(BindingFlags.Static | BindingFlags.Public)
                    select (T)x.GetValue(null));
        }



        public static void fixNullLightDirections(Model m)
        {
            var fx = m.Meshes[0].Effects[0];
            if (fx.GetType().Equals(typeof(BasicEffect)))
            {
                return;
            }
            EffectParameterCollection fxParams = ((EffectMaterial)fx).Parameters;
            int l = 0;
            Vector3[] lDirs = { new Vector3(-0.52f, -0.57f, -0.62f), new Vector3(0.71f, 0.34f, 0.60f), new Vector3(0.45f, -0.76f, 0.45f) };
            for (int i = 0; i < fxParams.Count; i++)
            {
                EffectParameter a = fxParams[i];
                if (a.Name.ToString().Contains("LightDir"))
                {
                    if (a.GetValueVector3().Equals(new Vector3(0, 0, 0)))
                    {
                        a.SetValue(lDirs[l % 3]);
                        l++;
                    }

                }
            }
        }
    }
}
