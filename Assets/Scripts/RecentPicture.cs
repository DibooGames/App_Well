using System;

[Serializable]
public class RecentPicture
{
    public string FilePath;
    public string Timestamp;

    public RecentPicture(string filePath, DateTime timestamp)
    {
        FilePath = filePath;
        Timestamp = timestamp.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public DateTime GetTimestamp()
    {
        return DateTime.Parse(Timestamp);
    }
}
