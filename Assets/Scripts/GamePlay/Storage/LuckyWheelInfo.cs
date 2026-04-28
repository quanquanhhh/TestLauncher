using System;
using Foundation.Storage;
using Newtonsoft.Json;

namespace GamePlay.Storage
{
    [Serializable]
    public class LuckyWheelInfo
    {
        [JsonProperty] private int leftTimes = 0;
        [JsonIgnore] public int  LeftTimes { get { return leftTimes; } set { leftTimes = value; } }
        
        [JsonProperty] private int rollingTimes = 0;
        [JsonIgnore] public int  RollingTimes { get { return rollingTimes; } set { rollingTimes = value; } }

        [JsonProperty] private long lastRollTimestamp = 0;
        [JsonIgnore]
        public long LastRollTimestamp
        {
            get
            {
                return lastRollTimestamp;
            }
            set
            {
                lastRollTimestamp = value;
                
                StorageManager.Instance.ChangeTimes++;
            }
        }
        [JsonProperty] private bool guideFinished;
        [JsonIgnore]
        public bool GuideFinished
        {
            get { return guideFinished; }
            set { guideFinished = value; }
        }

        [JsonProperty] private long vipLastTime;
        [JsonIgnore]
        public long VipLastTime
        {
            get { return vipLastTime; }
            set { vipLastTime = value; }
        }
    }
}