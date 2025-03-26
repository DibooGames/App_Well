using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EditableText : MonoBehaviour, IPointerUpHandler
{
    public TMP_Text text;
    private TMP_InputField inputField;
    private string originalText;
    
    void Start()
    {
        // Create input field if it doesn't exist
        if (inputField == null)
        {
            // Store original text component reference
            if (text == null)
                text = GetComponent<TMP_Text>();
                
            // Create game object with input field
            GameObject inputFieldGO = new GameObject("EditorInputField");
            inputFieldGO.transform.SetParent(transform.parent);
            inputFieldGO.transform.localPosition = transform.localPosition;
            inputFieldGO.transform.localRotation = transform.localRotation;
            inputFieldGO.transform.localScale = transform.localScale;
            
            // Add input field component
            inputField = inputFieldGO.AddComponent<TMP_InputField>();
            
            // Create text area for input field
            GameObject textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputFieldGO.transform);
            textArea.transform.localPosition = Vector3.zero;
            textArea.transform.localRotation = Quaternion.identity;
            textArea.transform.localScale = Vector3.one;
            
            // Create text component for input field
            TMP_Text inputText = Instantiate(text, textArea.transform);
            inputText.name = "Text";
            inputText.rectTransform.anchorMin = Vector2.zero;
            inputText.rectTransform.anchorMax = Vector2.one;
            inputText.rectTransform.offsetMin = Vector2.zero;
            inputText.rectTransform.offsetMax = Vector2.zero;
            
            // Setup input field
            inputField.textComponent = inputText;
            inputField.textViewport = textArea.GetComponent<RectTransform>() ?? textArea.AddComponent<RectTransform>();
            
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
        OpenKeyboard();
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
    }
}
