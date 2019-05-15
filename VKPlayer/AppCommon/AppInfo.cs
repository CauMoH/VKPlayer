using System;
using System.Reflection;

namespace VKPlayer.AppCommon
{
    internal static class AppInfo
    {
        public const string AppName = "VKPlayer";
        public const string AppMutexName = "VKPlayer{65A5A66E-8AC9-4EA4-A20E-AF90A8F3BAC4}";
        public const int VkAppId = 4033129;

        public static Version Version => Assembly.GetEntryAssembly().GetName().Version;
    }
}
