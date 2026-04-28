using System.Collections.Generic;
using DG.Tweening;
using Foundation;
using Foundation.AudioModule;
using Foundation.GridViewLoop;
using Foundation.Storage;
using GameConfig;
using GamePlay.Component;
using GamePlay.Storage;
using GamePlay.UIMain.Widget;
using GamePlay.Utility;
using Spine.Unity;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using Event = Foundation.Event;

namespace GamePlay.UIMain
{
    [Window("PhotoShow", WindowLayer.Popup)]
    public class ShowPhoto : UIWindow
    {
        [UIBinder("Content")] private  GameObject content;
        [UIBinder("Playgroup")] private  GameObject Playgroup;
        
        
        // [UIBinder("Coin")] private GameObject coin;
        // [UIBinder("Diamond")] private GameObject diamond;
        
        
        [UIBinder("Btn", "Time")] private Button likeBtn;
        [UIBinder("Text", "Time")] private TextMeshProUGUI time;
        [UIBinder("CloseBtn")] private Button closebtn;
        [UIBinder("PlayBtn")] private Button PlayBtn;
        [UIBinder("PauseBtn")] private Button PauseBtn;
        [UIBinder("like")] private GameObject like;
        [UIBinder("Bottom")] private Transform bottomGroup;

        [UIBinder("AdDownload")] private Button adDownload;
        [UIBinder("VipDownload")] private Button vipDownload;
        [UIBinder("SetBackGround")] private Button setBackGround;
        [UIBinder("ResetBackGround")] private Button resetBackGround;
        [UIBinder("Download")] private Button costDownload;
        [UIBinder("vipSpine")] private SkeletonGraphic vipSpine;


        private List<string> names   = new List<string>();
        private int currentIndex;
        private PhotoItem _photoItem;
        private ShowPhotosContent uicoverFlow;
        
        public override void OnCreate()
        {
            base.OnCreate();
            _photoItem = (PhotoItem)userDatas[0];
            int showIndex = 0;
            if (userDatas.Length > 1)
            {
                names = (List<string>)userDatas[1];
                CheckLock();
                showIndex = names.FindIndex(x => x == _photoItem.Name);
            }
            else
            {
                names.Add(_photoItem.Name);
            }
            closebtn.onClick.AddListener(() =>
            {
                
                AudioModule.Instance.ClickAudio();
                Close();
            });
            like.SetActive(_photoItem.isLike);
           likeBtn.onClick.AddListener(ChangeLike);
           Playgroup.SetActive(names.Count > 1);
           adDownload.onClick.AddListener(AdDownloadFun);
           setBackGround.onClick.AddListener(SetBackGroundFun);
           resetBackGround.onClick.AddListener(ResetBackGroundFun);
           costDownload.onClick.AddListener(CostDownloadFun);

           uicoverFlow = AddWidget<ShowPhotosContent>(content, true, names,showIndex);
           
           // AddWidget<DiamondWidget>(diamond);
           // AddWidget<CoinWidget>(coin);
           vipDownload.onClick.AddListener(() =>
           {
               DownloadPhoto("vip");
           });
           vipDownload.gameObject.SetActive(UserUtility.IsVip);
           adDownload.gameObject.SetActive(!UserUtility.IsVip);
           costDownload.gameObject.SetActive(!UserUtility.IsVip);
           PlayBtn.onClick.AddListener(PlayPhoto);
           PauseBtn.onClick.AddListener(PausePhoto);
           
           ShowBackGrounBtn();

           if (ViewUtility.UISize.x < 1080f)
           {
               var s = ViewUtility.GetEnoughXScale();
               bottomGroup.localScale = Vector3.one * s;
           }
           if (UserUtility.IsVip)
           {
               vipSpine.initialSkinName = "vip" + (UserUtility.IsLifeVip ? 1 : 2);
               vipSpine.Initialize(true);
           }

           PlayBtn.gameObject.SetActive(names.Count>1);
           time.text = _photoItem.time;
           ChangePlayBtn(true);
        }

        private void PausePhoto()
        {
            ChangePlayBtn(true);
            uicoverFlow.Pause();
        }


