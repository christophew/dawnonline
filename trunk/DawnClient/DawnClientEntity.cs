using System;
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
        public EntityType Specy { get; private set; }
        public float PlaceX { get; private set; }
        public float PlaceY { get; private set; }
        public float Angle { get; private set; }
        public int SpawnPointId { get; private set; }
        public bool IsActive { get; private set; }
        public int DamagePercent { get; private set; }
        public int FatiguePercent { get; private set; }
        public int Score { get; private set; }

        enum UpdateMode
        {
            PositionUpdate, StatusUpdate, InitialLoad
        }

        private UpdateMode Mode { get; set; }

        internal DawnClientEntity()
        {}

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
            newEntity.Specy = (EntityType)(byte)eventData[1];
            newEntity.IsActive = (bool)eventData[2];

            if (eventData.ContainsKey(3))
            {
                newEntity.SpawnPointId = (int)eventData[3];
                newEntity.DamagePercent = (byte)eventData[4];
                newEntity.FatiguePercent = (byte)eventData[5];
                newEntity.Score = (int)eventData[6];
            }

            return newEntity;
        }

        public static DawnClientEntity CreateStaticUpdate(Hashtable eventData)
        {
            var newEntity = new DawnClientEntity();

            newEntity.Mode = UpdateMode.InitialLoad;

            newEntity.Id = (int)eventData[0];
            newEntity.PlaceX = (float)eventData[1];
            newEntity.PlaceY = (float)eventData[2];
            newEntity.Angle = (float)eventData[3];
            newEntity.Specy = (EntityType)(byte)eventData[4];

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
                this.Specy = newData.Specy;
                this.IsActive = newData.IsActive;
                this.SpawnPointId = newData.SpawnPointId;
                this.DamagePercent = newData.DamagePercent;
                this.FatiguePercent = newData.FatiguePercent;
                this.Score = newData.Score;
            }
        }
    }
}
