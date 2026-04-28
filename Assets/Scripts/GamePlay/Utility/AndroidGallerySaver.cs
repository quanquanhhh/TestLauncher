using System;
using System.IO;
using GamePlay;
using UnityEngine;

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public static class AndroidGallerySaver
{
    public static bool SaveImage(string sourceFilePath, string targetFileName = null)
    {
        return SaveMedia(sourceFilePath, false, targetFileName);
    }

    public static bool SaveVideo(string sourceFilePath, string targetFileName = null)
    {
        return SaveMedia(sourceFilePath, true, targetFileName);
    }

    private static bool SaveMedia(string sourceFilePath, bool isVideo, string targetFileName)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (currentActivity == null)
            {
                Debug.LogError("currentActivity is null");
                return false;
            }

            if (string.IsNullOrEmpty(sourceFilePath) || !File.Exists(sourceFilePath))
            {
                Debug.LogError($"SaveMedia failed, file not found: {sourceFilePath}");
                return false;
            }

            string ext = Path.GetExtension(sourceFilePath)?.ToLowerInvariant(); 
            if (string.IsNullOrEmpty(targetFileName))
            {
                targetFileName = Path.GetFileName(sourceFilePath);
            }
            else
            {
                if (!targetFileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                    targetFileName += ext;
            }

            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                int sdkInt = version.GetStatic<int>("SDK_INT");
                if (sdkInt >= 29)
                    return SaveByMediaStore(currentActivity, sourceFilePath, targetFileName, isVideo);
                else
                    return SaveLegacyMediaStore(currentActivity, sourceFilePath, targetFileName, isVideo); 
            }
        }
        catch (Exception e)
        {
            Debug.LogError("SaveMedia exception: " + e);
            return false;
        }
#else
        Debug.LogWarning("SaveMedia only works on Android device.");
        return false;
