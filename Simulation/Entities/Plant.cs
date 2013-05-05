using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Builders;
using Microsoft.Xna.Framework;

namespace DawnOnline.Simulation.Entities
{
    internal class Plant : Creature
    {
        internal Plant(double bodyRadius) : base(bodyRadius)
        {}

        public override void Update(double timeDelta)
        {
            ApplyActionQueue(timeDelta);

            // Spawn seed
            if (CharacterSheet.Resource.PercentFilled > CharacterSheet.FoodValue)
            {
                var position = this.Place.Position;

                //var direction = new Vector3()

                // Add treasure where creature is killed
                var treasure = ObstacleBuilder.CreateTreasure(CreatureType, CharacterSheet.FoodValue);
                var angle = Globals.Radomizer.NextDouble()*MathHelper.TwoPi;
                MyEnvironment.AddObstacle(treasure, position, angle);

                // Move seed
                var maxForce = 200;
                var force = new Vector2((float)(Globals.Radomizer.NextDouble() - 0.5) * maxForce, (float)(Globals.Radomizer.NextDouble() -0.5) * maxForce);
                treasure.Place.Fixture.Body.ApplyForce(force);

                CharacterSheet.Resource.Decrease((int)CharacterSheet.FoodValue);
            }



            // Extract resources
            DoAutoResourceGather();
        }

        private void DoAutoResourceGather()
        {
            if ((DateTime.Now - _actionQueue.LastAutoResourceGainTime).TotalSeconds < CharacterSheet.AutoResourceGatherCoolDown)
                return;

            var resourcesGathered = MyEnvironment.GatherResources(CharacterSheet.AutoResourceGatherValue);
            CharacterSheet.Resource.Increase(resourcesGathered);

            _actionQueue.LastAutoResourceGainTime = DateTime.Now;
        }
    }
}
