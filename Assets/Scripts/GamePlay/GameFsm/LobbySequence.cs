using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Foundation;
using Foundation.Sequence;
using Foundation.Storage;
using GameConfig;
using GamePlay.Storage;
using GamePlay.UIMain;
using UnityEngine;

namespace GamePlay
{
    public class LobbySequence : SingletonScript<LobbySequence>
    {
        BaseInfo _baseInfo => StorageManager.Instance.GetStorage<BaseInfo>();
        ActivityInfo _activityInfo => StorageManager.Instance.GetStorage<ActivityInfo>();
        DailyInfo _dailyInfo => StorageManager.Instance.GetStorage<DailyInfo>();
        private MainUI _mainUI => UIModule.Instance.Get<MainUI>();
        
        FSequence _sequence = new FSequence();
        private bool isFinished = false;
        public void OnStartLobbySequence()
        {
            if (_mainUI == null)
            {
                return;
            }
            isFinished = false;
            _sequence = new FSequence();
             _sequence.InitList(new List<SequenceNode>()
             { 
                 new SequenceNode(FirstPlayGuide, "FirstPlayGuide"),
                 new SequenceNode(LimitGiftPackGuide, "LimitGiftPackGuide"),
                 new SequenceNode(BeautyEncountersGuide, "BeautyEncountersGuide"),
                 new SequenceNode(PassGuide, "PassGuide"),
                 new SequenceNode(DailyChanllengeGuide, "DailyChanllengeGuide"),
                 new SequenceNode(SecretGiftGuide, "SecretGiftGuide"),
                 new SequenceNode(LuckyWheelGuide, "LuckyWheelGuide"),
                 new SequenceNode(DailyGiftGuide, "DailyGiftGuide"),
                 new SequenceNode(Finished, "Finished"),
                 
             });
             
             CancellationTokenSource cancel =  new CancellationTokenSource();
             _sequence.Start(cancel);
        }

        public void CutLobbySequence()
        {
            _sequence.Stop();
            _sequence = null;
        }
        private void FirstPlayGuide(UniTaskCompletionSource obj)
        {
            if (!_baseInfo.FinishedGuide.Contains("FirstPlayGuide") && 
                _baseInfo.Level == 0 && _baseInfo.CurrentLevel.Level < 0)
            {
                UIModule.Instance.ShowAsync<UIGuide>(_mainUI.playTrans.transform,new Vector2(0,-110f));
            }
            else
            { 
                obj.TrySetResult(); 
            }
        }


        private void Finished(UniTaskCompletionSource obj)
        {
            isFinished = true;
            obj.TrySetResult();
        }


        public bool FinishTask(string tag)
        {
            if (_sequence == null || isFinished)
            {
                return false;
            }
            return _sequence.FinishCurrentTask(tag);
        }

        private void LimitGiftPackGuide(UniTaskCompletionSource source)
        {
            if (!_activityInfo.LimitPackInfo.GuideFinished && GameConfigSys._activityGuides[PhotoType.LimitedGift] &&
                _mainUI.limitgift.activeInHierarchy)
            {
                _activityInfo.LimitPackInfo.GuideFinished = true;
                UIModule.Instance.ShowAsync<UIGuide>(_mainUI.limitgift.transform);
                return;
            }

            source.TrySetResult();
        }
        
        private void BeautyEncountersGuide(UniTaskCompletionSource source)
        {
            if (!_activityInfo.BeautyInfo.GuideFinished && GameConfigSys._activityGuides[PhotoType.BeautyDraft] &&
                _mainUI.Beauty.activeInHierarchy)
            {
                _activityInfo.BeautyInfo.GuideFinished = true;
                UIModule.Instance.ShowAsync<UIGuide>(_mainUI.Beauty.transform);
                return;
            }
            source.TrySetResult();
        }

        private void PassGuide(UniTaskCompletionSource source)
        {
            if (!_activityInfo.PassInfo.GuideFinished && GameConfigSys._activityGuides[PhotoType.Pass] && _mainUI.Pass.activeInHierarchy)
            {
                _activityInfo.PassInfo.GuideFinished = true;
                UIModule.Instance.ShowAsync<UIGuide>(_mainUI.Pass.transform);
                return;
            }
            source.TrySetResult();
        }

        private void DailyChanllengeGuide(UniTaskCompletionSource source)
        {
            if (!_dailyInfo.GuideFinished && _mainUI.daily.activeInHierarchy)
            {
                
                _dailyInfo.GuideFinished = true;
                UIModule.Instance.ShowAsync<UIGuide>(_mainUI.daily.transform);
                return;
            }
            source.TrySetResult();
        }
        

        private void SecretGiftGuide(UniTaskCompletionSource source)
        {
            if (!_activityInfo.SecretGiftInfo.GuideFinished && GameConfigSys._activityGuides[PhotoType.SecretGift] && _mainUI.SecretGift.activeInHierarchy)
            {
                _activityInfo.SecretGiftInfo.GuideFinished = true;
                UIModule.Instance.ShowAsync<UIGuide>(_mainUI.SecretGift.transform);
                return;
            }
            source.TrySetResult();
        }
        private void LuckyWheelGuide(UniTaskCompletionSource source)
        {
            if (!_activityInfo.LuckyWheelInfo.GuideFinished && GameConfigSys._activityGuides[PhotoType.LuckyWheel] && _mainUI.luckywheel.activeInHierarchy)
            {
                _activityInfo.LuckyWheelInfo.GuideFinished = true;
                UIModule.Instance.ShowAsync<UIGuide>(_mainUI.luckywheel.transform);
                return;
            }

            source.TrySetResult();
        }
        
        private void DailyGiftGuide(UniTaskCompletionSource source)
        {
            if (!_activityInfo.SignInfo.GuideFinished && GameConfigSys._activityGuides[PhotoType.Sign] && _mainUI.Sign.activeInHierarchy)
            {
                _activityInfo.SignInfo.GuideFinished = true;
                UIModule.Instance.ShowAsync<UIGuide>(_mainUI.Sign.transform);
                return;
            }
            source.TrySetResult();
        }
        
    }
}