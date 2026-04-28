using System.Collections.Generic;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Foundation.Storage
{
    public class StorageManager : SingletonComponent<StorageManager>
    {
        private const string storageKey = "running";
        public bool ForceSave = false;
        public int ChangeTimes = 0;

        public bool isInit = false;
        float localInterval = 1.0f;
        float TickTimer = 0f;
        
        
        Dictionary<string, StorageBase> smap = new Dictionary<string, StorageBase>();
        
        Dictionary<System.Type, string> sdata = new Dictionary<System.Type, string>();
        
        
        public override void OnInit()
        {
            base.OnInit();
            
            ScheduleModule.Instance.RegisterUpdate(UpdateSave); 
        }

        public void Init(List<StorageBase> storages)
        {
            if (isInit)
            {
                return;
            }

            foreach (var storage in storages)
            {
                var type = storage.GetType().Name;
                smap[type] = storage;
            }

            LoadLocalData();

            isInit = true;
        }

        private void LoadLocalData()
        {
            string jdata = "{}";
            if (PlayerPrefs.HasKey(storageKey))
            {
                byte[] encrypt = System.Convert.FromBase64String(PlayerPrefs.GetString(storageKey));
                jdata = AesEncryptManager.Instance.DecryptToString(encrypt);
                FromJson(jdata);
            }
        }
        public void FromJson(string jsonData)
        {
            JObject jObj = JObject.Parse(jsonData);
            foreach (var type in smap.Keys)
            {
                var token = jObj[type];
                if (token == null)
                {
                    continue;
                }

                var str = token.ToString();
                JsonSerializerSettings setting = new JsonSerializerSettings();

                setting.NullValueHandling = NullValueHandling.Ignore;
                JsonConvert.PopulateObject(str, smap[type], setting);
            }
        }

        private void UpdateSave()
        {
            if (!isInit)
            {
                return;
            }
            
            if (ForceSave || (TickTimer > localInterval && ChangeTimes > 0))
            {
                ToLocalSave();
                ForceSave = false;
                TickTimer = 0f;
                
            }
            else
            {
                TickTimer += Time.deltaTime;
            }
        }

        public void SaveDataAppExit()
        {
            ToLocalSave();
        }
        private void ToLocalSave()
        {
            if (!isInit)
            {
                return;
            }

            JsonSerializerSettings setting = new JsonSerializerSettings();
            setting.NullValueHandling = NullValueHandling.Ignore;
            string jdata = JsonConvert.SerializeObject(smap, setting);
            byte[] encrypt = AesEncryptManager.Instance.EncryptString(jdata);
            PlayerPrefs.SetString( storageKey, System.Convert.ToBase64String(encrypt));
            
            ChangeTimes = 0;
        }

        public T GetStorage<T>() where T : StorageBase
        {
            
            System.Type stype =  typeof(T);
            if (!sdata.TryGetValue(stype, out string name))
            {
                name = stype.Name;
                sdata[stype] = name;
            }

            return (T)smap[name];
        }

        public void RemoveAllStorage()
        {
            sdata.Clear();
        }
        
    }
}