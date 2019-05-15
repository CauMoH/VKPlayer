using System;
using System.IO;

namespace VKPlayer.AppCommon
{
    internal static class PathHelper
    {
        private const string ConfigFileName = "config.xml";

        private const string MusicFolderName = "Download";

        public static string AppDataFolderPath
        {
            get
            {
                var rootFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(rootFolder, AppInfo.AppName);
            }
        }
        
        public static string MusicFolderPath => Path.Combine(AppDataFolderPath, MusicFolderName);

        public static string ConfigFilePath => Path.Combine(AppDataFolderPath, ConfigFileName);

        public static string GetTrackFilePath(string trackFileName)
        {
            return Path.Combine(MusicFolderPath, trackFileName);
        }
    }
}
