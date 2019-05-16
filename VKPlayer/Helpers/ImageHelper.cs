using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using VKPlayer.Logging;

namespace VKPlayer.Helpers
{
    public static class ImageHelper
    {
        public static async Task<BitmapImage> GetBitmapFromUrl(string url)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var image = new BitmapImage();
                    int BytesToRead = 100;

                    if (url.Contains("?ava=1"))
                    {
                        url = url.Replace("?ava=1", "");
                    }

                    WebRequest request = WebRequest.Create(new Uri(url));
                    request.Timeout = -1;
                    WebResponse response = request.GetResponse();
                    Stream responseStream = response.GetResponseStream();
                    BinaryReader reader = new BinaryReader(responseStream);
                    MemoryStream memoryStream = new MemoryStream();

                    byte[] bytebuffer = new byte[BytesToRead];
                    int bytesRead = reader.Read(bytebuffer, 0, BytesToRead);

                    while (bytesRead > 0)
                    {
                        memoryStream.Write(bytebuffer, 0, bytesRead);
                        bytesRead = reader.Read(bytebuffer, 0, BytesToRead);
                    }

                    image.BeginInit();
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    image.StreamSource = memoryStream;
                    image.EndInit();
                    image.Freeze();

                    return image;
                }
                catch (Exception e)
                {
                    LoggerFacade.WriteError("load photo error", e);

                    return null;
                }
            });
        }
    }
}
