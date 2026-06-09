using System.Collections.Generic;
using UnityEngine;

public class PizzaCheck : MonoBehaviour
{
    
    public List<GameObject> slices = new List<GameObject>();
    void Awake()
    {
        
      
    }

    void Start()
    { 
       
        int activeCount = 0;
        foreach (var s in slices)
        {
            if (s.activeSelf) activeCount++;
        }

        if (activeCount == slices.Count && slices.Count > 0)
        {
            int newActiveCount = Random.Range(1, 6);
            for (int i = 0; i < slices.Count; i++)
            {
                slices[i].SetActive(i < newActiveCount);
            }
        }
    }

    void Update()
    {
        int count = GetActiveSliceCount();
       

        if (count >= 6)
        {
            // Award gold for eating a complete pizza
            if (PlayerProgressManager.instance != null && GameManager.instance != null && GameManager.instance.gameSettings != null)
            {
                PlayerProgressManager.instance.AddGold(GameManager.instance.gameSettings.GoldPerEat);
                PlayerProgressManager.instance.IncrementPizzasEaten();
            }

            if (GameManager.instance != null)
            {
                Vector3Int foundKey = new Vector3Int(-999, -999, -999);
                foreach (var kvp in GameManager.instance.placedItems)
                {
                    if (kvp.Value == gameObject)
                    {
                        foundKey = kvp.Key;
                        break;
                    }
                }
                if (foundKey.x != -999)
                {
                    GameManager.instance.placedItems.Remove(foundKey);
                }
            }
            Destroy(gameObject);
        }
    }

   
    int GetActiveSliceCount()
    {
        int count = 0;
        foreach (Transform child in transform)
        {
            if (child.name.Contains("Pizza") && child.gameObject.activeSelf)
            {
                count++;
            }
        }
        return count;
    }

    public int GetActiveCount()
    {
        int count = 0;
        foreach (var s in slices)
        {
            if (s != null && s.activeSelf) count++;
        }
        return count;
    }

    public void SetActiveCount(int count)
    {
        for (int i = 0; i < slices.Count; i++)
        {
            if (slices[i] != null)
            {
                slices[i].SetActive(i < count);
            }
        }
    }
}
