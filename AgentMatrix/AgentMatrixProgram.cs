using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DawnClient;
using SharedConstants;

namespace AgentMatrix
{
    class Program
    {
        private static DawnClient.DawnClient _dawnClient;

        static void Main(string[] args)
        {
             _dawnClient = new DawnClient.DawnClient();
            var agentWorld = new AgentWorld();

            // Add eventhandler
            //_dawnClient.WorldLoadedEvent += delegate { _dawnClient.RequestCreatureCreationOnServer(EntityType.Predator, 100); };
            _dawnClient.WorldLoadedEvent += delegate
                                                {
                                                    for (int i = 0; i < 200; i++ )
                                                        agentWorld.CreateCreature(EntityType.Predator);
                                                };


            if (_dawnClient.Connect())
            {
                do
                {
                    _dawnClient.Update();
                    agentWorld.ProcessMyCreateRequests(_dawnClient.CreatureIds);
                    agentWorld.UpdateFromServer(_dawnClient.DawnWorld.GetEntities());
                    agentWorld.Think(_dawnClient.CreatureIds);
                    agentWorld.UpdateToServer(_dawnClient);

                    Thread.Sleep(50);

                    // Test
                    var allEntities = _dawnClient.DawnWorld.GetEntities();
                    var predators = allEntities.Count(e => e.Specy == EntityType.Predator);
                    var boxes = allEntities.Count(e => e.Specy == EntityType.Box);
                    var walls = allEntities.Count(e => e.Specy == EntityType.Wall);
                    var spawnpoints = allEntities.Count(e => e.Specy == EntityType.SpawnPoint);

                    

                    Console.WriteLine(_dawnClient.DawnWorld.WorldInformation);
                    Console.WriteLine("> boxes : " + boxes);
                    Console.WriteLine("> boxes2: " + agentWorld.GetEntities().Count(e => e.Specy == EntityType.Box));
                    Console.WriteLine("> walls : " + walls);
                    Console.WriteLine("> walls2: " + agentWorld.GetEntities().Count(e => e.Specy == EntityType.Wall));

                    Console.WriteLine("> Predators : " + predators);
                    Console.WriteLine("> Predators2: " + agentWorld.GetEntities().Count(e => e.Specy == EntityType.Predator));
                    Console.WriteLine("> Predators created: " + _dawnClient.CreatureIds.Count + " - " + string.Join(", ", _dawnClient.CreatureIds));

                    // Auto move forward
                    //if (_dawnClient.AvatarId != 0)
                    //{
                    //    _dawnClient.SendAvatorCommand(AvatarCommand.RunForward);
                    //}
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
