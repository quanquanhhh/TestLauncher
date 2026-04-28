using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Foundation;
using GameConfig;
using GamePlay.Utility;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using YooAsset;

namespace GamePlay.Component
{
    [RequireComponent(typeof(RawImage))]
    [DisallowMultipleComponent]
    public class UguiMediaSource : MonoBehaviour
    { 

        private RawImage _rawImage;
        private VideoPlayer _videoPlayer;
        private RenderTexture _renderTexture;

        private Image small;
        private bool _isVideo;
        private string currentName = "";
        Vector2 originalSize;
        private bool _forceShowSmallImg = false;
        private bool init = false;

        public void Init()
        {
            if (init)
            {
                return;
            }

            init = true;
            _rawImage = gameObject.TryGetOrAddComponent<RawImage>();
            _videoPlayer = gameObject.TryGetOrAddComponent<VideoPlayer>();
            small = _rawImage.transform.Find("small")?.GetComponent<Image>();
            if (small == null)
            {
                var obj = new GameObject();
                obj.transform.SetParent(transform);
                obj.name = "small";
                obj.transform.localScale = Vector3.one;
                obj.transform.localPosition = Vector3.zero;
                small = obj.AddComponent<Image>();
            }
            _renderTexture = new RenderTexture((int)ViewUtility.DesignSize.x, (int)ViewUtility.DesignSize.y, 0, RenderTextureFormat.ARGB32);
            _renderTexture.name = $"{gameObject.name}_MediaRT";
            _renderTexture.Create();

            _videoPlayer.playOnAwake = false;
            _videoPlayer.waitForFirstFrame = true;
            _videoPlayer.skipOnDrop = false;
            _videoPlayer.isLooping = true;
            _videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            _videoPlayer.targetTexture = _renderTexture;
            _videoPlayer.prepareCompleted += OnVideoPrepared;
            _videoPlayer.errorReceived += OnVideoErrorReceived;
            
            // _videoPlayer.TryGetOrAddComponent<VideoDebugProbe>();
            SetOriginalSize();
        }
        private void Awake()
        {
            // transform.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);
            Init();
        }
 
        private void OnDestroy()
        {
            if (_videoPlayer.isPlaying)
                _videoPlayer.Stop();
            _videoPlayer.prepareCompleted -= OnVideoPrepared;
            _videoPlayer.errorReceived -= OnVideoErrorReceived;


            _videoPlayer.targetTexture = null;
            _videoPlayer.url = string.Empty;

            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
                _renderTexture = null;
            }
        }

        public void SetOriginalSize()
        {
            originalSize = transform.GetComponent<RectTransform>().sizeDelta;
        }
        public async UniTask SetSource(string sourceName, bool isVideo, bool videoShowImg = false, bool forceShowSmallImg = true)
        {
            if (sourceName == currentName)
            {
                return;
            }
            _isVideo = isVideo;
            currentName = sourceName;
            _forceShowSmallImg = forceShowSmallImg;
            if (isVideo && videoShowImg)
                ShowSmallImg(sourceName);
            else if (isVideo)
                LoadVideo2(sourceName);
            else
                LoadImage(sourceName);
        }

        private void ShowSmallImg(string sourceName)
        {
            small.DOKill();
            var img = GameConfigSys.GetPhotoAtlasName(sourceName);
            var sp = AssetLoad.Instance.LoadSprite(sourceName, img);
            small.sprite = sp;
            GUtility.ApplyAspect(sp, originalSize, small.GetComponent<RectTransform>());
            small.gameObject.SetActive(true); 
            var a = small.color;
            a.a = 1;
            small.color = a;
            Debug.Log("sourceName =" + sourceName);
        }
        private async void LoadImage(string sourceName)
        { 
            if (!_forceShowSmallImg && !ResTool.HasResource(sourceName))
            {
                ShowSmallImg(sourceName);
            }
            else if (_forceShowSmallImg && !ResTool.HasResourcePerpared(sourceName))
            {
                ShowSmallImg(sourceName);
            }
            Texture2D texture = await ResTool.GetTexture(sourceName);
            if (texture != null && this != null && currentName == sourceName)
            {
                _rawImage.texture = texture;
                
                GUtility.ApplyAspect(texture, originalSize,_rawImage.GetComponent<RectTransform>());
                small.DOKill();
                small.DOFade(0, 0.25f).OnComplete(() =>
                {
                    small.gameObject.SetActive(false);
                });
            }
        }

