﻿using System;
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

            if (_dawnClient.Connect())
            {
                do
                {
                    _dawnClient.Update();
                    Thread.Sleep(1000);

                    // Test
                    var allEntities = _dawnClient.DawnWorld.GetEntities();
                    var predators = allEntities.Count(e => e.Specy == DawnClientEntity.EntityType.Predator);
                    var boxes = allEntities.Count(e => e.Specy == DawnClientEntity.EntityType.Box);
                    var walls = allEntities.Count(e => e.Specy == DawnClientEntity.EntityType.Wall);
                    var spawnpoints = allEntities.Count(e => e.Specy == DawnClientEntity.EntityType.SpawnPoint);

                    var myInfo = string.Format("Total: {0}, Walls: {1}, Boxes: {2}, Predators: {3}, SpawnPoints: {4}",
                                               allEntities.Count, walls, boxes, predators, spawnpoints);

                    Console.WriteLine(_dawnClient.DawnWorld.WorldInformation + " --> " + myInfo);

                    _dawnClient.SendAvatorCommand(AvatarCommand.RunForward);
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