using System.IO;

namespace VKPlayer.Helpers
{
    public static class StringHelper
    {
        public static string GetSafeFilename(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
