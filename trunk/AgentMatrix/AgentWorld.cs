using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using DawnClient;
using DawnOnline.Simulation;
using DawnOnline.Simulation.Builders;
using DawnOnline.Simulation.Entities;
using Microsoft.Xna.Framework;
using SharedConstants;

namespace AgentMatrix
{
    class AgentWorld
    {
        private readonly DawnOnline.Simulation.Environment _staticEnvironment = SimulationFactory.CreateEnvironment();

        public IList<IEntity> GetEntities()
        {
            return _staticEnvironment.GetObstacles();
        }

        public void Update(ReadOnlyCollection<DawnClientEntity> entities)
        {
            // Add + update
            foreach (var entity in entities)
            {
                // Obstacles
                if (entity.Specy == EntityType.Wall || entity.Specy == EntityType.Box)
                {
                    var myEntity = FindObstacle(entity.Id);

                    var position = new Vector2(entity.PlaceX, entity.PlaceY);
                    if (myEntity == null)
                    {
                        // Add new with existing id!
                        myEntity = CreateWorldEntity(entity);
                        if (!_staticEnvironment.AddObstacle(myEntity, position))
                        {
                            // TODO: insert without collision check
                            //throw new NotSupportedException("Should not happen!"); 
                        }
                    }

                    // Update
                    CloneBuilder.UpdatePosition(myEntity, position, entity.Angle);
                }

                // Creatures
                if (entity.Specy == EntityType.Predator)
                {
                    var myEntity = FindCreature(entity.Id);

                    var position = new Vector2(entity.PlaceX, entity.PlaceY);
                    if (myEntity == null)
                    {
                        // Add new with existing id!
                        myEntity = CreateWorldEntity(entity) as ICreature;
                        if (!_staticEnvironment.AddCreature(myEntity, position, entity.Angle))
                        {
                            // TODO: insert without collision check
                            //throw new NotSupportedException("Should not happen!"); 
                        }
                    }

                    // Update
                    CloneBuilder.UpdatePosition(myEntity, position, entity.Angle);
                }



                // Avatars
            }

            // TODO: delete
        }

        public void Think(ReadOnlyCollection<int> creatureIds)
        {
            int updateIntervalOnServer = 50;

             _staticEnvironment.Think(100, new TimeSpan(updateIntervalOnServer), creatureIds);
        }

        public void SendActionsToServer(DawnClient.DawnClient dawnClient)
        {
            var creatureIds = dawnClient.CreatureIds;
            var creatures = _staticEnvironment.GetCreatures().Where(c => creatureIds.Contains(c.Id));

            foreach (var creature in creatures)
            {
                var actionQueue = creature.MyActionQueue;

                if (actionQueue.ForwardThrustPercent > 0.0)
                {
                    dawnClient.SendEntityCommand(creature.Id, AvatarCommand.RunForward);
                }
                if (actionQueue.ForwardThrustPercent < 0.0)
                {
                    dawnClient.SendEntityCommand(creature.Id, AvatarCommand.RunBackward);
                }
                if (actionQueue.TurnPercent > 0.0)
                {
                    dawnClient.SendEntityCommand(creature.Id, AvatarCommand.TurnRight);

                }
                if (actionQueue.TurnPercent < 0.0)
                {
                    dawnClient.SendEntityCommand(creature.Id, AvatarCommand.TurnLeft);
                }

                //if (actionQueue.Attack)
                //{
                //    dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Attack);
                //}
                //if (actionQueue.Fire)
                //{
                //    dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Fire);
                //}
                //if (actionQueue.FireRocket)
                //{
                //    dawnClient.SendEntityCommand(creature.Id, AvatarCommand.Fire);
                //}
            }
        }

        public void ClearActionQueues()
        {
            var creatures = _staticEnvironment.GetCreatures();
            foreach (var creature in creatures)
            {
                creature.ClearActionQueue();
            }
        }

        public IEntity FindObstacle(int id)
        {
            return _staticEnvironment.GetObstacles().FirstOrDefault(e => e.Id == id);
        }

        public ICreature FindCreature(int id)
        {
            return _staticEnvironment.GetCreatures().FirstOrDefault(e => e.Id == id);
        }

        private static IEntity CreateWorldEntity(DawnClientEntity clientEntity)
        {
            if (CloneBuilder.IsObstacle(clientEntity.Specy))
                return CloneBuilder.CreateObstacle(clientEntity.Id, clientEntity.Specy, WorldConstants.WallHeight, WorldConstants.WallWide);
            if (CloneBuilder.IsCreature(clientEntity.Specy))
                return CloneBuilder.CreateCreature(clientEntity.Id, clientEntity.Specy);

            throw new NotImplementedException();
        }
    }
}
