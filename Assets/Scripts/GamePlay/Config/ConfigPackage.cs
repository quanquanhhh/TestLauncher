using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Foundation.Storage;
using GamePlay;
using GamePlay.Storage;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameConfig
{
    public enum PhotoType
    {
        Level1 = 0, //关卡
        Level2 = 1, //关卡选的广告
        Level3 = 2,
        Sign = 3,
        LimitedGift = 4,
        Shop = 5,
        BeautyDraft = 6,
        VIP = 7,
        LuckyWheel = 8, 
        SecretGift = 9,
        DailyChallenge = 10,
        Pass = 11,
        DraftPackage = 12,
    }
    [Serializable]
    public class ConfigPackage
    {
        public BaseGame BaseGame;
        public List<Shop> Shop;
        public List<Item> Item;
        public List<Photo> Photo;
        public List<Difficult> Difficult;
        public List<DifficultConfig> DifficultConfig; 
        public List<IAP> IAP;
        public List<VIP> VIP;
        public Sign Sign;
        public AdConfig AdConfig;
        
        public LimitPack LimitPack;
        public Wheel Wheel;
        public DraftPackage DraftPackage;
        public List<SecretGift> SecretGift;
        public List<SecretPack> SecretPack;
        public List<Beauty> Beauty;
        public List<Pass> Pass;
        public List<Daily> Daily;
    }

    public partial  class GameConfigSys
    {
        private static ConfigPackage _config;
        public static BaseGame baseGame;
        public static List<Shop> shop;
        public static List<Item> item;
        public static Wheel Wheel; 
        public static Sign sign;
        public static List<Difficult> leveldifficult;
        public static List<DifficultConfig> difficultConfig;
        public static List<IAP> iap;
        public static List<VIP> vips;
        public static List<Pass> pass;
        public static List<Daily> daily;
        public static AdConfig AdConfig;
        /// <summary>
        /// photo相关
        /// </summary>
        // public static List<Photo> allPhoto = new List<Photo>();
        //
        // public static List<Photo> HighLevelPhotos = new  List<Photo>();
        // public static List<Photo> AdPhotos = new List<Photo>();
        // public static List<Photo> NormalPhotos = new List<Photo>();
        // public static List<Photo> LuckyWheelPhotos = new List<Photo>();
        // public static List<Photo> ShopPhotos = new List<Photo>();
        

        public static List<string> LevelEasyConfig = new ();
        public static List<string> LevelNormalConfig = new();
        public static List<string> LevelHardConfig = new();
        
        //------------------------------------------------------------------------------------------
         
        public static void InitConfig(ConfigPackage config)
        {
            _config = config;
            baseGame = config.BaseGame;
            shop = config.Shop;
            item = config.Item;
            Wheel = config.Wheel; 
            leveldifficult =  config.Difficult;
            difficultConfig = config.DifficultConfig;
            iap = config.IAP;
            vips = config.VIP;
            pass = config.Pass;
            sign = config.Sign;
            daily = config.Daily;
            AdConfig = config.AdConfig;
            SetGuide(); 
            
            DealLevelDifficultConfig();
            // DealNoShowPhotos();

        }

        private static void DealLevelDifficultConfig()
        {
            foreach (var configItem in difficultConfig)
            {
                if (!string.IsNullOrEmpty(configItem.easy))
                {
                    LevelEasyConfig.Add(configItem.easy);
                }

                if (!string.IsNullOrEmpty(configItem.medium))
                {
                    LevelNormalConfig.Add(configItem.medium);
                }

                if (!string.IsNullOrEmpty(configItem.Hard))
                {
                    LevelHardConfig.Add(configItem.Hard);
                }
            }
        }
        
        /// <summary>
        /// 状态不为remove的图片
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static bool VaildPhoto(string name)
        {
            var data = StorageManager.Instance.GetStorage<PhotoInfo>();
            if (!data.PhotoState.ContainsKey(name))
            {
                return true;
            }
            return data.PhotoState[name].State != (int)PhotoState.Remove;
        }

        public static Item GetItemConfig(int itemId)
        {
            return item.Find(x=>x.id == itemId);
        }
 
        // public static void DealPhotos(List<Photo> config)
        // {
        //     foreach (var photo in config)
        //     {
        //         if (!VaildPhoto(photo.name))
        //         {
        //             continue;
        //         }
        //         allPhoto.Add(photo);
        //
        //         switch ((PhotoType)photo.sourceFrom)
        //         {
        //             case PhotoType.Level1: NormalPhotos.Add(photo); break;
        //             case PhotoType.Level2: AdPhotos.Add(photo); break;
        //             case PhotoType.LuckyWheel: LuckyWheelPhotos.Add(photo); break;
        //             case PhotoType.Shop: ShopPhotos.Add(photo); break;
        //         } 
        //         
        //     }
        // }
        

        #region luckywheel

        public static int GetLuckyWheelResult()
        { 
            var index = GUtility.GetWeightIndex(Wheel.Weight);
            return index;
        }
        

        #endregion

        #region Level

        public static Difficult GetDifficultByLevel(int level)
        {
            level = level % leveldifficult.Count;
            return leveldifficult[level];
        }

        public static List<int> GetDifficultConfig(int difficult)
        {
            List<string> check = new();
            switch (difficult)
            {
                case 1 : check = LevelEasyConfig;break;
                case 2: check = LevelNormalConfig;break;
                case 3: check = LevelHardConfig;break;
            }

            if (check.Count == 0)
            {
                Debug.LogError(" check difficult config error " + difficult);
                return null;
            }

            int rnd = Random.Range(0, check.Count);
            string str = check[rnd];
            string[] strArray = str.Split('|');
            List<int> result = new();
            foreach (var s in strArray)
            {
                int num = 0;
                int.TryParse(s, out num);
                result.Add(num);
            }
            return result;

        }
        

        #endregion
 

        #region 支付相关

        public static  Dictionary<string, (int,int)> GetAllProductName()
        {
            Dictionary<string, (int,int)> result = new();
            for (int i = 0; i < iap.Count; i++)
            {
                result.Add(iap[i].name, ( iap[i].type, iap[i].price));
            }
            return result;
        }

        public static Shop GetIAPKey(string name)
        {
            var item = shop.Find(x => x.name == name);
            return item;
        }
        #endregion

        #region LimitPack

        public static int LimitGiftOpenLevel()
        {
            return _config.LimitPack.OpenLevel;
        }

        public static string LimitGiftBuyKey()
        {
            return _config.LimitPack.BuyKey;
        }

        public static int LimitGiftTimer()
        {
            return  _config.LimitPack.Time;
        }

        public static int LimitGiftInterval()
        {
            return _config.LimitPack.Interval;
        }

        public static bool LimitGiftGuide()
        {
            return _config.LimitPack.HasGuide;
        }
        

        #endregion

        #region BeautyDraft

        public static int GetBeautyOpenLevel()
        {
            return _config.Beauty[0].OpenLevel;
        }

        public static bool CheckBeautyHintPop()
        {
            return StorageManager.Instance.GetStorage<BaseInfo>().Coin >= _config.Beauty[0].Condition;
        }
        public static List<Beauty> GetBeautyInfo()
        {
            return _config.Beauty;
        }

        private static Dictionary<string, string> beautyFirstPhoto;

        public static string GetBeautyKey(string name)
        {
            var storage = StorageManager.Instance.GetStorage<ActivityInfo>().BeautyInfo;
            if (beautyFirstPhoto == null || beautyFirstPhoto.Count == 0)
            {
                beautyFirstPhoto = new  Dictionary<string, string>();
                foreach (var info in storage.BeautyItemPhotos)
                {
                    string n = info.Value.photoNames[0];
                    beautyFirstPhoto.Add(n, info.Key);
                }
            }
            return beautyFirstPhoto[name];
        }
        private static void DealBeautyInfo()
        {
            var storage = StorageManager.Instance.GetStorage<ActivityInfo>().BeautyInfo;
            var name = activityPhotos[PhotoType.BeautyDraft];
            if (storage.BeautyItemPhotos.Count > 0 && (name.Count / 4) == storage.BeautyItemPhotos.Count)
            {
                return;
            }
            for (int index = 0; index < _config.Beauty.Count; index++)
            {
                var binfo =  _config.Beauty[index];

                if (storage.BeautyItemPhotos.ContainsKey(binfo.id.ToString()))
                {
                    continue;
                }
                var item = new BeautyItemPhotos();
                item.name = binfo.name;
                int start = index * 4;
                for (int j = 0; j < 4; j++)
                {
                    if (name.Count <= start + j)
                    {
                        storage.IsInit = true;
                        storage.ShowOrder = storage.BeautyItemPhotos.Keys.ToList();
                        return;
                    }
                    item.photoNames.Add(name[start + j]);
                }
                item.finishedCount = 0;
                storage.BeautyItemPhotos.Add(binfo.id.ToString(),item); 
            }
            storage.IsInit = true;
            storage.ShowOrder = storage.BeautyItemPhotos.Keys.ToList();
        }

        #endregion

        #region SecretGift

        public static List<SecretGift> GetSecretGiftInfo()
        {
            return _config.SecretGift;
        }

        public static List<SecretPack> GetSecretPackInfo()
        {
            return _config.SecretPack;
        }

        public static List<string> CheckSecretBuy()
        {
            List<string> names = new();
            var info = StorageManager.Instance.GetStorage<ActivityInfo>().SecretGiftInfo;
            foreach (var pack in _config.SecretPack)
            {
                if (info.Buys.ContainsKey(pack.name) && info.Buys[pack.name])
                {
                    continue;
                }
                names.Add(pack.name);
            }

            return names;
        }
        

        public static List<string> GetSecretGiftPhotos(string tag)
        {
            return activityPhotos[PhotoType.SecretGift].FindAll(x => GetPhotoByName(x).other == tag);
        }

        #endregion

        #region pass -checked

        public static void DealPassImage()
        {
            var pass = StorageManager.Instance.GetStorage<ActivityInfo>().PassInfo;
            var passphotos = activityPhotos[PhotoType.Pass];
            for (int i = 0; i < passphotos.Count; i++)
            {
                if (!pass.PassPhotoNames.Contains(passphotos[i]))
                {
                    pass.PassPhotoNames.Add(passphotos[i]);
                }
            }
        }
        #endregion

        public static void PreLoadActivityConfig()
        {
            DealBeautyInfo();
        }
    }
}