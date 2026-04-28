using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

public static class AtlasProcessUtility
{
    [Serializable]
    public class BundleFolderInfo
    {
        public string bundleName;
        public string folderAssetPath;
    }

    public static void ProcessAtlases(IList<BundleFolderInfo> bundleFolders, BuildTarget buildTarget)
    {
        if (bundleFolders == null || bundleFolders.Count == 0)
        {
            Debug.LogWarning("[Atlas] 没有可处理的数据。");
            return;
        }

        string platformName = GetAtlasPlatformName(buildTarget);
        int processedFolderCount = 0;
        int changedTextureCount = 0;
        int createdAtlasCount = 0;
        int updatedAtlasCount = 0;
        int skippedFolderCount = 0;

        try
        {
            for (int i = 0; i < bundleFolders.Count; i++)
            {
                var info = bundleFolders[i];
                float progress = (i + 1f) / bundleFolders.Count;
                EditorUtility.DisplayProgressBar(
                    "处理图集",
                    $"{info.bundleName} ({i + 1}/{bundleFolders.Count})",
                    progress);

                if (info == null ||
                    string.IsNullOrWhiteSpace(info.bundleName) ||
                    string.IsNullOrWhiteSpace(info.folderAssetPath))
                {
                    skippedFolderCount++;
                    continue;
                }

                if (!AssetDatabase.IsValidFolder(info.folderAssetPath))
                {
                    Debug.LogWarning($"[Atlas] 文件夹不存在，跳过：{info.folderAssetPath}");
                    skippedFolderCount++;
                    continue;
                }

                var texturePaths = GetTexturePathsInFolder(info.folderAssetPath);
                if (texturePaths.Count == 0)
                {
                    Debug.Log($"[Atlas] 文件夹内没有图片，跳过：{info.folderAssetPath}");
                    skippedFolderCount++;
                    continue;
                }

                processedFolderCount++;

                changedTextureCount += SetTexturesNoMipmaps(texturePaths);

                bool atlasCreated;
                var atlas = FindOrCreateAtlas(info.bundleName, info.folderAssetPath, out atlasCreated);
                if (atlas == null)
                {
                    Debug.LogError($"[Atlas] 创建或获取图集失败：{info.bundleName}");
                    continue;
                }

                if (atlasCreated)
                    createdAtlasCount++;
                else
                    updatedAtlasCount++;

                EnsureFolderPackedInAtlas(atlas, info.folderAssetPath);
                ApplyAtlasSettings(atlas, platformName);

                EditorUtility.SetDirty(atlas);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log(
            $"[Atlas] 处理完成。" +
            $"\n处理文件夹数：{processedFolderCount}" +
            $"\n跳过文件夹数：{skippedFolderCount}" +
            $"\n修改图片导入设置数：{changedTextureCount}" +
            $"\n新建图集数：{createdAtlasCount}" +
            $"\n更新图集数：{updatedAtlasCount}");
    }

    private static List<string> GetTexturePathsInFolder(string folderAssetPath)
    {
        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderAssetPath });
        var result = new List<string>(guids.Length);

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
                continue;

            // 排除非图片资源的极端情况
            string ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext is ".png" or ".jpg" or ".jpeg" or ".tga" or ".psd" or ".tif" or ".tiff" or ".bmp" or ".gif")
            {
                result.Add(path);
            }
        }

        return result;
    }

    private static int SetTexturesNoMipmaps(List<string> texturePaths)
    {
        int changedCount = 0;

        foreach (string texturePath in texturePaths)
        {
            var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer == null)
                continue;

            bool changed = false;

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            // 如果你希望强制都作为 Sprite，可以打开这两行：
            // if (importer.textureType != TextureImporterType.Sprite)
            // {
            //     importer.textureType = TextureImporterType.Sprite;
            //     changed = true;
            // }

            if (changed)
            {
                importer.SaveAndReimport();
                changedCount++;
            }
        }

        return changedCount;
    }

    private static SpriteAtlas FindOrCreateAtlas(string bundleName, string folderAssetPath, out bool created)
    {
        created = false;

        // 优先找同目录同名图集
        string atlasPathInFolder = $"{folderAssetPath}/{bundleName}.spriteatlas";
        var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPathInFolder);
        if (atlas != null)
            return atlas;

        // 全局找同名图集
        string[] guids = AssetDatabase.FindAssets($"{bundleName} t:SpriteAtlas");
        var matchedPaths = new List<string>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == bundleName)
            {
                matchedPaths.Add(path);
            }
        }

        if (matchedPaths.Count > 0)
        {
            // 优先选与目标文件夹同目录的
            string preferred = matchedPaths.FirstOrDefault(p =>
                string.Equals(NormalizePath(Path.GetDirectoryName(p)), NormalizePath(folderAssetPath), StringComparison.OrdinalIgnoreCase));

            string chosenPath = preferred ?? matchedPaths[0];
            atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(chosenPath);

            if (matchedPaths.Count > 1)
            {
                Debug.LogWarning(
                    $"[Atlas] 发现多个同名图集 {bundleName}，本次使用：{chosenPath}\n" +
                    string.Join("\n", matchedPaths));
            }

            return atlas;
        }

        // 没有则创建
        atlas = new SpriteAtlas();
        AssetDatabase.CreateAsset(atlas, atlasPathInFolder);
        created = true;

        Debug.Log($"[Atlas] 已创建图集：{atlasPathInFolder}");
        return atlas;
    }

    private static void EnsureFolderPackedInAtlas(SpriteAtlas atlas, string folderAssetPath)
    {
        var folderObj = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folderAssetPath);
        if (folderObj == null)
        {
            Debug.LogError($"[Atlas] 无法加载文件夹对象：{folderAssetPath}");
            return;
        }

        var packables = atlas.GetPackables();
        bool alreadyHasFolder = packables.Any(p =>
            string.Equals(NormalizePath(AssetDatabase.GetAssetPath(p)),
                          NormalizePath(folderAssetPath),
                          StringComparison.OrdinalIgnoreCase));

        if (alreadyHasFolder)
            return;

        // 为了避免旧图集里已经零散加了这个目录下的一堆图片，先移除这些零散项，再直接加整个文件夹
        var removable = packables
            .Where(p =>
            {
                string path = NormalizePath(AssetDatabase.GetAssetPath(p));
                return !string.IsNullOrEmpty(path)
                       && !string.Equals(path, NormalizePath(folderAssetPath), StringComparison.OrdinalIgnoreCase)
                       && IsPathInFolder(path, folderAssetPath);
            })
            .ToArray();

        if (removable.Length > 0)
        {
            atlas.Remove(removable);
        }

        atlas.Add(new UnityEngine.Object[] { folderObj });
    }

    private static void ApplyAtlasSettings(SpriteAtlas atlas, string platformName)
    {
        // 1. Packing Settings
        var packingSettings = atlas.GetPackingSettings();
        packingSettings.enableRotation = false;
        packingSettings.enableTightPacking = false;
        atlas.SetPackingSettings(packingSettings);

        // 2. Texture Settings
        var textureSettings = atlas.GetTextureSettings();
        textureSettings.generateMipMaps = false;
        atlas.SetTextureSettings(textureSettings);

        // 3. Platform Override
        var platformSettings = atlas.GetPlatformSettings(platformName);
        platformSettings.overridden = true;
        platformSettings.format = TextureImporterFormat.ASTC_6x6;
        platformSettings.crunchedCompression = false;
        atlas.SetPlatformSettings(platformSettings);

        // 如果你想立刻刷新 Pack Preview，可以手动点一下图集查看。
        // 某些项目里也会额外主动 PackAtlases，但为了减少版本差异导致的编译问题，这里先不强行调用。
    }

    private static string GetAtlasPlatformName(BuildTarget buildTarget)
    {
        switch (buildTarget)
        {
            case BuildTarget.Android:
                return "Android";

            case BuildTarget.iOS:
                return "iPhone";

            case BuildTarget.tvOS:
                return "tvOS";

            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
            case BuildTarget.StandaloneOSX:
            case BuildTarget.StandaloneLinux64:
#if UNITY_2021_2_OR_NEWER
            case BuildTarget.StandaloneLinuxUniversal:
#endif
                return "Standalone";

            default:
                // 其他平台先退回 Default，不强开 override
                // 但你这里需求是“对应平台打开 override”，所以建议你的工具主要给 Android/iOS 用。
                Debug.LogWarning($"[Atlas] 未专门适配的平台：{buildTarget}，将使用 Default 平台名。");
                return "DefaultTexturePlatform";
        }
    }

    private static bool IsPathInFolder(string assetPath, string folderAssetPath)
    {
        string folder = NormalizePath(folderAssetPath).TrimEnd('/') + "/";
        string path = NormalizePath(assetPath);
        return path.StartsWith(folder, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string path)
    {
        return string.IsNullOrEmpty(path) ? string.Empty : path.Replace("\\", "/");
    }
}