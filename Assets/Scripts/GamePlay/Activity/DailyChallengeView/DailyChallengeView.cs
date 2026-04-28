using System;
using System.Collections.Generic;
using Foundation;
using Foundation.Storage;
using GameConfig;
using GamePlay.Component;
using GamePlay.Storage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Event = Foundation.Event;

namespace GamePlay.UIMain
{
    [Window("DailyChallenge",WindowLayer.Popup)]
    public class DailyChallengeView : UIWindow
    {
        [UIBinder("Content")] private RectTransform  content;
        [UIBinder("dcell")] private Transform dcell;
        [UIBinder("Daily")] private RectTransform dailyContain;
        [UIBinder("DayTitle")] private Transform title;
        [UIBinder("CloseBtn")] private Button closeBtn;

        [UIBinder("Play")] private Button playBtn;
        [UIBinder("Photo")] private RectTransform photo;
        
        [UIBinder("Reward")] private GameObject reward;
        [UIBinder("ShowTips")] private Button showTip;
        private float heightcell;
        
        private int currentSelectDay;
        private int currentSelectMonth;
        private int currentSelectYear;
        
        private string currentData;
        private bool finishedAllPlayable = true;
        List<DailyCell> cells = new List<DailyCell>();
        private UguiMediaSource photoImg;

        private DailyInfo storage => StorageManager.Instance.GetStorage<DailyInfo>();
        
        public override void OnCreate()
        {
            base.OnCreate();
            content.offsetMax -= new Vector2(0, ViewUtility.AdjustTopHeight);
            DayOfWeek first;
            int weekcount;
            int daycount;
            
            closeBtn.onClick.AddListener(CloseFun);
            GetCurrentMonthCalendarInfo(out first, out weekcount, out daycount);

            Debug.Log(" dailychallenge "+ (int)first + " " +  weekcount + "  " + daycount );
            heightcell = dailyContain.sizeDelta.y / weekcount;
            CheckDefaultDay(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            CreateDaily(first, weekcount, daycount);
            SubScribeEvent<SelectDailyChallenge>(OnSelectFun);
            playBtn.gameObject.SetActive(!finishedAllPlayable);
            playBtn.onClick.AddListener(PlayGame);

            photo.sizeDelta = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
            photoImg = photo.TryGetOrAddComponent<UguiMediaSource>(); 
            UpdatePhoto();
            CreateReward();
            
            showTip.onClick.AddListener(ShowTip);
        }

        private void ShowTip()
        {

            UIModule.Instance.ShowAsync<DailyTip>();
        }

        private void CreateReward()
        {
            var info =GameConfigSys.daily;
            var model = reward.transform.Find("item");
            model.gameObject.SetActive(false);
            for (int i = 0; i < info.Count; i++)
            {
                int item = info[i].itemid;
                int amount = info[i].itemAmount;

                if (item == (int)ItemType.Photo)
                {
                    var a = UserUtility.UserType.ToLower();
                    var icon = GUtility.GetItemIcon((ItemType)item, a+"_more");
                    reward.transform.Find("photo").gameObject.SetActive(true);
                    reward.transform.Find("photo").GetComponent<Image>().sprite = icon;
                }
                else
                {
                    var obj = GameObject.Instantiate(model, model.parent);
                    obj.Find("amount").GetComponent<TextMeshProUGUI>().text =  amount.ToString();
                    var icon = GUtility.GetItemIcon((ItemType)item, "more");
                    obj.GetComponent<Image>().sprite = icon;
                    obj.gameObject.SetActive(true);
                }
            }
        }

        private bool UpdatePhoto()
        {
            if (!storage.DailyImg.ContainsKey(currentData))
            { 
                var name = GameConfigSys.GetOnePhoto(PhotoType.DailyChallenge);
                if (string.IsNullOrEmpty(name))
                {
                    return false;
                }
                storage.DailyImg.Add(currentData, name);
            }
            var check = storage.DailyImg[currentData];
            bool isvideo = check.ToLower().Contains(".mp4");
            string pname = GUtility.GetPhotoName(check);
            // photo.sizeDelta = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
            photoImg.SetSource(pname, isvideo, forceShowSmallImg: false);
            return true;
        }

        private void CloseFun()
        {
            Close();
            LobbySequence.Instance.FinishTask("DailyChanllengeGuide");
        }

        private void CheckDefaultDay(int year, int month, int day)
        {
            // var day = DateTime.Now.Day;
            // var month =  DateTime.Now.Month;
            var dai=  StorageManager.Instance.GetStorage<DailyInfo>().Daily;
            for (int i = day; i > 0; i--)
            {
                if (!dai.ContainsKey(month+"_"+i) || !dai[month +"_"+i])
                {
                    currentSelectDay = i;
                    currentSelectYear = year;
                    currentSelectMonth = month;
                    currentData = $"{year}_{month}_{day}";
                    finishedAllPlayable = false;
                    return;
                }
            }

            finishedAllPlayable = true;
        }

        
        private void PlayGame()
        {
            var name = StorageManager.Instance.GetStorage<DailyInfo>().DailyImg[currentData];
            // var name = GameConfigSys.GetOnePhoto(PhotoType.DailyChallenge);
            if (string.IsNullOrEmpty(name))
            {
                Event.Instance.SendEvent(new ShowTips("Not Enough Photo."));
                return;
            }

            var info = StorageManager.Instance.GetStorage<BaseInfo>();
            info.CurrentLevel.PhotoName = name;
            info.CurrentLevel.Level = info.Level;
            info.CurrentLevel.OtherInfo ="Daily"+ currentData; 

            GUtility.CheckPhotoState(name, (int)PhotoType.DailyChallenge, false);
            GameFsm.Instance.ToState<GameStatePlay>();
            Close();
        }

        private void OnSelectFun(SelectDailyChallenge obj)
        {
            if (obj.index == currentSelectDay)
            {
                return;
            }

            if (obj.index > DateTime.Now.Day)
            {
                Event.Instance.SendEvent(new ShowTips(" It's not time yet!"));
                return;
            }

            cells[currentSelectDay-1].UpdateSelect(false);
            cells[obj.index - 1].UpdateSelect(true);
            currentSelectDay = obj.index;
            currentData = $"{currentSelectYear}_{currentSelectMonth}_{currentSelectDay}";
            UpdatePhoto();
        }

        private void CreateDaily(DayOfWeek first, int weekcount, int daycount)
        {
            int dayofw = (int)first;
            int currentweek = 0;
            for (int i = 0; i < daycount; i++)
            {
                int index = i + 1;
                var ncell=  GameObject.Instantiate(dcell, dcell.parent);
                float x = title.Find(dayofw.ToString()).localPosition.x;
                float y = currentweek * -heightcell - 30;
                ncell.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y, 0);
                ncell.name = index.ToString();
                
                currentweek = dayofw == 6 ? currentweek + 1 : currentweek;
                dayofw = (dayofw + 1) % 7;
                var a = AddWidget<DailyCell>(ncell.gameObject, true, index);
                cells.Add(a);
                a.DefaultToday(currentSelectDay);
            }
            dcell.gameObject.SetActive(false);
        }
        public  void GetCurrentMonthCalendarInfo(
            out DayOfWeek firstDayOfWeek,
            out int weekCount,
            out int dayCount)
        {
            DateTime now = DateTime.Now;
            DateTime firstDay = new DateTime(now.Year, now.Month, 1);

            firstDayOfWeek = firstDay.DayOfWeek;
            dayCount = DateTime.DaysInMonth(now.Year, now.Month);

            int firstDayIndex = (int)firstDayOfWeek; // Sunday=0 ... Saturday=6
            int totalCells = firstDayIndex + dayCount;
            weekCount = (totalCells + 6) / 7;
        }
    }


