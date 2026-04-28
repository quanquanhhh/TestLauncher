using Foundation;
using Foundation.AudioModule;
using Foundation.Storage;
using GamePlay.Storage;
using UnityEngine;
using UnityEngine.UI;
using XGame.Scripts.IAP;
using Event = Foundation.Event;

namespace GamePlay.UIMain
{
    [Window("MainSetting", WindowLayer.Popup)]
    public class UISetting : UIWindow
    {
        [UIBinder("CloseBtn")] private Button closeBtn;
        
        [UIBinder("Btn","music")] private Button  musicBtn;
        [UIBinder("Btn","Audio")] private Button  audioBtn;
        [UIBinder("Btn","Shake")] private Button  shakeBtn;
        [UIBinder("Btn","AutoSize")] private Button  autoSizeBtn;

        [UIBinder("Switch", "music")] private Transform musicOn;
        [UIBinder("Switch", "Audio")] private Transform audioOn;
        [UIBinder("Switch", "Shake")] private Transform shakeOn;
        [UIBinder("Switch", "AutoSize")] private Transform autosizeOn;
        
        [UIBinder("Language")] Button setLanguageBtn;
        [UIBinder("Connectus")] Button connectUsBtn;
        [UIBinder("Privacy")] Button privacyBtn;

        [UIBinder("Restart")] private Button restartBtn;
        [UIBinder("Restore")] private Button restoreBtn;


        private bool isfromHome;
        private bool ismusic;
        private bool isaudio;
        private bool isshake;
        private bool isAutoSize;
        public override void OnCreate()
        {
            base.OnCreate();
            closeBtn.onClick.AddListener(Close);
            musicBtn.onClick.AddListener(OnMusicFun);
            audioBtn.onClick.AddListener(OnAudioFun);
            shakeBtn.onClick.AddListener(OnShakeFun);
            autoSizeBtn.onClick.AddListener(OnAutoSizeFun);
            
            setLanguageBtn.onClick.AddListener(OpenLanguage);
            connectUsBtn.onClick.AddListener(ConnectUsFun);
            privacyBtn.onClick.AddListener(PrivacyFun);

            isfromHome = (string)userDatas[0] == "home";
            ismusic = StorageManager.Instance.GetStorage<BaseInfo>().Setting["music"];
            isaudio = StorageManager.Instance.GetStorage<BaseInfo>().Setting["audio"];
            isshake = StorageManager.Instance.GetStorage<BaseInfo>().Setting["shake"];
            isAutoSize = StorageManager.Instance.GetStorage<BaseInfo>().Setting["autosize"];
            
            restartBtn.gameObject.SetActive(!isfromHome);
            restoreBtn.gameObject.SetActive(isfromHome);
            
            restartBtn.onClick.AddListener(RestartFun);
            restoreBtn.onClick.AddListener(RestoreFun);
            ChangeShow("music");
            ChangeShow("audio");
            ChangeShow("shake");
            ChangeShow("autosize");
        }

        private void RestoreFun()
        {
            IAPManager.Instance.RestorePurchases(true);
        }

        private void PrivacyFun()
        {
            UIModule.Instance.ShowAsync<UIPrivacy>();
        }

        private void ConnectUsFun()
        {
            AudioModule.Instance.ClickAudio();
            GUtility.ToEmail();
        }

        private void OpenLanguage()
        {
            AudioModule.Instance.ClickAudio();
            UIModule.Instance.ShowAsync<UILanguage>();
        }

        private void OnAutoSizeFun()
        {
            
            isAutoSize = !isAutoSize;
            AudioModule.Instance.ClickAudio();
            ChangeShow("autosize");
        }

        private void RestartFun()
        {
            AudioModule.Instance.ClickAudio();
            Close();
            Event.Instance.SendEvent(new ReplayGame());
        }

        public override void Close()
        {
            StorageManager.Instance.GetStorage<BaseInfo>().Setting["music"] = ismusic;
            StorageManager.Instance.GetStorage<BaseInfo>().Setting["audio"]=  isaudio;
            StorageManager.Instance.GetStorage<BaseInfo>().Setting["shake"] = isshake;
            StorageManager.Instance.GetStorage<BaseInfo>().Setting["autosize"] = isAutoSize;
            AudioModule.Instance.ClickAudio();
            base.Close();
        }


        private void OnShakeFun()
        {
            isshake = !isshake;
            AudioModule.Instance.ClickAudio();
            ChangeShow("shake");
        }

        private void OnAudioFun()
        {
            isaudio = !isaudio;
            AudioModule.Instance.ClickAudio();
            AudioModule.Instance.SetPlayAudioOn(isaudio);
            ChangeShow("audio");
        }

        private void OnMusicFun()
        {
            ismusic = !ismusic;
            AudioModule.Instance.ClickAudio();
            AudioModule.Instance.SoundOpen(ismusic);
            ChangeShow("music");
        }

        private void ChangeShow(string stage)
        {
            bool isshow = false;
            Transform check = null;
            switch (stage)
            {
                case "music":
                    check = musicOn;
                    isshow = ismusic;
                    break;
                case "audio":
                    check = audioOn;
                    isshow = isaudio;
                    break;
                case "shake":
                    check = shakeOn;
                    isshow = isshake;
                    break;
                case "autosize":
                    check = autosizeOn;
                    isshow = isAutoSize;
                    break;
            }

            check.Find("On").gameObject.SetActive(isshow);
            check.Find("Off").gameObject.SetActive(!isshow);
        }
    }
}