// using System;
// using System.IO;
// using UnityEditor;
// using UnityEngine;
// using GamePlay;
// using YooAsset;
// using YooAsset.Editor;
//
// namespace EditorTools
// {
//     public static class BuildVersionSetting
//     {
// #if UNITY_IOS
//         public static string serverVersion = "v1";
//         public static string resourcesRootVersion = "rv9";
//         public static string version = "1.0.9";
//         public static string versionCode = "14";
//         public const string bundleFolderName = "ios";
// #elif UNITY_ANDROID
//         public static string serverVersion = "v1";
//         public static string resourcesRootVersion = "rv11";
//         public static string version = "1.0.12";
//         public static string versionCode = "17";
//         public const string bundleFolderName = "android";
// #endif
//         public static string resourcesVersion = "v1";
//     }
//     
//     public static class YooAssetBuildEditorTools
//     {
//         private const string BuiltinTag = "BuildIn";
//
//         
//         public static void EncryptLocaleKv(string rootFolder)
//         {
// #if UNITY_IOS
//         var allFiles = Directory.GetFiles(Path.Combine(Application.dataPath, rootFolder));
//
//         foreach (var file in allFiles)
//         {
//             if (!file.EndsWith(".json"))
//                 continue;
//             var textFileName = Path.GetFileName(file);
//             textFileName = textFileName.Replace(Path.GetExtension(textFileName), "");
//
//             // if(!textFileName.StartsWith("locale_loading_")) 
//             //     continue;
//        
//             if (!textFileName.Contains("_ios"))
//             {
//                 var configTextAsset = File.ReadAllText(file);
//
//                 var listConfig = JsonConvert.DeserializeObject<List<LocaleItemConfig>>(configTextAsset);
//                 if (listConfig != null)
//                 {
//                     for (var i = 0; i < listConfig.Count; i++)
//                     {
//                         listConfig[i].Key = StringCoder.Encoding(listConfig[i].Key);
//                         listConfig[i].Value = StringCoder.Encoding(listConfig[i].Value);
//                     }
//
//                     var newContent = JsonConvert.SerializeObject(listConfig);
//
//                     //添加额外后缀
//                     var savePath = Path.Combine(Application.dataPath, rootFolder,
//                         $"{textFileName.Replace("locale_", "")}_ios.bytes");
//
//                     if (File.Exists(savePath))
//                     {
//                         File.Delete(savePath);
//                     }
//                     
//                     //文件加密之后，换一文件名存储
//                     File.WriteAllText(savePath, newContent);
//                     //删除老的未加密的配置
//                    
//                     File.Delete(file);
//
//                     if (File.Exists(file + ".meta"))
//                     {
//                         File.Delete(file + ".meta");
//                     }
//                 }
//             }
//
//         }
// #endif
//             AssetDatabase.Refresh();
//         }
//         private static void BuildAssetBundle(BuildTarget target, bool isDebug, bool copyBuildInBundle = false)
// {
//     if (!isDebug)
//     {
//         ConfigurationController.Instance.version = VersionStatus.RELEASE;
//     }
//
//     if (target == BuildTarget.iOS)
//     {
//         EncryptLocaleKv("InstallAssets/Configs/LocaleConfig");
//
//         if (copyBuildInBundle)
//             EncryptLocaleKv("Resources/Export/Configs/LocaleConfig");
//     }
//
//
//     }
// }
