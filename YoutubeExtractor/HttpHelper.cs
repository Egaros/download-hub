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
    internal static class HttpHelper
    {
        public async static Task<string> DownloadString(string url)
        {
            string pageSource;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            //request.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko";
//#if WINDOWS_PHONE_APP
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


        public static string HtmlDecode(string value)
        {
            return WebUtility.HtmlDecode(value);
        }


        public static IDictionary<string, string> ParseQueryString(string s)
        {
            // remove anything other than query string from url 
            if (s.Contains("?"))
            {
                s = s.Substring(s.IndexOf('?') + 1);
            }


            var dictionary = new Dictionary<string, string>();


            foreach (string vp in Regex.Split(s, "&"))
            {
                string[] strings = Regex.Split(vp, "=");
                dictionary.Add(strings[0], strings.Length == 2 ? UrlDecode(strings[1]) : string.Empty);
            }


            return dictionary;
        }


        public static string ReplaceQueryStringParameter(string currentPageUrl, string paramToReplace, string newValue)
        {
            var query = ParseQueryString(currentPageUrl);

            query[paramToReplace] = newValue;


            var resultQuery = new StringBuilder();
            bool isFirst = true;


            foreach (KeyValuePair<string, string> pair in query)
            {
                if (!isFirst)
                {
                    resultQuery.Append("&");
                }


                resultQuery.Append(pair.Key);
                resultQuery.Append("=");
                resultQuery.Append(pair.Value);


                isFirst = false;
            }


            var uriBuilder = new UriBuilder(currentPageUrl)
            {
                Query = resultQuery.ToString()
            };


            return uriBuilder.ToString();
        }


        public static string UrlDecode(string url)
        {
            return WebUtility.UrlDecode(url);
        }


        private static string ReadStreamFromResponse(WebResponse response)
        {
            using (Stream responseStream = response.GetResponseStream())
            {
                using (var sr = new StreamReader(responseStream))
                {
                    return sr.ReadToEnd();
                }
            }
        }

    }
}
