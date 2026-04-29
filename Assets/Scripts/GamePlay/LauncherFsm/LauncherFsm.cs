using System;
using Cysharp.Threading.Tasks;
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
        private Slider _progressSlider;
        private TextMeshProUGUI _progressText;
        private TextMeshProUGUI _debugText;
        
        private float _progressVelocity = 0f;
        private const float SmoothTime = 0.45f;   // 越小越快，越大越柔和
        private const float MaxSpeed = 100f;
        private const float MaxLoadingSeconds = 5f;
        private bool _isClosingLoading = false;
        private float _launchBeginTime;

        public Type[] fsmStateTypes =
        {
            typeof(LauncherSetUpView),
            typeof(LauncherBootstrap),
            typeof(LauncherGame),
        };

        private DOTween _doTween;

        public void ShowInitializeText(float progress)
        {
            EnsureLoadingRefs();
            if (_progressSlider != null && _progressText != null)
            {
                float clampedProgress = Mathf.Clamp(progress, 0f, 100f);
                _progressSlider.value = clampedProgress / 100;
                _progressText.text = $"Loading... {Mathf.FloorToInt(clampedProgress)}%";
            }
        }

        public void ShowDebugInfo(string info)
        {
#if !DEVELOPMENT_BUILD 
            return;
#endif
            
            EnsureLoadingRefs();
            if (_debugText != null)
            { 
                _debugText.text = info;
            }
        }

        private void EnsureLoadingRefs()
        {
            if (LoadingView == null)
            {
                return;
            }
            if (_progressSlider == null)
            {
                var sliderTransform = LoadingView.transform.Find("ProgressSlider");
                if (sliderTransform != null)
                {
                    _progressSlider = sliderTransform.GetComponent<Slider>();
                }
            }
            if (_progressText == null)
            {
                var progressTextTransform = LoadingView.transform.Find("ProgressSlider/ProgressText");
                if (progressTextTransform != null)
                {
                    _progressText = progressTextTransform.GetComponent<TextMeshProUGUI>();
                }
            }
            if (_debugText == null)
            {
                var debugTextTransform = LoadingView.transform.Find("ProgressSlider/DebugInfo");
                if (debugTextTransform != null)
                {
                    _debugText = debugTextTransform.GetComponent<TextMeshProUGUI>();
                }
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

        private async void OnGameUIFinished(GameUIFinished obj)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.25f));
            accumulateProgress = 100;
        }

        public void Start() 
        {
            if (_fsm == null)
            {
                throw new UnityException("You must initialize procedure first.");
            }
            
            MarkLaunchBegin();
            _fsm.Start<LauncherSetUpView>();
        }

        public void MarkLaunchBegin()
        {
            if (_launchBeginTime <= 0f)
            {
                _launchBeginTime = Time.realtimeSinceStartup;
            }
        }

        public bool IsLoadingTimeout()
        {
            if (_launchBeginTime <= 0f)
            {
                return false;
            }
            return (Time.realtimeSinceStartup - _launchBeginTime) >= MaxLoadingSeconds;
        }

        public int GetRemainLoadingMs()
        {
            if (_launchBeginTime <= 0f)
            {
                return (int)(MaxLoadingSeconds * 1000f);
            }
            float remain = Mathf.Max(0f, MaxLoadingSeconds - (Time.realtimeSinceStartup - _launchBeginTime));
            return Mathf.CeilToInt(remain * 1000f);
        }

        public void Update()
        {
            if (_fsm == null || LoadingView == null)
            {
                return;
            }
            
            accumulateProgress = Mathf.Clamp(accumulateProgress, 0f, 100f);
            if (accumulateProgress < 95f && !Mathf.Approximately(accumulateProgress, 100f))
            {
                float autoTarget = Mathf.Min(95f, displayProgress + Time.unscaledDeltaTime * 18f);
                accumulateProgress = Mathf.Max(accumulateProgress, autoTarget);
            }
            displayProgress = Mathf.SmoothDamp(
                displayProgress, 
                accumulateProgress, 
                ref _progressVelocity,
                SmoothTime,
                MaxSpeed,
                Mathf.Max(Time.unscaledDeltaTime, 0.001f));
                    
            if (Mathf.Abs(displayProgress - accumulateProgress) < 0.5f)
            {
                displayProgress = accumulateProgress;
            }
            

            
            ShowInitializeText(displayProgress);
            if (!string.IsNullOrEmpty(debugInfo))
            {
                ShowDebugInfo(debugInfo);
            }

            if (!_isClosingLoading && Mathf.Approximately(displayProgress, 100) && Mathf.Approximately(accumulateProgress, 100) )
            {
                _isClosingLoading = true;
                //准备关闭loading
                LoadingView.GetComponent<CanvasGroup>().DOFade(0, 0.35f).SetEase(Ease.OutSine).OnComplete((() =>
                {
                    if (LoadingView != null)
                    {
                        bool first = StorageManager.Instance.GetStorage<StatisticsInfo>().FirstPlay;
                        StatisticsMgr.Instance.StatisticsGameOpen(first);
                        GameObject.Destroy(LoadingView);
                        _progressSlider = null;
                        _progressText = null;
                        _debugText = null;
                        StorageManager.Instance.GetStorage<StatisticsInfo>().FirstPlay = false;
                        ScheduleProcess.LoadingFinished = true;
                    }
                }));
            }
            
        }

    }
}
