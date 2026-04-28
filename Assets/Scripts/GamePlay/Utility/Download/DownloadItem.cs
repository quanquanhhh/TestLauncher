using System;
using Cysharp.Threading.Tasks;
using GameConfig;
using UnityEngine;
using YooAsset;

namespace GamePlay
{
    public class DownloadItem
    {
        public async UniTask BeginDownload(ResourceDownloaderOperation downloader, Action successAction)
        { 
            // 注册下载回调 
            downloader.DownloadErrorCallback = OnDownloadErrorCallback;
            downloader.DownloadUpdateCallback = OnDownloadProgressCallback;
            downloader.DownloadFinishCallback = OnDownloadFinishCallback;
            downloader.DownloadFileBeginCallback = OnDownloadFileBeginCallback;
            downloader.BeginDownload();
            await downloader.Task;

            // 检测下载结果
            if (downloader.Status == EOperationStatus.Succeed)
            {
                successAction?.Invoke();
            }
        }
        
        private void OnDownloadFileBeginCallback(DownloadFileData data)
        {
            Debug.Log(data);
        }

        private void OnDownloadFinishCallback(DownloaderFinishData data)
        { 
        }

        
        private void OnDownloadProgressCallback(DownloadUpdateData data)
        { 
        }

        private void OnDownloadErrorCallback(DownloadErrorData   error)
        {
            Debug.Log(" error :" + error.ErrorInfo);
        }
    }
}