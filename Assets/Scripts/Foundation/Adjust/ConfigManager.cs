using Newtonsoft.Json;
using UnityEngine;

namespace Foundation
{
    [System.Serializable]
    public class APPConfig
    {
        public string platform;
        public string game_name;
        public string package_name;
        public string email;
        public string firebase_topic;
        public Libs libs;
    }
    [System.Serializable]
    public class Libs
    {
        public AppsFlyers appsflyer;
        public Adjusts_Config adjust;
        public string fb_appid;
        public string fb_apptoken;
        public Max_config max;
        public Topon topon;
    }
    
    [System.Serializable]
    public class AppsFlyers
    {
        public string dev_key;
        public string ios_appid;
    }
    
    [System.Serializable]
    public class Adjusts_Config
    {
        public string dev_key;
    }

    [System.Serializable]
    public class Max_config
    {
        public string rewarded_ad_unit_id;
        public string interstitial_ad_unit_id;
        public string banner_ad_unit_id;
        public string appopen_ad_unit_id;
    }

    [System.Serializable]
    public class Topon
    {
        public string app_id;
        public string app_key;
        public string rewarded_ad_unit_id;
        public string interstitial_ad_unit_id;
    }


    public class ConfigManager
    {
        private static string ConfigTxtPath = "/Resources/Config/";
        private const string RELEASE_CONFIG_PATH = "Config/config";
        private const string ENCRYPTION_KEY = "Running123123";
         
        private static string useKey;

        public static APPConfig useConfig;

        public static string AdjustDevKey
        {
            get
            {
                return useConfig.libs.adjust.dev_key;
            }
        }

        public static string AdRewardUnit
        {
            get
            {
                return useConfig.libs.max.rewarded_ad_unit_id;
            }
        }

        public static string AdInterstitialUnit
        {
            get
            {
                return useConfig.libs.max.interstitial_ad_unit_id;
            }
        }

        public static string AdBannerUnit
        {
            get
            {
                return useConfig.libs.max.banner_ad_unit_id;
            }
        }


        public static string AdOpenUnit
        {
            get
            {
                return useConfig.libs.max.appopen_ad_unit_id;
            }
        }

        
        
        public static void GetUseConfig()
        { 
// #if UNITY_IOS && (DEBUG_APP || DEBUG) 
//             useKey = ConfigSelect.IOSTest;
// #elif UNITY_ANDROID && (DEBUG_APP || DEBUG)
//             useKey = ConfigManager.AndroidTest;
// #elif UNITY_EDITOR
//             useKey = ConfigSelect.AndroidTest;
// #endif
//             if (string.IsNullOrEmpty(useKey))
//             {
//                 
//                 useKey = AndroidTest;
//             }
            
            TextAsset textAsset = Resources.Load<TextAsset>(RELEASE_CONFIG_PATH);
            if (textAsset != null) useKey = textAsset.text;
            useConfig = LoadConfig<APPConfig>(useKey);
        }
        private static string Encrypt(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText) || string.IsNullOrEmpty(key))
                return plainText;
            
            byte[] plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
            byte[] encryptedBytes = new byte[plainBytes.Length];
        
            for (int i = 0; i < plainBytes.Length; i++)
            {
                encryptedBytes[i] = (byte)(plainBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }
        
            return System.Convert.ToBase64String(encryptedBytes);
        }

        
#if UNITY_EDITOR
        public static void EncryptConfig()
        {
            try
            {
                string configFileName = "Config.txt";
                string configFilePath = Application.dataPath + ConfigTxtPath + configFileName;
            
                if (!System.IO.File.Exists(configFilePath))
                {
                    Debug.LogError("Not Found Config.txt: " + configFilePath);
                    return;
                }
           
                string plainText = System.IO.File.ReadAllText(configFilePath);
                Debug.Log("Original Txt Content: " + plainText);
                string encryptedText = Encrypt(plainText, ENCRYPTION_KEY);
                Debug.Log("Encrypt txt Content: " + encryptedText);
            
                System.IO.File.WriteAllText(configFilePath, encryptedText);          
                UnityEditor.AssetDatabase.Refresh();       
                Debug.Log("Configuration file encryption successful，Saved to: " + configFilePath);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Encryption configuration file failed: " + e.Message);
            }
        }
#endif
        
        private static T LoadConfig<T>(string path) where T : class
        {
            try
            {
                string encryptedContent = path;
            
                string jsonContent = Decrypt(encryptedContent, ENCRYPTION_KEY);

                if (string.IsNullOrEmpty(jsonContent))
                {
                    jsonContent = encryptedContent;
                }
                T config = JsonConvert.DeserializeObject<T>(jsonContent); 
                return config;
            }
            catch (System.Exception e)
            {
                return null;
            }
        }
        
        private static string Decrypt(string encryptedText, string key)
        {
            if (string.IsNullOrEmpty(encryptedText) || string.IsNullOrEmpty(key))
                return encryptedText;
            
            try
            {
                byte[] encryptedBytes = System.Convert.FromBase64String(encryptedText);
                byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
                byte[] decryptedBytes = new byte[encryptedBytes.Length];
            
                for (int i = 0; i < encryptedBytes.Length; i++)
                {
                    decryptedBytes[i] = (byte)(encryptedBytes[i] ^ keyBytes[i % keyBytes.Length]);
                }
            
                return System.Text.Encoding.UTF8.GetString(decryptedBytes);
            }
            catch
            {
                return null;
            }
        } 
    }
    
}