// using System;
// using System.IO;
// using System.Text;
// using System.Security.Cryptography;
// using UnityEngine;
// using YooAsset;
//
// // public static class SecureBundleCrypto
// // {
// //     private static readonly byte[] Magic = Encoding.ASCII.GetBytes("SBND");
// //     private const byte HeaderVersion = 1;
// //
// //     public const int NonceSize = 16;
// //     public const int HeaderSize = 4 + 1 + 8 + NonceSize; // magic + version + plainLength + nonce
// //
// //     // 这里一定要改成你自己项目固定的密钥，并且上线后不要再改
// //     // 否则旧资源会无法解密
// //     private static readonly byte[] MasterSecret = Encoding.UTF8.GetBytes(
// //         "RH04_Tile_Fixed_Master_Secret_Change_Me_2026"
// //     );
// //
// //     public readonly struct BundleHeader
// //     {
// //         public readonly byte[] Nonce;
// //         public readonly long PlainLength;
// //
// //         public BundleHeader(byte[] nonce, long plainLength)
// //         {
// //             Nonce = nonce;
// //             PlainLength = plainLength;
// //         }
// //     }
// //
// //     public static bool ShouldEncrypt(string bundleName)
// //     {
// //         return !string.IsNullOrEmpty(bundleName);
// //     }
// //
// //     public static byte[] DeriveAesKey(string bundleName)
// //     {
// //         bundleName ??= string.Empty;
// //         byte[] nameBytes = Encoding.UTF8.GetBytes(bundleName);
// //
// //         using (var hmac = new HMACSHA256(MasterSecret))
// //         {
// //             // 直接输出 32 字节，作为 AES-256 key
// //             return hmac.ComputeHash(nameBytes);
// //         }
// //     }
// //
// //     /// <summary>
// //     /// 确定性 nonce：
// //     /// 相同 bundleName + 相同明文内容 + 相同明文长度 => 相同 nonce
// //     /// 这样重复打包输出才能稳定
// //     /// </summary>
// //     public static byte[] CreateDeterministicNonce16(string bundleName, byte[] plainData)
// //     {
// //         bundleName ??= string.Empty;
// //         plainData ??= Array.Empty<byte>();
// //
// //         byte[] nameBytes = Encoding.UTF8.GetBytes(bundleName);
// //         byte[] lengthBytes = GetInt64BytesBigEndian(plainData.LongLength);
// //
// //         using (SHA256 sha256 = SHA256.Create())
// //         {
// //             sha256.TransformBlock(nameBytes, 0, nameBytes.Length, null, 0);
// //             sha256.TransformBlock(lengthBytes, 0, lengthBytes.Length, null, 0);
// //
// //             if (plainData.Length > 0)
// //                 sha256.TransformBlock(plainData, 0, plainData.Length, null, 0);
// //
// //             sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
// //
// //             byte[] hash = sha256.Hash;
// //             byte[] nonce = new byte[NonceSize];
// //             Buffer.BlockCopy(hash, 0, nonce, 0, NonceSize);
// //             return nonce;
// //         }
// //     }
// //
// //     public static byte[] BuildHeader(byte[] nonce, long plainLength)
// //     {
// //         if (nonce == null || nonce.Length != NonceSize)
// //             throw new ArgumentException($"Nonce must be {NonceSize} bytes.", nameof(nonce));
// //
// //         byte[] header = new byte[HeaderSize];
// //
// //         Buffer.BlockCopy(Magic, 0, header, 0, Magic.Length);
// //         header[4] = HeaderVersion;
// //
// //         byte[] lenBytes = GetInt64BytesBigEndian(plainLength);
// //         Buffer.BlockCopy(lenBytes, 0, header, 5, 8);
// //         Buffer.BlockCopy(nonce, 0, header, 13, NonceSize);
// //
// //         return header;
// //     }
// //
// //     public static BundleHeader ReadHeader(Stream stream)
// //     {
// //         if (stream == null)
// //             throw new ArgumentNullException(nameof(stream));
// //
// //         byte[] header = new byte[HeaderSize];
// //         ReadExactly(stream, header, 0, HeaderSize);
// //
// //         for (int i = 0; i < Magic.Length; i++)
// //         {
// //             if (header[i] != Magic[i])
// //                 throw new InvalidDataException("Invalid encrypted bundle header magic.");
// //         }
// //
// //         byte version = header[4];
// //         if (version != HeaderVersion)
// //             throw new InvalidDataException($"Unsupported encrypted bundle header version: {version}");
// //
// //         long plainLength = ReadInt64BigEndian(header, 5);
// //
// //         byte[] nonce = new byte[NonceSize];
// //         Buffer.BlockCopy(header, 13, nonce, 0, NonceSize);
// //
// //         return new BundleHeader(nonce, plainLength);
// //     }
// //
// //     public static void TransformAesCtrInPlace(byte[] data, byte[] key, byte[] nonce, long cryptoOffset)
// //     {
// //         if (data == null)
// //             throw new ArgumentNullException(nameof(data));
// //
// //         TransformAesCtrInPlace(data, 0, data.Length, key, nonce, cryptoOffset);
// //     }
// //
// //     public static void TransformAesCtrInPlace(byte[] data, int offset, int count, byte[] key, byte[] nonce, long cryptoOffset)
// //     {
// //         if (data == null)
// //             throw new ArgumentNullException(nameof(data));
// //         if (key == null || (key.Length != 16 && key.Length != 24 && key.Length != 32))
// //             throw new ArgumentException("AES key length must be 16/24/32 bytes.", nameof(key));
// //         if (nonce == null || nonce.Length != NonceSize)
// //             throw new ArgumentException($"Nonce must be {NonceSize} bytes.", nameof(nonce));
// //         if (offset < 0 || count < 0 || offset + count > data.Length)
// //             throw new ArgumentOutOfRangeException();
// //         if (cryptoOffset < 0)
// //             throw new ArgumentOutOfRangeException(nameof(cryptoOffset));
// //
// //         if (count == 0)
// //             return;
// //
// //         long blockIndex = cryptoOffset / 16;
// //         int blockOffset = (int)(cryptoOffset % 16);
// //
// //         using (Aes aes = Aes.Create())
// //         {
// //             aes.Mode = CipherMode.ECB;
// //             aes.Padding = PaddingMode.None;
// //             aes.Key = key;
// //
// //             using (ICryptoTransform encryptor = aes.CreateEncryptor())
// //             {
// //                 byte[] counterBlock = new byte[16];
// //                 byte[] keystream = new byte[16];
// //
// //                 int processed = 0;
// //                 while (processed < count)
// //                 {
// //                     BuildCounterBlock(nonce, blockIndex, counterBlock);
// //                     encryptor.TransformBlock(counterBlock, 0, 16, keystream, 0);
// //
// //                     for (int i = blockOffset; i < 16 && processed < count; i++)
// //                     {
// //                         data[offset + processed] ^= keystream[i];
// //                         processed++;
// //                     }
// //
// //                     blockIndex++;
// //                     blockOffset = 0;
// //                 }
// //             }
// //         }
// //     }
// //
// //     public static byte[] DecryptWholeFile(string filePath, string bundleName)
// //     {
// //         using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
// //         {
// //             BundleHeader header = ReadHeader(fs);
// //
// //             long encryptedLength = fs.Length - HeaderSize;
// //             if (encryptedLength != header.PlainLength)
// //                 throw new InvalidDataException($"Encrypted length mismatch. File={encryptedLength}, Header={header.PlainLength}");
// //
// //             if (header.PlainLength < 0 || header.PlainLength > int.MaxValue)
// //                 throw new InvalidDataException("Encrypted file is too large for LoadFromMemory fallback.");
// //
// //             byte[] data = new byte[header.PlainLength];
// //             ReadExactly(fs, data, 0, data.Length);
// //
// //             byte[] key = DeriveAesKey(bundleName);
// //             TransformAesCtrInPlace(data, key, header.Nonce, 0);
// //
// //             return data;
// //         }
// //     }
// //
// //     private static void BuildCounterBlock(byte[] nonce, long blockIndex, byte[] output)
// //     {
// //         Buffer.BlockCopy(nonce, 0, output, 0, NonceSize);
// //
// //         ulong carry = (ulong)blockIndex;
// //         for (int i = 15; i >= 0 && carry != 0; i--)
// //         {
// //             ulong sum = (ulong)output[i] + (carry & 0xFF);
// //             output[i] = (byte)(sum & 0xFF);
// //             carry = (carry >> 8) + (sum >> 8);
// //         }
// //     }
// //
// //     private static byte[] GetInt64BytesBigEndian(long value)
// //     {
// //         byte[] bytes = BitConverter.GetBytes(value);
// //         if (BitConverter.IsLittleEndian)
// //             Array.Reverse(bytes);
// //         return bytes;
// //     }
// //
// //     private static long ReadInt64BigEndian(byte[] buffer, int offset)
// //     {
// //         byte[] bytes = new byte[8];
// //         Buffer.BlockCopy(buffer, offset, bytes, 0, 8);
// //         if (BitConverter.IsLittleEndian)
// //             Array.Reverse(bytes);
// //         return BitConverter.ToInt64(bytes, 0);
// //     }
// //
// //     private static void ReadExactly(Stream stream, byte[] buffer, int offset, int count)
// //     {
// //         while (count > 0)
// //         {
// //             int read = stream.Read(buffer, offset, count);
// //             if (read <= 0)
// //                 throw new EndOfStreamException("Unexpected end of stream.");
// //             offset += read;
// //             count -= read;
// //         }
// //     }
// // }
//
// // public class AesCtrDecryptStream : Stream
// // {
// //     private readonly FileStream _fileStream;
// //     private readonly byte[] _key;
// //     private readonly byte[] _nonce;
// //     private readonly long _dataStartOffset;
// //     private readonly long _plainLength;
// //     private long _position;
// //
// //     public AesCtrDecryptStream(string filePath, string bundleName)
// //     {
// //         _fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
// //
// //         var header = SecureBundleCrypto.ReadHeader(_fileStream);
// //         _key = SecureBundleCrypto.DeriveAesKey(bundleName);
// //         _nonce = header.Nonce;
// //         _dataStartOffset = SecureBundleCrypto.HeaderSize;
// //         _plainLength = header.PlainLength;
// //         _position = 0;
// //     }
// //
// //     public override bool CanRead => true;
// //     public override bool CanSeek => true;
// //     public override bool CanWrite => false;
// //     public override long Length => _plainLength;
// //
// //     public override long Position
// //     {
// //         get => _position;
// //         set => Seek(value, SeekOrigin.Begin);
// //     }
// //
// //     public override void Flush()
// //     {
// //     }
// //
// //     public override int Read(byte[] buffer, int offset, int count)
// //     {
// //         if (buffer == null)
// //             throw new ArgumentNullException(nameof(buffer));
// //         if (offset < 0 || count < 0 || offset + count > buffer.Length)
// //             throw new ArgumentOutOfRangeException();
// //
// //         if (_position >= _plainLength)
// //             return 0;
// //
// //         int readableCount = (int)Math.Min(count, _plainLength - _position);
// //
// //         _fileStream.Position = _dataStartOffset + _position;
// //         int actualRead = _fileStream.Read(buffer, offset, readableCount);
// //         if (actualRead <= 0)
// //             return 0;
// //
// //         SecureBundleCrypto.TransformAesCtrInPlace(buffer, offset, actualRead, _key, _nonce, _position);
// //         _position += actualRead;
// //         return actualRead;
// //     }
// //
// //     public override long Seek(long offset, SeekOrigin origin)
// //     {
// //         long newPos;
// //         switch (origin)
// //         {
// //             case SeekOrigin.Begin:
// //                 newPos = offset;
// //                 break;
// //             case SeekOrigin.Current:
// //                 newPos = _position + offset;
// //                 break;
// //             case SeekOrigin.End:
// //                 newPos = _plainLength + offset;
// //                 break;
// //             default:
// //                 throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
// //         }
// //
// //         if (newPos < 0)
// //             throw new IOException("Attempted to seek before beginning of stream.");
// //
// //         _position = newPos;
// //         return _position;
// //     }
// //
// //     public override void SetLength(long value) => throw new NotSupportedException();
// //     public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
// //
// //     protected override void Dispose(bool disposing)
// //     {
// //         if (disposing)
// //             _fileStream?.Dispose();
// //
// //         base.Dispose(disposing);
// //     }
// // }
// //
// // public class SecureAesCtrEncryption : IEncryptionServices
// // {
// //     public EncryptResult Encrypt(EncryptFileInfo fileInfo)
// //     {
// //         EncryptResult result = new EncryptResult();
// //
// //         if (!SecureBundleCrypto.ShouldEncrypt(fileInfo.BundleName))
// //         {
// //             result.Encrypted = false;
// //             return result;
// //         }
// //
// //         byte[] plainData = File.ReadAllBytes(fileInfo.FileLoadPath);
// //         byte[] key = SecureBundleCrypto.DeriveAesKey(fileInfo.BundleName);
// //
// //         // 核心改动：不要随机 nonce，改成确定性 nonce
// //         byte[] nonce = SecureBundleCrypto.CreateDeterministicNonce16(fileInfo.BundleName, plainData);
// //
// //         byte[] encryptedData = new byte[plainData.Length];
// //         Buffer.BlockCopy(plainData, 0, encryptedData, 0, plainData.Length);
// //
// //         SecureBundleCrypto.TransformAesCtrInPlace(encryptedData, key, nonce, 0);
// //
// //         byte[] header = SecureBundleCrypto.BuildHeader(nonce, plainData.LongLength);
// //         byte[] finalData = new byte[header.Length + encryptedData.Length];
// //
// //         Buffer.BlockCopy(header, 0, finalData, 0, header.Length);
// //         Buffer.BlockCopy(encryptedData, 0, finalData, header.Length, encryptedData.Length);
// //
// //         result.Encrypted = true;
// //         result.EncryptedData = finalData;
// //         return result;
// //     }
// // }
// //
// // public class SecureAesCtrDecryption : IDecryptionServices
// // {
//     // private const uint ManagedReadBufferSize = 1024 * 64;
//     //
//     // public DecryptResult LoadAssetBundle(DecryptFileInfo fileInfo)
//     // {
//     //     var stream = new AesCtrDecryptStream(fileInfo.FileLoadPath, fileInfo.BundleName);
//     //
//     //     DecryptResult result = new DecryptResult();
//     //     result.ManagedStream = stream;
//     //     result.Result = AssetBundle.LoadFromStream(stream, fileInfo.FileLoadCRC, ManagedReadBufferSize);
//     //     return result;
//     // }
//     //
//     // public DecryptResult LoadAssetBundleAsync(DecryptFileInfo fileInfo)
//     // {
//     //     var stream = new AesCtrDecryptStream(fileInfo.FileLoadPath, fileInfo.BundleName);
//     //
//     //     DecryptResult result = new DecryptResult();
//     //     result.ManagedStream = stream;
//     //     result.CreateRequest = AssetBundle.LoadFromStreamAsync(stream, fileInfo.FileLoadCRC, ManagedReadBufferSize);
//     //     return result;
//     // }
//     //
//     // public DecryptResult LoadAssetBundleFallback(DecryptFileInfo fileInfo)
//     // {
//     //     byte[] plainData = SecureBundleCrypto.DecryptWholeFile(fileInfo.FileLoadPath, fileInfo.BundleName);
//     //
//     //     DecryptResult result = new DecryptResult();
//     //     result.Result = AssetBundle.LoadFromMemory(plainData, fileInfo.FileLoadCRC);
//     //     return result;
//     // }
//     //
//     // public byte[] ReadFileData(DecryptFileInfo fileInfo)
//     // {
//     //     return SecureBundleCrypto.DecryptWholeFile(fileInfo.FileLoadPath, fileInfo.BundleName);
//     // }
//     //
//     // public string ReadFileText(DecryptFileInfo fileInfo)
//     // {
//     //     byte[] bytes = SecureBundleCrypto.DecryptWholeFile(fileInfo.FileLoadPath, fileInfo.BundleName);
//     //     return Encoding.UTF8.GetString(bytes);
//     // }
//
// // }