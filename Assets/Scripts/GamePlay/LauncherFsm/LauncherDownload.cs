using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Foundation;
using Foundation.FSM;
using UnityEngine;
using YooAsset;
using Event = Foundation.Event;

namespace GamePlay.LauncherFsm
{
    /// <summary>
    /// FsmCreateDownloader
    /// </summary>
    public class LauncherDownload: LauncherBase
    {
        IFsm<LauncherFsm> _fsm;
        private int lastProgress = 0;
        float startTime;
        private List<string> checkMust = new();
        protected internal override async void OnEnter(IFsm<LauncherFsm> fsm)
        {
            base.OnEnter(fsm);
            _fsm = fsm;
            var str = fsm.Owner.GetCheckTags();
            DownloadUtility.Instance.OnCheckActivityDownLoad();
            DownloadUtility.Instance.OnCheckVipDownload();
            if (str.ContainsKey("must"))
            {
                checkMust = str["must"];
                checkMust.Add("BuildIn");
                var downloader = ResourceModule.Instance.CreateDownloader(checkMust);
                if (downloader.TotalDownloadCount != 0)
                {
                    startTime = Time.realtimeSinceStartup;
                    await BeginDownload(downloader,fsm);
                }
                else
                {
                    fsm.Owner.accumulateProgress += 50;
                    DownloadUtility.Instance.OnCheckLauncherGameAssets(checkMust);
                }
            }
            else
            {
                
                fsm.Owner.accumulateProgress += 50;
                DownloadUtility.Instance.OnCheckLauncherGameAssets(checkMust);
            }
            Debug.Log(" [DebugLogic] = LauncherDownload" );
            ChangeState<LauncherUpdateOver>(fsm);
        }
        private async UniTask BeginDownload(ResourceDownloaderOperation downloader, IFsm<LauncherFsm> fsm )
        { 
            // 注册下载回调 
            downloader.DownloadErrorCallback = OnDownloadErrorCallback;
            downloader.DownloadUpdateCallback = OnDownloadProgressCallback;
            downloader.DownloadFinishCallback = OnDownloadFinishCallback;
            downloader.DownloadFileBeginCallback = OnDownloadFileBeginCallback;
            downloader.BeginDownload();
            await downloader.Task;

            // 检测下载结果
            if (downloader.Status != EOperationStatus.Succeed)
                return; 
            DownloadUtility.Instance.OnCheckLauncherGameAssets(checkMust);
        }

        private void OnDownloadFileBeginCallback(DownloadFileData data)
        {
            Debug.Log(data);
        }

        private void OnDownloadFinishCallback(DownloaderFinishData data)
        {
            float endTime = Time.realtimeSinceStartup;
            float cost = (endTime - startTime);
            _fsm.Owner.debugInfo = $"download spend {cost:F2}s";   
        }

        
        private void OnDownloadProgressCallback(DownloadUpdateData data)
        {
            int add = (int)(data.Progress * 100 - lastProgress) / 2;
            if (add != 0)
            {
                lastProgress = (int)(data.Progress * 100);
            }

            var final = _fsm.Owner.accumulateProgress + add;
            _fsm.Owner.accumulateProgress = Math.Max(_fsm.Owner.accumulateProgress, final);
        }

        private void OnDownloadErrorCallback(DownloadErrorData   error)
        {
            Debug.Log(" error :" + error.ErrorInfo);
        }

    }
}