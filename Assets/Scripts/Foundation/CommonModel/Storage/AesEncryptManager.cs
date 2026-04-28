using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Foundation.Storage
{
    /// <summary>
    /// AES 加密管理器
    /// 说明：
    /// 1. 每次加密都会随机生成 IV
    /// 2. 输出格式： [16字节IV][密文]
    /// 3. Key 通过 password 做 SHA256 得到 32 字节
    /// </summary>
    public class AesEncryptManager : SingletonScript<AesEncryptManager>
    {
        /// <summary>
        /// 你可以改成自己的项目密钥
        /// 不要太短，建议 16 字符以上
        /// </summary>
        private string _password = "AXZ90_Running_RH04";

        public string Password
        {
            get => _password;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(value));
                _password = value;
            }
        }

        /// <summary>
        /// 通过 SHA256 把字符串转成 32 字节 Key
        /// </summary>
        private byte[] GetKeyBytes()
        {
            using (var sha = SHA256.Create())
            {
                return sha.ComputeHash(Encoding.UTF8.GetBytes(_password));
            }
        }

        /// <summary>
        /// 加密字节数组
        /// 输出格式：[16字节IV][密文]
        /// </summary>
        public byte[] EncryptBytes(byte[] plainBytes)
        {
            if (plainBytes == null || plainBytes.Length == 0)
                throw new ArgumentNullException(nameof(plainBytes));

            byte[] keyBytes = GetKeyBytes();

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.GenerateIV(); // 每次随机 IV
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var ms = new MemoryStream())
                {
                    // 先写入 IV
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(plainBytes, 0, plainBytes.Length);
                        cs.FlushFinalBlock();
                    }

                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// 解密字节数组
        /// 输入格式必须是：[16字节IV][密文]
        /// </summary>
        public byte[] DecryptBytes(byte[] cipherBytes)
        {
            if (cipherBytes == null || cipherBytes.Length <= 16)
                throw new ArgumentNullException(nameof(cipherBytes));

            byte[] keyBytes = GetKeyBytes();

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                byte[] iv = new byte[16];
                Buffer.BlockCopy(cipherBytes, 0, iv, 0, 16);
                aes.IV = iv;

                int dataLength = cipherBytes.Length - 16;

                using (var msInput = new MemoryStream(cipherBytes, 16, dataLength))
                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var cs = new CryptoStream(msInput, decryptor, CryptoStreamMode.Read))
                using (var msOutput = new MemoryStream())
                {
                    cs.CopyTo(msOutput);
                    return msOutput.ToArray();
                }
            }
        }

        /// <summary>
        /// 加密字符串，返回字节数组
        /// </summary>
        public byte[] EncryptString(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            return EncryptBytes(plainBytes);
        }

        /// <summary>
        /// 解密为字符串
        /// </summary>
        public string DecryptToString(byte[] cipherBytes)
        {
            byte[] plainBytes = DecryptBytes(cipherBytes);
            return Encoding.UTF8.GetString(plainBytes);
        }

        /// <summary>
        /// 加密文件
        /// </summary>
        public void EncryptFile(string inputPath, string outputPath)
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException($"Input file not found : {inputPath}");

            byte[] raw = File.ReadAllBytes(inputPath);
            byte[] enc = EncryptBytes(raw);
            File.WriteAllBytes(outputPath, enc);
        }

        /// <summary>
        /// 解密文件
        /// </summary>
        public void DecryptFile(string inputPath, string outputPath)
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException($"Input file not found : {inputPath}");

            byte[] enc = File.ReadAllBytes(inputPath);
            byte[] raw = DecryptBytes(enc);
            File.WriteAllBytes(outputPath, raw);
        }
    }
}