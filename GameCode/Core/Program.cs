using System;

namespace BeatShift
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (BeatShift game = new BeatShift())
            {
                game.Run();
            }
        }
    }
#endif
}

