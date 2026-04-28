using System;
using Foundation.Storage;
using Newtonsoft.Json;

namespace GamePlay.Storage
{
    [Serializable]
    public class SignInfo : StorageBase
    {
        [JsonProperty] private int claimCount;

        [JsonIgnore]
        public int ClaimCount
        {
            get { return claimCount; }
            set { claimCount = value; }
        }
        [JsonProperty] private long lastSignTime;
        [JsonIgnore]
        public long LastSignTime
        {
            get { return lastSignTime; }
            set { lastSignTime = value; }
        }
        [JsonProperty] private bool guideFinished;
        [JsonIgnore]
        public bool GuideFinished
        {
            get { return guideFinished; }
            set { guideFinished = value; }
        }
    }
}