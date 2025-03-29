using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Android;

public class MagazineManager : MonoBehaviour
{
    [SerializeField] private Image magazineImage;
    [SerializeField] private Button selectImageButton; 
    [SerializeField] private Button saveButton; // Add reference to the save button
    [SerializeField] private List<GameObject> interfaceObjects; // Add list of interface GameObjects

    private void Start()
    {
        Debug.Log("MagazineManager Start called");
        
        // Set up button listeners programmatically
        SetupButtonListener();
        SetupSaveButtonListener(); // Set up save button listener
        
        // Vérifier si NativeGallery est disponible
        CheckNativeGalleryAvailability();
        
        // Register for permission callbacks
        if (Application.platform == RuntimePlatform.Android)
        {
            PermissionCallbacks permissionCallbacks = new PermissionCallbacks();
            permissionCallbacks.PermissionGranted += OnPermissionGranted;
            permissionCallbacks.PermissionDenied += OnPermissionDenied;
            permissionCallbacks.PermissionDeniedAndDontAskAgain += OnPermissionDenied;
            
            // For Android 13+ (API 33+), we need READ_MEDIA_IMAGES instead of EXTERNAL_STORAGE
            #if UNITY_2023_1_OR_NEWER
            if (Permission.HasUserAuthorizedPermission("android.permission.READ_MEDIA_IMAGES"))
            {
                return;
            }
            Permission.RequestUserPermission("android.permission.READ_MEDIA_IMAGES", permissionCallbacks);
            Debug.Log("Requesting permission: android.permission.READ_MEDIA_IMAGES");
            #else
            Permission.RequestUserPermission(Permission.ExternalStorageRead, permissionCallbacks);
            Debug.Log("Requesting permission: " + Permission.ExternalStorageRead);
            #endif
        }
    }
    
    private void SetupButtonListener()
    {
        // Make sure button reference is assigned
        if (selectImageButton != null)
        {
            // Remove any existing listeners to avoid duplicates
            selectImageButton.onClick.RemoveAllListeners();
            
            // Add our listener
            selectImageButton.onClick.AddListener(OnSelectImageButtonPressed);
            Debug.Log("Button listener set up successfully");
        }
        else
        {
            Debug.LogError("Select Image Button reference is not assigned in the Inspector!");
        }
    }

    private void SetupSaveButtonListener()
    {
        if (saveButton != null)
        {
            saveButton.onClick.RemoveAllListeners();
            saveButton.onClick.AddListener(OnSaveButtonPressed);
            Debug.Log("Save button listener set up successfully");
        }
        else
        {
            Debug.LogError("Save Button reference is not assigned in the Inspector!");
        }
    }

    private void OnSaveButtonPressed()
    {
        Debug.Log("Save button pressed");
        StartCoroutine(SaveScreenshot());
    }

    private IEnumerator SaveScreenshot()
    {
        Debug.Log("SaveScreenshot coroutine started");

        // Deactivate all interface objects
        foreach (var obj in interfaceObjects)
        {
            if (obj != null) obj.SetActive(false);
        }

        // Wait for the end of the frame to ensure UI is hidden
        yield return new WaitForEndOfFrame();

        // Capture the screen as a texture
        Texture2D screenshotTexture = ScreenCapture.CaptureScreenshotAsTexture();
        Debug.Log("Screenshot captured as texture");

        // Save the texture to a file
        string screenshotPath = Path.Combine(Application.persistentDataPath, "screenshot.png");
        byte[] imageData = screenshotTexture.EncodeToPNG();
        File.WriteAllBytes(screenshotPath, imageData);
        Debug.Log("Screenshot saved to file at: " + screenshotPath);

        // Clean up the texture to free memory
        Object.Destroy(screenshotTexture);

        // Save the screenshot to the gallery
        NativeGallery.SaveImageToGallery(screenshotPath, "MagazineApp", "Screenshot.png");
        Debug.Log("Screenshot saved to gallery");

        // Reactivate all interface objects
        foreach (var obj in interfaceObjects)
        {
            if (obj != null) obj.SetActive(true);
        }

        Debug.Log("SaveScreenshot coroutine finished");
    }
    
    // This method should be assigned to your UI button in the Inspector
    // For Unity UI Button compatibility, the method must be public and use the correct signature
    public void OnSelectImageButtonPressed()
    {
        Debug.Log("Image button pressed"); // Add debug log to verify button press
        OpenGallery();
    }
    
    // Alternative method with no parameters that can be easily found in the inspector
    public void SelectImage()
    {
        Debug.Log("Select Image called");
        OpenGallery();
    }
    
    private void CheckNativeGalleryAvailability()
    {
        Debug.Log("Checking NativeGallery plugin availability...");
        
        // Vérifier les permissions actuelles
        NativeGallery.Permission permission = NativeGallery.CheckPermission(NativeGallery.PermissionType.Read, NativeGallery.MediaType.Image);
        Debug.Log("Current permission status: " + permission);
        
        // Vérifier si les méthodes de NativeGallery sont accessibles
        Debug.Log("Can select multiple files: " + NativeGallery.CanSelectMultipleFilesFromGallery());
        Debug.Log("Can select multiple media types: " + NativeGallery.CanSelectMultipleMediaTypesFromGallery());
    }
    
