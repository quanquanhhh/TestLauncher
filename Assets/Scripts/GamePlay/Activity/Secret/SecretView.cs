// using System.Collections.Generic;
// using Foundation;
// using Foundation.Storage;
// using GameConfig;
// using GamePlay.Storage;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
// using XGame.Scripts.IAP;
// using Event = Foundation.Event;
//
// namespace GamePlay.Activity
// {
//     [Window("SecretView",WindowLayer.Popup)]
//     public class SecretView : UIWindow
//     {
//         [UIBinder("CloseBtn")] private Button closeBtn;
//         [UIBinder("Item")] private Transform item;
//         [UIBinder("Photos")] private GameObject photosTrans;
//         [UIBinder("Content")] private RectTransform _content;
//         private SecretLoop _loop;
//         public override void OnCreate()
//         {
//             base.OnCreate();
//             _content.offsetMax -= new Vector2(0, ViewUtility.AdjustTopHeight);
//             var info = GameConfigSys.GetSecretGiftInfo();
//             _loop = AddWidget<SecretLoop>(photosTrans, true,info);
//             closeBtn.onClick.AddListener(Close);
//         }
//
//         public void UpdateLoop()
//         {
//             _loop.UpdateAll();
//         }
//     }
//
//     public class SecretViewItem
//     { 
//         private TextMeshProUGUI name;
//         private Image photo;
//         private GameObject lockImg;
//         private Button OpenAllPhotos;
//         private GameConfig.SecretGift _config;
//         private List<string> photos = new ();
//         private Button BuyBtn;
//         private TextMeshProUGUI price;
//         public SecretViewItem(GameObject obj)
//         {
//              
//             name = obj.transform.Find("Tag/Name").GetComponent<TextMeshProUGUI>();
//             photo = obj.transform.Find("mask/Img").GetComponent<Image>();
//             lockImg = obj.transform.Find("Lock").gameObject;
//             OpenAllPhotos = obj.transform.Find("OpenAllPhoto").GetComponent<Button>();
//             OpenAllPhotos.onClick.AddListener(OpenSecretPhotos);
//             BuyBtn = obj.transform.Find("BuyBtn").GetComponent<Button>();
//             BuyBtn.onClick.AddListener(BuyFun);
//             price = BuyBtn.transform.Find("price").GetComponent<TextMeshProUGUI>();
//             
//         }
//
//         private void BuyFun()
//         {
//             IAPManager.Instance.Purchase(_config.BuyKey, () =>
//             {
//                 StorageManager.Instance.GetStorage<ActivityInfo>().SecretGiftInfo.Buys[_config.name] = true;
//                 BuyBtn.gameObject.SetActive(false);
//                 
//                 Event.Instance.SendEvent(new AddPhoto((int)PhotoType.SecretGift,photos.Count,photos, photoStatus: (int)PhotoState.Unlock)); 
//             });
//         }
//
//         private void OpenSecretPhotos()
//         {
//             UIModule.Instance.ShowAsync<SecretViewPhotos>(photos, _config);
//         }
//
//         public void UpdateContent(int index, SecretGift secretGift)
//         {
//             name.text = secretGift.name;
//             var config = GameConfigSys.GetSecretGiftPhotos(secretGift.Group);
//             var photoitem = GameConfigSys.GetPhotoByName(config[0]);
//             var bundle = GameConfigSys.GetPhotoAtlasName(config[0]);
//             string pname = GUtility.GetPhotoName(photoitem.name);
//             _config = secretGift;
//             photos = config;
//             photo.sprite = AssetLoad.Instance.LoadSprite(pname, bundle);
//             price.text = IAPManager.Instance.GetLocalizedPrice(secretGift.BuyKey);
//             bool buyed = StorageManager.Instance.GetStorage<ActivityInfo>().SecretGiftInfo.Buys.ContainsKey(_config.name) &&
//                 StorageManager.Instance.GetStorage<ActivityInfo>().SecretGiftInfo.Buys[_config.name];
//             BuyBtn.gameObject.SetActive(!buyed);
//             GUtility.ApplyAspect(photo.sprite, photo.transform.parent.GetComponent<RectTransform>(), photo.GetComponent<RectTransform>() );
//         }
//     }
// }