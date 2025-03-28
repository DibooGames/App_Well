using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System;

public class PhotoCapture : MonoBehaviour
{
    [Header("Save Settings")]
    [SerializeField] private bool saveToGallery = true;
    [SerializeField] private Button captureButton;
    [SerializeField] private string imageFileName = "PhotoCapture";

    [SerializeField] private GameObject silhouetteOverlay;
    [SerializeField] private RawImage cameraFeed;
    [SerializeField] private WebCamTexture webcamTexture;

    private Texture2D capturedImage;
    private string lastSavedImagePath;

    void Start()
    {
        captureButton.onClick.AddListener(CaptureImage);
    }
    
    void CaptureImage()
    {
        // Temporarily disable silhouette overlay
        bool wasOverlayActive = silhouetteOverlay.activeSelf;
        silhouetteOverlay.SetActive(false);

        // Capture at end of frame to ensure UI updates
        StartCoroutine(CaptureAtEndOfFrame(wasOverlayActive));
    }

    IEnumerator CaptureAtEndOfFrame(bool restoreOverlay)
    {
        // Wait until the end of the frame to capture
        yield return new WaitForEndOfFrame();

        // Capture the camera feed
        int width = webcamTexture.width;
        int height = webcamTexture.height;

        capturedImage = new Texture2D(width, height);
        Color[] pixels = webcamTexture.GetPixels();

        // Apply rotation/mirroring if needed
        int rotationAngle = webcamTexture.videoRotationAngle;
        bool mirrored = webcamTexture.videoVerticallyMirrored;
        if (rotationAngle != 0 || mirrored)
        {
            pixels = AdjustCapturedPixels(pixels, width, height, rotationAngle, mirrored);
        }

        capturedImage.SetPixels(pixels);
        capturedImage.Apply();

        // Save to gallery
        if (saveToGallery)
        {
            SaveImageToGallery();
        }

        // Restore silhouette overlay if it was active
        if (restoreOverlay)
        {
            silhouetteOverlay.SetActive(true);
        }
    }

    private Color[] AdjustCapturedPixels(Color[] pixels, int width, int height, int rotationAngle, bool mirrored)
    {
        // Implémenter ici la rotation et le mirroring des pixels si nécessaire
        // Pour une implémentation simple, on retourne les pixels sans transformation
        // Une implémentation plus complète serait requise pour gérer les rotations
        return pixels;
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
            mediaStoreClass.CallStatic<string>("insertImage", contentResolver, filePath, fileName, "Image captured with PhotoCapture app");
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
        NativeGallery.SaveImageToGallery(filePath, "PhotoCapture", fileName);
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