using UnityEngine;
using UnityEngine.EventSystems;

public class DragUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 offsetPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        
        if (canvas == null)
        {
            Debug.LogError("DragUI script requires the UI element to be a child of a Canvas");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Store the offset between the touch position and the UI element position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out offsetPosition);
        offsetPosition = rectTransform.anchoredPosition - offsetPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null)
            return;

        // Convert screen position to canvas position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
        
        // Apply the new position plus the initial offset
        rectTransform.anchoredPosition = localPoint + offsetPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Optionally perform actions when drag ends
    }
}
