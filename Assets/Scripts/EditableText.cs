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
    private TMP_InputField inputField;
    private string originalText;

    private bool isDragging = false; // Track dragging state
    private bool isSelected = false; // Track selection state

    // Changed from incorrect instance field to non-static property that works with TextManager
    public EditableText CurrentlySelectedText;

    private Color originalColor;

    void Awake()
    {
        // Store original color
        if (text == null)
            text = GetComponent<TextMeshProUGUI>();

        originalColor = text.color;

        // Create input field if it doesn't exist
        if (inputField == null)
        {
            // Store original text component reference
            if (text == null)
                text = GetComponent<TextMeshProUGUI>();

            // Get the RectTransform of the text
            RectTransform originalRect = text.GetComponent<RectTransform>();

            // Create game object with input field
            GameObject inputFieldGO = new GameObject("EditorInputField");
            inputFieldGO.transform.SetParent(transform);

            // Add RectTransform to input field and copy properties from original
            RectTransform inputRect = inputFieldGO.AddComponent<RectTransform>();
            inputRect.anchoredPosition = originalRect.anchoredPosition;
            inputRect.anchorMin = originalRect.anchorMin;
            inputRect.anchorMax = originalRect.anchorMax;
            inputRect.pivot = originalRect.pivot;
            inputRect.sizeDelta = originalRect.sizeDelta;
            inputRect.offsetMin = originalRect.offsetMin;
            inputRect.offsetMax = originalRect.offsetMax;

            // Set world position/rotation/scale to match text exactly
            inputFieldGO.transform.position = text.transform.position;
            inputFieldGO.transform.rotation = text.transform.rotation;
            inputFieldGO.transform.localScale = text.transform.localScale;

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

            // Create text component for input field
            TMP_Text inputText = Instantiate(text, textArea.transform);
            inputText.name = "Text";

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

            // Add content type and other settings
            inputField.contentType = TMP_InputField.ContentType.Standard;

            // Add listener for real-time text updating
            inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
            inputField.onEndEdit.AddListener(OnInputFieldEndEdit);

            // Hide input field initially
            inputFieldGO.SetActive(false);
        }
    }

    void Update()
    {
        // Check for mouse clicks outside UI elements
        if (Input.GetMouseButtonDown(0) && isSelected)
        {
            CheckClickOutsideUI();
        }
    }

    private void CheckClickOutsideUI()
    {
        // Check if we're clicking on a UI element with EventSystem first
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // We hit some UI element, let's check if it has the UI tag
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
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
        // Check if pointer is over any UI element
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
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

    // Replace OnPointerClick with OnPointerUp to respond when finger is released
    public void OnPointerUp(PointerEventData eventData)
    {
        // Only process if not dragging
        if (!isDragging)
        {
            if (isSelected)
            {
                // Second click on selected text - open keyboard
                OpenKeyboard();
            }
            else
            {
                // First click - select this text
                Select();
            }
        }

        // Reset drag state on pointer up
        isDragging = false;
    }

    // Called by Unity's EventSystem when this object is selected
    public void OnSelect(BaseEventData eventData)
    {
        Select();
    }

    // Called by Unity's EventSystem when another object is selected
    public void OnDeselect(BaseEventData eventData)
    {
        // Skip deselection from the EventSystem - we'll handle this ourselves in CheckClickOutsideUI
        // to avoid recursive deselection errors
    }

    // Select this text object
    private void Select()
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
    }

    // Deselect this text object
    public void Deselect()
    {
        if (!isSelected)
            return;

       
       
        // Only clear TextManager.Text if it's still referencing this text
        if (textManager != null && textManager.Text == text)
        {
            textManager.Text = null; // Clear the TextManager's text reference
        }

        isSelected = false;

        // Ensure circular references are completely broken
        CurrentlySelectedText = null;
    }

    // Add method to set dragging state from MoveElements
    public void SetDragState(bool dragging)
    {
        isDragging = dragging;
    }

    public void OpenKeyboard()
    {
        originalText = text.text;

        // Show input field and hide original text
        inputField.gameObject.SetActive(true);
        text.gameObject.SetActive(false);

        // Set input field text to match original text
        inputField.text = originalText;

        // Focus the input field to bring up the keyboard
        inputField.Select();
        inputField.ActivateInputField();
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

        // Hide input field and show original text
        inputField.gameObject.SetActive(false);
        text.gameObject.SetActive(true);

        // Retain selection after editing
        Select();
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
                // Only destroy if it's not already being destroyed as our child
                if (inputField.transform.parent != transform)
                    Destroy(inputField.gameObject);
            }
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
}
