using System;
using System.Collections.Generic;
using Foundation.Storage;
using Newtonsoft.Json;

namespace GamePlay.Storage
{
    [Serializable]
    public class LevelInfo
    {
        [JsonProperty] private int level = -1;
        [JsonIgnore]
        public int Level
        {
            get
            {
                return level; 
            }
            set
            {
                level = value;
                StorageManager.Instance.ChangeTimes++;
            }
        }

        [JsonProperty] private string photoName = string.Empty;
        [JsonIgnore]
        public string PhotoName
        {
            get
            {
                return photoName;
            }
            set
            {
                photoName = value;
                StorageManager.Instance.ChangeTimes++;
            }
        }
 
        private string otherInfo ;
        [JsonIgnore]
        public string OtherInfo
        {
            get
            {
                return otherInfo;
            }
            set
            {
                otherInfo = value; 
            }
        }

        /// <summary>
        /// 当前关 三选一 显示的是什么， 一关一更新，同关不更新，
        /// 赋值时机 1. loading 2. 关卡开始后赋值下一关的 (不同bundle 为下载流出时间) 
        /// </summary>
        [JsonProperty] private List<string> levelShowSelectPic = new();
        [JsonIgnore]
        public List<string> LevelShowSelectPic
        {
            get{return levelShowSelectPic;}
            set{levelShowSelectPic = value;}
        }
        
        /// <summary>
        /// 刷新图片可能展示的图 （要去检测有没有准备好这个bundle）
        /// 赋值时机 同 levelShowSelectPic
        /// </summary>
        [JsonProperty] private List<string> levelRefreshShowPic = new();
        [JsonIgnore]
        public List<string> LevelRefreshShowPic
        {
            get{return levelRefreshShowPic;}
            set{levelRefreshShowPic = value;}
        }
        
        
        
        [JsonProperty] private Dictionary<int, int> leftVipPropCount = new();
        [JsonIgnore]
        public Dictionary<int, int> LeftVipPropCount
        {
            get
            {
                return leftVipPropCount;
            }
        }

    }
}