        private async UniTask LoadVideo2(string name)
        {
            var ct = this.GetCancellationTokenOnDestroy();

            if (_videoPlayer == null) return;
            try
            {
                ShowSmallImg(name);
                var url = await ResTool.GetVideo(name);
                // 1. 先等到这个对象和 VideoPlayer 真正激活
                await UniTask.WaitUntil(
                    () =>
                        this != null &&
                        gameObject != null &&
                        gameObject.activeInHierarchy &&
                        _videoPlayer != null &&
                        _videoPlayer.isActiveAndEnabled,
                    cancellationToken: ct);

                // 2. 等待期间如果被新请求顶掉了，直接退出
                if (currentName != name) return;
                if (_videoPlayer.isPlaying)
                {
                    _videoPlayer.Stop();
                }

                _rawImage.texture = _renderTexture;

                _videoPlayer.playOnAwake = false;
                _videoPlayer.isLooping = true;
                _videoPlayer.source = VideoSource.Url;
                _videoPlayer.url = url; 
                _videoPlayer.targetTexture = _renderTexture;
                _videoPlayer.aspectRatio = VideoAspectRatio.Stretch;

                // 3. 赋值完再检查一次，避免刚好这时被禁用
                await UniTask.WaitUntil(
                    () =>
                        this != null &&
                        gameObject != null &&
                        gameObject.activeInHierarchy &&
                        _videoPlayer != null &&
                        _videoPlayer.isActiveAndEnabled,
                    cancellationToken: ct);
                
                if (currentName != name) return; 

                if (!_videoPlayer.isPrepared)
                {
                    _videoPlayer.Prepare();
                }

                // 4. 等 prepare 完成；如果对象被销毁，ct 会取消然后直接走 catch return
                await UniTask.WaitUntil(
                    () =>
                        _videoPlayer != null &&
                        _videoPlayer.isPrepared,
                    cancellationToken: ct);

                if (currentName != name) return;  
                if (!_videoPlayer.isActiveAndEnabled || !gameObject.activeInHierarchy) return;

                // _videoPlayer.Play(); 
            }
            catch (OperationCanceledException)
            {
                // GameObject 被销毁时直接退出
                Debug.Log(" UguiMediaSource  Video UniTask Stop");
                return;
            }
        }
                
        private async void LoadVideo(string sourceName)
        {
            
            ShowSmallImg(sourceName);
            var url = await ResTool.GetVideo(sourceName);
            if (!string.IsNullOrEmpty(url) && this != null && currentName == sourceName)
            {
                if (_videoPlayer.isPlaying)
                {
                    _videoPlayer.Stop();
                }
                _videoPlayer.source = VideoSource.Url;
                _videoPlayer.url = url;
                // _videoPlayer.clip = null;
                _videoPlayer.targetTexture = _renderTexture;
                _videoPlayer.aspectRatio = VideoAspectRatio.Stretch;
                _videoPlayer.Prepare();
                try
                {
                    await UniTask.WaitUntil(
                        () => _videoPlayer.isPrepared,
                        cancellationToken: this.GetCancellationTokenOnDestroy());
                }
                catch (OperationCanceledException)
                {
                    // 对象销毁时取消，属于预期情况
                    return;
                }
                // await UniTask.WaitUntil(() => _videoPlayer.isPrepared,cancellationToken: this.GetCancellationTokenOnDestroy());
                // await UniTaskMgr.Instance.Yield();
                _rawImage.texture = _renderTexture; 
                _videoPlayer.Play();
                small.DOKill();
                small.DOFade(0, 0.25f).OnComplete(() =>
                {
                    small.gameObject.SetActive(false);
                }); 
            } 
        }

        private void OnVideoPrepared(VideoPlayer source)
        {
            if (source == null || !_isVideo)
                return;

            _rawImage.texture = _renderTexture;  
            ApplyAspect( ); 
            source.Play();
            
            small.DOKill();
            small.DOFade(0, 0.25f).OnComplete(() =>
            {
                small.gameObject.SetActive(false);
            });
        }

        private void OnVideoErrorReceived(VideoPlayer source, string message)
        {
            Debug.LogError($"[UguiMediaSource] Video error : {message}", this);
        }

 
        private void ApplyAspect( )
        {
            RectTransform rt = gameObject.GetComponent<RectTransform>(); 
            float width = originalSize.x;
            float height = originalSize.y;
            
            if (_isVideo)
            {
                float ratio = (float) width/_videoPlayer.width ;
                float ratio2 = (float) height / _videoPlayer.height ;
                float m = Math.Max(ratio, ratio2);
                rt.sizeDelta = new Vector2(_videoPlayer.width * m ,  _videoPlayer.height * m); 
            }
            else
            {
                float ratio = (float) width/_rawImage.texture.width;
                float ratio2 = (float) height /_rawImage.texture.height;
                float m = Math.Max(ratio, ratio2); 
                rt.sizeDelta = new Vector2(_rawImage.texture.width * m , _rawImage.texture.height * m);
                
            }
        }
    }
}