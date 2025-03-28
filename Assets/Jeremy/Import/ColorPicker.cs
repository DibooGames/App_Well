using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
    private float currentHue, currentSat, currentVal;
    [SerializeField] private RawImage hueImage, satValImage, outputImage;
    [SerializeField] private Slider hue_slider;
    private Texture2D hueTexture, svTexture, outputTexture;
    private Color currentColor;
    public TextManager textmanager;

    void Start()
    {
        CreateHueImage();
        CreateSVImage();
        CreateOutputImage();

        SetSV(0.8f, 0.8f);
        UpdateSVImage();
        UpdateOutputImage();
    }

    private void CreateHueImage()
    {
        hueTexture = new Texture2D(1, 16);
        hueTexture.wrapMode = TextureWrapMode.Clamp;

        for (int i = 0; i < hueTexture.height; i++)
        {
            hueTexture.SetPixel(0, i, Color.HSVToRGB((float)i / hueTexture.height, 1, 1));
        }

        hueTexture.Apply();
        currentHue = 0;

        hueImage.texture = hueTexture;
    }

    private void CreateSVImage()
    {
        svTexture = new Texture2D(16, 16);
        svTexture.wrapMode = TextureWrapMode.Clamp;

        for (int x = 0; x < svTexture.height; x++)
        {
            for (int y = 0; y < svTexture.width; y++)
            {
                svTexture.SetPixel(y, x, Color.HSVToRGB(currentHue, (float)y / svTexture.width, (float)x / svTexture.height));
            }
        }

        svTexture.Apply();
        currentSat = 0f;
        currentVal = 0f;

        satValImage.texture = svTexture;
    }

    private void CreateOutputImage()
    {
        outputTexture = new Texture2D(1, 16);
        outputTexture.wrapMode = TextureWrapMode.Clamp;

        currentColor = Color.HSVToRGB(currentHue, currentSat, currentVal);

        for (int i = 0; i < outputTexture.height; i++)
        {
            outputTexture.SetPixel(0, i, currentColor);
        }

        outputTexture.Apply();
        outputImage.texture = outputTexture;
    }

    private void UpdateOutputImage()
    {
        currentColor = Color.HSVToRGB(currentHue, currentSat, currentVal);

        for (int i = 0; i < outputTexture.height; i++)
        {
            outputTexture.SetPixel(0, i, currentColor);
        }

        outputTexture.Apply();
        // PaintManager.instance.currentColor = currentColor;
        textmanager.ChangeColor (currentColor);
    }

    public void SetSV(float S, float V)
    {
        currentSat = S;
        currentVal = V;

        UpdateOutputImage();
    }

    public void UpdateSVImage()
    {
        currentHue = hue_slider.value;

        for (int x = 0; x < svTexture.height; x++)
        {
            for (int y = 0; y < svTexture.width; y++)
            {
                svTexture.SetPixel(y, x, Color.HSVToRGB(currentHue, (float)y / svTexture.width, (float)x / svTexture.height));
            }
        }

        svTexture.Apply();
        UpdateOutputImage();
    }

    public Color ReturnCurrentColor() => currentColor;
}
