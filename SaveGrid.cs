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
                e.transform.rotation = Quaternion.identity;
                e.transform.localRotation = Quaternion.identity;
                
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
            var spawnPos = new Vector3(i * spacing, -1f * spacing, 0);
            var e = GameObject.Instantiate(Grid, spawnPos, Quaternion.identity, GameManager.instance.spawnHere.transform);
            e.transform.rotation = Quaternion.identity;
            e.transform.localRotation = Quaternion.identity;

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
      
        foreach (var pos in GameManager.instance.spareCoordinates)
        {
            GameObject randomPrefab = settings.PizzaType[Random.Range(0, settings.PizzaType.Count)];
            if (randomPrefab != null)
            {
                Vector3 spawnPos = new Vector3(pos.x * spacing, pos.y * spacing, pos.z * spacing);
                GameObject spawned = GameObject.Instantiate(randomPrefab, spawnPos, Quaternion.identity, GameManager.instance.spawnHere.transform);
                spawned.transform.rotation = Quaternion.identity;
                spawned.transform.localRotation = Quaternion.identity;
                
                Draggable dragComp = spawned.GetComponent<Draggable>();
                if (dragComp == null)
                {
                    dragComp = spawned.AddComponent<Draggable>();
                }

                PizzaCheck checkComp = spawned.GetComponent<PizzaCheck>();
                if (checkComp == null)
                {
                    checkComp = spawned.AddComponent<PizzaCheck>();
                }
                
                // Ensure there is a collider so dragging works
                if (spawned.GetComponent<Collider2D>() == null && spawned.GetComponent<Collider>() == null)
                {
                    spawned.AddComponent<BoxCollider2D>();
                }

                // Add DragProxy to all children with colliders (2D & 3D) so mouse events bubble up to Draggable
                foreach (var col in spawned.GetComponentsInChildren<Collider>(true))
                {
                    if (col.gameObject != spawned)
                    {
                        var proxy = col.gameObject.GetComponent<DragProxy>();
                        if (proxy == null) proxy = col.gameObject.AddComponent<DragProxy>();
                        proxy.targetDraggable = dragComp;
                    }
                }
                foreach (var col2D in spawned.GetComponentsInChildren<Collider2D>(true))
                {
                    if (col2D.gameObject != spawned)
                    {
                        var proxy = col2D.gameObject.GetComponent<DragProxy>();
                        if (proxy == null) proxy = col2D.gameObject.AddComponent<DragProxy>();
                        proxy.targetDraggable = dragComp;
                    }
                }

                // Freeze rotation if rigidbodies exist
                Rigidbody2D rb2d = spawned.GetComponent<Rigidbody2D>();
                if (rb2d != null)
                {
                    rb2d.constraints = rb2d.constraints | RigidbodyConstraints2D.FreezeRotation;
                }
                Rigidbody rb = spawned.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.constraints = rb.constraints | RigidbodyConstraints.FreezeRotation;
                }

                GameManager.instance.spawnedSpareItems.Add(spawned);
            }
        }
    }

}
