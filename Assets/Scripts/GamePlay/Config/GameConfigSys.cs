using System.Collections.Generic;
using System.Linq;
using Foundation.Storage;
using GamePlay;
using GamePlay.Storage;
using UnityEngine;

namespace GameConfig
{

 
    public partial class GameConfigSys
    {
        public static Dictionary<string, Photo> photos = new Dictionary<string, Photo>();
        public static Dictionary<string, Photo> level1Photos = new Dictionary<string, Photo>();
        public static Dictionary<string, Photo> level2Photos = new Dictionary<string, Photo>();
        public static Dictionary<string, Photo> level3Photos = new Dictionary<string, Photo>();
        public static Dictionary<string, Photo> shopPhotos = new Dictionary<string, Photo>();

        public static Dictionary<PhotoType, List<string>> activityPhotos = new(); 
        
        public static Dictionary<PhotoType, int> activityOpenLevel = new();

        private static Dictionary<string, PhotoItem> photoStorage => StorageManager.Instance.GetStorage<PhotoInfo>().PhotoState;
        
        static Dictionary<PhotoType, int> photoTypeCount = new Dictionary<PhotoType, int>();
        
        #region PreLoad

        private static void AddActivityPhoto(PhotoType name, string photo)
        {
            if (!activityPhotos.ContainsKey(name))
            {
                activityPhotos.Add(name, new List<string>());
            }
            activityPhotos[name].Add(photo);
        }

        public static bool HasPhoto(string name)
        {
            return StorageManager.Instance.GetStorage<PhotoInfo>().PhotoState.ContainsKey(name) && 
                   StorageManager.Instance.GetStorage<PhotoInfo>().PhotoState[name].State != (int)PhotoState.None;
        }
        public static void PreDealPhoto()
        {
            var dealInfo = new List<Photo>(_config.Photo);

            foreach (var photo in dealInfo)
            {
                if (!VaildPhoto(photo.name))
                {
                    continue;
                }

                var sname = GUtility.GetPhotoName(photo.name);
                if (photos.ContainsKey(photo.name) || photos.ContainsKey(sname))
                {
                    continue;
                }

                photos.Add(sname, photo);
                //photos 分类 level
                switch (photo.sourceFrom)
                {
                    case (int)PhotoType.Level1:
                        level1Photos.Add(photo.name, photo);
                        break;
                    case (int)PhotoType.Level2:
                        level2Photos.Add(photo.name, photo);
                        break;
                    case (int)PhotoType.Level3:
                        level3Photos.Add(photo.name, photo);
                        break;
                    case (int)PhotoType.Sign:
                    case (int)PhotoType.LimitedGift:
                    case (int) PhotoType.BeautyDraft:
                    case (int) PhotoType.VIP:
                    case (int) PhotoType.LuckyWheel:
                    case (int) PhotoType.SecretGift:
                    case (int) PhotoType.DailyChallenge:
                    case (int) PhotoType.Pass:
                    case (int) PhotoType.Shop:
                        AddActivityPhoto((PhotoType)photo.sourceFrom, photo.name); 
                        break; 
                }

                if (!photoStorage.ContainsKey(photo.name) || photoStorage[photo.name].State == (int)PhotoState.None)
                { 
                    SetPhotoTypeCount((PhotoType)photo.sourceFrom, 1);
                }

            }
        }

        public static void PreLoadActivittyOpenLevel()
        { 
            activityOpenLevel[PhotoType.LimitedGift] = _config.LimitPack.OpenLevel;
            activityOpenLevel[PhotoType.Sign] = _config.Sign.OpenLevel;
            activityOpenLevel[PhotoType.SecretGift] = _config.SecretGift[0].OpenLevel;
            activityOpenLevel[PhotoType.Pass] = _config.Pass[0].OpenLevel;
            activityOpenLevel[PhotoType.LuckyWheel] = _config.Wheel.OpenLevel;
            activityOpenLevel[PhotoType.BeautyDraft] = _config.Beauty[0].OpenLevel;
            activityOpenLevel[PhotoType.DraftPackage] = _config.DraftPackage.OpenLevel;
            activityOpenLevel[PhotoType.DailyChallenge] = 2;
            activityOpenLevel[PhotoType.VIP] = 1;
            activityOpenLevel[PhotoType.Shop] = 1;
        }
        #endregion

