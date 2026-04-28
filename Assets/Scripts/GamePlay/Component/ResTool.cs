using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Foundation;
using GamePlay.Component;
using UnityEngine;

namespace GamePlay.Utility
{
    public static class ResTool
    {
        private const string Magic = "XRES";
        private const byte Version = 1;
        private const string Password = "YourSecretKey_ChangeMe_2026";

        public struct DecryptResult
        {
            public byte[] RawBytes;
            public string Extension;
        }
        public static async UniTask<DecryptResult> DecryptBytesAsync(byte[] fileBytes)
        {
            return await UniTask.Run(() => DecryptBytes(fileBytes, Password));
        }

        public static DecryptResult DecryptBytes(byte[] fileBytes, string key)
        {
            if (fileBytes == null || fileBytes.Length < 40)
                throw new Exception("Invalid encrypted resource data.");

            int offset = 0;

            string magic = Encoding.ASCII.GetString(fileBytes, offset, 4);
            offset += 4;
            if (magic != Magic)
                throw new Exception("Invalid magic.");

            byte version = fileBytes[offset];
            offset += 1;
            if (version != Version)
                throw new Exception($"Unsupported version: {version}");

            byte extLen = fileBytes[offset];
            offset += 1;

            offset += 2; // reserved

            byte[] nonce = new byte[16];
            Buffer.BlockCopy(fileBytes, offset, nonce, 0, 16);
            offset += 16;

            ulong originalSize = BitConverter.ToUInt64(fileBytes, offset);
            offset += 8;

            int headSize = BitConverter.ToInt32(fileBytes, offset);
            offset += 4;

            string ext = Encoding.UTF8.GetString(fileBytes, offset, extLen);
            offset += extLen;

            int encHeadLen = headSize;
            int encBodyLen = fileBytes.Length - offset - encHeadLen;
            if (encBodyLen < 0)
                throw new Exception("Corrupted encrypted resource data.");

            byte[] encHead = new byte[encHeadLen];
            Buffer.BlockCopy(fileBytes, offset, encHead, 0, encHeadLen);
            offset += encHeadLen;

            byte[] encBody = new byte[encBodyLen];
            Buffer.BlockCopy(fileBytes, offset, encBody, 0, encBodyLen);

            ulong headSeed = DeriveSeed(key + "_head", nonce);
            ulong bodySeed = DeriveSeed(key + "_body", nonce);

            byte[] head = XorStream(encHead, headSeed);
            byte[] body = XorStream(encBody, bodySeed);

            byte[] raw = new byte[head.Length + body.Length];
            Buffer.BlockCopy(head, 0, raw, 0, head.Length);
            Buffer.BlockCopy(body, 0, raw, head.Length, body.Length);

            if ((ulong)raw.Length != originalSize)
                throw new Exception("Decrypted size mismatch.");

            return new DecryptResult
            {
                RawBytes = raw,
                Extension = ext
            };
        }
 

