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
            // FIRST IMPLEMENT: build obstacles
            foreach (var entity in entities)
            {
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
            }

            // TODO: delete
        }

        public IEntity FindObstacle(int id)
        {
            return _staticEnvironment.GetObstacles().FirstOrDefault(e => e.Id == id);
        }

        private static IEntity CreateWorldEntity(DawnClientEntity clientEntity)
        {
            if (CloneBuilder.IsObstacle(clientEntity.Specy))
                return CloneBuilder.CreateObstacle(clientEntity.Id, clientEntity.Specy, WorldConstants.WallHeight, WorldConstants.WallWide);

            throw new NotImplementedException();
        }
    }
}
