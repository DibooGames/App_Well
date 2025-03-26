using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveElements : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private bool isDragging = false;
    private Vector2 offset;

    [Header("Scaling Settings")]
    [SerializeField] private bool enableScaling = true;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 2.0f;
    [SerializeField] private float scaleSpeed = 0.01f;

    private Vector3 initialScale;
    private float initialDistance;
    private bool isScaling = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("This script must be attached to a UI element that is a child of a Canvas.");
        }
    }

    void Start()
    {
        initialScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        
        // Calculate the offset between the pointer position and the center of the UI element
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out offset);
        
        // Reset scaling state when starting a new interaction
        isScaling = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        // Handle multi-touch scaling
        if (enableScaling && Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);
            
            // If we just started scaling with two fingers
            if (!isScaling)
            {
                isScaling = true;
                initialDistance = Vector2.Distance(touch1.position, touch2.position);
                initialScale = transform.localScale;
            }
            
            // Calculate the current distance between touches
            float currentDistance = Vector2.Distance(touch1.position, touch2.position);
            
            // Calculate scaling factor based on the pinch gesture
            float scaleFactor = currentDistance / initialDistance;
            
            // Apply the scaling with limits
            Vector3 newScale = initialScale * scaleFactor;
            newScale.x = Mathf.Clamp(newScale.x, minScale, maxScale);
            newScale.y = Mathf.Clamp(newScale.y, minScale, maxScale);
            newScale.z = 1f; // Keep z scale at 1
            
            transform.localScale = newScale;
        }
        else
        {
            isScaling = false;
            
            // Convert screen position to local position within the canvas
            Vector2 localPointerPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out localPointerPosition))
            {
                // Adjust position by the initial offset to ensure the grab point remains under the finger
                rectTransform.localPosition = localPointerPosition - offset;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        
        // Reset scaling state when ending interaction
        if (Input.touchCount < 2)
        {
            isScaling = false;
        }
    }
}