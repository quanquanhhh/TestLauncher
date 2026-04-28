using System.Collections.Generic;
using Foundation.Storage;
using Newtonsoft.Json;

namespace GamePlay.Storage
{
    [System.Serializable]
    public class DailyInfo : StorageBase
    {
        [JsonProperty] private Dictionary<string, bool> daily = new();
        [JsonIgnore]
        public Dictionary<string, bool> Daily
        {
            get { return daily; } 
        }
        [JsonProperty] private Dictionary<string, string> dailyImg = new();
        [JsonIgnore]
        public Dictionary<string, string> DailyImg
        {
            get { return dailyImg; } 
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