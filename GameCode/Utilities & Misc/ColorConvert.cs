using System;
using Microsoft.Xna.Framework;

namespace BeatShift
{
    /// <summary>
    /// Deals with converting colours between RGB and HSL/HSV type values so hue can be controlled.
    /// </summary>
    public class ColorConvert
    {
        /// <summary>
        /// Returns the hue of a colour in RGB format
        /// </summary>
        /// <param name="R">Red</param>
        /// <param name="G">Green</param>
        /// <param name="B">Blue</param>
        /// <returns>Hue as a double</returns>
        public static double getHue(int R, int G, int B)
        {
            double alph;
            double beta;
            
            alph = 0.5*(2*R-(G+B));
            beta = Math.Sqrt(3)*0.5*(G-B);
            
            return Math.Atan2(beta,alph);
        }
                
        /// <summary>
        /// Gets the saturation value from an RGB
        /// </summary>
        /// <param name="R">Red</param>
        /// <param name="G">Green</param>
        /// <param name="B">Blue</param>
        /// <returns></returns>
        public static double getSat(int R, int G, int B)
        {
            double C;
            double alph = 0.5*(2*R-(G+B));
            double beta = Math.Sqrt(3)*0.5*(G-B);
            C = Math.Sqrt(Math.Pow(alph,2) + Math.Pow(beta,2));
            if(C==0) return 0;
            return (C/ getVal(R,G,B));
        }
        
        /// <summary>
        /// Gets the V='value' in the HSV colour system from RGB
        /// </summary>
        /// <param name="R">red</param>
        /// <param name="G">green</param>
        /// <param name="B">blue</param>
        /// <returns>value</returns>
        public static int getVal(int R, int G, int B)
        {
            if(R>=G && R>=B)
            {
                return R;
            }
            
            if(B>=G && B>=R)
            {
                return B;
            }
            
            if(G>=R && G>=B)
            {
                return G;
            }
            return G;//Default
        }
        
        /// <summary>
        /// Takes a colour in HSV colour space and returns it in RGB space as an XNA Color object.
        /// </summary>
        /// <param name="H">'Hue' between 0 and 360.</param>
        /// <param name="S">'Saturation' between 0 and 1</param>
        /// <param name="V">'Value' between 0 and 1</param>
        /// <returns></returns>
        public static Color getRGB(int H, double S, double V) {
            double dC = (V*S);
            double Hd = ((double)H)/60;
            double dX = (dC * (1 - Math.Abs((Hd % 2) - 1)));//dC * (1 - ((Hd + 1) % 2));

            int C = (int)(dC * 255);
            int X = (int)(dX * 255);

            //Console.WriteLine("H:" + H + " S:" + S + " V:" + V + ", C: " + C + " X:" + X + " Hd:" + Hd);
            
            if(Hd<1) {
                return new Color(C, X, 0);
            } else if (Hd<2) {
                return new Color(X, C, 0);
            } else if (Hd<3) {
                return new Color(0, C, X);
            } else if (Hd<4) {
                return new Color(0, X, C);
            } else if (Hd<5) {
                return new Color(X, 0, C);
            } else if (Hd<6) {
                return new Color(C, 0, X);
            }
            return new Color(0, 0, 0);
        }

    }
}
