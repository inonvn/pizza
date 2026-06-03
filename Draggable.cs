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

    void Awake()
    {
        mainCamera = Camera.main;
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    #region World Space Drag (for Colliders)

    void OnMouseDown()
    {
        // Ignore world drag if clicking on UI elements
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (mainCamera == null) mainCamera = Camera.main;

        originalPosition = transform.position;
        dragOffset = transform.position - GetMouseWorldPosition();
        if (GameManager.instance != null)
        {
            GameManager.instance.DragItem(gameObject);
        }
    }

    void OnMouseDrag()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject() && GameManager.instance.draggedItem != gameObject)
            return;

        if (mainCamera == null) mainCamera = Camera.main;

        transform.position = GetMouseWorldPosition() + dragOffset;
    }

    void OnMouseUp()
    {
        if (GameManager.instance != null && GameManager.instance.draggedItem == gameObject)
        {
            TryPlaceObject();
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mainCamera.WorldToScreenPoint(transform.position).z;
        return mainCamera.ScreenToWorldPoint(mousePoint);
    }

    #endregion

    #region UI Space Drag (for Canvas Elements)

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform != null ? rectTransform.position : transform.position;
        if (GameManager.instance != null)
        {
            GameManager.instance.DragItem(gameObject);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
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
    }

    public void OnEndDrag(PointerEventData eventData)
    {
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

        // Round to nearest integer coordinates on the grid
        int targetX = Mathf.RoundToInt(transform.position.x / spacing);
        int targetY = Mathf.RoundToInt(transform.position.y / spacing);
        Vector3Int targetGridPos = new Vector3Int(targetX, targetY, 0);

        bool isValidGrid = GameManager.instance.gridCoordinates.Contains(targetGridPos);
        bool isOccupied = GameManager.instance.placedItems.ContainsKey(targetGridPos) && 
                          GameManager.instance.placedItems[targetGridPos] != null;

        if (isValidGrid && !isOccupied)
        {
            // Snap to target cell center
            transform.position = new Vector3(targetX * spacing, targetY * spacing, originalPosition.z);
            GameManager.instance.placedItems[targetGridPos] = gameObject;

            // Remove from spare items list
            if (GameManager.instance.spawnedSpareItems.Contains(gameObject))
            {
                GameManager.instance.spawnedSpareItems.Remove(gameObject);
            }

            GameManager.instance.DropItem();
        }
        else
        {
            // Return to original position with DOTween transition animation
            transform.DOMove(originalPosition, 0.25f).OnComplete(() =>
            {
                GameManager.instance.DropItem();
            });
        }
    }
}
