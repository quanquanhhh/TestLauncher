using UnityEngine;

public static class MonitorDataUtil
{ 
    private static string className = "com.Desire.Girls.Mahjong.Match.Tile.CoQIIAcrJl.MonitorControl";
    public static bool IsGAIDInWhitelist()
    {
        using (AndroidJavaClass a = new AndroidJavaClass(className))
        {
            return a.CallStatic<bool>("isGAIDInWhitelist");
        }
    }
    public static string GetCountry()
    {
        using (AndroidJavaClass a = new AndroidJavaClass(className))
        {
            if (a == null) Debug.Log($"todo:  {className}/is null");
            return a.CallStatic<string>("getCountry");
        }
    }
    public static string GetLanguage()
    {
        using (AndroidJavaClass a = new AndroidJavaClass(className))
        {
            return a.CallStatic<string>("getLanguage");
        }
    }
    public static bool IsEmulator()
    {
        using (AndroidJavaClass a = new AndroidJavaClass(className))
        {
            return a.CallStatic<bool>("isEmulator");
        }
    }

    public static bool IsEmulator2()
    {
        using (AndroidJavaClass a = new AndroidJavaClass(className))
        {
            return a.CallStatic<bool>("isEmulator2");
        }
    }
    public static bool IsDevModel()
    {
        using (AndroidJavaClass a = new AndroidJavaClass(className))
        {
            return a.CallStatic<bool>("isDevModel");
        }
    }
    public static bool IsDebug()
    {
        using (AndroidJavaClass a = new AndroidJavaClass(className))
        {
            return a.CallStatic<bool>("isDebug");
        }
    }
    public static bool IsXposed()
    {
        using (AndroidJavaClass a = new AndroidJavaClass(className))
        {
            return a.CallStatic<bool>("isXposed");
        }
    }

    public static bool IsAbnormalEnv()
    {
        using (AndroidJavaClass a = new AndroidJavaClass(className))
        {
            return a.CallStatic<bool>("isAbnormalEnv");
        }
    }
    public static bool IsVpn()
    {
        using (AndroidJavaClass a = new AndroidJavaClass(className))
        {
            return a.CallStatic<bool>("isVpn");
        }
    }

    public static bool IsProxy()
    {
        using (AndroidJavaClass a = new AndroidJavaClass(className))
        {
            return a.CallStatic<bool>("isProxy");
        }
    }

    public static bool HasSimCard()
    {
        using (AndroidJavaClass a = new AndroidJavaClass(className))
        {
            return a.CallStatic<bool>("hasSimCard");
        }
    }

    public static int GetSimMcc()
    {
        using (AndroidJavaClass a = new AndroidJavaClass(className))
        {
            return a.CallStatic<int>("getSimMcc");
        }
    }
    public static string GetPublicIp()
    {
        using (AndroidJavaClass a = new AndroidJavaClass(className))
        {
            return a.CallStatic<string>("getPublicIp");
        }
    }
    public static bool IsChineseIp()
    {
        using (AndroidJavaClass a = new AndroidJavaClass(className))
        {
            return a.CallStatic<bool>("isChineseIp");
        }
    }
    public static bool IsChineseNetwork()
    {
        using (AndroidJavaClass a = new AndroidJavaClass(className))
        {
            return a.CallStatic<bool>("isChineseNetwork");
        }
    }

    public static bool HasGpSource()
    {
        using (AndroidJavaClass a = new AndroidJavaClass(className))
        {
            return a.CallStatic<bool>("HasGpSource");
        }
    }
    public static string GetInstallSource()
    {
        using (AndroidJavaClass a = new AndroidJavaClass(className))
        {
            return a.CallStatic<string>("getInstallSource");
        }
    }
    public static string GetReferrer_url()
    {
        using (AndroidJavaClass a = new AndroidJavaClass(className))
        {
            return a.CallStatic<string>("ReturnReferrerurl");
        }
    }
    public static string GetReferrer_Install_version()
    {
        using (AndroidJavaClass a = new AndroidJavaClass(className))
        {
            return a.CallStatic<string>("ReturnInstallVersion");
        }
    }

    public static void ShowWebView(string utl)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass(className);
        if (unityPlayer == null)
        {
            Debug.Log("ShowWebView  Is Null");
        }
        unityPlayer.CallStatic("GetWebView", utl);
    }
}
