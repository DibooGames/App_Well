using UnityEngine;

public class RecentPicturesManagerInitializer : MonoBehaviour
{
    [SerializeField] private GameObject recentPicturesManagerPrefab;
    
    private void Awake()
    {
        // If the manager doesn't exist yet, create it
        if (RecentPicturesManager.Instance == null)
        {
            if (recentPicturesManagerPrefab != null)
            {
                Instantiate(recentPicturesManagerPrefab);
            }
            else
            {
                GameObject managerObject = new GameObject("RecentPicturesManager");
                managerObject.AddComponent<RecentPicturesManager>();
            }
        }
    }
}
