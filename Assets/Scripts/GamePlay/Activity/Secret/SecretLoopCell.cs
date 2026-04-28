using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Foundation.Storage;
using GameConfig;
using GamePlay.Storage;
using GamePlay.UIMain.Widget;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XGame.Scripts.IAP;
using Event = Foundation.Event;
using Random = UnityEngine.Random;

namespace GamePlay.Activity
{
    public class SecretGiftItem //secret pack
    {
        private GameObject cell;
        private Button refreshBtn;
        private Button buyBtn;
        private GameObject bundle;
        private List<SecretPack> _packs;
        // private Dictionary<string, List<string>> photos = new();
        List<Dictionary<string, List<string>>> packPhotos = new (); // pack - names
        List<string> storageKey = new();
        private string key; 
        private int currentShowIndex = -1;
        public SecretGiftItem(GameObject cell)
        {
            this.cell = cell;
            refreshBtn = cell.transform.Find("refreshBtn").GetComponent<Button>();
            buyBtn = cell.transform.Find("BuyBtn").GetComponent<Button>();
            var price = buyBtn.transform.Find("price").GetComponent<TextMeshProUGUI>();
            bundle = cell.transform.Find("Bundle").gameObject;

            _packs = new List<SecretPack>(GameConfigSys.GetSecretPackInfo());
            
            
            key = _packs[0].BuyKey;
            price.text = IAPManager.Instance.GetLocalizedPrice(key);
            refreshBtn.onClick.AddListener(RefreshBtnFun);
            buyBtn.onClick.AddListener(BuyFun); 
            UpdateSelect();
        }

        private void RefreshBtnFun()
        {
            currentShowIndex = (currentShowIndex + 1) % packPhotos.Count;
            UpdateContent();
        }

        private void UpdateSelect()
        {
            int count = 0;
            packPhotos.Clear();
            storageKey.Clear();
            var temp = new List<SecretPack>(_packs);
 
            foreach (var packInfo in temp)
            {
                bool buy = StorageManager.Instance.GetStorage<ActivityInfo>().SecretGiftInfo.Buys
                               .ContainsKey(packInfo.name) &&
                           StorageManager.Instance.GetStorage<ActivityInfo>().SecretGiftInfo.Buys[packInfo.name];
                if (buy)
                {
                    continue;
                }
                var nitem = new Dictionary<string, List<string>>();
                for (int i = 0; i < packInfo.Group.Count; i++)
                {
                    string groupName = packInfo.Group[i];
                    string titleName = packInfo.showNames[i];
                    var names =GameConfigSys.GetSecretGiftPhotos(groupName); 
                    nitem[titleName] = names;
                } 
                packPhotos.Add(nitem);
                storageKey.Add(packInfo.name);
            }

            currentShowIndex = Random.Range(0, storageKey.Count);
        }
        private void BuyFun()
        {
            GameIAP.Purchase(key, () =>
            {
                List<string> getnames = new();

                var buyInfo = packPhotos[currentShowIndex]; 
                
                StorageManager.Instance.GetStorage<ActivityInfo>().SecretGiftInfo.Buys[storageKey[currentShowIndex]] = true;
                
                foreach (var pack in buyInfo)
                {
                    getnames.AddRange(pack.Value);
                }
                Event.Instance.SendEvent(new AddPhoto((int)PhotoType.SecretGift,getnames.Count, getnames, photoStatus: (int)PhotoState.Unlock));
                packPhotos.RemoveAt(currentShowIndex);
                storageKey.RemoveAt(currentShowIndex);
                currentShowIndex = Random.Range(0, storageKey.Count);
                
                
                var dic = new Dictionary<string, object>();
                dic["name"] = storageKey[currentShowIndex];
                dic["type"] = "iap";
                TBAMgr.Instance.SendLogEvent("secretbuy",dic);
                
                UpdateContent();
                
            },"secret");     
        }


        public void UpdateContent( )
        {
            if (packPhotos.Count == 0)
            {
                UIModule.Instance.Get<SecretView2>().UpdateLoopcell();
                return;
            } 
            currentShowIndex = Math.Max(0, currentShowIndex);
            // leftName = GameConfigSys.CheckSecretBuy();
            refreshBtn.gameObject.SetActive(packPhotos.Count > 1);

            var refreshInfo = packPhotos[currentShowIndex];
            int packIndex = 1;
            foreach (var pack in refreshInfo)
            {
                
                var trans = bundle.transform.Find(packIndex.ToString() +"/showPhoto");
                var cnt = bundle.transform.Find(packIndex.ToString() +"/cnt/count").GetComponent<TextMeshProUGUI>();
                var photo = trans.Find("photo");
                
                string name = GUtility.GetPhotoName(pack.Value[0]);
                var img = GameConfigSys.GetPhotoAtlasName(name);
                
                var sp = AssetLoad.Instance.LoadSprite(name, img);
                photo.GetComponent<Image>().sprite = sp;
                cnt.text = "x"+ pack.Value.Count;
                GUtility.ApplyAspect(sp, trans.GetComponent<RectTransform>(), photo.GetComponent<RectTransform>());
                packIndex++;
            }
        }
    }
    public class SecretGiftCell
    {
        private GameObject cell;
        private GameObject left;
        private GameObject right;
        private GameObject title;
        private int level;
        private SecretGift config1;
        private SecretGift config2;
        private List<string> names1;
        private List<string> names2;
        
        
        
