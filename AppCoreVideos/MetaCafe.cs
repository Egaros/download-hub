using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AppCoreVideos
{
    public class MetaCafe
    {
        static string baseUrl = "http://vl.mccont.com/ItemFiles/";
        public async static Task<MetaCafeVideo> GetMetaCafeVideo(string VideoUrl)
        {

            MetaCafeVideo video = new MetaCafeVideo();
            string html = await DownloadWebPage(VideoUrl);
            string downloadUrl = GetDownloadURL(html);
            string title = GetTitle(html);

            video.DownloadUrl = downloadUrl;
            video.VideoTitle = title;
            video.VideoUrl = VideoUrl;
            return video;

        }

        public static async Task<string> DownloadWebPage(string VideoUrl)
        {
            string buffer;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(VideoUrl);
                HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                buffer = reader.ReadToEnd();
                stream.Dispose();
                reader.Dispose();
            }
            catch (Exception exp)
            {
                Helper.HelperMethods.MessageUser("Please connect to the Internet and restart the application");
                buffer = "Error: " + exp.Message.ToString();
            }

            return (buffer);


        }
        public static string GetDownloadURL(string HTML)
        {
            string itemID;
            int lastIndex;
            //int firstIndex = HTML.IndexOf("%5C%2F%255BFrom%2520www.metacafe.com%255D%2520") + 46;
            int firstIndex = HTML.IndexOf("%22%3A0%2C%22mediaURL%22%3A%22") + 30;
            lastIndex = HTML.IndexOf(".mp4%22%2C%22access%22%3A%5B%7B%22key%22%3A%2");
            //lastIndex=decodedHTML.IndexOf('\'',lastIndex);
            itemID = HTML.Substring(firstIndex, lastIndex - firstIndex);
            string downloadUrl;
            downloadUrl = baseUrl + itemID + ".mp4";
            return downloadUrl;
        }

        public static string GetTitle(string HTML)
        {
            string title;
            int firstIndex = HTML.IndexOf("&normalizedTitle=") + 17;
            int lastIndex = HTML.IndexOf("&userPageID=0&description");
            title = HTML.Substring(firstIndex, lastIndex - firstIndex);
            title = title.Replace("_", " ");
            title = title + ".mp4";
            return title;

        }
    }
}
