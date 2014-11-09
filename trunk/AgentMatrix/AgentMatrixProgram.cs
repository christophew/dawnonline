using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using DawnClient;
using DawnOnline.AgentMatrix.Factories;
using DawnOnline.AgentMatrix.Repository;
using DawnOnline.Simulation.Builders;
using DawnOnline.Simulation.Entities;
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

        private static void CreateDawnClient()
        {
            Debug.Assert(_dawnClient == null);
            _dawnClient = new DawnClient.DawnClient();

            // Add eventhandler
            _dawnClient.WorldLoadedEvent += delegate
            {
                _dawnClient.IsMessageQueueRunning = false;

                // Initialize agent world
                _agentWorld = new AgentWorld(_dawnClient.InstanceId);

                // Lazy setup, because we first need the instanceId from the server
                Setup(_brainTypeArg);

                _dawnClient.IsMessageQueueRunning = true;

                // Initial Population
                //Console.WriteLine("Initial Population");
                //for (int i = 0; i < 5; i++)
                //{
                //    _agentWorld.AddCreature(AgentCreatureBuilder.CreateRabbitSpawnPoint());
                //    _agentWorld.AddCreature(AgentCreatureBuilder.CreateSpawnPoint());
                //    _agentWorld.AddCreature(AgentCreatureBuilder.CreateSpawnPoint2());
                //}
            };

            _dawnClient.EntityDestroyed += OnEntityDestroyedOnServer;
        }

        private static AgentWorld _agentWorld;
        private static AgentWorld GetAgentWorld(int instanceId)
        {
            if (_agentWorld == null)
                throw new InvalidOperationException("Agent world not yet created: is done on WorldLoadedEvent");

            return _agentWorld;
        }

        private static string _brainTypeArg;

        static void Main(string[] args)
        {
            _brainTypeArg = args[0];

            CreateDawnClient();

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
                        //WriteDebugInfo(agentWorld);
                        WriteFitness();


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

        private static void Setup(string brainType)
        {
            CreatureBuilder.SetClientServerMode(CreatureBuilder.ClientServerMode.Client);

            if (string.Equals("neural", brainType, StringComparison.InvariantCultureIgnoreCase))
            {
                AgentCreatureBuilder.SetBrainFactory(new NeuralBrainFactory());
                CreatureRepository.SetupRepository(brainType, true);              
            }
            else if (string.Equals("hardcoded", brainType, StringComparison.InvariantCultureIgnoreCase))
            {
                AgentCreatureBuilder.SetBrainFactory(new HardcodedBrainFactory());
                CreatureRepository.SetupRepository(brainType, false);
            }
            else
                throw new NotSupportedException();
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

            Console.WriteLine("> Predators  : " + predators + " / " + agentWorld.GetCreatures().Count(e => e.CreatureType == CreatureTypeEnum.Predator));
            Console.WriteLine("> Predators2 : " + predators2 + " / " + agentWorld.GetCreatures().Count(e => e.CreatureType == CreatureTypeEnum.Predator2));
            Console.WriteLine("> Rabbit     : " + rabbits + " / " + agentWorld.GetCreatures().Count(e => e.CreatureType == CreatureTypeEnum.Rabbit));

            Console.WriteLine("> SpawnPoints : " + spawnpoints + " / " + agentWorld.GetCreatures().Count(e => e.IsSpawnPoint));

            Console.WriteLine("> SpawnPoints Replicated: " + agentWorld.NrOfSpawnPointsReplicated);

            Console.WriteLine("> Creatures created: " + _dawnClient.CreatedCreatureIds.Count + " - " + string.Join(", ", _dawnClient.CreatedCreatureIds));
        }

        private static void WriteFitness()
        {
            double score = 0.0;

            var allEntities = _dawnClient.DawnWorld.GetEntities();

            foreach (var result in allEntities)
            {
                score += result.Score;
            }

            Console.WriteLine("Score: " + score);
        }

    }
}
