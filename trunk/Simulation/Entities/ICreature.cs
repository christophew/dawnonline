using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Brains;
using DawnOnline.Simulation.Statistics;
using SharedConstants;

namespace DawnOnline.Simulation.Entities
{
    public interface ICreature : IEntity
    {
        bool Alive { get; }
        bool IsTired { get; }
        CharacterSheet CharacterSheet { get; }

        bool IsSpawnPoint { get; }

        // TODO: add to CharacterSheet? 
        List<CreatureTypeEnum> FoodSpecies { get; }

        // TEMP PUBLIC
        Environment MyEnvironment { get; }
        ICreature FindCreatureToAttack(List<CreatureTypeEnum> ofTypes);
        void SetSpawnPoint(ICreature spawnPoint);
        IBrain Brain { get; }


        ActionQueue MyActionQueue { get; }

        void ApplyActionQueue(double timeDelta);
        void ClearActionQueue();
        bool CanAttack();

        IEntity SpawnPoint { get; }

        ICreature Replicate(ICreature mate);
        void Mutate();


        void TurnLeft();
        void TurnRight();
        void TurnLeftSlow();
        void TurnRightSlow();
        void StrafeLeft();
        void StrafeRight();
        void RunForward();
        void RunBackward();
        void WalkForward();
        void WalkBackward();

        void Attack();
        void Fire();
        void FireRocket();

        void BuildEntity(EntityTypeEnum entityType);

        void Rest();
        void RegisterSpawn();

        // For ICreature?
        void Turn(double percent);
        void Thrust(double percent);
    }
}
