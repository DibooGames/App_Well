using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RecentPicturesDisplayUI : MonoBehaviour
{
    [SerializeField] private GameObject pictureItemPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button clearAllButton;

    private List<GameObject> instantiatedItems = new List<GameObject>();

    private void OnEnable()
    {
        LoadRecentPictures();
        
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
            
        if (clearAllButton != null)
            clearAllButton.onClick.AddListener(ClearAllPictures);
    }

    private void OnDisable()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(ClosePanel);
            
        if (clearAllButton != null)
            clearAllButton.onClick.RemoveListener(ClearAllPictures);
    }

    public void LoadRecentPictures()
    {
        // Clear existing items
        foreach (var item in instantiatedItems)
        {
            Destroy(item);
        }
        instantiatedItems.Clear();

        // Get recent pictures from manager
        if (RecentPicturesManager.Instance == null)
            return;

        var pictures = RecentPicturesManager.Instance.RecentPictures;
        
        // Populate UI with pictures (newest first)
        for (int i = pictures.Count - 1; i >= 0; i--)
        {
            var picture = pictures[i];
            var texture = RecentPicturesManager.Instance.LoadTextureFromPath(picture.FilePath);
            
            if (texture != null)
            {
                GameObject item = Instantiate(pictureItemPrefab, contentParent);
                instantiatedItems.Add(item);
                
                // Set the image
                var image = item.GetComponentInChildren<RawImage>();
                if (image != null)
                {
                    image.texture = texture;
                }
                
                // Set the timestamp text if available
                var dateText = item.GetComponentInChildren<Text>();
                if (dateText != null)
                {
                    dateText.text = picture.Timestamp;
                }
            }
        }
    }

    private void ClosePanel()
    {
        gameObject.SetActive(false);
    }
    
    private void ClearAllPictures()
    {
        if (RecentPicturesManager.Instance != null)
        {
            RecentPicturesManager.Instance.ClearRecentPictures();
            LoadRecentPictures(); // Refresh the UI
        }
    }
}
