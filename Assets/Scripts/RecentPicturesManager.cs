using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

public class RecentPicturesManager : MonoBehaviour
{
    private const string SAVE_KEY = "RecentPicturesList";
    private const int MAX_RECENT_PICTURES = 20; // Maximum number of recent pictures to save
    
    public static RecentPicturesManager Instance { get; private set; }
    
    private RecentPicturesList recentPictures = new RecentPicturesList();

    public List<RecentPicture> RecentPictures => recentPictures.Pictures;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        LoadRecentPictures();
    }

    private void OnApplicationQuit()
    {
        SaveRecentPictures();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveRecentPictures();
        }
    }

    public void AddRecentPicture(string filePath, DateTime timestamp)
    {
        // Add the new picture
        recentPictures.Pictures.Add(new RecentPicture(filePath, timestamp));
        
        // Ensure we don't exceed the maximum number
        if (recentPictures.Pictures.Count > MAX_RECENT_PICTURES)
        {
            recentPictures.Pictures.RemoveAt(0);
        }
        
        // Save immediately to reduce risk of data loss
        SaveRecentPictures();
    }

    public void ClearRecentPictures()
    {
        recentPictures.Pictures.Clear();
        SaveRecentPictures();
    }

    private void SaveRecentPictures()
    {
        string jsonData = JsonUtility.ToJson(recentPictures);
        PlayerPrefs.SetString(SAVE_KEY, jsonData);
        PlayerPrefs.Save();
    }

    private void LoadRecentPictures()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string jsonData = PlayerPrefs.GetString(SAVE_KEY);
            recentPictures = JsonUtility.FromJson<RecentPicturesList>(jsonData) ?? new RecentPicturesList();
            
            // Validate that the files still exist and remove entries for missing files
            for (int i = recentPictures.Pictures.Count - 1; i >= 0; i--)
            {
                if (!File.Exists(recentPictures.Pictures[i].FilePath))
                {
                    recentPictures.Pictures.RemoveAt(i);
                }
            }
        }
        else
        {
            recentPictures = new RecentPicturesList();
        }
    }

    public Texture2D LoadTextureFromPath(string filePath)
    {
        if (!File.Exists(filePath))
            return null;
            
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }
}
