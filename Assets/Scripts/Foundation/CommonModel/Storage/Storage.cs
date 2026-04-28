using System.IO;
using UnityEngine;

namespace Foundation.Storage
{
    public class Storage : SingletonComponent<Storage>
    {
        private static string FilePath => 
            Path.Combine(Application.persistentDataPath, "user.json");
        private bool needSave = false;
        private string json = "";
        private string oldJson = "";

        public override void OnInit()
        {
            base.OnInit();
            ScheduleModule.Instance.RegisterUpdate(UpdateSave);
        }
 
        public string GetOldData()
        { 
            if (!File.Exists(FilePath))
            {
                return "";
            }
            json = File.ReadAllText(FilePath);
            oldJson = json;
            return json;
        }
        private void UpdateSave()
        {
            if (needSave)
            {
                UpdateSaveImmediately(); 
            }
        }
        public void UpdateSaveImmediately()
        {
            if (json == oldJson)
            { 
                return;
            } 
            File.WriteAllText(FilePath, json);
            oldJson = json;
            needSave = false;
        }
        
        public void Save(string json, bool forceSave = false)
        {
            if (forceSave)
            {
                needSave = true;
            }
            this.json = json;
        }
    }
}