using System;
using System.Collections.Generic;
using DG.Tweening;
using Foundation;
using Foundation.FSM;
using Foundation.Statistics;
using Foundation.Storage;
using GamePlay.Storage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Event = Foundation.Event;

namespace GamePlay.LauncherFsm
{
    public class LauncherFsm
    {
        
        private IFsmManager _fsmManager;
        private IFsm<LauncherFsm> _fsm;

        
        // public TextMeshProUGUI debugText;

        public float accumulateProgress = 0.0f;
        public float displayProgress = 0.0f;

        public string debugInfo;
        public GameObject LoadingView;
        
        private float _progressVelocity = 0f;
        private const float SmoothTime = 1f;   // 越小越快，越大越柔和
        private const float MaxSpeed = 100f;

        private bool isDebug = false;

        private Dictionary<string,List<string>> checkTags = new();  

        public Type[] fsmStateTypes =
        {
            typeof(LauncherSetUpView),
            typeof(LauncherGetResourceVersion),
            typeof(LauncherInitializeAsset),
            typeof(LauncherCheckConfig),
            typeof(LauncherDownload),
            typeof(LauncherUpdateOver),
            typeof(LauncherGame),
        };

        private DOTween _doTween;

        public void ShowInitializeText(float progress)
        {
            if (LoadingView != null)
            {
                var slider = LoadingView.transform.Find("ProgressSlider").GetComponent<Slider>();
                var textMeshProugui = LoadingView.transform.Find("ProgressSlider/ProgressText")
                    .GetComponent<TextMeshProUGUI>();
                float clampedProgress = Mathf.Clamp(progress, 0f, 100f);
                slider.value = clampedProgress / 100;
                textMeshProugui.text = $"Loading... {Mathf.FloorToInt(clampedProgress)}%";
            }
        }

        public void ShowDebugInfo(string info)
        {
#if !DEVELOPMENT_BUILD 
            return;
#endif
            
            if (LoadingView != null)
            { 
                var textMeshProugui = LoadingView.transform.Find("ProgressSlider/DebugInfo")
                    .GetComponent<TextMeshProUGUI>();
                textMeshProugui.text = info;
            }
        }
        private void InitRoot()
        {
            var mgr = GameObject.Find("Mgr");
            RootManager.MgrRoot = mgr;
            RootManager.UIRoot = GameObject.Find("Root").transform;  
            GameObject.DontDestroyOnLoad(RootManager.MgrRoot);
            GameObject.DontDestroyOnLoad(RootManager.UIRoot);
            RootManager.UIRoot = GameObject.Find("Root/UIRoot/UICanvas").transform;

            GameViewComponent._loadingLuncher = mgr.GetComponent<LoadingLuncher>(); 

        }

        public void Initialize()
        {
            _fsmManager = new FsmManager();

            var fsmStates = new FsmState<LauncherFsm>[fsmStateTypes.Length];

            for (var i = 0; i < fsmStateTypes.Length; i++)
            {
                fsmStates[i] = (FsmState<LauncherFsm>) Activator.CreateInstance(fsmStateTypes[i]);
            }

            InitRoot();
            _fsm = _fsmManager.CreateFsm("Launcher", this, fsmStates);
            Event.Instance.Subscribe<GameUIFinished>(OnGameUIFinished);
        }

        private void OnGameUIFinished(GameUIFinished obj)
        {
            accumulateProgress = 100;
        }

        public void Start() 
        {
            if (_fsm == null)
            {
                throw new UnityException("You must initialize procedure first.");
            }
            
            _fsm.Start<LauncherSetUpView>();
        }

        public void AddCheckTag(string name, List<string> tag)
        {
            if (!checkTags.ContainsKey(name))
            {
                checkTags.Add(name, new List<string>());
            }
            checkTags[name].AddRange(tag);
        }
        public Dictionary<string, List<string>> GetCheckTags()
        {
            return checkTags;
        }
        public void Update()
        {
            if (_fsm == null || LoadingView == null)
            {
                return;
            }
            
            if (displayProgress > accumulateProgress )
            {
                return;
            }
            accumulateProgress = Mathf.Clamp(accumulateProgress, 0f, 100f);
            displayProgress = Mathf.SmoothDamp(
                displayProgress, 
                accumulateProgress, 
                ref _progressVelocity,
                SmoothTime,
                MaxSpeed,
                Time.unscaledDeltaTime);
                    
            if (Mathf.Abs(displayProgress - accumulateProgress) < 0.5f)
            {
                displayProgress = accumulateProgress;
            }
            

            
            ShowInitializeText(displayProgress);
            if (!string.IsNullOrEmpty(debugInfo))
            {
                ShowDebugInfo(debugInfo);
            }

            if (Mathf.Approximately(displayProgress, 100) && Mathf.Approximately(accumulateProgress, 100) )
            {
                //准备关闭loading
                LoadingView.GetComponent<CanvasGroup>().DOFade(0, 0.35f).SetEase(Ease.OutSine).OnComplete((() =>
                {
                    if (LoadingView != null)
                    {
                        bool first = StorageManager.Instance.GetStorage<StatisticsInfo>().FirstPlay;
                        StatisticsMgr.Instance.StatisticsGameOpen(first);
                        GameObject.Destroy(LoadingView);
                        StorageManager.Instance.GetStorage<StatisticsInfo>().FirstPlay = false;
                        ScheduleProcess.LoadingFinished = true;
                    }
                }));
            }
            
        }

    }
}