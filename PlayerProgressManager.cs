using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerProgressManager : MonoBehaviour
{
    public static PlayerProgressManager instance;

    public int Level { get; private set; }
    public int CurrentXP { get; private set; }
    public int Gold { get; private set; }
    public int TotalPizzasEaten { get; private set; }
    public int TotalItemsPlaced { get; private set; }

    public int XPToNextLevel => 50 + (Level - 1) * 25;

    public event Action OnProgressChanged;
    public event Action OnLVChange;
    SaveAndLoad save = new SaveAndLoad();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Dynamically ensure AchievementManager is attached so it exists in runtime
            if (GetComponent<AchievementManager>() == null)
            {
                gameObject.AddComponent<AchievementManager>();
            }

            LoadProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        CheckAchievements();
        SaveProgress();
        OnProgressChanged?.Invoke();
        Debug.Log($"[PlayerProgress] +{amount} Gold → Total: {Gold}");
    }

    public void AddXP(int amount)
    {
        CurrentXP += amount;
        Debug.Log($"[PlayerProgress] +{amount} XP → {CurrentXP}/{XPToNextLevel}");

        while (CurrentXP >= XPToNextLevel)
        {
            CurrentXP -= XPToNextLevel;
            Level++;
            OnLVChange?.Invoke();
            Debug.Log($"[PlayerProgress] LEVEL UP! Now Level {Level}");
        }

        CheckAchievements();
        SaveProgress();
        OnProgressChanged?.Invoke();
    }

    public void IncrementPizzasEaten()
    {
        TotalPizzasEaten++;
        Debug.Log($"[PlayerProgress] Total Pizzas Eaten: {TotalPizzasEaten}");
        CheckAchievements();
        SaveProgress();
    }

    public void IncrementItemsPlaced()
    {
        TotalItemsPlaced++;
        Debug.Log($"[PlayerProgress] Total Items Placed: {TotalItemsPlaced}");
        CheckAchievements();
        SaveProgress();
    }

    public void CheckAchievements()
    {
        if (AchievementManager.instance != null)
        {
            AchievementManager.instance.CheckAchievements(Level, Gold, TotalPizzasEaten, TotalItemsPlaced);
        }
    }

    public void SaveProgressExternal()
    {
        SaveProgress();
    }

    private void SaveProgress()
    {
        List<string> unlockedIDs = AchievementManager.instance != null ? 
            AchievementManager.instance.GetUnlockedAchievementIDs() : new List<string>();

        SaveProgress savep = new SaveProgress(Level, CurrentXP, Gold, TotalPizzasEaten, TotalItemsPlaced, unlockedIDs);
        save.Save(savep);
    }

    private void LoadProgress()
    {
        var e = save.Load();
        Level = e.LVnow;
        CurrentXP = e.CurrentXp;
        Gold = e.GoldNow;
        TotalPizzasEaten = e.TotalPizzasEaten;
        TotalItemsPlaced = e.TotalItemsPlaced;

        if (AchievementManager.instance != null)
        {
            AchievementManager.instance.LoadUnlockedAchievements(e.UnlockedAchievementIDs);
        }

        Debug.Log($"[PlayerProgress] Loaded: Level {Level}, XP {CurrentXP}/{XPToNextLevel}, Gold {Gold}, Pizzas Eaten: {TotalPizzasEaten}, Items Placed: {TotalItemsPlaced}");
    }

    public void ResetProgress()
    {
        Level = 1;
        CurrentXP = 0;
        Gold = 0;
        TotalPizzasEaten = 0;
        TotalItemsPlaced = 0;
        if (AchievementManager.instance != null)
        {
            AchievementManager.instance.achievements.ForEach(a => a.isUnlocked = false);
        }
        SaveProgress();
        OnProgressChanged?.Invoke();
    }
}

