﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SharedConstants;

namespace DawnClient
{
    public class DawnClientEntity
    {
        public int Id { get; private set; }
        public EntityTypeEnum EntityType { get; private set; }
        public CreatureTypeEnum CreatureType { get; private set; }
        public float PlaceX { get; private set; }
        public float PlaceY { get; private set; }
        public float Angle { get; private set; }
        public int SpawnPointId { get; private set; }
        public bool IsActive { get; private set; }
        public int DamagePercent { get; private set; }
        public int FatiguePercent { get; private set; }
        public int ResourcePercent { get; private set; }
        public int Score { get; private set; }
        public bool IsDead { get; private set; }

        public bool IsSpawnPoint { get { return EntityType == EntityTypeEnum.SpawnPoint; } }

        public int InstanceId { get { return Id % 1000; } } // Check Globals.GenerateUniqueId for logic

        enum UpdateMode
        {
            PositionUpdate, StatusUpdate, InitialLoad
        }

        private UpdateMode Mode { get; set; }

        internal DawnClientEntity()
        {}

        public void Kill()
        {
            IsDead = true;
        }

        public static DawnClientEntity CreatePositionUpdate(Hashtable eventData)
        {
            var newEntity = new DawnClientEntity();

            newEntity.Mode = UpdateMode.PositionUpdate;

            newEntity.Id = (int)eventData[0];
            newEntity.PlaceX = (float)eventData[1];
            newEntity.PlaceY = (float)eventData[2];
            newEntity.Angle = (float)eventData[3];

            return newEntity;
        }

        public static DawnClientEntity CreateStatusUpdate(Hashtable eventData)
        {
            var newEntity = new DawnClientEntity();

            newEntity.Mode = UpdateMode.StatusUpdate;

            newEntity.Id = (int)eventData[0];
            newEntity.IsActive = (bool)eventData[1];

            if (eventData.ContainsKey(2))
            {
                newEntity.DamagePercent = (byte)eventData[2];
                newEntity.FatiguePercent = (byte)eventData[3];
                newEntity.ResourcePercent = (byte)eventData[4];
                newEntity.Score = (int)eventData[5];
            }

            return newEntity;
        }

        public static DawnClientEntity CreateAddedEntity(Hashtable eventData)
        {
            var newEntity = new DawnClientEntity();

            newEntity.Mode = UpdateMode.InitialLoad;

            newEntity.Id = (int)eventData[0];
            newEntity.EntityType = (EntityTypeEnum)(byte)eventData[1];
            newEntity.CreatureType = (CreatureTypeEnum)(byte)eventData[2];
            newEntity.PlaceX = (float)eventData[3];
            newEntity.PlaceY = (float)eventData[4];
            newEntity.Angle = (float)eventData[5];

            if (eventData.ContainsKey(6))
            {
                newEntity.SpawnPointId = (int)eventData[6];
            }

            return newEntity;
        }

        internal void UpdateFrom(DawnClientEntity newData)
        {
            Debug.Assert(Id == 0 || newData.Id == Id);

            Id = newData.Id;

            if (newData.Mode == UpdateMode.PositionUpdate || newData.Mode == UpdateMode.InitialLoad)
            {
                this.PlaceX = newData.PlaceX;
                this.PlaceY = newData.PlaceY;
                this.Angle = newData.Angle;
            }
            if (newData.Mode == UpdateMode.StatusUpdate || newData.Mode == UpdateMode.InitialLoad)
            {
                this.IsActive = newData.IsActive;
                this.DamagePercent = newData.DamagePercent;
                this.FatiguePercent = newData.FatiguePercent;
                this.ResourcePercent = newData.ResourcePercent;
                this.Score = newData.Score;
            }
            if (newData.Mode == UpdateMode.InitialLoad)
            {
                this.EntityType = newData.EntityType;
                this.CreatureType = newData.CreatureType;
                this.SpawnPointId = newData.SpawnPointId;
            }
        }
    }
}
