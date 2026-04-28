using Foundation.Storage;
using Newtonsoft.Json;

namespace GamePlay.Storage
{
    public class StatisticsInfo :  StorageBase
    {
        
        [JsonProperty] private bool firstInstall = true;
        [JsonIgnore]
        public bool FirstInstall
        {
            get { return firstInstall; }
            set { firstInstall = value; }
        }
        
        [JsonProperty] private bool firstPlay = true;
        [JsonIgnore]
        public bool FirstPlay
        {
            get { return firstPlay; }
            set { firstPlay = value; }
        }


    }
}