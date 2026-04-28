using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GamePlay.Storage
{
    [Serializable]
    public class BeautyInfo
    {
        [JsonProperty] private Dictionary<string, BeautyItemPhotos> BeautyItemPhoto = new();//章节 - 完成了几个
        [JsonIgnore]
        public Dictionary<string, BeautyItemPhotos> BeautyItemPhotos
        {
            get => BeautyItemPhoto;
        }

        [JsonProperty] private List<string> showOrder = new();
        [JsonIgnore]
        public List<string> ShowOrder
        {
            get => showOrder;
            set => showOrder = value;
        }
            
        [JsonProperty] private int noShowHintPopTimes = 0;
        [JsonIgnore]
        public int NoShowHintPopTimes
        {
            get => noShowHintPopTimes;
            set=> noShowHintPopTimes = value;
        }
        [JsonProperty] private int continueNoClickJump = 0;

        [JsonIgnore]
        public int ContinueNoClickJump
        {
            get => continueNoClickJump;
            set => continueNoClickJump = value;
        }
        [JsonProperty] private bool finishedGuide = false;

        [JsonIgnore]
        public bool FinishedGuide
        {
            get => finishedGuide;
            set{
                finishedGuide = value;}
        }

        [JsonProperty] private bool isInit = false;

        [JsonIgnore]
        public bool IsInit
        {
            get => isInit;
            set => isInit = value;
        }
        [JsonProperty] private bool guideFinished;
        [JsonIgnore]
        public bool GuideFinished
        {
            get { return guideFinished; }
            set { guideFinished = value; }
        }
    }

    public class BeautyItemPhotos
    {
        public string name = "";
        public bool unlock = false;
        public List<string> photoNames = new();
        public int finishedCount = 0;
    }
}