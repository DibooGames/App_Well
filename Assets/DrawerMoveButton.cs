using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawerMoveButton : MonoBehaviour
{
    [SerializeField] private RectTransform panel; // Reference to the panel that will slide
    [SerializeField] private Vector2 inPosition; // Position when panel is visible
    [SerializeField] private Vector2 outPosition; // Position when panel is hidden
    [SerializeField] private float slideSpeed = 10f; // Speed of the sliding animation
    
    private bool isVisible = false; // Current state of the panel
    
    // Start is called before the first frame update
    void Start()
    {
        if (panel == null)
        {
            // Try to find the panel in the parent
            panel = transform.parent.GetComponent<RectTransform>();
        }
        
        // Set initial position
        panel.anchoredPosition = outPosition;
    }

    // Update is called once per frame
    void Update()
    {
        // Smoothly move the panel towards the target position
        Vector2 targetPosition = isVisible ? inPosition : outPosition;
        
        panel.anchoredPosition = Vector2.Lerp(
            panel.anchoredPosition, 
            targetPosition, 
            slideSpeed * Time.deltaTime
        );
    }
    
    // Called when button is clicked
    public void TogglePanel()
    {
        isVisible = !isVisible;
    }
}
