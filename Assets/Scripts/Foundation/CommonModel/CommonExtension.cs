using System;
using System.Text;
using Cysharp.Threading.Tasks;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace Foundation
{
    public static class CommonExtension
    {
        public static async UniTask ChangeDataAsset(this SkeletonGraphic sk, string changeAssetName)
        { 
            var a = await AssetLoad.Instance.LoadAsset<SkeletonDataAsset>(changeAssetName);
            if (a==null)
            {
                return;
            }
            sk.skeletonDataAsset = a;
            sk.Initialize(true);
        }
        public static UniTask PlayAsync(this SkeletonGraphic sk, string animationName, bool loop = false, Action action = null)
        {
            var tcs = new UniTaskCompletionSource();
            TrackEntry entry = sk.AnimationState.SetAnimation(0, animationName, loop);

            // 如果是循环动画 → 没有播放结束 → 立即返回
            if (loop)
            {
                tcs.TrySetResult();
                return tcs.Task;
            }

            // 非循环动画 → 播放完触发
            entry.Complete += _ =>
            {
                action?.Invoke();
                tcs.TrySetResult();
            };

            return tcs.Task;
        }

        public static async UniTask PlayAnimation(this Animator animator, string name)
        {
            animator.Play(name);
            await UniTask.Yield();
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float length = stateInfo.length;

            await UniTask.Delay(TimeSpan.FromSeconds(length));
        }
        public static T TryGetOrAddComponent<T>(this Component component)
            where T : Component
        {
            if (component == null)
                return null;
            if (!component.TryGetComponent<T>(out var comp))
            {
                comp = component.gameObject.AddComponent<T>();
            }

            return comp;
        }
        public static T TryGetOrAddComponent<T>(this GameObject component)
            where T : Component
        {
            if (component == null)
                return null;
            if (!component.TryGetComponent<T>(out var comp))
            {
                comp = component.gameObject.AddComponent<T>();
            }

            return comp;
        }
        public static string EncodingText(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            return UnityWebRequest.EscapeURL(text, Encoding.UTF8);
        }
        
        public static string EncodingText(this Guid guid)
        {
            return guid.ToString().EncodingText();
        }

        
    }
}