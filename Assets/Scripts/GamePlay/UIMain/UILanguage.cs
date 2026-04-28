using System.Collections.Generic;
using Foundation;
using Foundation.AudioModule;
using Foundation.Storage;
using GamePlay.Storage;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Event = Foundation.Event;

namespace GamePlay.UIMain
{
    [Window("Language", WindowLayer.Popup)]
    public class UILanguage : UIWindow
    {
        
        [UIBinder("Toggle")] private GameObject toggle;
        [UIBinder("CloseBtn")] private Button closeBtn;

        private Locale current;
        private string curLanguage;
        private List<TextMeshProUGUI> listStr  = new();
        
        public override void OnCreate()
        {
            base.OnCreate();
            closeBtn.onClick.AddListener(Close);
            var hasLocalization = LocalizationSettings.AvailableLocales.Locales;
            curLanguage = StorageManager.Instance.GetStorage<BaseInfo>().LocaleCode;
            for (int i = 0; i < hasLocalization.Count; i++)
            {
                var t = hasLocalization[i];
                var obj = GameObject.Instantiate(toggle, toggle.transform.parent);
                var str = GUtility.GetLocalizedString(t.LocaleName);
                
                obj.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = str; 
                listStr.Add(obj.transform.Find("Label").GetComponent<TextMeshProUGUI>());
                obj.GetComponent<Toggle>().onValueChanged.AddListener((select) =>
                {
                    if (select)
                    {
                        SelectLanguage(t);
                    }
                });
                if (curLanguage == t.Identifier.Code)
                {
                    obj.GetComponent<Toggle>().isOn = true;
                    obj.GetComponent<RectTransform>().SetAsFirstSibling();
                }
            }
            toggle.gameObject.SetActive(false);
        }

        private async void SelectLanguage(Locale locale)
        {
            if (locale.Identifier.Code == curLanguage)
            {
                return;
            }
            AudioModule.Instance.ClickAudio();
            current = locale;
            LocalizationSettings.SelectedLocale = locale;
            StorageManager.Instance.GetStorage<BaseInfo>().LocaleCode = locale.Identifier.Code;
            await UniTaskMgr.Instance.WaitForFrame(1);
            Event.Instance.SendEvent(new LanguageChange());
            curLanguage = locale.Identifier.Code;
            
            
            
            var hasLocalization = LocalizationSettings.AvailableLocales.Locales;
            for (int i = 0; i < hasLocalization.Count; i++)
            {
                var t = hasLocalization[i];
                var str = GUtility.GetLocalizedString(t.LocaleName);
                listStr[i].text = str;  
            }
        }
    }
    
}