        public SecretGiftCell(GameObject cell)
        {
            this.cell = cell;
            left = cell.transform.Find("left").gameObject;
            right = cell.transform.Find("right").gameObject;
            title = cell.transform.Find("title").gameObject;
            level = StorageManager.Instance.GetStorage<BaseInfo>().Level;
            
            left.transform.Find("buy").GetComponent<Button>().onClick.AddListener(BuyFirst);
            right.transform.Find("buy").GetComponent<Button>().onClick.AddListener(BuySecond);
            
            left.transform.Find("info").GetComponent<Button>().onClick.AddListener(() => { OpenView(names1, config1);});
            right.transform.Find("info").GetComponent<Button>().onClick.AddListener(() => { OpenView(names2, config2);});
        }

        private void OpenView(List<string> p0, SecretGift secretGift)
        {
            UIModule.Instance.ShowAsync<SecretViewPhotos>(p0, secretGift, this);
        }

        private void BuySecond()
        {
            
            GameIAP.Purchase(config2.BuyKey, () =>
            {
                
                var dic = new Dictionary<string, object>();
                dic["name"] = config2.name;
                dic["type"] = "iap";
                TBAMgr.Instance.SendLogEvent("secretbuy",dic);
                
                StorageManager.Instance.GetStorage<ActivityInfo>().SecretGiftInfo.Buys[config2.name] = true;
                right.transform.Find("buy").gameObject.SetActive(false);
                Event.Instance.SendEvent(new AddPhoto((int)PhotoType.SecretGift,names2.Count, names2, photoStatus: (int)PhotoState.Unlock));
            },"secret");     
        }

        private void BuyFirst()
        {
            GameIAP.Purchase(config1.BuyKey, Isbuyed,"secret");
        }


        public void UpdateContent(int index, SecretGift finfo, SecretGift sinfo, bool showTitle)
        {
            title.SetActive(showTitle);
            left.SetActive(finfo != null);
            right.SetActive(sinfo != null); 
            SetPhoto(finfo, left, 1);
            SetPhoto(sinfo, right,2);
        }
 
        private void SetPhoto(SecretGift info, GameObject trans, int index)
        {
            if (info == null)
            {
                return;
            }
            var mask = trans.transform.Find("img");
            var photo = mask.Find("photo").GetComponent<Image>();
            var names =GameConfigSys.GetSecretGiftPhotos(info.Group);
            trans.transform.Find("cnt/count").GetComponent<TextMeshProUGUI>().text = "x" + names.Count;
            if (index == 1)
            {
                names1 = names;
                config1 = info;
            }
            else if(index == 2)
            {
                names2 = names;
                config2 = info;
            }
            string name = GUtility.GetPhotoName(names[0]);
            var img = GameConfigSys.GetPhotoAtlasName(name);
            var sp = AssetLoad.Instance.LoadSprite(name, img);
            photo.GetComponent<Image>().sprite = sp;
            GUtility.ApplyAspect(sp, mask.GetComponent<RectTransform>(), photo.GetComponent<RectTransform>());
            
            bool open = level >= info.lockLevel;
            bool buy = StorageManager.Instance.GetStorage<ActivityInfo>().SecretGiftInfo.Buys.ContainsKey(info.name) &&
                       StorageManager.Instance.GetStorage<ActivityInfo>().SecretGiftInfo.Buys[info.name];
            trans.transform.Find("lock").gameObject.SetActive(!open);
            trans.transform.Find("buy").gameObject.SetActive(open && !buy);
            trans.transform.Find("buy/price").GetComponent<TextMeshProUGUI>().text = 
                IAPManager.Instance.GetLocalizedPrice(info.BuyKey);
            trans.transform.Find("lock/level").GetComponent<TextMeshProUGUI>().text =
                "Lv. " + info.lockLevel;

            
        }

        public void Isbuyed()
        { 
            
            var dic = new Dictionary<string, object>();
            dic["name"] = config1.name;
            dic["type"] = "iap";
            TBAMgr.Instance.SendLogEvent("secretbuy",dic);
            
            StorageManager.Instance.GetStorage<ActivityInfo>().SecretGiftInfo.Buys[config1.name] = true;
            left.transform.Find("buy").gameObject.SetActive(false);
            Event.Instance.SendEvent(new AddPhoto((int)PhotoType.SecretGift,names1.Count,names1, photoStatus: (int)PhotoState.Unlock)); 
        }
    }
}