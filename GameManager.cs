using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;


public class GameManager : MonoBehaviour
{
    public GameSettings gameSettings;
    public static GameManager instance;
    private GameObject Grid;
    
    [HideInInspector]
    public GameObject draggedItem;

    public List<Vector3Int> gridCoordinates = new List<Vector3Int>();
    public List<GameObject> gridObjects = new List<GameObject>();

    public List<Vector3Int> spareCoordinates = new List<Vector3Int>();
    public List<GameObject> spareObjects = new List<GameObject>();
    public List<GameObject> spawnedSpareItems = new List<GameObject>();
    
    public Dictionary<Vector3Int, GameObject> placedItems = new Dictionary<Vector3Int, GameObject>();
    public GameObject spawnHere;
    private void Awake()
    {
        placedItems.Clear();
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Grid = gameSettings.Grid;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Onpress()
    {
        var e = GetComponent<SaveGrid>();
        e.SpawnGrid(Grid);
    }   
    public void DragItem(GameObject item)
    {
        draggedItem = item;
    }   
    public void DropItem()
    {
        draggedItem = null;
    }    
}
