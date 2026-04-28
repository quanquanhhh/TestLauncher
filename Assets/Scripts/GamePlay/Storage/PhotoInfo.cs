using System;
using System.Collections.Generic;
using Foundation.Storage;
using Newtonsoft.Json;

namespace GamePlay.Storage
{
    public enum PhotoState
    {
        None = 0,
        Lock = 1,
        Unlock = 2,
        Remove = 3,
    } 
    [System.Serializable]
    public class PhotoInfo : StorageBase
    {
        [JsonProperty] private Dictionary<string, PhotoItem> photoState = new();
        [JsonIgnore]
        public Dictionary<string, PhotoItem> PhotoState => photoState;

        [JsonProperty] private int likeCount = 0;
        [JsonIgnore]
        public int LikeCount
        {
            get
            {
                return likeCount;
            }
            set
            {
                likeCount = value;
                StorageManager.Instance.ChangeTimes++;
            }
        }

        [JsonProperty] private int albumsCount = 0;
        [JsonIgnore]
        public int AlbumsCount
        {
            get
            {
                return albumsCount;
            }
            set
            {
                albumsCount = value;
                StorageManager.Instance.ChangeTimes++;
            }
        }
    }

    [Serializable]
    public class PhotoItem
    {
        [JsonProperty] private string name;

        [JsonIgnore]
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
            }
        }
        [JsonProperty] private int state;

        [JsonIgnore]
        public int State
        {
            get { return state; }
            set
            {
                state = value;
            }
        } 
        public int from;
        public bool highLevel = false;
        public bool isLike = false;
        public int index;
        public int likeindex;
        public string time;
    }
}