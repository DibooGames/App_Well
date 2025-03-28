using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DressDrawerManager : MonoBehaviour
{
   
    [SerializeField] private Texture clothingImage;
    [SerializeField] private RawImage clothingholder;

     // Reference to the clothing image
    [SerializeField] private Sprite SilhouettePNG;
    [SerializeField] private Image silhouetteHolder;
    private Button button;

    private void Start() {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);



    }

      
      
      
        private void OnButtonClick()
        {
            // Add your button click logic here
            Debug.Log("Button clicked!");
            silhouetteHolder.sprite = SilhouettePNG;
            clothingholder.texture =  clothingImage;

        }
    }
    
    