        private void ChangePlayBtn(bool showPlay)
        {

            bottomGroup.GetComponent<RectTransform>().DOKill();
            bottomGroup.GetComponent<RectTransform>().DOAnchorPosY(showPlay ? 0 : -365, 0.5f);
            // diamond.GetComponent<RectTransform>().DOKill();
            // diamond.GetComponent<RectTransform>().DOAnchorPosY(showPlay? 0 : 128, 0.5f);
            closebtn.GetComponent<RectTransform>().DOKill();
            closebtn.GetComponent<RectTransform>().DOAnchorPosY(showPlay?0:128, 0.5f);
            PlayBtn.gameObject.SetActive(showPlay);
            PauseBtn.gameObject.SetActive(!showPlay);
        }
        private void PlayPhoto()
        {
            ChangePlayBtn(false);

            uicoverFlow.Play();
        }

        private void CheckLock()
        {
            List<string> result = new();
            var info = StorageManager.Instance.GetStorage<PhotoInfo>().PhotoState;
            for (int i = 0; i < names.Count; i++)
            {
                var name = names[i];
                if ( info.ContainsKey(name) && info[name].State == (int)PhotoState.Unlock)
                {
                    result.Add(name);
                }
            }

            names = result;
        }


        public override void Close()
        {
            var c = UIModule.Instance.Get<AlbumsView>();
            if (c!=null)
            {
                c.UpdateAllPanel();
            }
            base.Close();
        }

        private void CostDownloadFun()
        {
            AudioModule.Instance.ClickAudio();
            if (GUtility.IsEnoughItem(ItemType.Diamond, GameConfigSys.baseGame.DownloadCostAmount))
            {
                Event.Instance.SendEvent(new SubItem((int)ItemType.Diamond,  GameConfigSys.baseGame.DownloadCostAmount));
                DownloadPhoto("cost");
            }
        }

        private void ResetBackGroundFun()
        {
            AudioModule.Instance.ClickAudio();
            
            Event.Instance.SendEvent(new ChangeBackGround());
            StorageManager.Instance.GetStorage<BaseInfo>().SelectBG = "";
            ShowBackGrounBtn();
            
        }

        private void SetBackGroundFun()
        {
            AudioModule.Instance.ClickAudio();
            StorageManager.Instance.GetStorage<BaseInfo>().SelectBG = _photoItem.Name;
            Event.Instance.SendEvent(new ChangeBackGround());
            ShowBackGrounBtn();
        }

        private void ShowBackGrounBtn()
        {
            bool showset = _photoItem.Name != StorageManager.Instance.GetStorage<BaseInfo>().SelectBG;
            setBackGround.gameObject.SetActive(showset);
            resetBackGround.gameObject.SetActive(!showset);
        }

        private void AdDownloadFun()
        {
            AudioModule.Instance.ClickAudio();
            AdMgr.Instance.PlayRV(() =>
            {
                DownloadPhoto("rv");
            },"DownloadPhoto"); 
            
        }
        private void DownloadPhoto(string from)
        {
            AudioModule.Instance.ClickAudio();
            bool ok = false;
            var isvideo = _photoItem.Name.ToLower().Contains("mp4");
             
            if (isvideo)
            {
                ok = ResTool.SaveCachedVideoToPhone(_photoItem.Name);
            }
            else
            {
                ok = ResTool.SaveCachedMediaToPhone( _photoItem.Name);
            }

            if (ok)
            {
                var dic = new Dictionary<string, object>();
                dic["name"] = _photoItem.Name;
                dic["from"] = from;
                TBAMgr.Instance.SendLogEvent("download", dic);
            }
                
            Debug.Log("保存到手机结果: " + ok); 
        }
        private void ChangeLike()
        {
            AudioModule.Instance.ClickAudio();
            _photoItem.isLike = !_photoItem.isLike;
            _photoItem.likeindex = StorageManager.Instance.GetStorage<PhotoInfo>().LikeCount;
            StorageManager.Instance.GetStorage<PhotoInfo>().LikeCount += _photoItem.isLike ? 1 : -1;
            like.SetActive(_photoItem.isLike);
            if (_photoItem.isLike)
            {
                var dic = new Dictionary<string, object>();
                dic["name"] = _photoItem.Name.ToString(); 
                TBAMgr.Instance.SendLogEvent("like", dic);
            }
        }

        public void UpdatePhotoIndex(int index, string name)
        {
            
            var photoInfo = StorageManager.Instance.GetStorage<PhotoInfo>();
            _photoItem = photoInfo.PhotoState[name];
            like.SetActive(_photoItem.isLike); 
            ShowBackGrounBtn();
            
            time.text = _photoItem.time;
            
        }
    }
}