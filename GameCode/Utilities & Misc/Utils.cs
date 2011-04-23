using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

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
    }
}
