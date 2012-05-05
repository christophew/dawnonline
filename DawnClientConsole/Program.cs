using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DawnClient;

namespace DawnClientConsole
{
    class Program
    {
        private static DawnClient.DawnClient _dawnClient;

        static void Main(string[] args)
        {
            _dawnClient = new DawnClient.DawnClient();
            _dawnClient.Run();

            //do
            //{
            //    Console.WriteLine(_dawnClient.DawnWorld.WorldInformation);    
            //    Thread.Sleep(1000);
            //} 
            //while (!Console.KeyAvailable);

            _dawnClient.Stop();
        }
    }
}