    public void OpenGallery()
    {
        Debug.Log("OpenGallery method called");
        
        try {
            // Check and request permissions on Android
            if (Application.platform == RuntimePlatform.Android)
            {
                // Utiliser directement NativeGallery pour gérer les permissions
                NativeGallery.Permission permission = NativeGallery.RequestPermission(NativeGallery.PermissionType.Read, NativeGallery.MediaType.Image);
                Debug.Log("NativeGallery permission result: " + permission);
                
                if (permission != NativeGallery.Permission.Granted)
                {
                    Debug.LogWarning("Gallery permission not granted!");
                    if (permission == NativeGallery.Permission.ShouldAsk)
                    {
                        Debug.Log("Permission should be asked, requesting again...");
                        // Continuer car RequestPermission va gérer la demande de permission
                    }
                    else
                    {
                        // Montrer un message à l'utilisateur pour l'informer que l'application a besoin de cette permission
                        Debug.LogError("Permission denied! The app cannot access the gallery.");
                        return;
                    }
                }
            }
            
            Debug.Log("About to call NativeGallery.GetImageFromGallery");
            
            // Open native gallery picker avec un callback plus robuste
            NativeGallery.GetImageFromGallery((path) =>
            {
                Debug.Log("Gallery callback received with path: " + (path ?? "null"));
                if (path != null)
                {
                    // Load and apply the selected image on the main thread
                    StartCoroutine(LoadImageFromPathSafe(path));
                }
                else
                {
                    Debug.Log("User cancelled image selection or there was an error");
                }
            }, "Select Image for Magazine Cover", "image/*");
            
            Debug.Log("NativeGallery.GetImageFromGallery called successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Exception in OpenGallery: " + e.Message + "\nStackTrace: " + e.StackTrace);
        }
    }

    private IEnumerator LoadImageFromPathSafe(string path)
    {
        Debug.Log("LoadImageFromPathSafe called with path: " + path);
        
        // Vérifier si le fichier existe avant de l'utiliser
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("Image path is null or empty");
            yield break;
        }
        
        if (!File.Exists(path))
        {
            Debug.LogError("File does not exist at path: " + path);
            yield break;
        }
        
        // Utiliser NativeGallery pour charger l'image avec la taille maximale
        Texture2D texture = NativeGallery.LoadImageAtPath(path, -1);
        if (texture == null)
        {
            Debug.LogError("Failed to load image with NativeGallery");
            yield break;
        }
        
        // Create a sprite from the texture
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        
        // Apply to the magazine image
        magazineImage.sprite = sprite;
        
        // Ensure the image maintains its aspect ratio
        magazineImage.preserveAspect = true;
        
        Debug.Log("Image loaded successfully with dimensions: " + texture.width + "x" + texture.height);
        
        yield return null;
    }
    
    private IEnumerator LoadImageFromPath(string path)
    {
        Debug.Log("LoadImageFromPath called with path: " + path);
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            Debug.LogError("Invalid image path: " + path);
            yield break;
        }
        
        try
        {
            // Create a new Texture2D and load the image data
            Texture2D texture = new Texture2D(2, 2);
            byte[] imageData = File.ReadAllBytes(path);
            
            if (imageData == null || imageData.Length == 0)
            {
                Debug.LogError("Failed to load image data from path: " + path);
                yield break;
            }
            
            texture.LoadImage(imageData);
            
            // Create a sprite from the texture
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            
            // Apply to the magazine image
            magazineImage.sprite = sprite;
            
            // Ensure the image maintains its aspect ratio
            magazineImage.preserveAspect = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error loading image: " + e.Message);
        }
        
        yield return null;
    }

    private IEnumerator CheckPermissionAndOpenGallery()
    {
        Debug.Log("CheckPermissionAndOpenGallery coroutine started");
        yield return new WaitForEndOfFrame();
        OpenGallery();
    }

    // Call this when user permission response is received
    private void OnPermissionGranted(string permissionName)
    {
        Debug.Log("Permission granted: " + permissionName);
        if (permissionName == Permission.ExternalStorageRead || permissionName == "android.permission.READ_MEDIA_IMAGES")
        {
            StartCoroutine(CheckPermissionAndOpenGallery());
        }
    }

    private void OnPermissionDenied(string permissionName)
    {
        Debug.LogError("Permission denied: " + permissionName + " - Gallery will not work!");
        // Show a UI message to inform the user that the app needs this permission
    }
    
    // Add this function to verify AndroidManifest permissions
    private void OnEnable()
    {
        Debug.Log("Device: " + SystemInfo.deviceModel);
        Debug.Log("OS: " + SystemInfo.operatingSystem);
        
        if (Application.platform == RuntimePlatform.Android)
        {
            #if UNITY_2023_1_OR_NEWER
            Debug.Log("READ_MEDIA_IMAGES permission status: " + Permission.HasUserAuthorizedPermission("android.permission.READ_MEDIA_IMAGES"));
            #else
            Debug.Log("EXTERNAL_STORAGE_READ permission status: " + Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead));
            #endif
        }
    }
}
