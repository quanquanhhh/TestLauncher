using System;
using System.Collections.Generic;
using Foundation.Storage;
using Newtonsoft.Json;

namespace GamePlay.Storage
{
    [Serializable]
    public class PassInfo : StorageBase
    {
        [JsonProperty]
        private int currentIndex;
        [JsonIgnore]
        public int CurrentIndex { get { return currentIndex; } set { currentIndex = value; } }

        [JsonProperty]
        private List<string> passPhotoNames = new List<string>();
        [JsonIgnore]
        public List<string> PassPhotoNames { get { return passPhotoNames; } set { passPhotoNames = value; } }
        
        [JsonProperty] private bool isFinished;
        [JsonIgnore]
        public bool IsFinished { get { return isFinished; } set { isFinished = value; } }
        [JsonProperty] private bool guideFinished;
        [JsonIgnore]
        public bool GuideFinished
        {
            get { return guideFinished; }
            set { guideFinished = value; }
        }
    }
}