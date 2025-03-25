using UnityEngine;
using UnityEngine.Android;
using System.Collections.Generic;
using System.Text;

public class AndroidPermissionCheck : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI debugText;
    
    private List<string> debugMessages = new List<string>();
    private StringBuilder sb = new StringBuilder();
    
    void Start()
    {
        CheckPermissions();
    }
    
    public void CheckPermissions()
    {
        debugMessages.Clear();
        
        // Informations système
        LogInfo("Device: " + SystemInfo.deviceModel);
        LogInfo("OS: " + SystemInfo.operatingSystem);
        LogInfo("Unity version: " + Application.unityVersion);
        
        // Vérifier les permissions Android
        if (Application.platform == RuntimePlatform.Android)
        {
            // Android 13+ (API 33+)
            CheckPermission("android.permission.READ_MEDIA_IMAGES");
            // Android < 13
            CheckPermission(Permission.ExternalStorageRead);
            CheckPermission(Permission.ExternalStorageWrite);
            
            // Vérifier le statut NativeGallery
            LogInfo("NativeGallery.CanSelectMultipleFiles: " + NativeGallery.CanSelectMultipleFilesFromGallery());
            
            // Vérifier les permissions via NativeGallery
            NativeGallery.Permission permReadImg = NativeGallery.CheckPermission(NativeGallery.PermissionType.Read, NativeGallery.MediaType.Image);
            LogInfo("NativeGallery Read Image Permission: " + permReadImg);
        }
        else
        {
            LogInfo("Not running on Android - permission check skipped");
        }
        
        // Mise à jour du texte de débogage s'il existe
        UpdateDebugText();
    }
    
    private void CheckPermission(string permissionName)
    {
        bool hasPermission = Permission.HasUserAuthorizedPermission(permissionName);
        LogInfo("Permission " + permissionName + ": " + (hasPermission ? "GRANTED" : "DENIED"));
    }
    
    private void LogInfo(string message)
    {
        Debug.Log(message);
        debugMessages.Add(message);
        UpdateDebugText();
    }
    
    private void UpdateDebugText()
    {
        if (debugText != null)
        {
            sb.Clear();
            foreach (string msg in debugMessages)
            {
                sb.AppendLine(msg);
            }
            debugText.text = sb.ToString();
        }
    }
    
    // Bouton pour rafraîchir les vérifications
    public void RefreshPermissionCheck()
    {
        CheckPermissions();
    }
    
    // Bouton pour ouvrir les paramètres d'application
    public void OpenAppSettings()
    {
        if (NativeGallery.CanOpenSettings())
        {
            NativeGallery.OpenSettings();
        }
        else
        {
            LogInfo("Cannot open app settings on this device");
        }
    }
}
