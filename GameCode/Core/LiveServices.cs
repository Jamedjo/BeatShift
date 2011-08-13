using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework;

namespace BeatShift
{
    public class LiveServices
    {
        public static GamerServicesComponent gamerServices { get; set; }


        public static void initializeGamerServices(BeatShift mainGame)
        {
#if (XBOX || DEBUG)
            // Console.Write("Initializing networking (GamerServicesDispatcher)... ");
            //This block of code needs to be uncommented somewhere
            //Unfortunately it is very slow (5s) on my PC and delays
            //Startup significantly.
            try
            {
                GamerServicesDispatcher.WindowHandle = mainGame.Window.Handle;
                if (!GamerServicesDispatcher.IsInitialized)
                    GamerServicesDispatcher.Initialize(mainGame.Services);
            }
            catch (Exception e)
            {
                //DO SOMTHING SENSIBLE HERE.
                //triggered if live services not running
                Console.WriteLine("Unable to initialize GamerServicesDispatcher.");
            }
            // Console.WriteLine("   ...done.");
#endif
        }

        public static bool GuideIsVisible(){
#if (XBOX || DEBUG)
            if(Guide.IsVisible) return true;
#endif
            return false;
        }

        public static void Update(GameTime gameTime)
        {
#if (XBOX || DEBUG)
            gamerServices.Update(gameTime);
#endif
        }

    }
}
