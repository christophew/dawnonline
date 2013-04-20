using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DawnClient;
using SharedConstants;

namespace DawnClientConsole
{
    class Program
    {
        private static DawnClient.DawnClient _dawnClient;

        static void Main(string[] args)
        {
            _dawnClient = new DawnClient.DawnClient();
            _dawnClient.WorldLoadedEvent += delegate
            {
                _dawnClient.RequestAvatarCreationOnServer();
            };


            if (_dawnClient.Connect())
            {
                do
                {
                    if (!_dawnClient.WorldLoaded)
                    {
                        _dawnClient.Update();
                        Thread.Sleep(100);
                    }
                    else
                    {
                        Thread.Sleep(25);
                        _dawnClient.SendCommandsToServer();
                        _dawnClient.Update();

                        // Test
                        var allEntities = _dawnClient.DawnWorld.GetEntities();
                        var predators = allEntities.Count(e => e.Specy == EntityType.Predator || e.Specy == EntityType.Predator2);
                        var boxes = allEntities.Count(e => e.Specy == EntityType.Box);
                        var walls = allEntities.Count(e => e.Specy == EntityType.Wall);
                        var spawnpoints = allEntities.Count(e => e.IsSpawnPoint);

                        var myInfo = string.Format("Total: {0}, Walls: {1}, Boxes: {2}, Predators: {3}, SpawnPoints: {4}",
                                                   allEntities.Count, walls, boxes, predators, spawnpoints);

                        Console.WriteLine(_dawnClient.DawnWorld.WorldInformation + " --> " + myInfo);

                        if (_dawnClient.AvatarId != 0)
                        {
                            _dawnClient.SendAvatorCommand(AvatarCommand.RunForward);
                        }
                    }
                }
                while (!Console.KeyAvailable);
            }
            else
            {
                Console.WriteLine("Unknown hostname!");
            }

            _dawnClient.Disconnect(); //<- uncomment this line to see a faster disconnect/leave on the other clients.
        }
    }
}
