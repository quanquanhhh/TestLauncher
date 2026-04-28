using System.Collections.Generic;
using Foundation.Storage;
using Newtonsoft.Json;

namespace GamePlay.Storage
{
    public class SecretGiftInfo : StorageBase
    {
        [JsonProperty] private Dictionary<string, bool> buys = new();

        [JsonIgnore]
        public Dictionary<string, bool> Buys
        {
            get { return buys; }
            set { buys = value; }
        }
        
        
        [JsonProperty] private long lastShowDiscountTime ;
        [JsonIgnore]
        public long LastShowDiscountTime
        {
            get { return lastShowDiscountTime; }
            set { lastShowDiscountTime = value; }
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