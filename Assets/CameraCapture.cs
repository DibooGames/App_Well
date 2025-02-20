using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraCapture : MonoBehaviour
{
    public RawImage rawImage;        // Pour afficher le flux de la caméra
    public Button captureButton;     // Bouton UI pour démarrer le compte à rebours
    public Text countdownText;       // (Optionnel) Pour afficher le compte à rebours à l'écran
    private WebCamTexture webCamTexture;

    [SerializeField] private GameObject Dress;

    void Start()
    {
        // Vérification de la disponibilité d'une caméra
        if (WebCamTexture.devices.Length > 0)
        {
            // Utilisation de la première caméra disponible
            webCamTexture = new WebCamTexture();
            rawImage.texture = webCamTexture;
            rawImage.material.mainTexture = webCamTexture;
            webCamTexture.Play();
        }
        else
        {
            Debug.Log("Aucune caméra disponible sur cet appareil.");
        }

        // Ajout du listener pour le bouton
        captureButton.onClick.AddListener(StartCountdown);
    }

  public void StartCountdown()
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

        Debug.Log("Compte à rebours: " + countdown);
        yield return new WaitForSeconds(1f);
        countdown--;
    }

    if (countdownText != null)
        countdownText.text = "";

    // Attendre la fin du frame pour capturer l'écran
    yield return new WaitForEndOfFrame();
    Texture2D screenTexture = ScreenCapture.CaptureScreenshotAsTexture();
    rawImage.texture = screenTexture;
    Debug.Log("Capture d'écran utilisée comme texture.");
    Dress.SetActive(true);
}

}
