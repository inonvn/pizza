using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 dragOffset;
    private Camera mainCamera;
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector3 originalPosition;
    private float initialZDistance;
    private bool wasKinematic3D;
    private bool wasKinematic2D;
    private Rigidbody rb3D;
    private Rigidbody2D rb2D;

    void Awake()
    {
        mainCamera = Camera.main;
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        rb3D = GetComponent<Rigidbody>();
        rb2D = GetComponent<Rigidbody2D>();
    }

    private GameObject GetRootSpawnedItem()
    {
        if (GameManager.instance == null) return null;
        Transform current = transform;
        while (current != null)
        {
            if (GameManager.instance.spawnedSpareItems.Contains(current.gameObject))
            {
                return current.gameObject;
            }
            current = current.parent;
        }
        return null;
    }

    private bool IsInSpareSlots()
    {
        return GetRootSpawnedItem() != null;
    }

    private bool CanDrag()
    {
        if (GameManager.instance == null) return false;
        if (IsInSpareSlots()) return true;

        return false;
    }

    #region World Space Drag (for Colliders) - DEPRECATED (Centralized in GameManager)
    // Legacy mouse handlers removed to prevent double-drag conflicts with GameManager centralized dragging
    #endregion

    #region UI Space Drag (for Canvas Elements)

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (GameManager.instance == null)
        {
            Debug.Log("OnBeginDrag: Ignored drag because GameManager.instance is null.");
            return;
        }
        if (!CanDrag())
        {
            Debug.Log($"OnBeginDrag: Ignored drag because {gameObject.name} is not draggable.");
            return;
        }
        if (rb3D != null)
        {
            wasKinematic3D = rb3D.isKinematic;
            rb3D.isKinematic = true;
        }
        if (rb2D != null)
        {
            wasKinematic2D = rb2D.isKinematic;
            rb2D.isKinematic = true;
        }

        originalPosition = rectTransform != null ? rectTransform.position : transform.position;
        GameManager.instance.DragItem(gameObject);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (GameManager.instance == null || GameManager.instance.draggedItem != gameObject)
            return;

        if (rectTransform != null)
        {
            if (canvas == null) canvas = GetComponentInParent<Canvas>();
            
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
            }
            else
            {
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector3 worldPos))
                {
                    rectTransform.position = worldPos;
                }
            }
        }
        transform.localRotation = Quaternion.identity;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (rb3D != null) rb3D.isKinematic = wasKinematic3D;
        if (rb2D != null) rb2D.isKinematic = wasKinematic2D;
        if (GameManager.instance != null && GameManager.instance.draggedItem == gameObject)
        {
            TryPlaceObject();
        }
    }

    #endregion

    private void TryPlaceObject()
    {
        if (GameManager.instance == null) return;

        float spacing = 1.0f;
        if (GameManager.instance.gameSettings != null)
        {
            spacing = GameManager.instance.gameSettings.GridSpacing;
        }

        int targetX = Mathf.RoundToInt(transform.position.x / spacing);
        int targetY = Mathf.RoundToInt(transform.position.y / spacing);
        Vector3Int targetGridPos = new Vector3Int(targetX, targetY, 0);

        bool isValidGrid = GameManager.instance.gridCoordinates.Contains(targetGridPos);
        bool isOccupied = GameManager.instance.placedItems.ContainsKey(targetGridPos) && 
                          GameManager.instance.placedItems[targetGridPos] != null &&
                          GameManager.instance.placedItems[targetGridPos] != gameObject;

        if (isValidGrid && !isOccupied)
        {
            // Remove from old slot if it was previously placed
            Vector3Int oldKey = new Vector3Int(-999, -999, -999);
            foreach (var kvp in GameManager.instance.placedItems)
            {
                if (kvp.Value == gameObject)
                {
                    oldKey = kvp.Key;
                    break;
                }
            }
            if (oldKey.x != -999)
            {
                GameManager.instance.placedItems.Remove(oldKey);
            }

            transform.position = new Vector3(targetX * spacing, targetY * spacing, originalPosition.z);
            transform.localRotation = Quaternion.identity;
            GameManager.instance.placedItems[targetGridPos] = gameObject;

            GameObject rootSpawned = GetRootSpawnedItem();
            if (rootSpawned != null)
            {
                GameManager.instance.spawnedSpareItems.Remove(rootSpawned);
            }

            GameManager.instance.DropItem();
            GameManager.instance.CheckNeighborsAndMerge(targetGridPos);
            GameManager.instance.CheckGridFull();

            // If all spare items have been placed, spawn a new batch
            if (GameManager.instance.spawnedSpareItems.Count == 0)
            {
                SaveGrid saveGrid = GameManager.instance.GetComponent<SaveGrid>();
                if (saveGrid != null)
                {
                    saveGrid.SpawnSpareItems();
                }
            }
        }
        else
        {
            transform.DOMove(originalPosition, 0.25f).OnComplete(() =>
            {
                GameManager.instance.DropItem();
            });
            transform.DOLocalRotate(Vector3.zero, 0.25f);
        }
    }

    public float GetInitialZDistance()
    {
        return initialZDistance;
    }

    public bool StartCentralizedDrag(Vector3 mouseWorldPos)
    {
        if (GameManager.instance == null || !CanDrag())
            return false;

        if (mainCamera == null) mainCamera = Camera.main;

        if (rb3D != null)
        {
            wasKinematic3D = rb3D.isKinematic;
            rb3D.isKinematic = true;
        }
        if (rb2D != null)
        {
            wasKinematic2D = rb2D.isKinematic;
            rb2D.isKinematic = true;
        }

        originalPosition = transform.position;
        initialZDistance = mainCamera.WorldToScreenPoint(transform.position).z;
        dragOffset = transform.position - mouseWorldPos;
        
        GameManager.instance.DragItem(gameObject);
        return true;
    }

    public void UpdateCentralizedDrag(Vector3 mouseWorldPos)
    {
        transform.position = mouseWorldPos + dragOffset;
        transform.localRotation = Quaternion.identity;
    }

    public void EndCentralizedDrag()
    {
        if (rb3D != null) rb3D.isKinematic = wasKinematic3D;
        if (rb2D != null) rb2D.isKinematic = wasKinematic2D;

        TryPlaceObject();
    }
}

public class DragProxy : MonoBehaviour
{
    public Draggable targetDraggable;
}
