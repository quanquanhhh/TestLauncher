

using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_IOS
using Unity.Notifications.iOS;
#endif
using Random = UnityEngine.Random;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

namespace Foundation.Notification
{
    public class PushNotification : SingletonScript<PushNotification>
    {
        
        public void ClearNotifications()
        {
            
#if UNITY_IOS
    iOSNotificationCenter.ApplicationBadge = 0;
    iOSNotificationCenter.RemoveAllScheduledNotifications();
    iOSNotificationCenter.RemoveAllDeliveredNotifications();
#elif UNITY_ANDROID
            AndroidNotificationCenter.CancelAllNotifications();
#endif
        }

        public void CheckRequestAndroidPush()
        {
            
            bool isFirstOpen = PlayerPrefs.GetInt("my_first_launch") == 0;
            if (isFirstOpen)
            {
                //首次启动，不管什么情况都调用
                UnityEngine.Android.Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
            }
            else
            {
                bool isOpenPush = CheckNotificationPermission();
                if (!isOpenPush)
                {
                    //推送没打开的时候，再次调用。
                    UnityEngine.Android.Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
                }
            }
            PlayerPrefs.SetInt("my_first_launch", 1);
        }
        public static bool CheckNotificationPermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        using (var notificationManager = currentActivity.Call<AndroidJavaObject>("getSystemService", "notification"))
        {
            return notificationManager.Call<bool>("areNotificationsEnabled");
        }
#elif UNITY_IOS && !UNITY_EDITOR
        //iOS检测是否开启推送
        var settings = iOSNotificationCenter.GetNotificationSettings();
        return settings.NotificationCenterSetting == NotificationSetting.Enabled;
#else
            return true;
#endif
        }
        public void AddNotifications()
        {
            ClearNotifications();
            for (int i = 0; i < 48; i++)
            {
                int idx = Random.Range(0, NotificationsConfig.NormalNotificationContent.Count);
                RigesterNotification(
                    $"nomalGirl4notifications{i}", // 通知唯一标识,normalNotificationIdentifier必须改名
                    NotificationsConfig.NormalNotificationTitle, // 通知标题
                    NotificationsConfig.NormalNotificationContent[idx], // 通知内容
                    DateTime.UtcNow.AddHours(i + 1), // 通知时间
                    false);
            }
            RigesterNotification(
                $"specialGirl4notifications",// 通知唯一标识,specialNotificationIdentifier必须改名
                NotificationsConfig.CircleNotificationTitle,
                NotificationsConfig.CircleNotificationContent,
                DateTime.UtcNow.AddHours(1.5f),
                true);
        }
        
        private void RigesterNotification(string id, string title, string content, DateTime time, bool circle)
        {
#if UNITY_IOS
    iOSNotificationTimeIntervalTrigger trigger;
    if (circle)
    {
        trigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = time - DateTime.UtcNow,
            Repeats = true
        };
    }
    else
    {
        trigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = time - DateTime.UtcNow,
            Repeats = false
        };
    }

    var notification = new iOSNotification()
    {
        Identifier = id,
        Title = title,
        Body = content,
        ShowInForeground = false,
        Trigger = trigger
    };
    iOSNotificationCenter.ScheduleNotification(notification);
#elif UNITY_ANDROID
            var channel = new AndroidNotificationChannel()
            {
                Id = id,
                Name = "fortunedivinenotifications", //每个项目必须改名
                Importance = Importance.High,
                Description = id
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);

            var notification = new AndroidNotification();
            notification.Title = title;
            notification.Text = content;
            notification.FireTime = time;
            if (circle)
            {
                notification.RepeatInterval = time - DateTime.UtcNow;
            }
            notification.LargeIcon = "fortunedivine"; //每个项目必须改名
            AndroidNotificationCenter.SendNotification(notification, id);
#endif
        }
    
        
    }

    public class NotificationsConfig
    {
        public static string NormalNotificationTitle = "Match tiles, have fun! Play now.";
        public static List<string> NormalNotificationContent = new List<string>()
        {
            "Solve puzzles, have fun! Join today.",
            "Play tile games, enjoy the fun!",
            "Turn tiles into fun. Start playing!",
            "Puzzle your way to relaxation. Play now!",
            "Dive into tile challenges. Join us!",
            "Enjoy real fun with tile matches. Play today!",
            "Compete, match, and enjoy. Start now!",
            "Play tile games, discover new challenges!",
            "Challenge your brain, have a blast! Join now!"
        };
        public static string CircleNotificationTitle = "Fun delivered!";
        public static string CircleNotificationContent = "Can you solve the ultimate tile challenge? Play now and have fun!";

    }
}