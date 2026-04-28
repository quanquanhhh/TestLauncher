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
//     [Window("SecretGift", WindowLayer.Popup)]
//     public class SecretGiftView : UIWindow
//     {
//         [UIBinder("CloseBtn")] private Button CloseBtn;
//         [UIBinder("BuyBtn")] private Button BuyBtn;
//         [UIBinder("price")] private TextMeshProUGUI price;
//         [UIBinder("layout")] private Transform layout;
//         private List<SecretGift> _config;
//         private List<string> photos;
//         public override void OnCreate()
//         {
//             base.OnCreate();
//             CreateCell();
//             CloseBtn.onClick.AddListener(CloseFun);
//             price.text = IAPManager.Instance.GetLocalizedPrice(_config.BuyKey);
//             BuyBtn.onClick.AddListener(BuyPack);
//         }
//
//         private void CloseFun()
//         {
//             
//             LobbySequence.Instance.FinishTask("SecretGiftGuide");
//         }
//
//         private void BuyPack()
//         {
//             IAPManager.Instance.Purchase(_config.BuyKey, () =>
//             {
//                 StorageManager.Instance.GetStorage<ActivityInfo>().SecretGiftInfo.IsBuySecret = true;
//                 Event.Instance.SendEvent(new AddPhoto((int)PhotoType.SecretGift, photos.Count,photos,
//                     () =>
//                     {
//                         LobbySequence.Instance.FinishTask("SecretGiftGuide");
//                     }, true));
//                 Event.Instance.SendEvent(new BuySecret());
//                 Close();
//                 
//             });
//         }
//
//         private async void CreateCell()
//         {
//             photos = GameConfigSys.GetNoShowPhotos(PhotoType.SecretGift);
//             _config = GameConfigSys.GetSecretGiftInfo();
//
//             for (int index = 0; index < 4; index++)
//             {
//                 var item = layout.Find(index.ToString());
//                 item.Find("tag").GetComponent<TextMeshProUGUI>().text = _config.PackName[index];
//                 item.Find("mask/count").GetComponent<TextMeshProUGUI>().text = "x" + _config.PackCount[index];
//                 string str = photos[index];
//                 str = GUtility.GetPhotoName(str);
//                 string atlas =  GameConfigSys.GetPhotoAtlasName(str);
//                 var sp = AssetLoad.Instance.LoadSprite(str,  atlas);
//                 // var p = await AssetLoad.Instance.LoadAsset<Texture2D>(str);
//                 item.Find("pmask/photo").GetComponent<Image>().sprite = sp;
//                 GUtility.ApplyAspect(sp, item.Find("pmask").GetComponent<RectTransform>(),item.Find("pmask/photo").GetComponent<RectTransform>());
//             }
//         }
//     }
// }