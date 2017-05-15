using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YoutubeExtractor
{
    public static class Helper
    {
        /// <summary> 
        /// Decode a string 
        /// </summary> 
        /// <param name="str"></param> 
        /// <returns></returns> 
        public static string UrlDecode(string str)
        {
            return System.Net.WebUtility.UrlDecode(str);
        }


        public static bool IsValidUrl(string url)
        {
            string pattern = @"^(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?$";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(url);
        }


        /// <summary> 
        /// Gets the txt that lies between these two strings 
        /// </summary> 
        public static string GetTxtBtwn(string input, string start, string end, int startIndex)
        {
            return GetTxtBtwn(input, start, end, startIndex, false);
        }


        /// <summary> 
        /// Gets the txt that lies between these two strings 
        /// </summary> 
        public static string GetLastTxtBtwn(string input, string start, string end, int startIndex)
        {
            return GetTxtBtwn(input, start, end, startIndex, true);
        }

        /// <summary> 
        /// Gets the txt that lies between these two strings 
        /// </summary> 
        private static string GetTxtBtwn(string input, string start, string end, int startIndex, bool UseLastIndexOf)
        {
            int index1 = UseLastIndexOf ? input.LastIndexOf(start, startIndex) :
                                          input.IndexOf(start, startIndex);
            if (index1 == -1) return "";
            index1 += start.Length;
            int index2 = input.IndexOf(end, index1);
            if (index2 == -1) return input.Substring(index1);
            return input.Substring(index1, index2 - index1);
        }


        /// <summary> 
        /// Split the input text for this pattren 
        /// </summary> 
        public static string[] Split(string input, string pattren)
        {
            return Regex.Split(input, pattren);
        }

        /// <summary> 
        /// Returns the content of a given web adress as string. 
        /// </summary> 
        /// <param name="Url">URL of the webpage</param> 
        /// <returns>Website content</returns> 
        public async static Task<string> DownloadWebPage(string Url)
        {
            return await DownloadWebPage(Url, null);
        }


        private async static Task<string> DownloadWebPage(string Url, string stopLine)
        {
            // Open a connection 
            string pageSource;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            //request.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko";
//#if WINDOWS_APP
             request.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1667.0 Safari/537.36";
//#endif
            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            pageSource = reader.ReadToEnd();
            stream.Dispose();
            reader.Dispose();
            return pageSource;

        }
        /// <summary> 
        /// Get the ID of a youtube video from its URL 
        /// </summary> 
        public static string GetVideoIDFromUrl(string url)
        {
            url = url.Substring(url.IndexOf("?") + 1);
            string[] props = url.Split('&');


            string videoid = "";
            foreach (string prop in props)
            {
                if (prop.StartsWith("v="))
                    videoid = prop.Substring(prop.IndexOf("v=") + 2);
            }


            return videoid;
        }

        private async static Task<string> DownloadWebPage1(string Url, string stopLine)
        {
            // Open a connection 
            string pageSource;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            //request.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko";
            // request.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1667.0 Safari/537.36";

            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            pageSource = reader.ReadToEnd();
            stream.Dispose();
            reader.Dispose();
            return pageSource;

        }

    }
}
