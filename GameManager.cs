using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using DG.Tweening;

public enum GameState
{
    AfterStart,
    InGame,
    Pause,
    EndGame
}

public class GameManager : MonoBehaviour
{
    public GameState currentState = GameState.AfterStart;
    public GameSettings gameSettings;
    public static GameManager instance;
    private GameObject Grid;
    private UIManager UImanager;
    
    [HideInInspector]
    public GameObject draggedItem;

    public List<Vector3Int> gridCoordinates = new List<Vector3Int>();
    public List<GameObject> gridObjects = new List<GameObject>();

    public List<Vector3Int> spareCoordinates = new List<Vector3Int>();
    public List<GameObject> spareObjects = new List<GameObject>();
    public List<GameObject> spawnedSpareItems = new List<GameObject>();
    
    public Dictionary<Vector3Int, GameObject> placedItems = new Dictionary<Vector3Int, GameObject>();
    public GameObject spawnHere;
    public GameObject Paratic;
    public GameObject Thanhtuu;
    public GameObject DayReward;
   
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
        UImanager = GetComponent<UIManager>();
        Grid = gameSettings.Grid;
    }

    private Draggable activeDraggableCentralized;

   
    void Update()
    {
        if (currentState != GameState.InGame) return;

        if (Input.GetMouseButtonDown(0))
        {
           
           

            Camera cam = Camera.main;
            if (cam != null)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                Draggable target = null;

              
                RaycastHit hit;
               
                if (Physics.Raycast(ray, out hit))
                {
                  
                    target = hit.collider.GetComponent<Draggable>();
                    if (target == null)
                    {
                        print("click1");
                        DragProxy proxy = hit.collider.GetComponent<DragProxy>();
                        if (proxy != null) target = proxy.targetDraggable;
                    }
                }

                
                if (target == null)
                {
                    RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray);
                    if (hit2D.collider != null)
                    {
                        target = hit2D.collider.GetComponent<Draggable>();
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
                    print("e");
                    if (target.StartCentralizedDrag(mouseWorldPos))
                    {
                        activeDraggableCentralized = target;
                    }
                    else
                    {
                        activeDraggableCentralized = null;
                    }
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
      UImanager.  GameStart.SetActive(false);
        var e = GetComponent<SaveGrid>();

        e.SpawnGrid(Grid);
        SetState(GameState.InGame);
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
        Debug.Log($"[GameManager] State changed to: {currentState}");
    }

    public void TogglePause()
    {
        if (currentState == GameState.InGame)
            SetState(GameState.Pause);
        else if (currentState == GameState.Pause)
            SetState(GameState.InGame);
    }

    public void CheckGridFull()
    {
        if (currentState != GameState.InGame) return;

        foreach (var coord in gridCoordinates)
        {
            if (!placedItems.ContainsKey(coord) || placedItems[coord] == null)
                return; 
        }

        
        SetState(GameState.EndGame);
        
    }   
    public void GameOver()
    {
       UImanager. GameOverUI.SetActive(true);
    }    
    public void DragItem(GameObject item)
    {
        draggedItem = item;
    }   
    public void DropItem()
    {
        draggedItem = null;
    }

    private string GetPizzaTypeName(GameObject pizza)
    {
        string name = pizza.name;
        int cloneIdx = name.IndexOf("(Clone)");
        if (cloneIdx >= 0)
            name = name.Substring(0, cloneIdx);
        return name.Trim();
    }
    public void CheckNeighborsAndMerge(Vector3Int pos)
    {
        StartCoroutine(CheckNeighborsAndMergeCoroutine(pos));
    }

    private System.Collections.IEnumerator CheckNeighborsAndMergeCoroutine(Vector3Int pos)
    {
        yield return new WaitForSeconds(1.0f);

        if (!placedItems.ContainsKey(pos))
        {
            CheckGridFull();
            yield break;
        }
        GameObject placed = placedItems[pos];
        if (placed == null)
        {
            CheckGridFull();
            yield break;
        }

        PizzaCheck placedCheck = placed.GetComponent<PizzaCheck>();
        if (placedCheck == null)
        {
            CheckGridFull();
            yield break;
        }

        string placedType = GetPizzaTypeName(placed);
        int maxSlices = placedCheck.slices.Count;
        int currentCount = placedCheck.GetActiveCount();

        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(1, 0, 0)
        };

        List<Vector3Int> toRemove = new List<Vector3Int>();

        foreach (var dir in directions)
        {
            if (currentCount >= maxSlices) break;

            Vector3Int neighborPos = pos + dir;
            if (!placedItems.ContainsKey(neighborPos)) continue;

            GameObject neighbor = placedItems[neighborPos];
            if (neighbor == null || neighbor == placed) continue;

            string neighborType = GetPizzaTypeName(neighbor);
            if (neighborType != placedType) continue;

            PizzaCheck neighborCheck = neighbor.GetComponent<PizzaCheck>();
            if (neighborCheck == null) continue;

            int neighborCount = neighborCheck.GetActiveCount();
            if (neighborCount <= 0) continue;

            int needed = maxSlices - currentCount;
            int transfer = Mathf.Min(needed, neighborCount);

            for (int t = 0; t < transfer; t++)
            {
                int neighborSliceIdx = neighborCount - 1;
                int placedSliceIdx = currentCount;

                if (neighborSliceIdx >= 0 && neighborSliceIdx < neighborCheck.slices.Count &&
                    placedSliceIdx >= 0 && placedSliceIdx < placedCheck.slices.Count)
                {
                    GameObject sliceToAnimate = neighborCheck.slices[neighborSliceIdx];
                    GameObject targetSliceSlot = placedCheck.slices[placedSliceIdx];

                    if (sliceToAnimate != null && targetSliceSlot != null)
                    {
                        Vector3 origLocalPos = sliceToAnimate.transform.localPosition;
                        Quaternion origLocalRot = sliceToAnimate.transform.localRotation;
                        Transform origParent = sliceToAnimate.transform.parent;

                        Vector3 targetWorldPos = targetSliceSlot.transform.position;
                        Quaternion targetWorldRot = targetSliceSlot.transform.rotation;

                        // Visually show slice moving
                        bool animationComplete = false;
                        sliceToAnimate.transform.DOMove(targetWorldPos, 0.4f);
                        sliceToAnimate.transform.DORotateQuaternion(targetWorldRot, 0.4f).OnComplete(() => {
                            animationComplete = true;
                        });

                        yield return new WaitUntil(() => animationComplete);

                        sliceToAnimate.SetActive(false);
                        sliceToAnimate.transform.parent = origParent;
                        sliceToAnimate.transform.localPosition = origLocalPos;
                        sliceToAnimate.transform.localRotation = origLocalRot;

                        targetSliceSlot.SetActive(true);
                    }
                }

                currentCount++;
                neighborCount--;
            }

            if (neighborCount <= 0)
            {
                toRemove.Add(neighborPos);
            }
        }

        foreach (var key in toRemove)
        {
            if (placedItems.ContainsKey(key))
            {
                GameObject emptyPizza = placedItems[key];
                placedItems.Remove(key);
                    if (emptyPizza != null)
                    {
                        Vector3 pos1 = emptyPizza.transform.position;
                        Destroy(emptyPizza);
                        if (Paratic != null)
                        {
                            GameObject fx = Instantiate(Paratic, pos1, Quaternion.identity);
                            Destroy(fx, 1f);
                        }
                    }
            }
        }

        CheckGridFull();
    }
    }

