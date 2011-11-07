using System;

namespace DawnGame
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (var game = new Game2())
            //using (var game = new Game1())
            {
                game.Run();
            }
        }
    }
}

