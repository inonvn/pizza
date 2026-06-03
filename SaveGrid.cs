using UnityEngine;

public class SaveGrid : MonoBehaviour
{
    void Start()
    {

    }
    public void SpawnGrid(GameObject Grid)
    {
        print("e");
        if (GameManager.instance != null)
        {
            GameManager.instance.gridCoordinates.Clear();
            GameManager.instance.gridObjects.Clear();
            GameManager.instance.spareCoordinates.Clear();
            GameManager.instance.spareObjects.Clear();
            GameManager.instance.placedItems.Clear();
        }

        
        float spacing = GameManager.instance.gameSettings.GridSpacing;
        for (int i = 0; i < GameManager.instance.gameSettings.BoardSizeX; i++)
        {
            for (int j = 0; j < GameManager.instance.gameSettings.BoardSizeY; j++)
            {
                var vec = new Vector3Int(i, j, 0);
                var spawnPos = new Vector3(i * spacing, j * spacing, 0);
                var e = GameObject.Instantiate(Grid, spawnPos, Quaternion.identity, GameManager.instance.spawnHere.transform);
                
                if (GameManager.instance != null)
                {
                    GameManager.instance.gridCoordinates.Add(vec);
                    GameManager.instance.gridObjects.Add(e);
                }
            }
        }

        // Spawn spare slots in front of the board (y = -1)
        for (int i = 0; i < GameManager.instance.gameSettings.BoardSizeX; i++)
        {
            var vec = new Vector3Int(i, -1, 0);
            var spawnPos = new Vector3(i * spacing, -1 * spacing, 0);
            var e = GameObject.Instantiate(Grid, spawnPos, Quaternion.identity);

            if (GameManager.instance != null)
            {
                GameManager.instance.spareCoordinates.Add(vec);
                GameManager.instance.spareObjects.Add(e);
            }
        }

      
        SpawnSpareItems();
    }

    public void SpawnSpareItems()
    {
        if (GameManager.instance == null || GameManager.instance.gameSettings == null) return;

        // Clear existing spawned items
        foreach (var item in GameManager.instance.spawnedSpareItems)
        {
            if (item != null) Destroy(item);
        }
        GameManager.instance.spawnedSpareItems.Clear();

        var settings = GameManager.instance.gameSettings;
        if (settings.PizzaType == null || settings.PizzaType.Count == 0) return;

        float spacing = settings.GridSpacing;
        // Spawn a random item from PizzaType on each spare slot
        foreach (var pos in GameManager.instance.spareCoordinates)
        {
            GameObject randomPrefab = settings.PizzaType[Random.Range(0, settings.PizzaType.Count)];
            if (randomPrefab != null)
            {
                Vector3 spawnPos = new Vector3(pos.x * spacing, pos.y * spacing, pos.z * spacing);
                GameObject spawned = GameObject.Instantiate(randomPrefab, spawnPos, Quaternion.identity);
                
                // Add Draggable component to the spawned item
                if (spawned.GetComponent<Draggable>() == null)
                {
                    spawned.AddComponent<Draggable>();
                }
                
                // Ensure there is a collider so dragging works
                if (spawned.GetComponent<Collider2D>() == null && spawned.GetComponent<Collider>() == null)
                {
                    spawned.AddComponent<BoxCollider2D>();
                }

                GameManager.instance.spawnedSpareItems.Add(spawned);
            }
        }
    }

}
