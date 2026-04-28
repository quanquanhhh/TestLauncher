using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Foundation;
using Foundation.Storage;
using GameConfig;
using GamePlay.Storage;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using Event = Foundation.Event;
using Random = UnityEngine.Random;

namespace GamePlay
{
    public static class GUtility
    {
        public static int TopUISorting = 20000;
        
        public static string GetCommaFormat(this int num)
        {
            return num.ToString("N0", CultureInfo.InvariantCulture);
        }

        public static string GetPhotoName(string str)
        {
            
            str = Path.GetFileNameWithoutExtension(str);
            return str;
        }
        static List<string> iconName = new List<string>()
        {
            "coin","diamond","prop1","prop2","prop3","photo"
        };
        public static Sprite GetItemIcon(ItemType type, string fix = "")
        { 
            string name = iconName[(int)type - 1] + "_icon"; 
 
            if (!string.IsNullOrEmpty(fix))
            {
                name += "_"+fix;
            }

            return AssetLoad.Instance.LoadSprite(name);
        }

        private const string DefaultTableName = "LocalizationTables";
        public static string GetLocalizedString(string key, params object[] args  )
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;

            var handle = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(DefaultTableName, key, args);
            return handle.WaitForCompletion();
        }
        public static int GetWeightIndex(List<int> weight)
        {
            int allweight = 0;
            foreach (var i in weight)
            {
                allweight += i;
            }

            if (allweight == 0)
            {
                return 0;
            }
            int rnd = Random.Range(0, allweight);
            for (int index = 0; index < weight.Count; index++)
            {
                rnd -= weight[index];
                if (rnd <= 0)
                {
                    return index;
                }
            } 
            return 0;
        }
        public static List<int> GetWeightIndex(List<int> weight, int count)
        {
            int allweight = 0;
            foreach (var i in weight)
            {
                allweight += i;
            }
            List<int> result = new List<int>();
            for (int i = 0; i < count; i++)
            {
                int rnd = Random.Range(0, allweight);
                
                for (int index = 0; index < weight.Count; index++)
                {
                    if (result.Contains(index))
                    {
                        continue;
                    }
                    rnd -= weight[index];
                    if (rnd <= 0)
                    {
                        allweight -= weight[index];
                        result.Add(index);
                        break;
                    }
                }
            }
            return result;
        }
        

        public static bool IsEnoughItem(ItemType item, int needCount)
        {
            if (item == ItemType.Coin)
            {
                if (StorageManager.Instance.GetStorage<BaseInfo>().Coin < needCount)
                {
                    Event.Instance.SendEvent(new ShowTips("No Enough Coins."));
                }
                return
                    StorageManager.Instance.GetStorage<BaseInfo>().Coin >= needCount;
            }
            else if (item == ItemType.Diamond)
            {
                if (StorageManager.Instance.GetStorage<BaseInfo>().Diamond < needCount)
                {
                    Event.Instance.SendEvent(new ShowTips("No Enough Diamonds."));
                }
                return
                    StorageManager.Instance.GetStorage<BaseInfo>().Diamond >= needCount;
            }
            else if (item == ItemType.RemoveProp || item == ItemType.RandomProp || item == ItemType.UndoProp)
            {
                return 
                    StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.LeftVipPropCount.ContainsKey((int)item) && StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.LeftVipPropCount[(int)item] > 0 || 
                    StorageManager.Instance.GetStorage<BaseInfo>().Currency[(int)item] > 0; 
            }
            return false;
        }

        static BaseInfo baseInfo = StorageManager.Instance.GetStorage<BaseInfo>();
        private static Dictionary<string, PhotoItem> photoInfo => StorageManager.Instance.GetStorage<PhotoInfo>().PhotoState;
        public static void CheckPhotoState(string name, int from, bool high = false, PhotoState state = PhotoState.None)
        {
            if (!photoInfo.ContainsKey(name))
            {
                var item = new PhotoItem();
                item.Name = name; 
                item.from = (int)from;
                item.highLevel = high;
                item.State = (int)state;
                photoInfo.Add(name, item);
            }
            else
            {
                photoInfo[name].State = (int)state;
            }
        }
        
        public static void ApplyAspect( Texture texture, RectTransform rt)
        { 
            float width = rt.rect.width;
            float height = rt.rect.height;
            float ratio = (float) width/texture.width;
            float ratio2 = (float) height /texture.height;
            float m = Math.Max(ratio, ratio2); 
            rt.sizeDelta = new Vector2(texture.width * m , texture.height * m);
        }
        public static void ApplyAspect( Sprite sprite, RectTransform rt, RectTransform change)
        { 
            float width = rt.rect.width;
            float height = rt.rect.height;
            float ratio = (float) width/sprite.rect.width;
            float ratio2 = (float) height /sprite.rect.height;
            float m = Math.Max(ratio, ratio2); 
            change.sizeDelta = new Vector2(sprite.rect.width * m , sprite.rect.height * m);
        }

        public static void ApplyAspect(Sprite sprite, float width, float height, RectTransform change)
        {
             
            float ratio = (float) width/sprite.rect.width;
            float ratio2 = (float) height /sprite.rect.height;
            float m = Math.Max(ratio, ratio2); 
            change.sizeDelta = new Vector2(sprite.rect.width * m , sprite.rect.height * m);
        }

        public static void ApplyAspect( Sprite sprite, Vector2 rt, RectTransform change)
        { 
            float width = rt.x;
            float height = rt.y;
            float ratio = (float) width/sprite.rect.width;
            float ratio2 = (float) height /sprite.rect.height;
            float m = Math.Max(ratio, ratio2); 
            change.sizeDelta = new Vector2(sprite.rect.width * m , sprite.rect.height * m);
        }

        public static void ApplyAspect(Texture sprite, Vector2 size, RectTransform change)
        {
            float width = size.x;
            float height = size.y;
            float ratio = (float) width/sprite.width;
            float ratio2 = (float) height /sprite.height;
            float m = Math.Max(ratio, ratio2); 
            change.sizeDelta = new Vector2(sprite.width * m , sprite.height * m);
        }
        public static string GetRemainTimeText(long remainSeconds)
        { 
            if (remainSeconds < 0)
                remainSeconds = 0;

            long hours = remainSeconds / 3600;
            long minutes = (remainSeconds % 3600) / 60;
            long seconds = remainSeconds % 60;

            if (hours > 0)
                return $"{hours:00}:{minutes:00}:{seconds:00}";

            return $"{minutes:00}:{seconds:00}";
        }

        public static string GetPhotoTag(string name)
        { 
            var p = GameConfigSys.GetPhotoByName(name);
            string t = p.Tag;
            if (!string.IsNullOrEmpty(p.other))
            {
                t += "_" + p.other;
            }

            return t;
        }
        public static void ToEmail()
        {
            var email = ConfigManager.useConfig.email;
            Application.OpenURL("mailto:"+email);
        }

        private static bool shake => StorageManager.Instance.GetStorage<BaseInfo>().Setting["shake"];
        public static void Vibrate()
        {
            if (shake)
            {
#if UNITY_ANDROID || UNITY_IOS
                Handheld.Vibrate();
#endif
            }
        }
    }
}