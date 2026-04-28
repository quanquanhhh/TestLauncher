      
using System;
using System.Collections;
using System.Collections.Generic;
using Foundation;
using UnityEditor;
using UnityEngine;

public class ConfigAppWindow : EditorWindow
{

    [MenuItem("Tools/加密配置JSON")]
    public static void EncryptJson()
    {
        ConfigManager.EncryptConfig();
    }


    [InitializeOnLoad]
    public class NewMonoBehaviour
    {
        static NewMonoBehaviour()
        {
            BuildPlayerWindow.RegisterBuildPlayerHandler(ChenConfig);
        }

        public static void ChenConfig(BuildPlayerOptions obj)
        {
            if (EditorUserBuildSettings.buildAppBundle)
            {
#if UNITY_ANDROID
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
                PlayerSettings.Android.useCustomKeystore = true;
#endif
                DoBuild(obj);
            }
            else
            {
                DoBuild(obj);
            }
        }


        static void DoBuild(BuildPlayerOptions obj)
        {
            int buttonIndex = EditorUtility.DisplayDialogComplex(
                "打包前确认", 
                $"游戏名:{PlayerSettings.productName}\n包名:{PlayerSettings.applicationIdentifier}\n版本号:{Application.version}\n构建版本号:{PlayerSettings.Android.bundleVersionCode}\n是否支持arm64:{(int)PlayerSettings.Android.targetArchitectures > 1}\nSdk版本：{PlayerSettings.Android.targetSdkVersion}",
                "使用本地JSON",
                "开始打包",    
                "取消"          
            );

            switch (buttonIndex)
            {
                case 0: 
                    
                    ConfigManager.GetUseConfig();
                    PlayerSettings.productName = ConfigManager.useConfig.game_name;
                    PlayerSettings.applicationIdentifier = ConfigManager.useConfig.package_name;
                    BuildWithConfig(obj, "测试JSON");
                    break;
                case 1:
                    BuildWithConfig(obj, "默认配置");
                    break;
                case 2:
                case -1:
                    Debug.Log("取消构建");
                    break;
            }
        }

        static void BuildWithConfig(BuildPlayerOptions obj, string configType)
        {
            var pre = obj.locationPathName.Substring(0, obj.locationPathName.LastIndexOf('/') + 1);
            var end = obj.locationPathName.Substring(obj.locationPathName.Length - 4, 4);
            string buildName;
            
            if (!EditorUserBuildSettings.buildAppBundle)
            {
                buildName = pre + PlayerSettings.productName + DateTime.Now.ToString("_MMdd_HHmm") + end;
            }
            else
            {
                buildName = pre + PlayerSettings.productName + "_" + Application.version + "_" + PlayerSettings.Android.bundleVersionCode + end;
            }
            
            obj.locationPathName = buildName;
            Debug.Log($"{configType} - 构建路径: {buildName}");
            BuildPipeline.BuildPlayer(obj);
        }
    }
}

    