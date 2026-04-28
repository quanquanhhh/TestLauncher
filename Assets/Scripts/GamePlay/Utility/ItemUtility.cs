using System;
using System.Collections.Generic;
using Foundation;
using Foundation.Storage;
using GameConfig;
using GamePlay.Game.Popup;
using GamePlay.Storage; 

namespace GamePlay
{
    public class ItemUtility : SingletonScript<ItemUtility>
    {
        public void OnInit()
        {
            Event.Instance.Subscribe<AddItem>(OnAddItem);
            Event.Instance.Subscribe<SubItem>(OnSubItem);
            Event.Instance.Subscribe<AddPhoto>(OnAddPhoto);
            Event.Instance.Subscribe<RestoreBuff>(OnRestore);
        }

        private void OnRestore(RestoreBuff obj)
        {
            if (obj.productid == "monthly_vip")
            {
                StorageManager.Instance.GetStorage<BaseInfo>().Buff.IsVip = true;
                StorageManager.Instance.GetStorage<BaseInfo>().Buff.RemoveAds = true;
                StorageManager.Instance.GetStorage<BaseInfo>().Buff.VipExpire = obj.expire;
            }
            else if (obj.productid == "permanentvip" || obj.productid == "permanentVip")
            {
                StorageManager.Instance.GetStorage<BaseInfo>().Buff.IsVip = true;
                StorageManager.Instance.GetStorage<BaseInfo>().Buff.RemoveAds = true;
                StorageManager.Instance.GetStorage<BaseInfo>().Buff.IsPermanent = true;
            }
            else if (obj.productid == "remove_ads" || obj.productid == "remove_ads_pack")
            {
                StorageManager.Instance.GetStorage<BaseInfo>().Buff.RemoveAds = true;
                StorageManager.Instance.GetStorage<BaseInfo>().Buff.PermanentRemoveAds = true;
            }
        }

        // public bool HasEnoughPhotoInShop(int count, PhotoType from)
        // {
        //     return GameConfigSys.CheckEnoughShopPhoto(count,from); 
        // }
        private void AddPhoto(string name, int from, int state)
        {
            var info = StorageManager.Instance.GetStorage<PhotoInfo>();
            if (!info.PhotoState.ContainsKey(name))
            {
                PhotoItem n = new PhotoItem();
                n.Name = name;
                n.index = info.AlbumsCount;
                n.State = state;
                n.from = from; 
                info.PhotoState.Add(name, n);
            }
            else
            {
                info.PhotoState[name].index = info.AlbumsCount;
                info.PhotoState[name].State = state;
            }
            info.PhotoState[name].time = DateTime.Now.Year+"." + DateTime.Now.Month + "."+ DateTime.Now.Day ;
        }
        private void OnAddPhoto(AddPhoto obj)
        {
            List<string> names = new();
            if (obj.photoname != null &&  obj.photoname.Count > 0)
            { 
                for (int index = 0; index < obj.photoname.Count; index++)
                {
                    AddPhoto(obj.photoname[index], obj.photofrom, obj.photoStatus);
                }

                names = obj.photoname;

            }
            else
            {
                //需要随机
                // var items = GameConfigSys.GetUnClaimPhoto(obj.photofrom);
                var items = GameConfigSys.activityPhotos[(PhotoType)obj.photofrom];
                int count = Math.Min(items.Count, obj.count);
                int checkindex = 0;
                while (count > 0 && checkindex < items.Count)
                {
                    if (!GameConfigSys.HasPhoto(items[checkindex]))
                    {
                        AddPhoto(items[checkindex], obj.photofrom, obj.photoStatus); 
                        names.Add(items[checkindex]);
                        count--;
                    }
                    checkindex++;
                } 
            }

            if (!obj.showpop)
            {
                obj.endAction?.Invoke();
                return;
            }
            if (names.Count == 1)
            {
                UIModule.Instance.ShowAsync<GetPhoto>(names[0], obj.endAction);
            }
            else if(names.Count > 1)
            {
                UIModule.Instance.ShowAsync<GetPhotos>(names, obj.endAction);
            }
        }

        private void OnSubItem(SubItem obj)
        {
            var info = StorageManager.Instance.GetStorage<BaseInfo>();
            int amount = obj.itemCount;
            switch ((ItemType)obj.itemType)
            {
                case  ItemType.Coin:
                    info.Coin -= amount;
                    break;
                case ItemType.Diamond:
                    info.Diamond -= amount;
                    break;
            }
            Event.Instance.SendEvent(new ItemCountChangeShow((int)obj.itemType));
            
        }

        private void OnAddItem(AddItem obj)
        {
            int amount = obj.itemCount;
            var info = StorageManager.Instance.GetStorage<BaseInfo>();
            switch ((ItemType)obj.itemType)
            {
                case  ItemType.Coin:
                    info.Coin += amount;
                    break;
                case ItemType.Diamond:
                    info.Diamond += amount;
                    break;
                case ItemType.RandomProp:
                case ItemType.RemoveProp:
                case ItemType.UndoProp:
                    info.Currency[obj.itemType] += amount;
                    break;
                case ItemType.RemoveAds:
                    StorageManager.Instance.GetStorage<BaseInfo>().Buff.RemoveAds = true;
                    StorageManager.Instance.GetStorage<BaseInfo>().Buff.PermanentRemoveAds = true;
                    break;
            }
            Event.Instance.SendEvent(new ItemCountChangeShow((int)obj.itemType, true, obj.startPos ));
             
        }

        public void AddVipPropLeftCount()
        {
            BaseInfo storage = StorageManager.Instance.GetStorage<BaseInfo>();
            var vipLeftTimes = !storage.Buff.IsVip ? 0 :
                storage.Buff.IsPermanent
                    ? GameConfigSys.vips[1].VipFreePropCount
                    : GameConfigSys.vips[0].VipFreePropCount;
            
            StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.LeftVipPropCount[(int)ItemType.RemoveProp] =
                vipLeftTimes;
            StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.LeftVipPropCount[(int)ItemType.UndoProp] =
                vipLeftTimes;
            StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.LeftVipPropCount[(int)ItemType.RandomProp] =
                vipLeftTimes; 
        }

        public void OpenPurcher(string buykey, Action callback)
        {
            callback?.Invoke();
        }
        
        
 
    }
    
}