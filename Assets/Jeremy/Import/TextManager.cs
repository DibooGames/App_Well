using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextManager : MonoBehaviour
{
    public List<TextMeshProUGUI> Text= new List<TextMeshProUGUI>();
    // Start is called before the first frame update
    public void ChangeColor(Color CurrentColor) {
        foreach(TextMeshProUGUI T in Text) {
            T.color = CurrentColor;
        }
    }
   
}
