using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace GameConfig
{
    public static class ConfigCrypto
    {
        /// <summary>
        /// 解密 txt 文本为 json
        /// 流程：
        /// 1. Base64 解码
        /// 2. XOR 解密
        /// 3. zlib 解压
        /// 4. UTF8 转字符串
        /// </summary>
        public static string DecryptToJson(string encryptedText, string key)
        {
            if (string.IsNullOrEmpty(encryptedText))
                throw new ArgumentException("encryptedText is null or empty");

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key is null or empty");

            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] keyBytes = GetSha256Bytes(key);

            // XOR 解密
            byte[] compressedBytes = new byte[encryptedBytes.Length];
            for (int i = 0; i < encryptedBytes.Length; i++)
            {
                compressedBytes[i] = (byte)(encryptedBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            // zlib 解压
            byte[] jsonBytes = DecompressZlib(compressedBytes);

            return Encoding.UTF8.GetString(jsonBytes);
        }

        /// <summary>
        /// 从 Resources 加载 txt 并解密
        /// 例如 Assets/Resources/configs.txt
        /// 调用：DecryptResourceText("configs", "gxjtext-key")
        /// </summary>
        public static string DecryptResourceText(string text, string key)
        { 
            return DecryptToJson(text, key);
        }

        private static byte[] GetSha256Bytes(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            }
        }

        /// <summary>
        /// 兼容 Unity 的 zlib 解压
        /// Python 那边用的是 zlib.compress，所以这里不能直接把完整数据丢给 DeflateStream
        /// 需要去掉 zlib 头 2 字节 和 adler32 尾 4 字节
        /// </summary>
        private static byte[] DecompressZlib(byte[] zlibData)
        {
            if (zlibData == null || zlibData.Length < 6)
                throw new Exception("Invalid zlib data.");

            int deflateLength = zlibData.Length - 2 - 4;
            byte[] deflateData = new byte[deflateLength];
            Buffer.BlockCopy(zlibData, 2, deflateData, 0, deflateLength);

            using (MemoryStream input = new MemoryStream(deflateData))
            using (DeflateStream deflateStream = new DeflateStream(input, CompressionMode.Decompress))
            using (MemoryStream output = new MemoryStream())
            {
                deflateStream.CopyTo(output);
                return output.ToArray();
            }
        }
    }
}