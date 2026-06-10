using System;

using System.Collections.Generic;
using DG.Tweening;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject GameOverUI;
    public GameObject GameStart;
    public GameObject IconGame;
    public CanvasGroup Fade;
    public Image XPbar;
    public TextMeshProUGUI LVNow;
    public TextMeshProUGUI LVNext;
    public TextMeshProUGUI Coin;
    public CanvasGroup ThanhTuu;
    public CanvasGroup ThanhTuuCha;
    public CanvasGroup Dayreward;
    public CanvasGroup DayrewardCha;

    private void Start()
    {
        PlayerProgressManager.instance.OnProgressChanged += UI_Change;
        PlayerProgressManager.instance.OnLVChange += LV_Change_Text;
        UI_Change();
        LV_Change_Text();
    }
    public void OnThanhTuu()
    {
        IconGame.SetActive(false);
        foreach(Transform e1 in ThanhTuu.gameObject.transform)
        {
            Destroy(e1.gameObject);
        }    
        ThanhTuu.gameObject.SetActive(true);
        RandomInon.FadeOut(ThanhTuu);
        DayrewardCha.gameObject.SetActive(false);
        var e = Instantiate(GameManager.instance.Thanhtuu, ThanhTuu.gameObject.transform);
    }    
    public void GetRewardDay()
    {
        
        const string key = "LastDailyRewardTicks";
        long lastTicks = 0;
        if (PlayerPrefs.HasKey(key))
            long.TryParse(PlayerPrefs.GetString(key), out lastTicks);

        DateTime lastClaim = new DateTime(lastTicks);
        bool canClaim = lastTicks == 0 || (DateTime.UtcNow - lastClaim).TotalHours >= 24;

        if (!canClaim)
        {
            Debug.Log("[UIManager] Daily reward already claimed. Try again later.");
            return;
        }

        // Grant reward – sum of all day‑reward amounts
        int totalReward = 0;
        foreach (var reward in AchievementManager.instance.thanhtuu.DayRewardList)
        {
            totalReward += reward.SoLuong;
        }

        PlayerProgressManager.instance.AddGold(totalReward);
        UI_Change(); // update UI displays

        // Record claim time
        PlayerPrefs.SetString(key, DateTime.UtcNow.Ticks.ToString());
        PlayerPrefs.Save();

        // Close reward UI and show locked version
        // Animate the day reward panel shrinking before hiding
        if (Dayreward != null)
        {
            var rt = Dayreward.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.DOScale(Vector3.one * 0.5f, 0.3f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => {
                        Dayreward.gameObject.SetActive(false);
                        DayrewardCha.gameObject.SetActive(true);
                        // Reset scale for next open
                        rt.localScale = Vector3.one;
                    });
                return; // exit early; the rest will run in OnComplete
            }
        }
        // Fallback if animation not possible
        Dayreward.gameObject.SetActive(false);
        DayrewardCha.gameObject.SetActive(true);
    }    
    public void OnDayReward()
    {
        var Cha = Dayreward.gameObject.transform.GetChild(0);
        IconGame.SetActive(false);
        foreach(Transform e1 in Cha.gameObject.transform)
        {
            Destroy(e1.gameObject);
        }    
        Dayreward.gameObject.SetActive(true);

        RandomInon.FadeOut(Dayreward);
        
        ThanhTuuCha.gameObject.SetActive(false);
        int so = 0;
        foreach (var f in AchievementManager.instance.thanhtuu.DayRewardList)
        {
            so += 1;
            
            var e = Instantiate(GameManager.instance.DayReward, Cha.gameObject.transform).GetComponent<ShowUiFor>();
            e.Text.SetText("DAY "+so.ToString());
            e.Img.sprite = f.Icon;

        }
    }
    public void ExitAll()
    {
        IconGame.SetActive(true);
        ThanhTuu.gameObject.SetActive(false);
        Dayreward.gameObject.SetActive(false);
        ThanhTuuCha.gameObject.SetActive(true);
        DayrewardCha.gameObject.SetActive(true);
    }    

    private void UI_Change()
    {
       XPbar.fillAmount =(float) PlayerProgressManager.instance.CurrentXP/PlayerProgressManager.instance.XPToNextLevel;
        Coin.SetText(PlayerProgressManager.instance.Gold.ToString());
    }
    private void LV_Change_Text()
    {
        LVNow.SetText(PlayerProgressManager.instance.Level.ToString());
        LVNext.SetText((PlayerProgressManager.instance.Level+1).ToString());
    }    

    private void OnDestroy()
    {
        if (PlayerProgressManager.instance != null)
        {
            PlayerProgressManager.instance.OnProgressChanged -= UI_Change;
            PlayerProgressManager.instance.OnLVChange -= LV_Change_Text;
        }
    }
    
   
}
[System.Serializable]
public class SaveProgress
{
    public int LVnow;
    public int CurrentXp;
    public int GoldNow;
    public int TotalPizzasEaten;
    public int TotalItemsPlaced;
    public List<string> UnlockedAchievementIDs;

    public SaveProgress (int LVNow, int CurrentXp, int GoldNow, int TotalPizzasEaten, int TotalItemsPlaced, List<string> UnlockedAchievementIDs)
    {
        this.LVnow = LVNow;
        this.CurrentXp = CurrentXp;
        this.GoldNow = GoldNow;
        this.TotalPizzasEaten = TotalPizzasEaten;
        this.TotalItemsPlaced = TotalItemsPlaced;
        this.UnlockedAchievementIDs = UnlockedAchievementIDs != null ? UnlockedAchievementIDs : new List<string>();
    }
}

public class SaveAndLoad
{
    private string saveKey = "keySave";
    public void Save(SaveProgress save)
    {
        string json = JsonUtility.ToJson(save);
        PlayerPrefs.SetString(saveKey, json);
        PlayerPrefs.Save();
    }
    public SaveProgress Load()
    {
        if (PlayerPrefs.HasKey(saveKey))
        {
            string json = PlayerPrefs.GetString(saveKey);
            SaveProgress save = JsonUtility.FromJson<SaveProgress>(json);
            if (save.UnlockedAchievementIDs == null)
            {
                save.UnlockedAchievementIDs = new List<string>();
            }
            return save;
        }
        return new SaveProgress(1, 0, 0, 0, 0, new List<string>());
    }
}

