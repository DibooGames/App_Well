using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InstantiateText : MonoBehaviour
{

    public GameObject textPrefab; // Reference to the Text prefab
    public RectTransform textParent; // Parent object to hold the instantiated Text objects

    public TextManager textManager; // Reference to the TextManager script
    public Button button;
    public Button deleteButton; // Reference to the delete button

    public TMP_InputField inputField; // Reference to the TMP_InputField for user input
    private void Start()
    {
        // Add a listener to the button to call the InstantiateTextObject method when clicked
        button.onClick.AddListener(InstantiateTextObject);
    }
    private void InstantiateTextObject()
    {
        // Instantiate the Text prefab as a child of the textParent
        GameObject newTextObject = Instantiate(textPrefab, textParent);

        // Set the rect position to the center of the parent
        RectTransform rectTransform = newTextObject.GetComponent<RectTransform>();
        if (rectTransform != null && textParent != null)
        {
            rectTransform.anchoredPosition = Vector2.zero; // Center the text in the parent
        }

        // Get the EditableText component from the instantiated object
        EditableText editableText = newTextObject.GetComponent<EditableText>();
        if (editableText != null)
        {
            editableText.textManager = textManager; // Assign the TextManager
            editableText.inputField = null; // Ensure inputField is reset
            editableText.deleteButton = deleteButton; // Assign the delete button

            // Ensure delete button functionality works
            deleteButton.onClick.RemoveAllListeners(); // Clear previous listeners
            deleteButton.onClick.AddListener(() => editableText.DeleteSelectedText());
        }

        // Get the TextMeshProUGUI component from the instantiated object
        TextMeshProUGUI textComponent = newTextObject.GetComponent<TextMeshProUGUI>();

        // Set the text and color using the TextManager script
        if (textComponent != null && textManager != null)
        {
            textComponent.text = "New Text"; // Set your desired text here
            textManager.ChangeColor(Color.white); // Set your desired color here
        }
    }
}
