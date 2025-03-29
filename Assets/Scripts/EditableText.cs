using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EditableText : MonoBehaviour, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    public TextMeshProUGUI text;
    public TextManager textManager; // Reference to the TextManager script - now HideInInspector since it will be auto-assigned
    [SerializeField]public TMP_InputField inputField;
    private string originalText;

    private bool isDragging = false; // Track dragging state
    private bool isSelected = false; // Track selection state

    // Change from property to static field so it can be shared between instances
    public static EditableText CurrentlySelectedText;

    public Color originalColor;

    // Add canvas reference
    private Canvas parentCanvas;
    
    // Delete button reference
    public Button deleteButton;

    void Awake()
    {
       

        // Find TextManager in the scene if not assigned
        if (textManager == null)
        {
            textManager = FindObjectOfType<TextManager>();
            if (textManager == null)
            {
                Debug.LogError("EditableText: No TextManager found in the scene!");
                return;
            }
        }
        
        // Fix the delete button null check
        if (deleteButton == null)
        {
            // Find gameobject named DeleteButton in the scene
            GameObject deleteButtonGO = GameObject.Find("DeleteButton");
            if (deleteButtonGO != null)
            {
                deleteButton = deleteButtonGO.GetComponent<Button>();
            }

            if (deleteButton == null)
            {
                Debug.LogError("EditableText: No DeleteButton found in the scene!");
                return;
            }
        }

        // Store original color
        if (text == null)
            text = GetComponent<TextMeshProUGUI>();

        originalColor = text.color;

        // Find parent canvas
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogError("EditableText: Cannot find parent canvas!");
            return;
        }

        // Create input field if it doesn't exist
        if (inputField == null)
        {
            // Store original text component reference
            if (text == null)
                text = GetComponent<TextMeshProUGUI>();

            // Get the RectTransform of the text
            RectTransform originalRect = text.GetComponent<RectTransform>();

            // Create game object with input field DIRECTLY UNDER CANVAS
            GameObject inputFieldGO = new GameObject("EditorInputField_" + gameObject.name);
            inputFieldGO.transform.SetParent(parentCanvas.transform);

            // Add RectTransform to input field
            RectTransform inputRect = inputFieldGO.AddComponent<RectTransform>();

            // Convert text's rect positions to canvas space
            RectTransform textRectTransform = text.GetComponent<RectTransform>();
            Vector3[] textCorners = new Vector3[4];
            textRectTransform.GetWorldCorners(textCorners);

            // Convert world corners to canvas space
            for (int i = 0; i < 4; i++)
            {
                textCorners[i] = parentCanvas.transform.InverseTransformPoint(textCorners[i]);
            }

            // Set rect transform position and size based on text's position in canvas space
            inputRect.anchorMin = new Vector2(0, 0);
            inputRect.anchorMax = new Vector2(0, 0);
            inputRect.pivot = textRectTransform.pivot;

            // Position at bottom-left corner
            Vector2 bottomLeft = textCorners[0];
            Vector2 topRight = textCorners[2];
            inputRect.anchoredPosition = bottomLeft;
            inputRect.sizeDelta = new Vector2(topRight.x - bottomLeft.x, topRight.y - bottomLeft.y);

            // Add input field component
            inputField = inputFieldGO.AddComponent<TMP_InputField>();

            // Create text area for input field
            GameObject textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputFieldGO.transform);

            // Add RectTransform to text area and set to fill parent
            RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = Vector2.zero;
            textAreaRect.offsetMax = Vector2.zero;

            // Create a new TextMeshPro text component instead of instantiating the original
            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(textArea.transform);

            // Add TextMeshProUGUI component
            TextMeshProUGUI inputText = textObject.AddComponent<TextMeshProUGUI>();

            // Setup input field rect transform to match original text
            RectTransform inputTextRect = inputText.GetComponent<RectTransform>();
            inputTextRect.anchorMin = Vector2.zero;
            inputTextRect.anchorMax = Vector2.one;
            inputTextRect.offsetMin = Vector2.zero;
            inputTextRect.offsetMax = Vector2.zero;

            // Setup input field
            inputField.textComponent = inputText;
            inputField.textViewport = textAreaRect;

            // Copy text properties
            inputText.fontSize = text.fontSize;
            inputText.color = text.color;
            inputText.font = text.font;
            inputText.alignment = text.alignment;

            // Copy text and other properties that might be important
            inputText.text = text.text;
            inputText.enableWordWrapping = text.enableWordWrapping;
            inputText.richText = text.richText;
            inputText.fontStyle = text.fontStyle;
            inputText.overflowMode = text.overflowMode;

            // Add content type and other settings
            inputField.contentType = TMP_InputField.ContentType.Standard;

            // Add listener for real-time text updating
            inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
            inputField.onEndEdit.AddListener(OnInputFieldEndEdit);

            // Hide input field initially
            inputFieldGO.SetActive(false);
        }

        // Ensure delete button is initially hidden
        if (deleteButton != null)
        {
            deleteButton.gameObject.SetActive(false);
            deleteButton.onClick.AddListener(DeleteSelectedText);
        }
    }

    void Update()
    {
        // Check for touch/click outside UI elements
        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began || 
             Input.GetMouseButtonDown(0)) && isSelected)
        {
            CheckInputOutsideUI();
        }

        // Hide delete button if no text is selected
        if (!isSelected && deleteButton != null && deleteButton.gameObject.activeSelf)
        {
            deleteButton.gameObject.SetActive(false);
        }
    }

    private void CheckInputOutsideUI()
    {
        // Check if we're touching/clicking on a UI element with EventSystem
        if (EventSystem.current.IsPointerOverGameObject() || 
            (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)))
        {
            // We hit some UI element, let's check if it has the UI tag
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.touchCount > 0 ? (Vector2)Input.GetTouch(0).position : Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            // Check if any of the UI elements under the pointer have the UI tag
            bool hitUITag = false;
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.CompareTag("UI"))
                {
                    hitUITag = true;
                    break;
                }
            }

            // If we didn't hit a UI tagged object, deselect
            if (!hitUITag)
            {
                Deselect();
            }
            // Otherwise, we hit a UI tagged object, so do nothing
        }
        else
        {
            // We didn't hit any UI at all, so deselect
            Deselect();
        }
    }

    private bool IsPointerOverUIElement()
    {
        // Check if pointer/touch is over any UI element
        bool isOverUI = EventSystem.current.IsPointerOverGameObject();
        
        // Also check for touch input
        if (Input.touchCount > 0)
            isOverUI = isOverUI || EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            
        if (!isOverUI) return false; // Quick exit if not over UI at all

        // Check if pointer is over any UI element with the UI tag
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.touchCount > 0 ? (Vector2)Input.GetTouch(0).position : Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag("UI"))
            {
                return true;
            }
        }

        return false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Ignore if we're dragging
        if (isDragging)
        {
            isDragging = false;
            return;
        }

        // If text is already selected, open the input field for editing
        if (isSelected)
        {
            if (inputField != null)
            {
                // Ensure input field is activated and aligned
                inputField.gameObject.SetActive(true); // Ensure input field is visible
                AlignInputFieldWithText();
                OpenKeyboard();
            }
        }
        // Otherwise, select it and then open the keyboard
        else
        {
            Select();
            OpenKeyboard(); // Start editing immediately after selection
        }

        // Reset drag state
        isDragging = false;
    }

    // Align the input field with the text position
    private void AlignInputFieldWithText()
    {
        if (inputField != null && text != null && parentCanvas != null)
        {
            RectTransform textRectTransform = text.GetComponent<RectTransform>();
            Vector3[] textCorners = new Vector3[4];
            textRectTransform.GetWorldCorners(textCorners);

            // Convert world corners to canvas space
            for (int i = 0; i < 4; i++)
            {
                textCorners[i] = parentCanvas.transform.InverseTransformPoint(textCorners[i]);
            }

            RectTransform inputRect = inputField.GetComponent<RectTransform>();
            Vector2 bottomLeft = textCorners[0];
            inputRect.anchoredPosition = bottomLeft;
            inputRect.sizeDelta = new Vector2(textCorners[2].x - bottomLeft.x, textCorners[2].y - bottomLeft.y);
        }
    }

    // Called by Unity's EventSystem when this object is selected
    public void OnSelect(BaseEventData eventData)
    {
        Debug.Log("OnSelect called");
        // Only trigger Select() if not already selected to avoid canceling 
        // double-tap behavior that would trigger OpenKeyboard()
        if (!isSelected)
        {
            Select();
        }
    }

    // Called by Unity's EventSystem when another object is selected
    public void OnDeselect(BaseEventData eventData)
    {
        // Skip deselection from the EventSystem - we'll handle this ourselves in CheckClickOutsideUI
        // to avoid recursive deselection errors
    }

    // Select this text object
    public void Select()
    {
        // Only set TextManager.Text if necessary
        if (textManager != null && textManager.Text != text)
        {
            textManager.Text = text; // Set the TextManager's text to this text
        }

        // Deselect previous text if there was one
        if (CurrentlySelectedText != null && CurrentlySelectedText != this)
        {
            CurrentlySelectedText.Deselect();
        }

        // Select this text
        isSelected = true;
        CurrentlySelectedText = this;

        // Visual indicator of selection (highlight text)
        if (text != null)
        {
            text.color = Color.yellow; // Or any other visual indicator
        }

        // Only set as selected if it's not already the selected game object
        if (EventSystem.current.currentSelectedGameObject != gameObject)
        {
            // Make sure the EventSystem knows this is selected
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
        
        // Show delete button if assigned
        if (deleteButton != null)
        {
            deleteButton.gameObject.SetActive(true);
        }
    }

    // Deselect this text object
    public void Deselect()
    {
        if (!isSelected)
            return;
        
        // Reset color to original
        if (text != null)
        {
            text.color = originalColor;
        }
      
        // Only clear TextManager.Text if it's still referencing this text
        if (textManager != null && textManager.Text == text)
        {
            textManager.Text = null; // Clear the TextManager's text reference
        }

        isSelected = false;

        // Ensure circular references are completely broken
        if (CurrentlySelectedText == this)
        {
            CurrentlySelectedText = null;
        }
        
        // Hide delete button
        if (deleteButton != null)
        {
            deleteButton.gameObject.SetActive(false);
        }
    }
    
    // Add method to set dragging state from MoveElements
    public void SetDragState(bool dragging)
    {
        isDragging = dragging;
    }

    // Debug the actual keyboard opening process
    public void OpenKeyboard()
    {
        Debug.Log("Opening keyboard for text: " + text.text);
        originalText = text.text;

        // Ensure input field is instantiated if not already
        if (inputField == null)
        {
            // Find parent canvas
            if (parentCanvas == null)
            {
                parentCanvas = GetComponentInParent<Canvas>();
                if (parentCanvas == null)
                {
                    Debug.LogError("EditableText: Cannot find parent canvas!");
                    return;
                }
            }

            // Create input field dynamically
            GameObject inputFieldGO = new GameObject("EditorInputField_" + gameObject.name);
            inputFieldGO.transform.SetParent(parentCanvas.transform);

            RectTransform inputRect = inputFieldGO.AddComponent<RectTransform>();
            inputRect.anchorMin = Vector2.zero;
            inputRect.anchorMax = Vector2.one;
            inputRect.offsetMin = Vector2.zero;
            inputRect.offsetMax = Vector2.zero;

            inputField = inputFieldGO.AddComponent<TMP_InputField>();

            GameObject textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputFieldGO.transform);

            RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = Vector2.zero;
            textAreaRect.offsetMax = Vector2.zero;

            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(textArea.transform);

            TextMeshProUGUI inputText = textObject.AddComponent<TextMeshProUGUI>();
            RectTransform inputTextRect = inputText.GetComponent<RectTransform>();
            inputTextRect.anchorMin = Vector2.zero;
            inputTextRect.anchorMax = Vector2.one;
            inputTextRect.offsetMin = Vector2.zero;
            inputTextRect.offsetMax = Vector2.zero;

            inputField.textComponent = inputText;
            inputField.textViewport = textAreaRect;

            inputText.fontSize = text.fontSize;
            inputText.color = text.color;
            inputText.font = text.font;
            inputText.alignment = text.alignment;

            inputField.contentType = TMP_InputField.ContentType.Standard;
            inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
            inputField.onEndEdit.AddListener(OnInputFieldEndEdit);

            inputFieldGO.SetActive(false);
        }

        // Set input field text to match original text
        inputField.text = originalText;

        // Focus the input field to bring up the keyboard
        inputField.Select();
        inputField.ActivateInputField();

        // Force touch keyboard on mobile
        TouchScreenKeyboard.Open(inputField.text, TouchScreenKeyboardType.Default);
    }

    private void OnInputFieldValueChanged(string newText)
    {
        // Update the original text in real-time as user types
        text.text = newText;
    }

    private void OnInputFieldEndEdit(string newText)
    {
        // We still update the text here as a safeguard
        text.text = newText;
        
        // Hide input field - DON'T DISABLE THE TEXT!
        inputField.gameObject.SetActive(false);

        // Retain selection after editing
        Select();
    }
    
    public void DeleteSelectedText()
    {
        if (isSelected && gameObject != null)
        {
            // Hide the delete button before destroying the text
            if (deleteButton != null)
            {
                deleteButton.gameObject.SetActive(false);
            }

            // Deselect before destroying to clean up references
            Deselect();

            // Destroy the text object
            Destroy(gameObject);
        }
    }

    // Add necessary cleanup methods
    void OnDestroy()
    {
        // Clean up event listeners to prevent memory leaks
        if (inputField != null)
        {
            inputField.onValueChanged.RemoveAllListeners();
            inputField.onEndEdit.RemoveAllListeners();

            // If the inputField was created dynamically, destroy it properly
            if (inputField.gameObject != null)
            {
                Destroy(inputField.gameObject);
            }
        }
        
        // Remove delete button listener
        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveListener(DeleteSelectedText);
        }

        // Break circular references
        if (CurrentlySelectedText == this)
        {
            CurrentlySelectedText = null;
        }

        // Clear TextManager reference if needed
        if (textManager != null && textManager.Text == text)
        {
            textManager.Text = null;
        }
    }

    // Ensure OnDisable is also handling cleanup of references
    void OnDisable()
    {
        // If this object was selected when disabled, deselect it
        if (isSelected)
        {
            Deselect();
        }
    }

    // Update method to reposition the input field if the text moves
    void LateUpdate()
    {
        // If input field is active, ensure it stays aligned with text
        if (inputField != null && inputField.gameObject.activeSelf)
        {
            RectTransform textRectTransform = text.GetComponent<RectTransform>();
            Vector3[] textCorners = new Vector3[4];
            textRectTransform.GetWorldCorners(textCorners);

            // Convert world corners to canvas space
            for (int i = 0; i < 4; i++)
            {
                textCorners[i] = parentCanvas.transform.InverseTransformPoint(textCorners[i]);
            }

            RectTransform inputRect = inputField.GetComponent<RectTransform>();
            Vector2 bottomLeft = textCorners[0];
            inputRect.anchoredPosition = bottomLeft;
            inputRect.sizeDelta = new Vector2(textCorners[2].x - bottomLeft.x, textCorners[2].y - bottomLeft.y);
        }
    }
}


