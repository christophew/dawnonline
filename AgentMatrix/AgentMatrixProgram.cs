using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using DawnClient;
using DawnOnline.AgentMatrix.Repository;
using SharedConstants;

namespace DawnOnline.AgentMatrix
{
    class Program
    {
        private static DawnClient.DawnClient _dawnClient;

        private static HashSet<int> _destroyQueue = new HashSet<int>();
        private static void OnEntityDestroyedOnServer(object sender, DawnClient.DawnClient.EntityDestroyedEventArgs args)
        {
            //lock (_destroyQueue)
            {
                foreach (int newId in args.DestroyedIds)
                {
                    if (!_destroyQueue.Contains(newId))
                        _destroyQueue.Add(newId);
                }
            }
        }

        private static AgentWorld _agentWorld;
        private static AgentWorld GetAgentWorld(int instanceId)
        {
            if (_agentWorld != null)
                return _agentWorld;

            _agentWorld = new AgentWorld(instanceId);

            // Initial population
            _dawnClient.WorldLoadedEvent += delegate
                                                {
                                                    for (int i = 0; i < 5; i++)
                                                    {
                                                        _agentWorld.AddCreature(AgentCreatureBuilder.CreateSpawnPoint());
                                                        _agentWorld.AddCreature(AgentCreatureBuilder.CreateSpawnPoint2());
                                                    }
                                                };
            return _agentWorld;
        }

        static void Main(string[] args)
        {
             _dawnClient = new DawnClient.DawnClient();

            // Add eventhandler
            //_dawnClient.WorldLoadedEvent += delegate { _dawnClient.RequestCreatureCreationOnServer(EntityType.Predator, 100); };

            _dawnClient.EntityDestroyed += OnEntityDestroyedOnServer;

            var stopWatch = new Stopwatch();

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
                        stopWatch.Reset();
                        stopWatch.Start();

                        _dawnClient.Update();

                        var agentWorld = GetAgentWorld(_dawnClient.InstanceId);
                        agentWorld.ProcessMyCreateRequests(_dawnClient.CreatedCreatureIds);
                        agentWorld.UpdateFromServer(_dawnClient.DawnWorld.GetEntities());
                        agentWorld.DoPhysics();

                        //lock (_destroyQueue)
                        {
                            agentWorld.ApplyDeleteFromServer(_destroyQueue);
                            _dawnClient.CleanupCreatedCreatureIds(_destroyQueue);
                            _destroyQueue.Clear();
                        }

                        _dawnClient.Update(); // = update state of physical sensors (bumpers, ed)
                        agentWorld.Think(50, _dawnClient.CreatedCreatureIds);

                        agentWorld.RepopulateWorld(_dawnClient.CreatedCreatureIds);
                        agentWorld.UpdateToServer(_dawnClient);
                        _dawnClient.SendCommandsToServer();

                        // Persist
                        CreatureRepository.GetRepository().Save();

                        // Test
                        WriteDebugInfo(agentWorld);


                        // Create stable cycles
                        stopWatch.Stop();
                        if (stopWatch.ElapsedMilliseconds < 100)
                        {
                            Thread.Sleep((int)(100 - stopWatch.ElapsedMilliseconds));
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

        private static void WriteDebugInfo(AgentWorld agentWorld)
        {
            var allEntities = _dawnClient.DawnWorld.GetEntities();
            var predators = allEntities.Count(e => e.CreatureType == CreatureTypeEnum.Predator);
            var predators2 = allEntities.Count(e => e.CreatureType == CreatureTypeEnum.Predator2);
            var rabbits = allEntities.Count(e => e.CreatureType == CreatureTypeEnum.Rabbit);
            var boxes = allEntities.Count(e => e.EntityType == EntityTypeEnum.Box);
            var walls = allEntities.Count(e => e.EntityType == EntityTypeEnum.Wall);
            var spawnpoints = allEntities.Count(e => e.IsSpawnPoint);

                    
            Console.WriteLine("> boxes : " + boxes + " / " + agentWorld.GetEntities().Count(e => e.EntityType == EntityTypeEnum.Box));
            Console.WriteLine("> walls : " + walls + " / " + agentWorld.GetEntities().Count(e => e.EntityType == EntityTypeEnum.Wall));

            Console.WriteLine("> Predators : " + predators + " / " + agentWorld.GetCreatures().Count(e => e.CreatureType == CreatureTypeEnum.Predator));
            Console.WriteLine("> Predators : " + predators2 + " / " + agentWorld.GetCreatures().Count(e => e.CreatureType == CreatureTypeEnum.Predator2));
            Console.WriteLine("> Predators : " + rabbits + " / " + agentWorld.GetCreatures().Count(e => e.CreatureType == CreatureTypeEnum.Rabbit));

            Console.WriteLine("> SpawnPoints : " + spawnpoints + " / " + agentWorld.GetCreatures().Count(e => e.IsSpawnPoint));

            Console.WriteLine("> SpawnPoints Replicated: " + agentWorld.NrOfSpawnPointsReplicated);

            Console.WriteLine("> Creatures created: " + _dawnClient.CreatedCreatureIds.Count + " - " + string.Join(", ", _dawnClient.CreatedCreatureIds));
        }
    }
}
