using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AppCoreMusic
{
    public class Mp3SkullExtract
    {
        public static async Task<string> DownloadHtml(string url)
        {
            string buffer;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                buffer = reader.ReadToEnd();
                stream.Dispose();
                reader.Dispose();
            }
            catch (WebException ex)
            {
                Helper.HelperMethods.MessageUser("Please connect to the Internet and restart the application");
                buffer = ex.Message;
            }
            catch (Exception ex)
            {
                Helper.HelperMethods.MessageUser(ex.Message + "\nRestart the application");
                buffer = ex.Message;
            }
            return (buffer);
        }
        public static List<Mp3SkullMusic> GetListOfDownloads(string html)
        {
            List<Mp3SkullMusic> list = new List<Mp3SkullMusic>();
            while (html != "")
            {
                int index = html.IndexOf("<!-- info mp3 here -->");
                if (index < 0)
                    return list;
                string tempHtml = html.Substring(index);
                try
                {
                    int sizeIndex = tempHtml.IndexOf("mb");
                    string size = tempHtml.Substring(sizeIndex - 6, 5);
                    size = size.Replace(">", " ");
                    size = size.Trim();
                    int timeIndex = tempHtml.IndexOf(":");
                    string time = tempHtml.Substring(timeIndex - 1, 4);
                    int bitrateIndex = tempHtml.IndexOf("kbps");
                    string bitRate = tempHtml.Substring(bitrateIndex - 4, 8);
                    int nameFirstIndex = tempHtml.IndexOf("<b>");
                    int nameLastIndex = tempHtml.IndexOf("</b>");
                    string name = tempHtml.Substring(nameFirstIndex + 3, nameLastIndex - nameFirstIndex - 3);
                    int downLoadLinkIndex = tempHtml.IndexOf("href")+6;
                    int lastIndex = tempHtml.IndexOf("rel=") - 2;
                    string downloadLink = tempHtml.Substring(downLoadLinkIndex, lastIndex - downLoadLinkIndex);
                    Mp3SkullMusic music = new Mp3SkullMusic();
                    music.bitRate = bitRate;
                    music.name = name;
                    music.size = size + "mb";
                    music.time = time;
                    music.downloadLink = downloadLink;
                    list.Add(music);
                    html = html.Substring(index + 23);
                }
                catch (Exception ex)
                {
                    return list;
                }
                
                if (list.Count == 30)
                    break;
            }
            return list;


        }

        public static List<Mp3SkullMusic> GetListOfTopMusic(string html)
        {
            List<Mp3SkullMusic> list = new List<Mp3SkullMusic>();
            while (html != "")
            {
                int index = html.IndexOf("topsong");
                if (index < 0)
                    return list;
                html = html.Substring(index);

                int nameFirstIndex = html.IndexOf("mp3\">");
                int nameLastIndex = html.IndexOf("</a>");
                string name = html.Substring(nameFirstIndex + 5, nameLastIndex - nameFirstIndex - 5);


                int sizeFirstIndex = html.IndexOf("<b>");
                int sizeLastIndex = html.IndexOf("</b>");
                string size = html.Substring(sizeFirstIndex + 3, sizeLastIndex - sizeFirstIndex - 3);

                html = html.Substring(sizeLastIndex + 5);

                int timeFirstIndex = html.IndexOf("<b>");
                int timeLastIndex = html.IndexOf("</b>");
                string time = html.Substring(timeFirstIndex + 3, timeLastIndex - timeFirstIndex - 3);

                //int bitrateIndex = tempHtml.IndexOf("kbps");
                //string bitRate = tempHtml.Substring(bitrateIndex - 4, 8);
                
                //int downLoadLinkIndex = tempHtml.IndexOf("http");
                //int lastIndex = tempHtml.IndexOf("rel=") - 2;
                //string downloadLink = tempHtml.Substring(downLoadLinkIndex, lastIndex - downLoadLinkIndex);
                Mp3SkullMusic music = new Mp3SkullMusic();
                music.name = name;
                music.size = size;
                music.time = time;
                list.Add(music);

            }
            return list;

        }
    }

    
}
