﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnClient
{
    public class DawnClientEntity
    {
        // TODO: share with Simulation
        public enum EntityType
        {
            Unknown,
            Avatar,
            Predator,
            Rabbit,
            Plant,
            Turret,
            Wall,
            Box,
            Treasure,
            PredatorFactory,
            Bullet,
            Rocket,
            SpawnPoint
        }

        public int Id { get; private set; }
        public EntityType Specy { get; private set; }
        public float PlaceX { get; private set; }
        public float PlaceY { get; private set; }
        public float Angle { get; private set; }
        public int SpawnPointId { get; private set; }

        internal DawnClientEntity()
        {}

        internal DawnClientEntity(Hashtable eventData)
        {
            this.Id = (int)eventData[0];
            this.Specy = (EntityType)(byte)eventData[1];
            this.PlaceX = (float)eventData[2];
            this.PlaceY = (float)eventData[3];
            this.Angle = (float)eventData[4];

            if (eventData.ContainsKey(5))
            {
                this.SpawnPointId = (int) eventData[5];
            }
        }

        internal void UpdateFrom(DawnClientEntity original)
        {
            this.Id = original.Id;
            this.Specy = original.Specy;
            this.PlaceX = original.PlaceX;
            this.PlaceY = original.PlaceY;
            this.Angle = original.Angle;
            this.SpawnPointId = original.SpawnPointId;
        }
    }
}