        public static Photo GetNextLevelPhoto(int checkLevelType)
        {
            Dictionary<string, Photo> checkDic = checkLevelType == 0 ? level1Photos : checkLevelType == 1 ? level2Photos : level3Photos;
            foreach (var photoitem in checkDic)
            {
                if (!photoStorage.ContainsKey(photoitem.Key))
                {
                    return photoitem.Value;
                }
            }

            var current = StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.PhotoName;
            foreach (var photoitem in checkDic)
            {
                if (current != photoitem.Key && (photoStorage[photoitem.Key].State == (int)PhotoState.None ||
                                                 photoStorage[photoitem.Key].State == (int)PhotoState.Remove))
                {
                    return photoitem.Value;
                }
            }
            
            //保护 全部图片都被玩过了，随缘抽
            var l = checkDic.Keys.ToList();
            l.Remove(current);
            int random = Random.Range(0, l.Count);
            return checkDic[l[random]];
        }
 

        public static string GetPhotoAtlasName(string photoName)
        {
            string str = GetPhotoBundleTag(photoName);
            return str + "_asset";
        }
        public static string GetPhotoBundleTag(string photoName)
        {
            photoName = GUtility.GetPhotoName(photoName);
            string str = photos[photoName].Tag;
            
            if (!string.IsNullOrEmpty(photos[photoName].other))
            {
                str += "_"+photos[photoName].other;
            }
            return str ;
        }
        private static void SetPhotoTypeCount(PhotoType name, int add)
        {
            if (!photoTypeCount.ContainsKey(name))
            {
                photoTypeCount[name] = 0;
            }
            photoTypeCount[name] += add;
        }

        public static List<string> GetActivityBundleTag(PhotoType name)
        {
            
            activityPhotos[name].RemoveAll(x=>string.IsNullOrEmpty(x));
            if (!activityPhotos.ContainsKey(name))
            {
                return new List<string>();
            }
            var check = activityPhotos[name];
            List<string> tags = new();
            foreach (var photo in check)
            {
                string bundleTag = GetPhotoBundleTag(photo); 
                if (!string.IsNullOrEmpty(bundleTag) && !tags.Contains(bundleTag)) 
                    tags.Add(bundleTag);
            }

            return tags;
        }
        public static Photo GetPhotoByName(string name)
        {
            name = GUtility.GetPhotoName(name);
            return photos[name];
        }

        public static string GetOnePhoto(PhotoType from)
        {
            activityPhotos[from].RemoveAll(x=>string.IsNullOrEmpty(x));
            bool has = false;
            int index = 0;
            string result = "";
            while (!has &&  activityPhotos[from].Count > index)
            {
                var one = activityPhotos[from][index];

                index++;
                if (StorageManager.Instance.GetStorage<PhotoInfo>().PhotoState.ContainsKey(one) ||
                    StorageManager.Instance.GetStorage<DailyInfo>().DailyImg.ContainsValue(one))
                {
                    continue;
                }
                else
                {
                    has = true;
                    result = one;
                }
            } 

            return result;

        }
        public static List<string> GetNoShowPhotos(PhotoType from)
        {
            activityPhotos[from].RemoveAll(x=>string.IsNullOrEmpty(x));
            List<string> l = new();
            for (int i = 0; i < activityPhotos[from].Count; i++)
            {
                var name =  activityPhotos[from][i];
                
                if (photoStorage.ContainsKey(name) && photoStorage[name].State != (int)PhotoState.None)
                { 
                    activityPhotos[from][i] = "";
                    continue;
                }
                else
                {
                    l.Add(name);
                } 
            }
            activityPhotos[from].RemoveAll(x=>string.IsNullOrEmpty(x));
            return l;
        }
        public static bool CheckEnoughShopPhoto(int need)
        {
            int count = 0;
            activityPhotos[PhotoType.Shop].RemoveAll(x=>string.IsNullOrEmpty(x));
            for (int i = 0; i < activityPhotos[PhotoType.Shop].Count; i++)
            {
                var name =  activityPhotos[PhotoType.Shop][i];
                
                if (photoStorage.ContainsKey(name) && photoStorage[name].State != (int)PhotoState.None)
                { 
                    activityPhotos[PhotoType.Shop][i] = "";
                    continue;
                }
                else
                {
                    count++;
                }
                if (count >= need)
                {
                    return true;
                }
            }
            return false;
        }
        
    }
}