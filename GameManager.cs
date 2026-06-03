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

    private Draggable activeDraggableCentralized;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Avoid clicking through UI
            if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;

            Camera cam = Camera.main;
            if (cam != null)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                Draggable target = null;

                // 3D Raycast
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    target = hit.collider.GetComponentInParent<Draggable>();
                    if (target == null)
                    {
                        DragProxy proxy = hit.collider.GetComponent<DragProxy>();
                        if (proxy != null) target = proxy.targetDraggable;
                    }
                }

                // 2D Raycast if 3D didn't hit
                if (target == null)
                {
                    RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray);
                    if (hit2D.collider != null)
                    {
                        target = hit2D.collider.GetComponentInParent<Draggable>();
                        if (target == null)
                        {
                            DragProxy proxy = hit2D.collider.GetComponent<DragProxy>();
                            if (proxy != null) target = proxy.targetDraggable;
                        }
                    }
                }

                if (target != null)
                {
                    Vector3 mouseWorldPos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cam.WorldToScreenPoint(target.transform.position).z));
                    activeDraggableCentralized = target;
                    activeDraggableCentralized.StartCentralizedDrag(mouseWorldPos);
                }
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (activeDraggableCentralized != null)
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    Vector3 mouseWorldPos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, activeDraggableCentralized.GetInitialZDistance()));
                    activeDraggableCentralized.UpdateCentralizedDrag(mouseWorldPos);
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (activeDraggableCentralized != null)
            {
                activeDraggableCentralized.EndCentralizedDrag();
                activeDraggableCentralized = null;
            }
        }
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
