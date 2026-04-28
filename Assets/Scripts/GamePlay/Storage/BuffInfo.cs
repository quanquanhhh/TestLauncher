using System;
using Foundation.Storage;
using Newtonsoft.Json;

namespace GamePlay.Storage
{
    [Serializable]
    public class BuffInfo
    {
        [JsonProperty] private bool isVip = false;
        [JsonIgnore]
        public bool IsVip
        {
            get { return isVip; }
            set
            {
                isVip = value;
                StorageManager.Instance.ForceSave = true;
            }
        }
        [JsonProperty] private long vipExpire ;
        [JsonIgnore]
        public long VipExpire
        {
            get { return vipExpire; }
            set
            {
                vipExpire = value;
                StorageManager.Instance.ForceSave = true;
            }
        }
        
        [JsonProperty]private bool isPermanent = false;
        [JsonIgnore]
        public bool IsPermanent
        {
            get { return isPermanent; }
            set
            {
                isPermanent = value;
                StorageManager.Instance.ForceSave = true;
            }
        }
        
        
        [JsonProperty] private bool removeAds = false;
        [JsonIgnore]
        public bool RemoveAds
        {
            get { return removeAds; }
            set { removeAds = value; }
        }
        [JsonProperty] private bool permanentRemoveAds = false;
        [JsonIgnore]
        public bool PermanentRemoveAds
        {
            get { return permanentRemoveAds; }
            set { permanentRemoveAds = value; }
        }
        
    }
}