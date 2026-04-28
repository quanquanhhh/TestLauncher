using Foundation;
using Foundation.Storage;
using GameConfig;
using GamePlay.Storage;
using GamePlay.UIMain.Shop;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Event = UnityEngine.Event;

namespace GamePlay
{
    public class PropWidget : UIWidget
    {
        // [UIBinder("")] private Button btn;
        [UIBinder("ImgAdd")] private GameObject  imgAdd;
        [UIBinder("ImgNum")] private GameObject  imgNum;
        [UIBinder("TextNum")] private  TextMeshProUGUI  textNum;
        [UIBinder("SpineMonthly")] private GameObject   monthVIP;
        [UIBinder("SpineLifetime")] private GameObject  lifeVIP;

        private ItemType _itemType;
        
        public int vipLeftTimes;

        private int leftCount;
        
        BaseInfo storage = StorageManager.Instance.GetStorage<BaseInfo>();
        public override void OnCreate()
        {
            base.OnCreate();
            _itemType = (ItemType)userDatas[0];

            vipLeftTimes = storage.CurrentLevel.LeftVipPropCount.ContainsKey((int)_itemType)? storage.CurrentLevel.LeftVipPropCount[(int)_itemType] : 0;
            UpdateCount();
            SubScribeEvent<UpdatePropCount>(OnUpdatePropCount);
            // btn.onClick.AddListener(OpenShop);
        }

        private void OnUpdatePropCount(UpdatePropCount obj)
        {
            UpdateCount();
            
        }

        // private void OpenShop()
        // {
        //     if (leftCount + vipLeftTimes == 0)
        //     {
        //         UIModule.Instance.ShowAsync<UIShop>();
        //     }
        // }

        public void SubCount()
        {
            if (vipLeftTimes > 0)
            {
                vipLeftTimes--;
                storage.CurrentLevel.LeftVipPropCount[(int)_itemType]--;
            }
            else
            {
                storage.Currency[(int)_itemType]--;
            }
        }
        public bool IsEnough()
        {
            return vipLeftTimes > 0 || storage.Currency[(int)_itemType] > 0; 
        }
        public void UpdateCount()
        {
            var currency = storage.Currency;
            if (!currency.ContainsKey((int)_itemType))
            {
                currency.Add((int)_itemType, 0);
            }
            int count = currency[(int)_itemType];
            if (count > 0)
            {
                textNum.text = count.ToString();
            }
            imgNum.SetActive(count > 0);
            imgAdd.SetActive(count == 0);
            leftCount = count;
            if (storage.Buff.IsVip && vipLeftTimes > 0)
            {
                monthVIP.SetActive(!storage.Buff.IsPermanent);
                lifeVIP.SetActive(storage.Buff.IsPermanent);
            }
            else
            {
                monthVIP.SetActive(false);
                lifeVIP.SetActive(false);
            }
            
        }
    }
}