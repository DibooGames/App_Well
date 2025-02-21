using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CaptureAndRotate : MonoBehaviour
{
    public RawImage rawImage;       // Affichage de la caméra ou de la capture
    public Button captureButton;    // Bouton pour lancer le compte à rebours
    public Text countdownText;      // (Optionnel) Affichage du compte à rebours

    private WebCamTexture webCamTexture;

    void Start()
    {
        if (WebCamTexture.devices.Length > 0)
        {
            // On récupère la première caméra disponible
            webCamTexture = new WebCamTexture();
            rawImage.texture = webCamTexture;
            webCamTexture.Play();
            
            // On ajuste la rotation pour que l'image s'affiche en portrait.
            // La rotation à appliquer est donnée par videoRotationAngle.
            int rotationAngle = webCamTexture.videoRotationAngle;
            
            // Si l'image est miroir (par exemple avec la caméra frontale), on peut avoir besoin d'inverser la rotation
            if (webCamTexture.videoVerticallyMirrored)
                rawImage.rectTransform.localEulerAngles = new Vector3(0, 0, rotationAngle);
            else
                rawImage.rectTransform.localEulerAngles = new Vector3(0, 0, -rotationAngle);
        }
        else
        {
            Debug.Log("Aucune caméra disponible sur cet appareil.");
        }
    }

    public void Capture()
    {
        StartCoroutine(CountdownAndCapture());
    }

    IEnumerator CountdownAndCapture()
    {
        int countdown = 3;
        while (countdown > 0)
        {
            if (countdownText != null)
                countdownText.text = countdown.ToString();
            yield return new WaitForSeconds(1f);
            countdown--;
        }
        if (countdownText != null)
            countdownText.text = "";

        yield return new WaitForEndOfFrame();
        Texture2D capturedTexture = ScreenCapture.CaptureScreenshotAsTexture();

        // Rotation de la texture de 90 degrés (ajustez la valeur selon vos besoins)
        Texture2D rotatedTexture = RotateTexture(capturedTexture, true);
        rawImage.texture = rotatedTexture;
    }

    /// <summary>
    /// Effectue une rotation de la texture de 90 degrés.
    /// Si clockwise vaut true, la rotation est dans le sens horaire.
    /// </summary>
    Texture2D RotateTexture(Texture2D originalTexture, bool clockwise)
    {
        int width = originalTexture.width;
        int height = originalTexture.height;
        Texture2D rotatedTexture = new Texture2D(height, width);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (clockwise)
                {
                    rotatedTexture.SetPixel(j, width - i - 1, originalTexture.GetPixel(i, j));
                }
                else
                {
                    rotatedTexture.SetPixel(height - j - 1, i, originalTexture.GetPixel(i, j));
                }
            }
        }
        rotatedTexture.Apply();
        return rotatedTexture;
    }
}
