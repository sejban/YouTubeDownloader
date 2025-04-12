using System.IO;

namespace YouTubeDownloader.Utils
{
    public static class FileHelper
    {
        public static void SaveFile(string filePath, byte[] fileData)
        {
            File.WriteAllBytes(filePath, fileData);
        }
    }
}