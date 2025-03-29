using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextManager : MonoBehaviour
{
    public TextMeshProUGUI Text;
    private EditableText editableText; // Reference to the EditableText component
    private EditableText textComponent; // Reference to the EditableText component

    public List<TMP_FontAsset> fonts; // List of fonts to choose from
    // Start is called before the first frame update
    public void ChangeColor(Color CurrentColor) {
        
            if(Text)
            {
                editableText = Text.GetComponent<EditableText>();
                if (editableText == null)
                {
                    Debug.Log("EditableText component not found on TextMeshProUGUI.");
                    return;
                }

                Debug.Log("Text is not null, changing color.");
                editableText.originalColor = CurrentColor;
                
            }
            else
            {
                Debug.Log("Text is null, cannot change color.");
            }
         
       
    }

    public void GoNextInFontList()
    {
        if (Text != null && fonts.Count > 0)
        {
            int currentFontIndex = fonts.IndexOf(Text.font);
            int nextFontIndex = (currentFontIndex + 1) % fonts.Count;
            Text.font = fonts[nextFontIndex];
            Debug.Log("Font changed to: " + fonts[nextFontIndex].name);
        }
        else
        {
            Debug.Log("Text is null or font list is empty, cannot change font.");
        }
    }

    public void GoPreviousInFontList()
    {
        if (Text != null && fonts.Count > 0)
        {
            int currentFontIndex = fonts.IndexOf(Text.font);
            int previousFontIndex = (currentFontIndex - 1 + fonts.Count) % fonts.Count;
            Text.font = fonts[previousFontIndex];
            Debug.Log("Font changed to: " + fonts[previousFontIndex].name);
           
        }
        else
        {
            Debug.Log("Text is null or font list is empty, cannot change font.");
        }
    }


   
}
