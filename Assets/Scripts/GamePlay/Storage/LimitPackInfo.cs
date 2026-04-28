using System;
using Foundation.Storage;
using Newtonsoft.Json;

namespace GamePlay.Storage
{
    [Serializable]
    public class LimitPackInfo : StorageBase
    {
        [JsonProperty] private long lastOpenTime;
        [JsonIgnore]
        public long LastOpenTime
        {
            get { return lastOpenTime; } 
            set { lastOpenTime = value; }
        }

        [JsonProperty] private int leftPhotos;

        [JsonIgnore]
        public int LeftPhotos
        {
            get { return leftPhotos; }
            set { leftPhotos = value; }
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