        private static string GetStableFileName(string input, string key)
        {
            using var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input + "|" + key));
            return BitConverter.ToString(hash).Replace("-", "").Substring(0, 24);
        }

        private static ulong DeriveSeed(string key, byte[] nonce)
        {
            using var sha = SHA256.Create();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            byte[] all = new byte[keyBytes.Length + nonce.Length];
            Buffer.BlockCopy(keyBytes, 0, all, 0, keyBytes.Length);
            Buffer.BlockCopy(nonce, 0, all, keyBytes.Length, nonce.Length);

            byte[] hash = sha.ComputeHash(all);
            return BitConverter.ToUInt64(hash, 0);
        }

        private static byte[] XorStream(byte[] data, ulong seed)
        {
            ulong x = seed;
            byte[] output = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                x ^= x >> 12;
                x ^= x << 25;
                x ^= x >> 27;
                ulong rnd = x * 2685821657736338717UL;
                output[i] = (byte)(data[i] ^ (byte)(rnd & 0xFF));
            }

            return output;
        }
        
        //资源加载

        public static Dictionary<string, string> videoCache = new();
        public static Dictionary<string, string> imageCache = new();
        public static Dictionary<string, Texture2D> textureCache = new();
        
        public static void DeleteTempFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning("DeleteTempFile failed: " + e.Message);
            }
        }

        public static bool HasResourcePerpared(string name)
        {
            if (textureCache.ContainsKey(name) || videoCache.ContainsKey(name))
            {
                return true;
            }
            return false;
        }
        public static bool HasResource(string name)
        {
            if (textureCache.ContainsKey(name) || videoCache.ContainsKey(name) ||
                XResDownloadQueue.Instance.ResourceInCache(name))
            {
                return true;
            }
            return false;
        }

        public static async UniTask<Texture2D> GetTexture(string name)
        {
            if (textureCache.TryGetValue(name, out var cachedTexture) && cachedTexture != null)
            {
                return cachedTexture;
            }

            string tempPath = await GetLastImageFile(name);
            if (!string.IsNullOrEmpty(tempPath))
            {
                imageCache[name] = tempPath;

                Texture2D localTexture = await LoadTextureFromFile(tempPath, name);
                if (localTexture != null)
                    return localTexture;
            }

            var res = await XResDownloadQueue.TryGetXRes(name);
            if (res != null)
            {
                if (textureCache.TryGetValue(name, out var tex) && tex != null)
                {
                    return tex;
                }

                await CreateTempImageFileFromXResBytes(res, name);
                if (textureCache.TryGetValue(name, out var newTex) && newTex != null)
                {
                    return newTex;
                }
            }

            return null;
        }
        public static async UniTask<string> GetLastImageFile(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            string dir = Path.Combine(Application.persistentDataPath, "TempImage");
            if (!Directory.Exists(dir))
                return null;

            try
            {
                return await UniTask.RunOnThreadPool(() =>
                {
                    string[] files = Directory.GetFiles(dir, name + ".*", SearchOption.TopDirectoryOnly);
                    if (files == null || files.Length == 0)
                        return null;

                    string latestFile = null;
                    DateTime latestTime = DateTime.MinValue;

                    for (int i = 0; i < files.Length; i++)
                    {
                        string file = files[i];
                        if (!File.Exists(file))
                            continue;

                        DateTime writeTime = File.GetLastWriteTime(file);
                        if (writeTime > latestTime)
                        {
                            latestTime = writeTime;
                            latestFile = file;
                        }
                    }

                    return latestFile;
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning("GetLastImageFile failed: " + e.Message);
                return null;
            }
        }
        public static Texture2D CreateTextureFromXResBytes(DecryptResult result, string name)
        {
            if (textureCache.ContainsKey(name))
            {
                return textureCache[name];
            } 

            string ext = result.Extension?.ToLower(); 

            if (result.RawBytes == null || result.RawBytes.Length == 0)
            {
                Debug.LogError("Decrypt result RawBytes is empty.");
                return null;
            }

            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            bool ok = texture.LoadImage(result.RawBytes, false);
            if (!ok)
            {
                UnityEngine.Object.Destroy(texture);
                Debug.LogError("Texture2D.LoadImage failed. ext = " + ext);
                return null;
            }

            textureCache[name] = texture;
            return texture;
        }
        
        public static async UniTask<string> GetLastFile(string name, string tempFolder)
        {
            if (string.IsNullOrEmpty(name) || checkingList.Contains(name))
                return null;

            string dir = Path.Combine(Application.persistentDataPath,tempFolder);
            if (!Directory.Exists(dir))
                return null;

            try
            {
                checkingList.Add(name);
                return await UniTask.RunOnThreadPool(() =>
                {
                    string[] files = Directory.GetFiles(dir, name + ".*", SearchOption.TopDirectoryOnly);
                    if (files == null || files.Length == 0)
                        return null;

                    string latestFile = null;
                    DateTime latestTime = DateTime.MinValue;

                    for (int i = 0; i < files.Length; i++)
                    {
                        string file = files[i];
                        if (!File.Exists(file))
                            continue;

                        DateTime writeTime = File.GetLastWriteTime(file);
                        if (writeTime > latestTime)
                        {
                            latestTime = writeTime;
                            latestFile = file;
                        }
                    }

                    return latestFile;
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning("GetLastFile failed: " + e.Message);
                return null;
            }
        }
       static  List<string> checkingList = new List<string>();
        public static async UniTask<string> GetVideo(string name)
        { 
            if (videoCache.ContainsKey(name))
            {
                return videoCache[name];
            }
            else
            {
                
                var temp = await GetLastFile(name, "TempVideo");
                checkingList.Remove(name);
                if (!string.IsNullOrEmpty(temp) && !videoCache.ContainsKey(name))
                {
                    videoCache.Add(name, temp);
                    return temp;
                }
                var res = await XResDownloadQueue.TryGetXRes(name);
                if (res != null)
                {
                    if (videoCache.ContainsKey(name))
                    {
                        return videoCache[name];
                    }
                    else
                    { 
                        await CreateTempFileFromXResBytes(res, name);
                        return videoCache[name];
                    }
                }
            }

            return "";
        }
        public static async UniTask<string> CreateTempVideoFileFromXResBytes(DecryptResult result, string name)
        {
            if (videoCache.ContainsKey(name))
            {
                return videoCache[name];
            } 

            string ext = result.Extension?.ToLower(); 
            if (result.RawBytes == null || result.RawBytes.Length == 0)
            {
                Debug.LogError("Decrypt result RawBytes is empty.");
                return null;
            }

            try
            {
                string dir = Path.Combine(Application.persistentDataPath, "TempVideo");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                string fileNameWithoutExt = name;
                // string fileNameWithoutExt = Guid.NewGuid().ToString("N");

                string filePath = Path.Combine(dir, fileNameWithoutExt + ext);
                File.WriteAllBytes(filePath, result.RawBytes);
                videoCache[name] =  filePath;
                return filePath;
            }
            catch (Exception e)
            {
                Debug.LogError("Write temp video file failed: " + e.Message);
                return null;
            }
        }

        public static async UniTask CreateTempFileFromXResBytes(byte[] bytes, string photo)
        {
            if (videoCache.ContainsKey(photo) || textureCache.ContainsKey(photo))
            {
                return;
            }
            if (bytes == null || bytes.Length == 0)
            {
                Debug.LogError("xresBytes is null or empty." + photo);
                return ;
            }
            DecryptResult result;
            try
            {
                result = await DecryptBytesAsync(bytes);
            }
            catch (Exception e)
            {
                Debug.LogError("Decrypt xres video failed: " + e.Message);
                return;
            }
            string ext = result.Extension?.ToLower();
            if (ext.Contains("mp4"))
            {
                await CreateTempVideoFileFromXResBytes(result, photo);
            }
            else
            {
                CreateTextureFromXResBytes(result, photo);
            }
        }
        
        public static bool SaveCachedMediaToPhone(string name)
        {
            name = GUtility.GetPhotoName(name);
            if (videoCache.ContainsKey(name))
            {
                return SaveCachedVideoToPhone(name);
            }

            if (textureCache.ContainsKey(name))
            {
                return SaveCachedTextureToPhone(name, false, 95);
            }

            Debug.LogError($"SaveCachedMediaToPhone failed, cache not found: {name}");
            return false;
        }
        
        public static bool SaveCachedVideoToPhone(string name)
        {
            if (!videoCache.TryGetValue(name, out var videoPath) || string.IsNullOrEmpty(videoPath))
            {
                return false;
            }

            if (!File.Exists(videoPath))
            {
                Debug.LogError($"SaveCachedVideoToPhone failed, file not found: {videoPath}");
                return false;
            }

            return AndroidGallerySaver.SaveVideo(videoPath, Path.GetFileName(videoPath));
        }
        
        public static bool SaveCachedTextureToPhone(string name, bool saveAsJpg = false, int jpgQuality = 95)
        {
            if (!textureCache.TryGetValue(name, out var texture) || texture == null)
            {
                Debug.LogError($"SaveCachedTextureToPhone failed, texture cache not found: {name}");
                return false;
            }

            try
            {
                string tempImagePath = WriteTextureToTempFile(name, texture, saveAsJpg, jpgQuality);
                if (string.IsNullOrEmpty(tempImagePath) || !File.Exists(tempImagePath))
                {
                    Debug.LogError("SaveCachedTextureToPhone failed, temp image file create failed.");
                    return false;
                }

                return AndroidGallerySaver.SaveImage(tempImagePath, Path.GetFileName(tempImagePath));
            }
            catch (Exception e)
            {
                Debug.LogError("SaveCachedTextureToPhone exception: " + e.Message);
                return false;
            }
        }
        
        public static async UniTask CreateTempImageFileFromXResBytes(byte[] bytes, string name)
        {
            if (textureCache.ContainsKey(name))
            {
                return;
            }

            if (bytes == null || bytes.Length == 0)
            {
                Debug.LogError("xresBytes is null or empty. " + name);
                return;
            }

            DecryptResult result;
            try
            {
                result = await DecryptBytesAsync(bytes);
            }
            catch (Exception e)
            {
                Debug.LogError("Decrypt xres image failed: " + e.Message);
                return;
            }

            string ext = result.Extension?.ToLower();
            if (string.IsNullOrEmpty(ext))
                ext = ".png";

            if (result.RawBytes == null || result.RawBytes.Length == 0)
            {
                Debug.LogError("Decrypt result RawBytes is empty.");
                return;
            }

            try
            {
                string dir = Path.Combine(Application.persistentDataPath, "TempImage");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string filePath = Path.Combine(dir, name + ext);
                File.WriteAllBytes(filePath, result.RawBytes);
                imageCache[name] = filePath;

                await LoadTextureFromFile(filePath, name);
            }
            catch (Exception e)
            {
                Debug.LogError("Write temp image file failed: " + e.Message);
            }
        }
        
        private static async UniTask<Texture2D> LoadTextureFromFile(string filePath, string name)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                byte[] bytes = await UniTask.RunOnThreadPool(() => File.ReadAllBytes(filePath));
                if (bytes == null || bytes.Length == 0)
                    return null;

                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                bool ok = texture.LoadImage(bytes, false);
                if (!ok)
                {
                    UnityEngine.Object.Destroy(texture);
                    Debug.LogError("LoadTextureFromFile failed: " + filePath);
                    return null;
                }

                textureCache[name] = texture;
                return texture;
            }
            catch (Exception e)
            {
                Debug.LogWarning("LoadTextureFromFile failed: " + e.Message);
                return null;
            }
        }
        
        
        private static string WriteTextureToTempFile(string name, Texture2D texture, bool saveAsJpg, int jpgQuality)
        {
            byte[] bytes;
            string ext;

            if (saveAsJpg)
            {
                bytes = texture.EncodeToJPG(Mathf.Clamp(jpgQuality, 1, 100));
                ext = ".jpg";
            }
            else
            {
                bytes = texture.EncodeToPNG();
                ext = ".png";
            }

            if (bytes == null || bytes.Length == 0)
            {
                Debug.LogError("WriteTextureToTempFile failed, encode bytes is empty.");
                return null;
            }

            string dir = Path.Combine(Application.persistentDataPath, "TempImage");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // string safeName = string.IsNullOrEmpty(name) ? Guid.NewGuid().ToString("N") : name;
            string fileName = name+  ext;
            string filePath = Path.Combine(dir, fileName);

            File.WriteAllBytes(filePath, bytes);
            return filePath;
        }

    }
}