using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.IO;
using System;

public class SilhouetteCapture : MonoBehaviour
{
    [Header("UI Elements")]
    public RawImage cameraFeed;          // Affiche le flux de la caméra
    public Image silhouetteOverlay;      // Image avec le contour de la silhouette
    public TMP_Text timerText;               // Texte du compte à rebours
    public Button captureButton;         // Bouton pour déclencher la capture
    public Image garmentOverlay;         // Vêtement à afficher après la capture

    [Header("Save Settings")]
    [SerializeField] private bool saveToGallery = true;
    [SerializeField] private string imageFileName = "SilhouetteCapture";
    
    private WebCamTexture webcamTexture;
    private Texture2D capturedImage;
    private string lastSavedImagePath;

    void Start()
    {
        Screen.orientation=ScreenOrientation.Portrait;
        // Démarrer le flux de la caméra
        if (WebCamTexture.devices.Length > 0)
        {
            // webcamTexture = new WebCamTexture(WebCamTexture.devices[0].name, 1280, 720);
            webcamTexture = new WebCamTexture(WebCamTexture.devices[0].name, Screen.height, Screen.width);
            cameraFeed.texture = webcamTexture;
            webcamTexture.Play();

            // Wait a frame to ensure camera has started
            StartCoroutine(AdjustCameraDisplay());
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

    private IEnumerator AdjustCameraDisplay()
    {
        // Wait for webcam to initialize properly
        yield return new WaitForSeconds(0.1f);
        
        int rotationAngle = webcamTexture.videoRotationAngle;
        cameraFeed.rectTransform.localEulerAngles = new Vector3(0, 0, -rotationAngle);

        if (webcamTexture.videoVerticallyMirrored)
            cameraFeed.uvRect = new Rect(1, 0, -1, 1);
        else
            cameraFeed.uvRect = new Rect(0, 0, 1, 1);
        
        // For portrait mode, we need different calculations
        bool isPortrait = Screen.height > Screen.width;
        
        // Make sure the RawImage stretches to fill its parent container
        RectTransform rt = cameraFeed.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 1);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        // Reset scale first
        cameraFeed.rectTransform.localScale = Vector3.one;
        
        // Get the aspect ratio - for portrait mode we need to use inverted ratio
        float cameraRatio = (float)webcamTexture.width / (float)webcamTexture.height;
        float screenRatio = (float)Screen.width / (float)Screen.height;
      
        
        // In portrait mode, we want to fill height and center horizontally
        if (isPortrait)
        {
            // Set aspect ratio proportions to maintain camera dimensions
            float scaleFactor = (float)Screen.width / webcamTexture.width;
            float scaleHeight = webcamTexture.height * scaleFactor;
            float normalizedHeight = scaleHeight / Screen.height;
            
            // Create a centered, proportional rect that fills the height
            float xOffset = (1f - (cameraRatio * screenRatio)) / 2f;
            cameraFeed.uvRect = new Rect(0, 0, 1, 1);
            
            if (webcamTexture.videoVerticallyMirrored)
                cameraFeed.uvRect = new Rect(0, 1, 1, -1);
            
            // Stretch to fill the view
            rt.sizeDelta = new Vector2(0, 0);
            rt.localScale = new Vector3(1.0f / screenRatio * cameraRatio, 1.0f, 1.0f);
        }
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
        capturedImage = new Texture2D(webcamTexture.width, webcamTexture.height);
        capturedImage.SetPixels(webcamTexture.GetPixels());
        capturedImage.Apply();

        // Sauvegarder l'image dans la galerie si l'option est activée
        if (saveToGallery)
        {
            SaveImageToGallery();
            
            // Add to recent pictures list
            if (!string.IsNullOrEmpty(lastSavedImagePath))
            {
                DateTime timestamp = DateTime.Now;
                RecentPicturesManager.Instance.AddRecentPicture(lastSavedImagePath, timestamp);
            }
        }

        // Remplacer le flux live par l'image capturée
        cameraFeed.texture = capturedImage;

        // Masquer l'overlay de la silhouette et afficher l'image du vêtement
        silhouetteOverlay.gameObject.SetActive(false);
        garmentOverlay.gameObject.SetActive(true);
    }

    private void SaveImageToGallery()
    {
        try 
        {
            byte[] imageBytes = capturedImage.EncodeToPNG();
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"{imageFileName}_{timestamp}.png";

            #if UNITY_ANDROID
                SaveImageAndroid(imageBytes, fileName);
            #elif UNITY_IOS
                SaveImageIOS(imageBytes, fileName);
            #else
                SaveImageStandalone(imageBytes, fileName);
            #endif

            Debug.Log($"Image saved as {fileName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save image: {e.Message}");
        }
    }

    #if UNITY_ANDROID
    private void SaveImageAndroid(byte[] imageBytes, string fileName)
    {
        // Vérifier et demander les permissions si nécessaire
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        
        // Sauvegarder l'image dans la galerie
        AndroidJavaClass mediaStoreClass = new AndroidJavaClass("android.provider.MediaStore$Images$Media");
        AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
        
        // Créer un fichier temporaire
        string cachePath = Application.temporaryCachePath;
        string filePath = Path.Combine(cachePath, fileName);
        File.WriteAllBytes(filePath, imageBytes);
        
        lastSavedImagePath = filePath;
        
        // Insérer dans la galerie
        AndroidJavaClass fileClass = new AndroidJavaClass("java.io.File");
        AndroidJavaObject fileObject = new AndroidJavaObject("java.io.File", filePath);
        AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
        AndroidJavaObject uri = uriClass.CallStatic<AndroidJavaObject>("fromFile", fileObject);
        
        currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
            mediaStoreClass.CallStatic<string>("insertImage", contentResolver, filePath, fileName, "Image captured with SilhouetteCapture app");
        }));
    }
    #endif

    #if UNITY_IOS
    private void SaveImageIOS(byte[] imageBytes, string fileName)
    {
        string filePath = Path.Combine(Application.temporaryCachePath, fileName);
        File.WriteAllBytes(filePath, imageBytes);
        
        lastSavedImagePath = filePath;
        
        // Utiliser l'API iOS pour sauvegarder dans la galerie
        UnityEngine.iOS.Device.SetNoBackupFlag(filePath);
        NativeGallery.SaveImageToGallery(filePath, "SilhouetteCapture", fileName);
    }
    #endif

    private void SaveImageStandalone(byte[] imageBytes, string fileName)
    {
        // Pour les plateformes desktop, sauvegarder dans les Documents
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string filePath = Path.Combine(documentsPath, fileName);
        File.WriteAllBytes(filePath, imageBytes);
        
        lastSavedImagePath = filePath;
    }
}