#endif
    }

    private static bool SaveLegacyMediaStore(AndroidJavaObject activity, string path, string fileName, bool isVideo)
    {
        if (isVideo)
        {
            return SaveVideoInPhoneLegacy(activity, path, fileName);
        }
        else
        {
            return SaveImageInPhoneLegacy(activity, path, fileName);
            
        }
    }

    private static bool SaveImageInPhoneLegacy(AndroidJavaObject activity, string imagePath, string fileName)
    {
        try
        {
            Debug.Log("Using public directory for Android 9-");
            
            // 获取公共图片目录
            AndroidJavaClass environment = new AndroidJavaClass("android.os.Environment");
            AndroidJavaObject picturesDir = environment.CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory", 
                environment.GetStatic<string>("DIRECTORY_PICTURES"));
            
            // 创建应用子目录
            AndroidJavaObject appDir = new AndroidJavaObject("java.io.File", picturesDir, Application.productName);
            if (!appDir.Call<bool>("exists"))
            {
                bool created = appDir.Call<bool>("mkdirs");
                Debug.Log("Created app directory: " + created);
            }
            
            // 创建目标文件
            string displayName = SanitizeFileName(fileName);
            AndroidJavaObject destFile = new AndroidJavaObject("java.io.File", appDir, displayName);
            string destPath = destFile.Call<string>("getAbsolutePath");
            
            Debug.Log($"Saving to: {destPath}");
            
            // 复制文件
            File.Copy(imagePath, destPath, true);
            
            // 扫描文件
            AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
            AndroidJavaClass mediaScannerConnection = new AndroidJavaClass("android.media.MediaScannerConnection");
            mediaScannerConnection.CallStatic("scanFile", 
                context, 
                new string[] { destPath }, 
                new string[] { "image/jpeg" }, 
                null);
            
            Debug.Log("Image saved to public directory");
            // MainController.ShowTips("Image saved to album");
            
        } catch (Exception e)
        {
            Debug.LogError("SaveImageToPublicDirectory failed: " + e.Message);
            Debug.LogError("Stack trace: " + e.StackTrace);
            // MainController.ShowTips("Failed to save image");
            return false;
        }

        return true;
    }

    private static bool SaveVideoInPhoneLegacy(AndroidJavaObject activity, string videoPath, string fileName)
    {
        
        try
        {
            Debug.Log("Using public directory for Android 9- (Video)");
            
            // 获取公共视频目录
            AndroidJavaClass environment = new AndroidJavaClass("android.os.Environment");
            AndroidJavaObject moviesDir = environment.CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory", 
                environment.GetStatic<string>("DIRECTORY_MOVIES"));
            
            // 创建应用子目录
            AndroidJavaObject appDir = new AndroidJavaObject("java.io.File", moviesDir, Application.productName);
            if (!appDir.Call<bool>("exists"))
            {
                bool created = appDir.Call<bool>("mkdirs");
                Debug.Log("Created app directory: " + created);
            }
            
            // 创建目标文件
            string displayName = SanitizeFileName(fileName);
            AndroidJavaObject destFile = new AndroidJavaObject("java.io.File", appDir, displayName);
            string destPath = destFile.Call<string>("getAbsolutePath");
            
            Debug.Log($"Saving to: {destPath}");
            
            // 复制文件
            File.Copy(videoPath, destPath, true);
            
            // 扫描文件
            AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
            AndroidJavaClass mediaScannerConnection = new AndroidJavaClass("android.media.MediaScannerConnection");
            mediaScannerConnection.CallStatic("scanFile", 
                context, 
                new string[] { destPath }, 
                new string[] { "video/mp4" }, 
                null);
            
            Debug.Log("Video saved to public directory");
            // MainController.ShowTips("Video saved to album");
            
        } catch (Exception e)
        {
            Debug.LogError("SaveVideoToPublicDirectory failed: " + e.Message);
            Debug.LogError("Stack trace: " + e.StackTrace);
            return false;
            // MainController.ShowTips("Failed to save video");
        }

        return true;
    }
 
    private static bool SaveByMediaStore(AndroidJavaObject currentActivity, string sourceFilePath,
        string targetFileName ,bool isVideo)
    {
        if (isVideo)
        {
            return SaveVideoInPhone(currentActivity, sourceFilePath, targetFileName);
        }
        else
        {
            return SaveImageInPhone(currentActivity, sourceFilePath, targetFileName);
        }
    }

    private static bool SaveImageInPhone(AndroidJavaObject activity, string imagePath, string fileName)
    {
        
        try
        {
            Debug.Log("Using MediaStore API for Android 10+");
            
            AndroidJavaObject contentResolver = activity.Call<AndroidJavaObject>("getContentResolver");
            
            // 创建ContentValues
            AndroidJavaObject values = new AndroidJavaObject("android.content.ContentValues");
            
            // 设置文件名和MIME类型
            string displayName = SanitizeFileName(fileName);
            
            // 设置基本字段
            values.Call("put", "title", displayName);
            values.Call("put", "mime_type", "image/jpeg");
            
            // 直接使用Uri.parse创建EXTERNAL_CONTENT_URI
            AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
            AndroidJavaObject externalContentUri = uriClass.CallStatic<AndroidJavaObject>("parse", "content://media/external/images/media");
            
            // 插入到MediaStore
            AndroidJavaObject uri = contentResolver.Call<AndroidJavaObject>("insert", externalContentUri, values);
            
            if (uri == null)
            {
                Debug.LogError("Failed to insert into MediaStore");
                // MainController.ShowTips("Failed to save image");
                return false;
            }
            
            // 打开输出流
            AndroidJavaObject outputStream = contentResolver.Call<AndroidJavaObject>("openOutputStream", uri);
            if (outputStream == null)
            {
                Debug.LogError("Failed to open output stream");
                // MainController.ShowTips("Failed to save image");
                return false;
            }
            
            // 读取文件数据
            byte[] bytes = File.ReadAllBytes(imagePath);
            
            // 写入数据
            outputStream.Call("write", bytes);
            
            // 关闭流
            outputStream.Call("close");
            
            Debug.Log("Image saved via MediaStore API");
            // MainController.ShowTips("Image saved to album");
            
        } catch (Exception e)
        {
            Debug.LogError("SaveImageWithMediaStore failed: " + e.Message);
            Debug.LogError("Stack trace: " + e.StackTrace);
            return false;
            // MainController.ShowTips("Failed to save image");
        }

        return true;
    }

    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return "media_" + DateTime.Now.Ticks;
        }

        // 移除不支持的字符
        string invalidChars = new string(Path.GetInvalidFileNameChars());
        string sanitized = fileName;
        foreach (char c in invalidChars)
        {
            sanitized = sanitized.Replace(c.ToString(), "");
        }

        // 限制文件名长度
        if (sanitized.Length > 50)
        {
            sanitized = sanitized.Substring(0, 50);
        }

        return sanitized;
    } 
    private static bool SaveVideoInPhone(AndroidJavaObject activity, string videoPath, string fileName)
    {
        
        try
        {
            Debug.Log("Using MediaStore API for Android 10+ (Video)");
            
            AndroidJavaObject contentResolver = activity.Call<AndroidJavaObject>("getContentResolver");
            
            // 直接使用Uri.parse创建EXTERNAL_CONTENT_URI
            AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
            AndroidJavaObject externalContentUri = uriClass.CallStatic<AndroidJavaObject>("parse", "content://media/external/video/media");
            
            // 创建ContentValues
            AndroidJavaObject values = new AndroidJavaObject("android.content.ContentValues");
            
            // 设置文件名和MIME类型
            string displayName = SanitizeFileName(fileName);
            
            // 设置基本字段
            values.Call("put", "title", displayName);
            values.Call("put", "mime_type", "video/mp4");
            
            // 插入到MediaStore
            AndroidJavaObject uri = contentResolver.Call<AndroidJavaObject>("insert", externalContentUri, values);
            
            if (uri == null)
            {
                Debug.LogError("Failed to insert into MediaStore (Video)");
                // MainController.ShowTips("Failed to save video");
                return false;
            }
            
            Debug.Log($"MediaStore URI (Video): {uri.ToString()}");
            
            // 打开输出流
            AndroidJavaObject outputStream = contentResolver.Call<AndroidJavaObject>("openOutputStream", uri);
            if (outputStream == null)
            {
                Debug.LogError("Failed to open output stream (Video)");
                // MainController.ShowTips("Failed to save video");
                return false;
            }
            
            // 读取文件数据
            byte[] bytes = File.ReadAllBytes(videoPath);
            
            // 写入数据
            outputStream.Call("write", bytes);
            Debug.Log($"Wrote {bytes.Length} bytes to output stream (Video)");
            
            // 关闭流
            outputStream.Call("close");
            
            Debug.Log("Video saved via MediaStore API");
            // MainController.ShowTips("Video saved to album");
            
        } catch (Exception e)
        {
            Debug.LogError("SaveVideoWithMediaStore failed: " + e.Message);
            Debug.LogError("Stack trace: " + e.StackTrace);
            // MainController.ShowTips("Failed to save video");
        }
        return true;
    }
    
}