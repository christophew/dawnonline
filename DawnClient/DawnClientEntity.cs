using System;
using System.Collections;
using System.Collections.Generic;
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

        internal DawnClientEntity()
        {}

        internal DawnClientEntity(Hashtable eventData)
        {
            this.Id = (int)eventData[0];
            this.Specy = (EntityType)(byte)eventData[1];
            this.PlaceX = (float)eventData[2];
            this.PlaceY = (float)eventData[3];
            this.Angle = (float)eventData[4];
            this.IsActive = (bool)eventData[5];

            if (eventData.ContainsKey(6))
            {
                this.SpawnPointId = (int) eventData[6];
                this.DamagePercent = (byte)eventData[7];
                this.FatiguePercent = (byte)eventData[8];
                this.Score = (int)eventData[9];
            }
        }

        internal void UpdateFrom(DawnClientEntity original)
        {
            this.Id = original.Id;
            this.Specy = original.Specy;
            this.PlaceX = original.PlaceX;
            this.PlaceY = original.PlaceY;
            this.Angle = original.Angle;
            this.IsActive = original.IsActive;
            this.SpawnPointId = original.SpawnPointId;
            this.DamagePercent = original.DamagePercent;
            this.FatiguePercent = original.FatiguePercent;
            this.Score = original.Score;
        }
    }
}
