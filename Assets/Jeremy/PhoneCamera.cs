using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhoneCamera : MonoBehaviour
{
    private bool camAvailable;
    private WebCamTexture backCam;
    private Texture defaultBackground;

    public RawImage background;
    public RawImage snapshotDisplay; // Pour afficher la capture d'écran
    public AspectRatioFitter fit;

    // Start is called before the first frame update
    private void Start()
    {
        defaultBackground = background.texture;
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.Log("No camera detected");
            camAvailable = false;
            return;
        }

        for (int i = 0; i < devices.Length; i++)
        {
            if (!devices[i].isFrontFacing)
            {
                backCam = new WebCamTexture(devices[i].name,Screen.width,Screen.height);
            }
        }

        if(backCam == null)
        {
            Debug.Log("Unable to find back Camera");
            return;
        }

        backCam.Play();
        background.texture = backCam;

        camAvailable = true;
    }



    // Update is called once per frame
    private void Update()
    {
        if (!camAvailable)
            return;

        float ratio = (float)backCam.width / (float)backCam.height;
        fit.aspectRatio = ratio;

        float scaleY = backCam.videoVerticallyMirrored ? -1f : 1f;
        background.rectTransform.localScale = new Vector3(1f*ratio, scaleY*ratio, 1f*ratio);

        int orient = -backCam.videoRotationAngle;
        background.rectTransform.localEulerAngles = new Vector3(0,0,orient);
    }



    
    public void TakeSnapshot()
    {
        if (!camAvailable)
            return;

        
        Texture2D snapshot = new Texture2D(backCam.width, backCam.height);
        snapshot.SetPixels(backCam.GetPixels());
        snapshot.Apply();

        
        snapshot = RotateTexture(snapshot, backCam.videoRotationAngle);

        
        snapshotDisplay.texture = snapshot;
        snapshotDisplay.enabled = false;
        snapshotDisplay.enabled = true;

        Debug.Log("Snapshot taken and rotated: " + backCam.videoRotationAngle + "°");
    }




    private Texture2D RotateTexture(Texture2D original, int angle)
    {
        int width = original.width;
        int height = original.height;
        Texture2D rotated = new Texture2D(height, width); // Inversion largeur/hauteur

        Color[] pixels = original.GetPixels();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int newX = 0, newY = 0;

                // Applique une rotation selon l'angle
                switch (angle)
                {
                    case 90:
                        newX = y;
                        newY = width - 1 - x;
                        break;
                    case 180:
                        newX = width - 1 - x;
                        newY = height - 1 - y;
                        break;
                    case 270:
                        newX = height - 1 - y;
                        newY = x;
                        break;
                    default:
                        newX = x;
                        newY = y;
                        break;
                }

                rotated.SetPixel(newX, newY, pixels[x + y * width]);
            }
        }

        rotated.Apply();
        return rotated;
    }

}

// it still works perfect in 2023