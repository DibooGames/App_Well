using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class SilhouetteCapture : MonoBehaviour
{
    [Header("UI Elements")]
    public RawImage cameraFeed;          // Affiche le flux de la caméra
    public Image silhouetteOverlay;      // Image avec le contour de la silhouette
    public TMP_Text timerText;               // Texte du compte à rebours
    public Button captureButton;         // Bouton pour déclencher la capture
    public Image garmentOverlay;         // Vêtement à afficher après la capture

    private WebCamTexture webcamTexture;

    void Start()
    {
        // Démarrer le flux de la caméra
        if (WebCamTexture.devices.Length > 0)
        {
            webcamTexture = new WebCamTexture(WebCamTexture.devices[0].name, 1280, 720);
            cameraFeed.texture = webcamTexture;
            webcamTexture.Play();

            int rotationAngle = webcamTexture.videoRotationAngle;
            cameraFeed.rectTransform.localEulerAngles = new Vector3(0, 0, -rotationAngle);

            if (webcamTexture.videoVerticallyMirrored)
                cameraFeed.uvRect = new Rect(1, 0, -1, 1);
            else
                cameraFeed.uvRect = new Rect(0, 0, 1, 1);



           


        }
        else
        {
            Debug.Log("Aucune caméra disponible !");
        }

        // Par défaut, le vêtement est masqué
        garmentOverlay.gameObject.SetActive(false);
        timerText.gameObject.SetActive(true);

        // Lancer la séquence de capture quand le bouton est cliqué
        captureButton.onClick.AddListener(StartCapture);
    }

    void StartCapture()
    {
        StartCoroutine(CaptureSequence());
    }

    IEnumerator CaptureSequence()
    {
        timerText.gameObject.SetActive(true);
        // Démarrer un compte à rebours de 3 secondes
        int countdown = 3;
        while (countdown > 0)
        {
            timerText.text = countdown.ToString();
            yield return new WaitForSeconds(1f);
            countdown--;
        }
        timerText.text = "";
        timerText.gameObject.SetActive(false);

        // Attendre la fin du frame pour capturer l'image
        yield return new WaitForEndOfFrame();

        // Créer une texture et copier les pixels du flux de la caméra
        Texture2D capturedImage = new Texture2D(webcamTexture.width, webcamTexture.height);
        capturedImage.SetPixels(webcamTexture.GetPixels());
        capturedImage.Apply();

        // Optionnel : Vous pouvez sauvegarder l'image ou la traiter ici

        // Remplacer le flux live par l'image capturée
        cameraFeed.texture = capturedImage;

        // Masquer l'overlay de la silhouette et afficher l'image du vêtement
        silhouetteOverlay.gameObject.SetActive(false);
        garmentOverlay.gameObject.SetActive(true);
    }
}
