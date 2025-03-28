using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EditableText : MonoBehaviour, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    public TMP_Text text;
    private TMP_InputField inputField;
    private string originalText;
    private bool isDragging = false; // Track dragging state
    private bool isSelected = false; // Track selection state
    
    // Changed from private field to public property with private setter
    public static EditableText CurrentlySelectedText { get; private set; } = null;
    
    private Color originalColor;
    
    void Start()
    {
        // Store original color
        if (text == null)
            text = GetComponent<TMP_Text>();
        
        originalColor = text.color;
        
        // Create input field if it doesn't exist
        if (inputField == null)
        {
            // Store original text component reference
            if (text == null)
                text = GetComponent<TMP_Text>();
                
            // Get the RectTransform of the text
            RectTransform originalRect = text.GetComponent<RectTransform>();
            
            // Create game object with input field
            GameObject inputFieldGO = new GameObject("EditorInputField");
            inputFieldGO.transform.SetParent(transform.parent);
            
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
        Deselect();
    }
    
    // Select this text object
    private void Select()
    {
        // Deselect previous text if there was one
        if (CurrentlySelectedText != null && CurrentlySelectedText != this)
        {
            CurrentlySelectedText.Deselect();
        }
        
        // Select this text
        isSelected = true;
        CurrentlySelectedText = this;
        
        // Visual indicator of selection (highlight text)
        text.color = Color.yellow; // Or any other visual indicator
        
        // Make sure the EventSystem knows this is selected
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
    
    // Deselect this text object
    public void Deselect()
    {
        if (isSelected)
        {
            isSelected = false;
            text.color = originalColor;
            
            if (CurrentlySelectedText == this)
            {
                CurrentlySelectedText = null;
            }
            
            // If this was the EventSystem's selected object, clear it
            if (EventSystem.current.currentSelectedGameObject == gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
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
}