    public class DailyCell : UIWidget
    {
        [UIBinder("day")] private TextMeshProUGUI day;
        [UIBinder("day2")] private TextMeshProUGUI day2;
        [UIBinder("finish")] private GameObject finish;
        [UIBinder("select")] private Button select;

        private int index;
        public override void OnCreate()
        {
            base.OnCreate();
            index = (int)userDatas[0];
            day.SetText(index.ToString());
            day2.SetText(index.ToString());
            // finish.SetActive(false);
            day2.gameObject.SetActive(false); 
            select.onClick.AddListener(SelectFun);
            var str = DateTime.Now.Year +"_" + DateTime.Now.Month + "_" + index;
            bool finished = StorageManager.Instance.GetStorage<DailyInfo>().Daily.ContainsKey(str);
            finish.SetActive(finished);
            // SubScribeEvent<SelectDailyChallenge>(OnSelectFun);
        }

        // private void OnSelectFun(SelectDailyChallenge obj)
        // {
        //     day2.gameObject.SetActive(obj.index == index);
        // }

        private void SelectFun()
        {
            Event.Instance.SendEvent(new SelectDailyChallenge(index));
        }

        public void DefaultToday(int day)
        { 
            if (day == index)
            {
                day2.gameObject.SetActive(true);
            }
        }

        public void UpdateSelect(bool isshow)
        {
            day2.gameObject.SetActive(isshow);
        }
    }
}