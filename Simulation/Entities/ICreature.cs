﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnOnline.Simulation.Statistics;
using SharedConstants;

namespace DawnOnline.Simulation.Entities
{
    public interface ICreature : IEntity
    {
        bool Alive { get; }
        bool IsTired { get; }
        CharacterSheet CharacterSheet { get; }

        // TODO: add to CharacterSheet? 
        List<EntityType> FoodSpecies { get; }

        // TEMP PUBLIC
        Environment MyEnvironment { get; }
        ICreature FindCreatureToAttack(List<EntityType> ofTypes);


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

        void BuildEntity(EntityType entityType);

        void Rest();
        void RegisterSpawn();

        // For ICreature?
        void Turn(double percent);
        void Thrust(double percent);
    }
}
