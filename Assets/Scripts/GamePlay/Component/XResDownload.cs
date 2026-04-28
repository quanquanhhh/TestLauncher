using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using GamePlay.Utility;
using UnityEngine;
using UnityEngine.Networking;

namespace GamePlay.Component
{
    public class XResDownloadQueue : MonoBehaviour
    {
        private static XResDownloadQueue _instance;

        public static XResDownloadQueue Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject(nameof(XResDownloadQueue));
                    _instance = go.AddComponent<XResDownloadQueue>();
                    DontDestroyOnLoad(go);
                }

                return _instance;
            }
        }

        public static async UniTask<byte[]> TryGetXRes(string photoname)//带格式的
        {
            
            var xres = await Instance.TryGetXResInternal(photoname);
            return xres;
        } 
        private string cacheFolderName = "XResCache";
        private bool cancelCurrentWhenPause = true;
        private int timeout = 10;

        private readonly Queue<DownloadTask> _queue = new Queue<DownloadTask>();
        private readonly HashSet<string> _queuedOrRunning = new HashSet<string>();
        private readonly Dictionary<string, List<RequestCallback>> _pendingCallbacks =
            new Dictionary<string, List<RequestCallback>>();

        private UnityWebRequest _currentRequest;
        private CancellationTokenSource _currentCts;
        private bool _isRunning;
        private bool _isPaused;

        private struct RequestCallback
        {
            public Action<string> OnSuccess;
            public Action<string> OnFail;
        }

        private class DownloadTask
        {
            public string Url;
        }

        public void DownloadImage(string url, Action<string> onSuccess = null, Action<string> onFail = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                onFail?.Invoke("url is null or empty.");
                return;
            }

            string cachePath = GetCachePath(url);
            if (File.Exists(cachePath))
            {
                try
                {
                    FileInfo info = new FileInfo(cachePath);
                    if (info.Exists && info.Length > 0)
                    {
                        onSuccess?.Invoke(cachePath);
                        return;
                    }

                    File.Delete(cachePath);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Check cache failed: {e.Message}");
                }
            }

            if (_pendingCallbacks.TryGetValue(url, out var callbackList))
            {
                callbackList.Add(new RequestCallback
                {
                    OnSuccess = onSuccess,
                    OnFail = onFail
                });
            }
            else
            {
                _pendingCallbacks[url] = new List<RequestCallback>
                {
                    new RequestCallback
                    {
                        OnSuccess = onSuccess,
                        OnFail = onFail
                    }
                };
            }

            if (_queuedOrRunning.Contains(url))
                return;

            _queuedOrRunning.Add(url);
            _queue.Enqueue(new DownloadTask { Url = url });

            TryStartNext().Forget();
        }

        public UniTask<string> DownloadImageAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
                return UniTask.FromException<string>(new Exception("url is null or empty."));

            if (TryGetCachedFilePath(url, out string filePath))
                return UniTask.FromResult(filePath);

            var tcs = new UniTaskCompletionSource<string>();

            DownloadImage(
                url,
                cachePath => tcs.TrySetResult(cachePath),
                err => tcs.TrySetResult("DownloadFailed" + err)
            );

            return tcs.Task;
        }

        public bool ResourceInCache(string photoname)
        {
            string url = GlobalSetting.PhotoUrl+"/" + photoname +".xres";
            if (TryGetCachedBytes(url, out byte[] cachedBytes))
            {
                return true;
            }
            return false;
        }
        
        public async UniTask<byte[]> TryGetXResInternal(string photoname)
        {
            if (string.IsNullOrEmpty(photoname))
                throw new Exception("url is null or empty.");

            string url = GlobalSetting.PhotoUrl+"/" + photoname +".xres";
            if (TryGetCachedBytes(url, out byte[] cachedBytes))
            {
                return cachedBytes;
            }

            string cachePath = await DownloadImageAsync(url);
            if (cachePath.Contains("DownloadFailed"))
            {
                return null;
            }
            else if (TryReadFileBytes(cachePath, out byte[] bytes))
            {
                return bytes;
            }
            return null;
        }

        private async UniTaskVoid TryStartNext()
        {
            if (_isPaused || _isRunning)
                return;

            while (!_isPaused && !_isRunning && _queue.Count > 0)
            {
                DownloadTask task = _queue.Dequeue();
                if (task == null || string.IsNullOrEmpty(task.Url))
                    continue;

                _isRunning = true;
                _currentCts = new CancellationTokenSource();

                try
                {
                    string cachePath = await DownloadToCacheAsync(task.Url, _currentCts.Token);
                    CompleteSuccess(task.Url, cachePath);
                }
                catch (OperationCanceledException)
                {
                    RequeueCurrent(task.Url);
                }
                catch (Exception e)
                {
                    CompleteFail(task.Url, e.Message);
                }
                finally
                {
                    CleanupCurrentState();
                }
            }
        }

        private async UniTask<string> DownloadToCacheAsync(string url, CancellationToken ct)
        {
            string cachePath = GetCachePath(url);

            if (File.Exists(cachePath))
            {
                FileInfo info = new FileInfo(cachePath);
                if (info.Exists && info.Length > 0)
                    return cachePath;

                try { File.Delete(cachePath); } catch { }
            }

            using var request = UnityWebRequest.Get(url);
            _currentRequest = request;
            request.timeout = timeout;

            await request.SendWebRequest().ToUniTask(cancellationToken: ct);

#if UNITY_2020_1_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                throw new Exception("Download failed: " + request.error + " url = " + url);
            }

            byte[] data = request.downloadHandler.data;
            if (data == null || data.Length == 0)
                throw new Exception("Downloaded data is empty. url = " + url);

            string dir = Path.GetDirectoryName(cachePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string tempPath = cachePath + ".tmp";

            await UniTask.RunOnThreadPool(() =>
            {
                File.WriteAllBytes(tempPath, data);

                if (File.Exists(cachePath))
                    File.Delete(cachePath);

                File.Move(tempPath, cachePath);
            }, cancellationToken: ct);

            return cachePath;
        }

        private void RequeueCurrent(string url)
        {
            if (string.IsNullOrEmpty(url))
                return;

            if (!_queuedOrRunning.Contains(url))
                _queuedOrRunning.Add(url);

            _queue.Enqueue(new DownloadTask { Url = url });
        }

        private void CompleteSuccess(string url, string cachePath)
        {
            if (_pendingCallbacks.TryGetValue(url, out var callbacks))
            {
                for (int i = 0; i < callbacks.Count; i++)
                {
                    try
                    {
                        callbacks[i].OnSuccess?.Invoke(cachePath);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }

                _pendingCallbacks.Remove(url);
            }

            _queuedOrRunning.Remove(url);
        }

        private void CompleteFail(string url, string error)
        {
            if (_pendingCallbacks.TryGetValue(url, out var callbacks))
            {
                for (int i = 0; i < callbacks.Count; i++)
                {
                    try
                    {
                        callbacks[i].OnFail?.Invoke(error);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }

                _pendingCallbacks.Remove(url);
            }

            _queuedOrRunning.Remove(url);
        }

        private void CleanupCurrentState()
        {
            _currentRequest = null;

            try
            {
                _currentCts?.Dispose();
            }
            catch { }

            _currentCts = null;
            _isRunning = false;

            if (!_isPaused)
                TryStartNext().Forget();
        }

        private void OnApplicationPause(bool pause)
        {
            _isPaused = pause;

            if (pause && cancelCurrentWhenPause)
            {
                try { _currentCts?.Cancel(); } catch { }
                try { _currentRequest?.Abort(); } catch { }
            }
            else if (!pause)
            {
                TryStartNext().Forget();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                _isPaused = false;
                TryStartNext().Forget();
            }
        }

        public bool TryGetCachedFilePath(string url, out string filePath)
        {
            filePath = null;

            if (string.IsNullOrEmpty(url))
                return false;

            try
            {
                string path = GetCachePath(url);
                if (!File.Exists(path))
                    return false;

                FileInfo info = new FileInfo(path);
                if (!info.Exists || info.Length <= 0)
                    return false;

                filePath = path;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("TryGetCachedFilePath failed: " + e.Message);
                filePath = null;
                return false;
            }
        }

        public bool TryGetCachedBytes(string url, out byte[] bytes)
        {
            bytes = null;

            if (string.IsNullOrEmpty(url))
                return false;

            try
            {
                string path = GetCachePath(url);
                if (!File.Exists(path))
                    return false;

                FileInfo info = new FileInfo(path);
                if (!info.Exists || info.Length <= 0)
                    return false;

                bytes = File.ReadAllBytes(path);
                return bytes != null && bytes.Length > 0;
            }
            catch (Exception e)
            {
                Debug.LogWarning("TryGetCachedBytes failed: " + e.Message);
                bytes = null;
                return false;
            }
        }

        public bool TryReadFileBytes(string filePath, out byte[] bytes)
        {
            bytes = null;

            if (string.IsNullOrEmpty(filePath))
                return false;

            try
            {
                if (!File.Exists(filePath))
                    return false;

                FileInfo info = new FileInfo(filePath);
                if (!info.Exists || info.Length <= 0)
                    return false;

                bytes = File.ReadAllBytes(filePath);
                return bytes != null && bytes.Length > 0;
            }
            catch (Exception e)
            {
                Debug.LogWarning("TryReadFileBytes failed: " + e.Message);
                bytes = null;
                return false;
            }
        }

        private string GetCacheDirectory()
        {
            return Path.Combine(Application.persistentDataPath, cacheFolderName);
        }

        private string GetCachePath(string url)
        {
            string fileName = GetStableFileName(url) + ".xres";
            return Path.Combine(GetCacheDirectory(), fileName);
        }

        private string GetStableFileName(string input)
        {
            using var md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            StringBuilder sb = new StringBuilder(hash.Length * 2);
            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("x2"));

            return sb.ToString();
        }
    }
}