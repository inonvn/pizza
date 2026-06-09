using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Achievement
{
    public string id;
    public string title;
    public int Soluong;
    public TypeThanhtuu thanhtuuType;
    public bool isUnlocked;
    public Sprite Icon;

    public Achievement(string id, string title, TypeThanhtuu type,int soluong, Sprite icon)
    {
        this.id = id;
        this.title = title;
        this.thanhtuuType = type;
        Soluong = soluong;
        this.isUnlocked = false;
        Icon = icon;

    }
}

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager instance;

    public List<Achievement> achievements = new List<Achievement>();
    public event Action<Achievement> OnAchievementUnlocked;
    [SerializeField] public ThanhTuu thanhtuu; // Assign via inspector

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            if (thanhtuu == null)
            {
                Debug.LogError("[AchievementManager] ThanhTuu reference is missing. Please assign it in the inspector.");
                return;
            }
            DontDestroyOnLoad(gameObject);
            InitializeAchievements();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAchievements()
    {
        if (thanhtuu == null)
        {
            Debug.LogError("[AchievementManager] ThanhTuu reference is missing. Cannot initialize achievements.");
            return;
        }
        achievements.Clear();
       
        foreach(var f in thanhtuu.TuuList)
        {
            Achievement ach = new Achievement(f.ID, f.Text, f.type, f.soLuong, f.Icon);
            achievements.Add(ach);
        }
            

    }

    public void LoadUnlockedAchievements(List<string> unlockedIDs)
    {
        if (unlockedIDs == null) return;

        foreach (var achievement in achievements)
        {
            if (unlockedIDs.Contains(achievement.id))
            {
                achievement.isUnlocked = true;
            }
        }
    }

    public List<string> GetUnlockedAchievementIDs()
    {
        List<string> unlocked = new List<string>();
        foreach (var achievement in achievements)
        {
            if (achievement.isUnlocked)
            {
                unlocked.Add(achievement.id);
            }
        }
        return unlocked;
    }

    public void CheckAchievements(int currentLevel, int currentGold, int totalPizzasEaten, int totalItemsPlaced)
    {
        bool changed = false;

        foreach (var achievement in achievements)
        {
            if (achievement.isUnlocked) continue;

            bool shouldUnlock = false;
            switch (achievement.thanhtuuType)
            {
                case TypeThanhtuu.PlaceCount:
                    shouldUnlock = totalItemsPlaced >= achievement.Soluong ;
                    break;
                case TypeThanhtuu.PizzaEat:
                    shouldUnlock = totalPizzasEaten >= achievement.Soluong;
                    break;
                case TypeThanhtuu.CoinCount:
                  
                    shouldUnlock = currentGold >= achievement.Soluong;
                    break;
                case TypeThanhtuu.LVCount:
                    shouldUnlock = currentLevel >= achievement.Soluong;
                    break;
            }

            if (shouldUnlock)
            {
                achievement.isUnlocked = true;
                changed = true;
                Debug.Log($"[AchievementManager] UNLOCKED: {achievement.title} - {achievement.thanhtuuType}");
                OnAchievementUnlocked?.Invoke(achievement);
            }
        }

        if (changed && PlayerProgressManager.instance != null)
        {
            PlayerProgressManager.instance.SaveProgressExternal();
        }
    }
}
