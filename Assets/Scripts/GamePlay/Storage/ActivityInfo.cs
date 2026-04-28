using Foundation.Storage;
using Newtonsoft.Json;

namespace GamePlay.Storage
{
    [System.Serializable]
    public class ActivityInfo : StorageBase
    {
        [JsonProperty] private LuckyWheelInfo _luckywheel = new();

        [JsonIgnore]
        public LuckyWheelInfo LuckyWheelInfo
        {
            get { return _luckywheel; }
        }
        
        [JsonProperty] private LimitPackInfo _limitpack = new();

        [JsonIgnore]
        public LimitPackInfo LimitPackInfo
        {
            get { return _limitpack; }
        }

        
        [JsonProperty] private BeautyInfo _beautyInfo = new();

        [JsonIgnore]
        public BeautyInfo BeautyInfo
        {
            get { return _beautyInfo; }
        }

        [JsonProperty] private PassInfo _passInfo = new();
        [JsonIgnore]
        public PassInfo PassInfo
        {
            get { return _passInfo; }
        }
        
        [JsonProperty] private SignInfo _signInfo = new();
        [JsonIgnore]
        public SignInfo SignInfo
        {
            get { return _signInfo; }
        }

        [JsonProperty] private SecretGiftInfo _secretGiftInfo = new();

        [JsonIgnore]
        public SecretGiftInfo SecretGiftInfo
        {
            get { return _secretGiftInfo; }
            
        } 
            

    }
}