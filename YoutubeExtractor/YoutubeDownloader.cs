using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace YoutubeExtractor
{
    public class YoutubeDownloader
    {
        public async static Task<List<VideoQuality>> GetYouTubeVideoUrls(params string[] videosUrls)
        {
            List<VideoQuality> urls = new List<VideoQuality>();
            foreach (string videoUrl in videosUrls)
            {
                string videoId = Helper.GetVideoIDFromUrl(videoUrl);
                string input = "";
                input = await Helper.DownloadWebPage("http://www.youtube.com/watch?v=" + videoId);

                string expression = Helper.GetTxtBtwn(input, "fmt_stream_map\": \"",
                  "\"", 0).Replace(@"\/", "/").Replace(@"\u0026", "&");
                if (expression == "")
                {
                    input = await Helper.DownloadWebPage("http://www.youtube.com/get_video_info?video_id=" +
                               videoId + "&asv=3&el=detailpage&hl=en_US");
                    expression = WebUtility.UrlDecode(Helper.GetTxtBtwn(input, "fmt_stream_map=", "&", 0));
                }
                string str14 = Helper.GetTxtBtwn(input, "'VIDEO_TITLE': '", "'", 0);
                if (str14 == "") str14 = Helper.GetTxtBtwn(input, "\"title\" content=\"", "\"", 0);
                if (str14 == "") str14 = Helper.GetTxtBtwn(input, "&title=", "&", 0);
                str14 = str14.Replace(@"\", "").Replace("'", "&#39;").Replace(
                   "\"", "&quot;").Replace("<", "&lt;").Replace(
                   ">", "&gt;").Replace("+", " ");
                if (!expression.Contains("url")) continue;

                string[] fmtUrls = expression.Split(',');
                foreach (string str21 in fmtUrls)
                {
                    //string itag = "";
                    string url = "";
                    string[] strArray2 = str21.Split('&');
                    foreach (string str22 in strArray2)
                    {
                        if (str22.StartsWith("url"))
                        {
                            url = str22.Substring(4);
                        }
                    }
                    VideoQuality q = new VideoQuality();
                    q.VideoUrl = videoUrl;
                    q.VideoTitle = Helper.UrlDecode(str14);
                    q.DownloadUrl = WebUtility.UrlDecode(url) + "&title=" + str14;
                    if (getQuality(q))
                        urls.Add(q);
                }
                //    foreach (string str21 in Helper.Split(expression, "="))
                //    {
                //        string[] strArray2 = Helper.Split(str21, "&qual");
                //        VideoQuality q = new VideoQuality();
                //        q.VideoUrl = videoUrl;
                //        q.VideoTitle = Helper.UrlDecode(str14);
                //        q.DownloadUrl = WebUtility.UrlDecode(strArray2[0]) + "&title=" + str14;
                //        if (getQuality(q))
                //            urls.Add(q);
                //    }
            }
            return urls;
        }
        private static bool getQuality(VideoQuality q)
        {
            if (q.DownloadUrl.Contains("itag=5"))
                q.SetQuality("flv", new Size(320, 240));
            else if (q.DownloadUrl.Contains("itag=6"))
                q.SetQuality("flv", new Size(480, 360));
            else if (q.DownloadUrl.Contains("itag=17"))
                q.SetQuality("mp4", new Size(320, 144));

            else if (q.DownloadUrl.Contains("itag=18"))
                q.SetQuality("mp4", new Size(480, 360));
            else if (q.DownloadUrl.Contains("itag=22"))
                q.SetQuality("mp4", new Size(1280, 720));

            else if (q.DownloadUrl.Contains("itag=34"))
                q.SetQuality("flv", new Size(400, 226));
            else if (q.DownloadUrl.Contains("itag=35"))
                q.SetQuality("flv", new Size(640, 380));
            else if (q.DownloadUrl.Contains("itag=36"))
                q.SetQuality("mp4", new Size(420, 240));
            else if (q.DownloadUrl.Contains("itag=37"))
                q.SetQuality("mp4", new Size(1920, 1280));
            else if (q.DownloadUrl.Contains("itag=38"))
                q.SetQuality("mp4", new Size(4096, 72304));


            else if (q.DownloadUrl.Contains("itag=133"))
                q.SetQuality("webm", new Size(426, 240));
            else if (q.DownloadUrl.Contains("itag=134"))
                q.SetQuality("webm", new Size(640, 360));
            else if (q.DownloadUrl.Contains("itag=135"))
                q.SetQuality("webm", new Size(854, 480));
            else if (q.DownloadUrl.Contains("itag=136"))
                q.SetQuality("webm", new Size(1280, 720));
            else if (q.DownloadUrl.Contains("itag=137"))
                q.SetQuality("webm", new Size(1920, 226));


            else if (q.DownloadUrl.Contains("itag=242"))
                q.SetQuality("mp4", new Size(426, 240));
            else if (q.DownloadUrl.Contains("itag=243"))
                q.SetQuality("mp4", new Size(640, 360));
            else if (q.DownloadUrl.Contains("itag=244"))
                q.SetQuality("mp4", new Size(854, 480));
            else if (q.DownloadUrl.Contains("itag=247"))
                q.SetQuality("mp4", new Size(1280, 720));
            else if (q.DownloadUrl.Contains("itag=248"))
                q.SetQuality("mp4", new Size(1920, 1080));

            else return false;
            return true;
        }

    }
}
