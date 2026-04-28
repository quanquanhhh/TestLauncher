// using UnityEngine;
// using UnityEngine.Video;
//
// public class VideoDebugProbe : MonoBehaviour
// {
//     public VideoPlayer vp;
//
//     private long _lastFrame = -1;
//     private float _lastRealtime = -1f;
//
//     void Awake()
//     {
//         Debug.Log(" Add VideoDebugProb ------ ");
//         vp = transform.GetComponent<VideoPlayer>();
//         vp.errorReceived += (p, msg) =>
//         {
//             Debug.LogError($"[VP][error] {msg}");
//         };
//
//         vp.seekCompleted += (p) =>
//         {
//             Debug.LogWarning($"[VP][seekCompleted] time={p.time:F3}, frame={p.frame}, isPlaying={p.isPlaying}");
//         };
//
//         vp.loopPointReached += (p) =>
//         {
//             Debug.LogWarning($"[VP][loopPointReached] time={p.time:F3}, frame={p.frame}");
//         };
//
//         vp.prepareCompleted += (p) =>
//         {
//             Debug.Log($"[VP][prepareCompleted] time={p.time:F3}, frame={p.frame}, size={p.width}x{p.height}");
//         };
//
//         vp.sendFrameReadyEvents = true;
//         vp.frameReady += OnFrameReady;
//     }
//
//     private void OnFrameReady(VideoPlayer p, long frame)
//     {
//         float now = Time.realtimeSinceStartup;
//
//         if (_lastFrame >= 0)
//         {
//             float dt = now - _lastRealtime;
//             long frameGap = frame - _lastFrame;
//
//             if (dt > 0.15f || frameGap > 2)
//             {
//                 Debug.LogWarning(
//                     $"[VP][stall?] dt={dt:F3}s frameGap={frameGap} " +
//                     $"vp.time={p.time:F3} frame={frame} isPlaying={p.isPlaying} isPrepared={p.isPrepared}");
//             }
//         }
//
//         _lastFrame = frame;
//         _lastRealtime = now;
//     }
// }