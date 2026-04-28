using System;
using System.Collections.Generic;
using Foundation;
using Foundation.AudioModule;
using Foundation.Storage;
using GamePlay.Component;
using GamePlay.Storage;
using GamePlay.Utility;
using UnityEngine;
using UnityEngine.UI;
using Event = Foundation.Event;

namespace GamePlay.Game.Popup
{
    [Window("GetPhoto", WindowLayer.Popup)]
    public class GetPhoto : UIWindow
    {
        [UIBinder("Img")] private Transform Img;
        [UIBinder("Download")] private Button download;
        [UIBinder("VipDownload")] private Button vipDownload;
        [UIBinder("Claim")] private Button closeBtn;

        private string from = "";
        protected string photoname;
        private bool isvideo;
        private Action endAction = null;
        public override void OnCreate()
        {
            base.OnCreate();
            var media = Img.TryGetOrAddComponent<UguiMediaSource>();
            
            photoname = (string)userDatas[0];
            if (userDatas.Length >= 2)
            {
                endAction = (Action)userDatas[1];
            }
            
            if (userDatas.Length >= 3)
            {
                from = (string)userDatas[2];
            }
            download.gameObject.SetActive(from == "level");
            isvideo = photoname.ToLower().Contains("mp4");
            media.SetSource(GUtility.GetPhotoName(photoname), isvideo);
            
            
            closeBtn.onClick.AddListener(ContinueFun);
            download.onClick.AddListener(DownloadPhoto);
            vipDownload.onClick.AddListener(VipDownloadPhoto);
            download.gameObject.SetActive(!UserUtility.IsVip);
            vipDownload.gameObject.SetActive(UserUtility.IsVip);
        }

        private async void VipDownloadPhoto()
        {
            
            AudioModule.Instance.ClickAudio();
            
            bool ok = SaveImg();
            //check download 
            if (!ok)
            {
                await XResDownloadQueue.TryGetXRes(photoname);
                ok = SaveImg();
            }

            if (ok)
            {
                
                var dic = new Dictionary<string, object>();
                dic["name"] = photoname;
                dic["from"] = "vip";
                TBAMgr.Instance.SendLogEvent("download", dic);
            }
            ContinueFun();
        }

        private bool SaveImg()
        {
            bool ok = false;
            ok = ResTool.SaveCachedMediaToPhone(photoname); 

            return ok;
        }
        private void DownloadPhoto()
        {
            AudioModule.Instance.ClickAudio();
            AdMgr.Instance.PlayRV(async () =>
            {
                bool ok = SaveImg();
                if (!ok)
                {
                    await XResDownloadQueue.TryGetXRes(photoname);
                    ok = SaveImg();
                }
                if (ok)
                {
                    
                    var dic = new Dictionary<string, object>();
                    dic["name"] = photoname;
                    dic["from"] = "rv";
                    TBAMgr.Instance.SendLogEvent("download", dic);

                    
                    Event.Instance.SendEvent("Save success!");
                }
                ContinueFun();
            },"DownloadPhoto"); 
        }

        private void ContinueFun()
        {
            endAction?.Invoke();
            AudioModule.Instance.ClickAudio();
            Close();
        }
    }
}