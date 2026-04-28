using System.Collections.Generic;
using Foundation.Storage;
using Newtonsoft.Json;

namespace GamePlay.Storage
{
    [System.Serializable]
    public class BaseInfo : StorageBase
    {
        [JsonProperty] private string userType = "A";

        [JsonIgnore]
        public string UserType
        {
            get { return userType; }
            set {
                userType = value;
                StorageManager.Instance.ChangeTimes++;
            }
        }
        [JsonProperty] private int level = 0;
        [JsonIgnore]
        public int Level
        {
            get { return level; }
            set
            {
                level = value;
                StorageManager.Instance.ForceSave = true;
            }
        }
        
        [JsonProperty] private LevelInfo currentLevel = new LevelInfo();
        [JsonIgnore]
        public LevelInfo CurrentLevel
        {
            get { return currentLevel; }
            set{ 
                currentLevel = value;
                StorageManager.Instance.ChangeTimes++;}
        }
        
        
        [JsonProperty] private int coin = 0;
        [JsonIgnore]
        public int Coin
        {
            get { return coin; }
            set
            {
                coin = value;
                StorageManager.Instance.ChangeTimes++;
            }
        }
        
        [JsonProperty] private int diamond = 0;
        [JsonIgnore]
        public int Diamond
        {
            get { return diamond; }
            set
            {
                diamond = value;
                StorageManager.Instance.ChangeTimes++;
                StorageManager.Instance.ForceSave = true;
            }
        }
        
        [JsonProperty] private BuffInfo _buff = new BuffInfo();
        [JsonIgnore]
        public BuffInfo Buff
        {
            get { return _buff; }
        }
        


        [JsonProperty] private string selectBg;
        [JsonIgnore]
        public string SelectBG
        {
            get { return selectBg; }
            set { 
                selectBg = value; 
                StorageManager.Instance.ChangeTimes++; 
            }
        }

        [JsonProperty] private string currentBg;
        [JsonIgnore]
        public string CurrentBg
        {
            get { return currentBg; }
            set { 
                currentBg = value; 
                StorageManager.Instance.ChangeTimes++; 
            }
        }

        [JsonProperty] private Dictionary<int, int> currency = new();

        [JsonIgnore]
        public Dictionary<int, int> Currency
        {
            get { return currency; }
        }

        [JsonProperty] private Dictionary<string, bool> setting = new();
        [JsonIgnore]
        public Dictionary<string, bool> Setting
        {
            get { return setting; }
        }

        [JsonProperty] private string localeCode;
        [JsonIgnore]
        public string LocaleCode
        {
            get { return localeCode; }
            set {
                localeCode = value;
                StorageManager.Instance.ChangeTimes++;
            }
        }

        [JsonProperty] private Dictionary<int, int> flyItemInLobby= new();

        [JsonIgnore]
        public Dictionary<int, int> FlyItemInLobby
        {
            get { return flyItemInLobby; }
            set {
                flyItemInLobby = value;
                StorageManager.Instance.ChangeTimes++;
            }
        }

        [JsonProperty] private List<string> finishedGuide = new();

        [JsonIgnore]
        public List<string> FinishedGuide
        {
            get { return finishedGuide; }
            set {
                finishedGuide = value;
                StorageManager.Instance.ChangeTimes++;
            }
        }
    }
}