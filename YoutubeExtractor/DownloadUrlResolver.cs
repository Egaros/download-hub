using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YoutubeExtractor
{
    public static class DownloadUrlResolver
    {
        private const int CorrectSignatureLength = 81;
        private const string SignatureQuery = "signature";

        /// <summary> 
        /// Decrypts the signature in the <see cref="VideoInfo.DownloadUrl" /> property and sets it 
        /// to the decrypted URL. Use this method, if you have decryptSignature in the <see 
        /// cref="GetDownloadUrls" /> method set to false. 
        /// </summary> 
        /// <param name="videoInfo">The video info which's downlaod URL should be decrypted.</param> 
        /// <exception cref="YoutubeParseException"> 
        /// There was an error while deciphering the signature. 
        /// </exception> 
        public async static void DecryptDownloadUrl(VideoInfo videoInfo)
        {
            IDictionary<string, string> queries = HttpHelper.ParseQueryString(videoInfo.DownloadUrl);

            if (queries.ContainsKey(SignatureQuery))
            {
                string encryptedSignature = queries[SignatureQuery];

                string decrypted;


                try
                {
                    decrypted = await GetDecipheredSignature(videoInfo.HtmlPlayerVersion, encryptedSignature);
                }


                catch (Exception ex)
                {
                    throw new YoutubeParseException("Could not decipher signature", ex);
                }


                videoInfo.DownloadUrl = HttpHelper.ReplaceQueryStringParameter(videoInfo.DownloadUrl, SignatureQuery, decrypted);
                videoInfo.RequiresDecryption = false;
            }
        }


        /// <summary> 
        /// Gets a list of <see cref="VideoInfo" />s for the specified URL. 
        /// </summary> 
        /// <param name="videoUrl">The URL of the YouTube video.</param> 
        /// <param name="decryptSignature"> 
        /// A value indicating whether the video signatures should be decrypted or not. Decrypting 
        /// consists of a HTTP request for each <see cref="VideoInfo" />, so you may want to set 
        /// this to false and call <see cref="DecryptDownloadUrl" /> on your selected <see 
        /// cref="VideoInfo" /> later. 
        /// </param> 
        /// <returns>A list of <see cref="VideoInfo" />s that can be used to download the video.</returns> 
        /// <exception cref="ArgumentNullException"> 
        /// The <paramref name="videoUrl" /> parameter is <c>null</c>. 
        /// </exception> 
        /// <exception cref="ArgumentException"> 
        /// The <paramref name="videoUrl" /> parameter is not a valid YouTube URL. 
        /// </exception> 
        /// <exception cref="VideoNotAvailableException">The video is not available.</exception> 
        /// <exception cref="WebException"> 
        /// An error occurred while downloading the YouTube page html. 
        /// </exception> 
        /// <exception cref="YoutubeParseException">The Youtube page could not be parsed.</exception> 
        public async static Task<IEnumerable<VideoInfo>> GetDownloadUrls(string videoUrl, bool decryptSignature = true)
        {
            if (videoUrl == null)
                throw new ArgumentNullException("videoUrl");


            bool isYoutubeUrl = TryNormalizeYoutubeUrl(videoUrl, out videoUrl);


            if (!isYoutubeUrl)
            {
                throw new ArgumentException("URL is not a valid youtube URL!");
            }


            try
            {
                JObject json = await LoadJson(videoUrl);


                string videoTitle = GetVideoTitle(json);


                IEnumerable<ExtractionInfo> downloadUrls = ExtractDownloadUrls(json);


                IEnumerable<VideoInfo> infos = GetVideoInfos(downloadUrls, videoTitle).ToList();

                string htmlPlayerVersion = GetHtml5PlayerVersion(json);

                foreach (VideoInfo info in infos)
                {
                    info.HtmlPlayerVersion = htmlPlayerVersion;


                    if (decryptSignature && info.RequiresDecryption)
                    {
                        DecryptDownloadUrl(info);
                    }
                }

                return infos;
            }

            catch (Exception ex)
            {
                if (ex is WebException || ex is VideoNotAvailableException)
                {
                    throw;
                }


                ThrowYoutubeParseException(ex, videoUrl);
            }


            return null; // Will never happen, but the compiler requires it 
        }


#if PORTABLE
 
 
         public static System.Threading.Tasks.Task<IEnumerable<VideoInfo>> GetDownloadUrlsAsync(string videoUrl, bool decryptSignature = true) 
         { 
             return System.Threading.Tasks.Task.Run(() => GetDownloadUrls(videoUrl, decryptSignature)); 
         } 
 
 
#endif

        /// <summary> 
        /// Normalizes the given YouTube URL to the format http://youtube.com/watch?v={youtube-id} 
        /// and returns whether the normalization was successful or not. 
        /// </summary> 
        /// <param name="url">The YouTube URL to normalize.</param> 
        /// <param name="normalizedUrl">The normalized YouTube URL.</param> 
        /// <returns> 
        /// <c>true</c>, if the normalization was successful; <c>false</c>, if the URL is invalid. 
        /// </returns> 
        public static bool TryNormalizeYoutubeUrl(string url, out string normalizedUrl)
        {
            url = url.Trim();

            url = url.Replace("youtu.be/", "youtube.com/watch?v=");
            url = url.Replace("www.youtube", "youtube");
            url = url.Replace("youtube.com/embed/", "youtube.com/watch?v=");
            url = url.Replace("m.youtube", "youtube");

            if (url.Contains("/v/"))
            {
                url = "http://youtube.com" + new Uri(url).AbsolutePath.Replace("/v/", "/watch?v=");
            }

            url = url.Replace("/watch#", "/watch?");

            IDictionary<string, string> query = HttpHelper.ParseQueryString(url);


            string v;

            if (!query.TryGetValue("v", out v))
            {
                normalizedUrl = null;
                return false;
            }

            normalizedUrl = "http://youtube.com/watch?v=" + v;


            return true;
        }

        private static IEnumerable<ExtractionInfo> ExtractDownloadUrls(JObject json)
        {
            string[] splitByUrls = GetStreamMap(json).Split(',');
            string[] adaptiveFmtSplitByUrls = GetAdaptiveStreamMap(json).Split(',');
            splitByUrls = splitByUrls.Concat(adaptiveFmtSplitByUrls).ToArray();

            foreach (string s in splitByUrls)
            {
                IDictionary<string, string> queries = HttpHelper.ParseQueryString(s);
                string url;

                bool requiresDecryption = false;

                if (queries.ContainsKey("s") || queries.ContainsKey("sig"))
                {
                    requiresDecryption = queries.ContainsKey("s");
                    string signature = queries.ContainsKey("s") ? queries["s"] : queries["sig"];


                    url = string.Format("{0}&{1}={2}", queries["url"], SignatureQuery, signature);


                    string fallbackHost = queries.ContainsKey("fallback_host") ? "&fallback_host=" + queries["fallback_host"] : String.Empty;


                    url += fallbackHost;
                }


                else
                {
                    url = queries["url"];
                }

                url = HttpHelper.UrlDecode(url);
                url = HttpHelper.UrlDecode(url);

                yield return new ExtractionInfo { RequiresDecryption = requiresDecryption, Uri = new Uri(url) };
            }
        }


        private static string GetAdaptiveStreamMap(JObject json)
        {
            JToken streamMap = json["args"]["adaptive_fmts"];

            return streamMap.ToString();
        }

        private async static Task<string> GetDecipheredSignature(string htmlPlayerVersion, string signature)
        {
            if (signature.Length == CorrectSignatureLength)
            {
                return signature;
            }

            return await Decipherer.DecipherWithVersion(signature, htmlPlayerVersion);
        }

        private static string GetHtml5PlayerVersion(JObject json)
        {
            var regex = new Regex(@"html5player-(.+?)\.js");


            string js = json["assets"]["js"].ToString();


            return regex.Match(js).Result("$1");
        }

        private static string GetStreamMap(JObject json)
        {
            JToken streamMap = json["args"]["url_encoded_fmt_stream_map"];

            string streamMapString = streamMap == null ? null : streamMap.ToString();


            if (streamMapString == null || streamMapString.Contains("been+removed"))
            {
                throw new VideoNotAvailableException("Video is removed or has an age restriction.");
            }

            return streamMapString;
        }


        private static IEnumerable<VideoInfo> GetVideoInfos(IEnumerable<ExtractionInfo> extractionInfos, string videoTitle)
        {
            var downLoadInfos = new List<VideoInfo>();

            foreach (ExtractionInfo extractionInfo in extractionInfos)
            {
                string itag = HttpHelper.ParseQueryString(extractionInfo.Uri.Query)["itag"];

                int formatCode = int.Parse(itag);


                VideoInfo info = VideoInfo.Defaults.SingleOrDefault(videoInfo => videoInfo.FormatCode == formatCode);

                if (info != null)
                {
                    info = new VideoInfo(info)
                    {
                        DownloadUrl = extractionInfo.Uri.ToString(),
                        Title = videoTitle,
                        RequiresDecryption = extractionInfo.RequiresDecryption
                    };
                }

                else
                {
                    info = new VideoInfo(formatCode)
                    {
                        DownloadUrl = extractionInfo.Uri.ToString()
                    };
                }

                downLoadInfos.Add(info);
            }

            return downLoadInfos;
        }

        private static string GetVideoTitle(JObject json)
        {
            JToken title = json["args"]["title"];


            return title == null ? String.Empty : title.ToString();
        }


        private static bool IsVideoUnavailable(string pageSource)
        {
            const string unavailableContainer = "<div id=\"watch-player-unavailable\">";


            return pageSource.Contains(unavailableContainer);
        }

        private async static Task<JObject> LoadJson(string url)
        {
            int i = 0;
            string pageSource = await HttpHelper.DownloadString(url);

            if (IsVideoUnavailable(pageSource))
            {
                throw new VideoNotAvailableException();
            }
            //while (i < 6)
            //{
            //    pageSource = pageSource.Substring(pageSource.IndexOf("<script>") + 8);
            //    i++;
            //}
            //int index1 = pageSource.IndexOf("ytplayer.config");
            //pageSource = pageSource.Substring(index1);
            //int index = pageSource.IndexOf("</script>");
            //pageSource = pageSource.Substring(0, index);
            //pageSource = pageSource.Substring(0, pageSource.Length / 2);
            //int index = pageSource.IndexOf("ytplayer.config");
            //int lastIndex = pageSource.IndexOf("}());", index);
            //string extractedJson=pageSource.Substring(index,lastIndex-index);
            var dataRegex = new Regex(@"ytplayer\.config\s*=\s*(\{.+?\});", RegexOptions.Multiline);

            string extractedJson = dataRegex.Match(pageSource).Result("$1");
            //string extractedJson = "ytplayer.config = {\"url_v9as2\": \"https://s.ytimg.com/yts/swfbin/player-vfllVed9W/cps.swf\", \"attrs\": {\"id\": \"movie_player\"}, \"args\": {\"eventid\": \"JADmU6OxBdH84QLYloGwAg\", \"timestamp\": 1407582244, \"url_encoded_fmt_stream_map\": \"type=video%2Fmp4%3B+codecs%3D%22avc1.64001F%2C+mp4a.40.2%22\u0026quality=hd720\u0026url=https%3A%2F%2Fr1---sn-cvh7zn7z.c.youtube.com%2Fvideoplayback%3Fid%3Do-AB6PBmjGxBUzAy95o_3DDAGtZZsunDtsGaRo9mLMGCH8%26fexp%3D901454%252C902408%252C927622%252C931983%252C934024%252C934030%252C946022%26upn%3DWDviZUt2v4o%26key%3Dyt5%26cp%3DU0lPTFZLUV9JTkNPOV9ORVVJOnNpdTd4dHpBbUlJ%26ms%3Dnxu%26requiressl%3Dyes%26ratebypass%3Dyes%26mt%3D1407581698%26sver%3D3%26itag%3D22%26sparams%3Dcp%252Cid%252Cip%252Cipbits%252Citag%252Cratebypass%252Crequiressl%252Csource%252Cupn%252Cexpire%26signature%3D902AE11563D8271D7066EFDD1014E1F6899F6853.84F25E4FB08F881F545E4B57D63302CCB74E0192%26mm%3D30%26source%3Dyoutube%26expire%3D1407603844%26mws%3Dyes%26mv%3Du%26ipbits%3D8%26ip%3D117.222.223.185\u0026fallback_host=tc.v5.cache3.c.youtube.com\u0026itag=22,type=video%2Fmp4%3B+codecs%3D%22avc1.42001E%2C+mp4a.40.2%22\u0026quality=medium\u0026url=https%3A%2F%2Fr1---sn-cvh7zn7z.c.youtube.com%2Fvideoplayback%3Fid%3Do-AB6PBmjGxBUzAy95o_3DDAGtZZsunDtsGaRo9mLMGCH8%26fexp%3D901454%252C902408%252C927622%252C931983%252C934024%252C934030%252C946022%26upn%3DWDviZUt2v4o%26key%3Dyt5%26cp%3DU0lPTFZLUV9JTkNPOV9ORVVJOnNpdTd4dHpBbUlJ%26ms%3Dnxu%26requiressl%3Dyes%26ratebypass%3Dyes%26mt%3D1407581698%26sver%3D3%26itag%3D18%26sparams%3Dcp%252Cid%252Cip%252Cipbits%252Citag%252Cratebypass%252Crequiressl%252Csource%252Cupn%252Cexpire%26signature%3DDADFBF372249B009D77BC5F49BD0F253211BE05F.D5545FC5A5622AF0E70CC6B85C240EAC2C0A7F50%26mm%3D30%26source%3Dyoutube%26expire%3D1407603844%26mws%3Dyes%26mv%3Du%26ipbits%3D8%26ip%3D117.222.223.185\u0026fallback_host=tc.v19.cache6.c.youtube.com\u0026itag=18\", \"ytfocEnabled\": \"1\", \"fexp\": \"901454,902408,927622,931983,934024,934030,946022\", \"referrer\": null, \"rmktPingThreshold\": 0, \"plid\": \"AAUAMEem16xOD4Ma\", \"ldpj\": \"-6\", \"watermark\": \",https://s.ytimg.com/yts/img/watermark/youtube_watermark-vflHX6b6E.png,https://s.ytimg.com/yts/img/watermark/youtube_hd_watermark-vflAzLcD6.png\", \"enablejsapi\": 1, \"cr\": \"IN\", \"length_seconds\": 41, \"baseUrl\": \"https://googleads.g.doubleclick.net/pagead/viewthroughconversion/962985656/\", \"cbr\": \"IE\", \"iv_load_policy\": 1, \"iv_module\": \"https://s.ytimg.com/yts/swfbin/player-vfl8MfiIN/iv_module.swf\", \"adaptive_fmts\": \"index=713-840\u0026clen=6992259\u0026bitrate=2189770\u0026lmt=1407060637275561\u0026type=video%2Fmp4%3B+codecs%3D%22avc1.640028%22\u0026init=0-712\u0026size=1920x1080\u0026url=https%3A%2F%2Fr1---sn-cvh7zn7z.c.youtube.com%2Fvideoplayback%3Flmt%3D1407060637275561%26key%3Dyt5%26fexp%3D901454%252C902408%252C927622%252C931983%252C934024%252C934030%252C946022%26sver%3D3%26gir%3Dyes%26expire%3D1407603844%26mws%3Dyes%26upn%3DswJfsAW6COc%26mv%3Du%26ipbits%3D8%26id%3Do-AB6PBmjGxBUzAy95o_3DDAGtZZsunDtsGaRo9mLMGCH8%26dur%3D39.921%26clen%3D6992259%26cp%3DU0lPTFZLUV9JTkNPOV9ORVVJOnNpdTd4dHpBbUlJ%26ms%3Dnxu%26requiressl%3Dyes%26mt%3D1407581698%26itag%3D137%26sparams%3Dclen%252Ccp%252Cdur%252Cgir%252Cid%252Cip%252Cipbits%252Citag%252Clmt%252Crequiressl%252Csource%252Cupn%252Cexpire%26mm%3D30%26ip%3D117.222.223.185%26signature%3D64D2B755390DEB7B6E1E7BD2840AB69946F279C1.CEE9B089BFBFC08583AD7962D91D91AE2C463BC0%26source%3Dyoutube\u0026itag=137,index=235-365\u0026clen=5281776\u0026bitrate=1316961\u0026lmt=1406988184312555\u0026type=video%2Fwebm%3B+codecs%3D%22vp9%22\u0026init=0-234\u0026size=1920x1080\u0026url=https%3A%2F%2Fr1---sn-cvh7zn7z.c.youtube.com%2Fvideoplayback%3Flmt%3D1406988184312555%26key%3Dyt5%26fexp%3D901454%252C902408%252C927622%252C931983%252C934024%252C934030%252C946022%26sver%3D3%26gir%3Dyes%26expire%3D1407603844%26mws%3Dyes%26upn%3DswJfsAW6COc%26mv%3Du%26ipbits%3D8%26id%3Do-AB6PBmjGxBUzAy95o_3DDAGtZZsunDtsGaRo9mLMGCH8%26dur%3D39.960%26clen%3D5281776%26cp%3DU0lPTFZLUV9JTkNPOV9ORVVJOnNpdTd4dHpBbUlJ%26ms%3Dnxu%26requiressl%3Dyes%26mt%3D1407581698%26itag%3D248%26sparams%3Dclen%252Ccp%252Cdur%252Cgir%252Cid%252Cip%252Cipbits%252Citag%252Clmt%252Crequiressl%252Csource%252Cupn%252Cexpire%26mm%3D30%26ip%3D117.222.223.185%26signature%3D3C2F8A0D3137103B2BE5B51F960675CC9CE5CEEC.A84A1274927174AFB2AB260F79F319EA1EF87EFA%26source%3Dyoutube\u0026itag=248,index=711-838\u0026clen=3618877\u0026bitrate=1076065\u0026lmt=1407060579277365\u0026type=video%2Fmp4%3B+codecs%3D%22avc1.4d401f%22\u0026init=0-710\u0026size=1280x720\u0026url=https%3A%2F%2Fr1---sn-cvh7zn7z.c.youtube.com%2Fvideoplayback%3Flmt%3D1407060579277365%26key%3Dyt5%26fexp%3D901454%252C902408%252C927622%252C931983%252C934024%252C934030%252C946022%26sver%3D3%26gir%3Dyes%26expire%3D1407603844%26mws%3Dyes%26upn%3DswJfsAW6COc%26mv%3Du%26ipbits%3D8%26id%3Do-AB6PBmjGxBUzAy95o_3DDAGtZZsunDtsGaRo9mLMGCH8%26dur%3D39.921%26clen%3D3618877%26cp%3DU0lPTFZLUV9JTkNPOV9ORVVJOnNpdTd4dHpBbUlJ%26ms%3Dnxu%26requiressl%3Dyes%26mt%3D1407581698%26itag%3D136%26sparams%3Dclen%252Ccp%252Cdur%252Cgir%252Cid%252Cip%252Cipbits%252Citag%252Clmt%252Crequiressl%252Csource%252Cupn%252Cexpire%26mm%3D30%26ip%3D117.222.223.185%26signature%3D425D170B4DD076D650BB0D72DC06B6D187315539.44E9A77DBD7CB0FC02D0067A94385F7830969B4B%26source%3Dyoutube\u0026itag=136,index=235-365\u0026clen=2690636\u0026bitrate=692405\u0026lmt=1406988159038149\u0026type=video%2Fwebm%3B+codecs%3D%22vp9%22\u0026init=0-234\u0026size=1280x720\u0026url=https%3A%2F%2Fr1---sn-cvh7zn7z.c.youtube.com%2Fvideoplayback%3Flmt%3D1406988159038149%26key%3Dyt5%26fexp%3D901454%252C902408%252C927622%252C931983%252C934024%252C934030%252C946022%26sver%3D3%26gir%3Dyes%26expire%3D1407603844%26mws%3Dyes%26upn%3DswJfsAW6COc%26mv%3Du%26ipbits%3D8%26id%3Do-AB6PBmjGxBUzAy95o_3DDAGtZZsunDtsGaRo9mLMGCH8%26dur%3D39.960%26clen%3D2690636%26cp%3DU0lPTFZLUV9JTkNPOV9ORVVJOnNpdTd4dHpBbUlJ%26ms%3Dnxu%26requiressl%3Dyes%26mt%3D1407581698%26itag%3D247%26sparams%3Dclen%252Ccp%252Cdur%252Cgir%252Cid%252Cip%252Cipbits%252Citag%252Clmt%252Crequiressl%252Csource%252Cupn%252Cexpire%26mm%3D30%26ip%3D117.222.223.185%26signature%3D0C0554ACED375007D48F3C466477390085AFDD52.6539EC339BC511F0464E96BDC321EAEA6C0965DE%26source%3Dyoutube\u0026itag=247,index=710-837\u0026clen=1791833\u0026bitrate=545044\u0026lmt=1407060576811008\u0026type=video%2Fmp4%3B+codecs%3D%22avc1.4d401e%22\u0026init=0-709\u0026size=854x480\u0026url=https%3A%2F%2Fr1---sn-cvh7zn7z.c.youtube.com%2Fvideoplayback%3Flmt%3D1407060576811008%26key%3Dyt5%26fexp%3D901454%252C902408%252C927622%252C931983%252C934024%252C934030%252C946022%26sver%3D3%26gir%3Dyes%26expire%3D1407603844%26mws%3Dyes%26upn%3DswJfsAW6COc%26mv%3Du%26ipbits%3D8%26id%3Do-AB6PBmjGxBUzAy95o_3DDAGtZZsunDtsGaRo9mLMGCH8%26dur%3D39.921%26clen%3D1791833%26cp%3DU0lPTFZLUV9JTkNPOV9ORVVJOnNpdTd4dHpBbUlJ%26ms%3Dnxu%26requiressl%3Dyes%26mt%3D1407581698%26itag%3D135%26sparams%3Dclen%252Ccp%252Cdur%252Cgir%252Cid%252Cip%252Cipbits%252Citag%252Clmt%252Crequiressl%252Csource%252Cupn%252Cexpire%26mm%3D30%26ip%3D117.222.223.185%26signature%3D9B2F0993995F062F8394EB57F543F70691474854.73F1916FFC76469C9C25B64ADD52A2C25D090A86%26source%3Dyoutube\u0026itag=135,index=235-365\u0026clen=1348217\u0026bitrate=381768\u0026lmt=1406988149531287\u0026type=video%2Fwebm%3B+codecs%3D%22vp9%22\u0026init=0-234\u0026size=854x480\u0026url=https%3A%2F%2Fr1---sn-cvh7zn7z.c.youtube.com%2Fvideoplayback%3Flmt%3D1406988149531287%26key%3Dyt5%26fexp%3D901454%252C902408%252C927622%252C931983%252C934024%252C934030%252C946022%26sver%3D3%26gir%3Dyes%26expire%3D1407603844%26mws%3Dyes%26upn%3DswJfsAW6COc%26mv%3Du%26ipbits%3D8%26id%3Do-AB6PBmjGxBUzAy95o_3DDAGtZZsunDtsGaRo9mLMGCH8%26dur%3D39.960%26clen%3D1348217%26cp%3DU0lPTFZLUV9JTkNPOV9ORVVJOnNpdTd4dHpBbUlJ%26ms%3Dnxu%26requiressl%3Dyes%26mt%3D1407581698%26itag%3D244%26sparams%3Dclen%252Ccp%252Cdur%252Cgir%252Cid%252Cip%252Cipbits%252Citag%252Clmt%252Crequiressl%252Csource%252Cupn%252Cexpire%26mm%3D30%26ip%3D117.222.223.185%26signature%3D9BB9B21D63374916D987CDFB6D7D135BA5DDD422.944878C1AE389CA36DD1E0901C690605E8E9589A%26source%3Dyoutube\u0026itag=244,index=710-837\u0026clen=857989\u0026bitrate=263967\u0026lmt=1407060566758168\u0026type=video%2Fmp4%3B+codecs%3D%22avc1.4d401e%22\u0026init=0-709\u0026size=640x360\u0026url=https%3A%2F%2Fr1---sn-cvh7zn7z.c.youtube.com%2Fvideoplayback%3Flmt%3D1407060566758168%26key%3Dyt5%26fexp%3D901454%252C902408%252C927622%252C931983%252C934024%252C934030%252C946022%26sver%3D3%26gir%3Dyes%26expire%3D1407603844%26mws%3Dyes%26upn%3DswJfsAW6COc%26mv%3Du%26ipbits%3D8%26id%3Do-AB6PBmjGxBUzAy95o_3DDAGtZZsunDtsGaRo9mLMGCH8%26dur%3D39.921%26clen%3D857989%26cp%3DU0lPTFZLUV9JTkNPOV9ORVVJOnNpdTd4dHpBbUlJ%26ms%3Dnxu%26requiressl%3Dyes%26mt%3D1407581698%26itag%3D134%26sparams%3Dclen%252Ccp%252Cdur%252Cgir%252Cid%252Cip%252Cipbits%252Citag%252Clmt%252Crequiressl%252Csource%252Cupn%252Cexpire%26mm%3D30%26ip%3D117.222.223.185%26signature%3D86A0C9B892692BA85E5035DF44004794FD32FCCB.A6FED7B19A939DFDCCCE3255CA99DDC4966C58AD%26source%3Dyoutube\u0026itag=134,index=235-365\u0026clen=726599\u0026bitrate=200515\u0026lmt=1406988129350400\u0026type=video%2Fwebm%3B+codecs%3D%22vp9%22\u0026init=0-234\u0026size=640x360\u0026url=https%3A%2F%2Fr1---sn-cvh7zn7z.c.youtube.com%2Fvideoplayback%3Flmt%3D1406988129350400%26key%3Dyt5%26fexp%3D901454%252C902408%252C927622%252C931983%252C934024%252C934030%252C946022%26sver%3D3%26gir%3Dyes%26expire%3D1407603844%26mws%3Dyes%26upn%3DswJfsAW6COc%26mv%3Du%26ipbits%3D8%26id%3Do-AB6PBmjGxBUzAy95o_3DDAGtZZsunDtsGaRo9mLMGCH8%26dur%3D39.960%26clen%3D726599%26cp%3DU0lPTFZLUV9JTkNPOV9ORVVJOnNpdTd4dHpBbUlJ%26ms%3Dnxu%26requiressl%3Dyes%26mt%3D1407581698%26itag%3D243%26sparams%3Dclen%252Ccp%252Cdur%252Cgir%252Cid%252Cip%252Cipbits%252Citag%252Clmt%252Crequiressl%252Csource%252Cupn%252Cexpire%26mm%3D30%26ip%3D117.222.223.185%26signature%3D4849C1F7D0A9D328A9C37D44005363F419199C66.33714C0BCC72152E13927E4699F4DD839C5AA2C0%26source%3Dyoutube\u0026itag=243,index=675-802\u0026clen=1216250\u0026bitrate=256877\u0026lmt=1407061280695949\u0026type=video%2Fmp4%3B+codecs%3D%22avc1.4d4015%22\u0026init=0-674\u0026size=426x240\u0026url=https%3A%2F%2Fr1---sn-cvh7zn7z.c.youtube.com%2Fvideoplayback%3Flmt%3D1407061280695949%26key%3Dyt5%26fexp%3D901454%252C902408%252C927622%252C931983%252C934024%252C934030%252C946022%26sver%3D3%26gir%3Dyes%26expire%3D1407603844%26mws%3Dyes%26upn%3DswJfsAW6COc%26mv%3Du%26ipbits%3D8%26id%3Do-AB6PBmjGxBUzAy95o_3DDAGtZZsunDtsGaRo9mLMGCH8%26dur%3D39.921%26clen%3D1216250%26cp%3DU0lPTFZLUV9JTkNPOV9ORVVJOnNpdTd4dHpBbUlJ%26ms%3Dnxu%26requiressl%3Dyes%26mt%3D1407581698%26itag%3D133%26sparams%3Dclen%252Ccp%252Cdur%252Cgir%252Cid%252Cip%252Cipbits%252Citag%252Clmt%252Crequiressl%252Csource%252Cupn%252Cexpire%26mm%3D30%26ip%3D117.222.223.185%26signature%3D207E22B2695B4A2808A92C6CAAC0BE4D0DEEEE00.44709BCC00111AFE30A9FEE7426D89F3F129284E%26source%3Dyoutube\u0026itag=133,index=234-363\u0026clen=381811\u0026bitrate=102555\u0026lmt=1406988117738772\u0026type=video%2Fwebm%3B+codecs%3D%22vp9%22\u0026init=0-233\u0026size=426x240\u0026url=https%3A%2F%2Fr1---sn-cvh7zn7z.c.youtube.com%2Fvideoplayback%3Flmt%3D1406988117738772%26key%3Dyt5%26fexp%3D901454%252C902408%252C927622%252C931983%252C934024%252C934030%252C946022%26sver%3D3%26gir%3Dyes%26expire%3D1407603844%26mws%3Dyes%26upn%3DswJfsAW6COc%26mv%3Du%26ipbits%3D8%26id%3Do-AB6PBmjGxBUzAy95o_3DDAGtZZsunDtsGaRo9mLMGCH8%26dur%3D39.960%26clen%3D381811%26cp%3DU0lPTFZLUV9JTkNPOV9ORVVJOnNpdTd4dHpBbUlJ%26ms%3Dnxu%26requiressl%3Dyes%26mt%3D1407581698%26itag%3D242%26sparams%3Dclen%252Ccp%252Cdur%252Cgir%252Cid%252Cip%252Cipbits%252Citag%252Clmt%252Crequiressl%252Csource%252Cupn%252Cexpire%26mm%3D30%26ip%3D117.222.223.185%26signature%3DD63EA98BCA34C7FB523B283E0966DD833BF28379.B2203F3897AC82C1B8CD92D0E8AB8286C1E735E0%26source%3Dyoutube\u0026itag=242,index=672-799\u0026clen=540369\u0026bitrate=114216\u0026lmt=1407060538544413\u0026type=video%2Fmp4%3B+codecs%3D%22avc1.4d400c%22\u0026init=0-671\u0026size=256x144\u0026url=https%3A%2F%2Fr1---sn-cvh7zn7z.c.youtube.com%2Fvideoplayback%3Flmt%3D1407060538544413%26key%3Dyt5%26fexp%3D901454%252C902408%252C927622%252C931983%252C934024%252C934030%252C946022%26sver%3D3%26gir%3Dyes%26expire%3D1407603844%26mws%3Dyes%26upn%3DswJfsAW6COc%26mv%3Du%26ipbits%3D8%26id%3Do-AB6PBmjGxBUzAy95o_3DDAGtZZsunDtsGaRo9mLMGCH8%26dur%3D39.921%26clen%3D540369%26cp%3DU0lPTFZLUV9JTkNPOV9ORVVJOnNpdTd4dHpBbUlJ%26ms%3Dnxu%26requiressl%3Dyes%26mt%3D1407581698%26itag%3D160%26sparams%3Dclen%252Ccp%252Cdur%252Cgir%252Cid%252Cip%252Cipbits%252Citag%252Clmt%252Crequiressl%252Csource%252Cupn%252Cexpire%26mm%3D30%26ip%3D117.222.223.185%26signature%3DB0F9D951933ECE5870B88D7C368D35F48AB13BBA.D1138DEC11D05EB7422E39E612300172863FE3B2%26source%3Dyoutube\u0026itag=160,index=592-683\u0026clen=643755\u0026bitrate=145565\u0026lmt=1407060474261684\u0026type=audio%2Fmp4%3B+codecs%3D%22mp4a.40.2%22\u0026init=0-591\u0026url=https%3A%2F%2Fr1---sn-cvh7zn7z.c.youtube.com%2Fvideoplayback%3Flmt%3D1407060474261684%26key%3Dyt5%26fexp%3D901454%252C902408%252C927622%252C931983%252C934024%252C934030%252C946022%26sver%3D3%26gir%3Dyes%26expire%3D1407603844%26mws%3Dyes%26upn%3DswJfsAW6COc%26mv%3Du%26ipbits%3D8%26id%3Do-AB6PBmjGxBUzAy95o_3DDAGtZZsunDtsGaRo9mLMGCH8%26dur%3D40.054%26clen%3D643755%26cp%3DU0lPTFZLUV9JTkNPOV9ORVVJOnNpdTd4dHpBbUlJ%26ms%3Dnxu%26requiressl%3Dyes%26mt%3D1407581698%26itag%3D140%26sparams%3Dclen%252Ccp%252Cdur%252Cgir%252Cid%252Cip%252Cipbits%252Citag%252Clmt%252Crequiressl%252Csource%252Cupn%252Cexpire%26mm%3D30%26ip%3D117.222.223.185%26signature%3DD894482A8D078A88E1C9F49696BB5F7BA7167EA2.C9169F62F48F89769C14FF8F46D9691F827C00AB%26source%3Dyoutube\u0026itag=140,index=4452-4518\u0026clen=476486\u0026bitrate=115582\u0026lmt=1406988101266808\u0026type=audio%2Fwebm%3B+codecs%3D%22vorbis%22\u0026init=0-4451\u0026url=https%3A%2F%2Fr1---sn-cvh7zn7z.c.youtube.com%2Fvideoplayback%3Flmt%3D1406988101266808%26key%3Dyt5%26fexp%3D901454%252C902408%252C927622%252C931983%252C934024%252C934030%252C946022%26sver%3D3%26gir%3Dyes%26expire%3D1407603844%26mws%3Dyes%26upn%3DswJfsAW6COc%26mv%3Du%26ipbits%3D8%26id%3Do-AB6PBmjGxBUzAy95o_3DDAGtZZsunDtsGaRo9mLMGCH8%26dur%3D39.995%26clen%3D476486%26cp%3DU0lPTFZLUV9JTkNPOV9ORVVJOnNpdTd4dHpBbUlJ%26ms%3Dnxu%26requiressl%3Dyes%26mt%3D1407581698%26itag%3D171%26sparams%3Dclen%252Ccp%252Cdur%252Cgir%252Cid%252Cip%252Cipbits%252Citag%252Clmt%252Crequiressl%252Csource%252Cupn%252Cexpire%26mm%3D30%26ip%3D117.222.223.185%26signature%3D0B933DC31F9078C60AFA1824AF3B5BB91D14D0F0.A7440FC207C27D4464B3D4AD4DE3950D861EB382%26source%3Dyoutube\u0026itag=171\", \"focEnabled\": \"1\", \"enablecsi\": \"1\", \"pltype\": \"contentugc\", \"ptk\": \"youtube_none\", \"uid\": \"yvOJDBxhi1yqW97hXw3BDw\", \"tmi\": \"1\", \"video_id\": \"KDFDawjXsXg\", \"keywords\": \"new,latest,movie,Bollywood,Film,Indian,2013,update,behind,the,scene,Clip,Promo,trailers,Teasers,interview,stars,shooting,entertainment,Aamir Khan (Film Actor),Anushka Sharma (Award Winner),Sanjay Dutt (Film Actor),Boman Irani (Film Actor),Sushant Singh Rajput (Award Winner),Saurabh Shukla (Film Director),Rajkumar Hirani (Film Director),Vidhu Vinod Chopra (Film Director),Peekay (Film),Poster (Collection Category),Official,motion poster\", \"idpj\": \"-8\", \"c\": \"WEB\", \"ssl\": 1, \"account_playback_token\": \"QUFFLUhqbEk3a0ZYMmpDN3BZdGJNcFFBaUZrMkxXWjROd3xBQ3Jtc0tscUQwWF9lY3doWnh6LUVvNDZxV0EySFg0dFRlSE1OV2ZrSDBFOXgteFEwZVNadmNKUHVCMkw3SkN1TGxMY3YzV1BPa3pzeXZvczRYbUtUd1AtTkIyTTdoSUpEWWZKcHNnZWFPWVVEa2wwSHZld0NuYw==\", \"cbrver\": \"11.0\", \"iv3_module\": \"1\", \"loaderUrl\": \"https://www.youtube.com/watch?v=KDFDawjXsXg\", \"t\": \"1\", \"dashmpd\": \"https://www.youtube.com/api/manifest/dash/playback_host/r1---sn-cvh7zn7z.c.youtube.com/fexp/901454%2C902408%2C927622%2C931983%2C934024%2C934030%2C946022/mv/u/cmbypass/yes/ipbits/8/id/o-AB6PBmjGxBUzAy95o_3DDAGtZZsunDtsGaRo9mLMGCH8/cp/U0lPTFZLUV9JTkNPOV9ORVVJOnNpdTd4dHpBbUlJ/ms/nxu/requiressl/yes/mt/1407581698/itag/0/sparams/as%2Ccmbypass%2Ccp%2Cgcr%2Cid%2Cip%2Cipbits%2Citag%2Cplayback_host%2Crequiressl%2Csource%2Cexpire/mm/30/ip/117.222.223.185/key/yt5/sver/3/expire/1407603844/mws/yes/as/fmp4_audio_clear%2Cwebm_audio_clear%2Cfmp4_sd_hd_clear%2Cwebm_sd_hd_clear%2Cwebm2_sd_hd_clear/upn/g-7iQezqxL8/gcr/in/source/youtube/signature/9D60793C8CAC94902A6CBF97D78DE60683384C92.ED9AF14BD03E808222954A470FC7BF03CC3D9DF6\", \"remarketing_url\": \"https://googleads.g.doubleclick.net/pagead/viewthroughconversion/962985656/?cver=unknown\u0026ptype=view\u0026data=backend%3Ds_monetization_data%3Bcname%3DWEB%3Bcver%3Dunknown%3Bptype%3Dview%3Btype%3Dview%3Butuid%3DyvOJDBxhi1yqW97hXw3BDw%3Butvid%3DKDFDawjXsXg\u0026backend=s_monetization_data\u0026label=followon_view\u0026foc_id=yvOJDBxhi1yqW97hXw3BDw\u0026cname=WEB\u0026aid=P-qHlGrAeBw\", \"host_language\": \"en\", \"ucid\": \"UCyvOJDBxhi1yqW97hXw3BDw\", \"csi_page_type\": \"watch,watch7_html5\", \"storyboard_spec\": \"https://i.ytimg.com/sb/KDFDawjXsXg/storyboard3_L$L/$N.jpg|48#27#100#10#10#0#default#gRStUqBkx2yWEcyn03wADmBIMa4|80#45#41#10#10#1000#M$M#__3moJ-hvBXCrHt1szgrMYSXpAQ|160#90#41#5#5#1000#M$M#ZGvVeybKFNSO7xVj8HzYmkoJL0s|320#180#41#3#3#1000#M$M#VGN40S3l-I5nkvoWOxtWceE6s5Y\", \"vid\": \"KDFDawjXsXg\", \"no_get_video_log\": \"1\", \"fmt_list\": \"22/1280x720/9/0/115,18/640x360/9/0/115\", \"cos\": \"Windows\", \"hl\": \"en_US\", \"atc\": \"a=3\u0026b=5jKxXsYo5aBs5XFuwfARl7ZoSPM\u0026c=1407582244\u0026d=1\u0026e=KDFDawjXsXg\u0026c3a=16\u0026c1a=1\u0026hh=CB1Cj0aRtWVFz_azfWzFftkTLag\", \"cosver\": \"6.3\", \"title\": \"PK Official Motion Poster I Releasing December 19, 2014\", \"advideo\": \"1\", \"sw\": \"1.0\", \"aid\": \"P-KwzTuRh-E\", \"rmktEnabled\": \"1\", \"vq\": \"auto\", \"dash\": \"1\", \"iv_invideo_url\": \"https://www.youtube.com/annotations_invideo?cap_hist=1\u0026cta=2\u0026video_id=KDFDawjXsXg\"}, \"assets\": {\"js\": \"//s.ytimg.com/yts/jsbin/html5player-en_US-vflArxUZc/html5player.js\", \"css\": \"//s.ytimg.com/yts/cssbin/www-player-vflZFrHj6.css\", \"html\": \"/html5_player_template\"}, \"html5\": true, \"min_version\": \"8.0.0\", \"url\": \"https://s.ytimg.com/yts/swfbin/player-vfllVed9W/watch_as3.swf\", \"params\": {\"allowscriptaccess\": \"always\", \"allowfullscreen\": \"true\", \"bgcolor\": \"#000000\"}, \"sts\": 16289, \"url_v8\": \"https://s.ytimg.com/yts/swfbin/player-vfllVed9W/cps.swf\"};(function() {if (!!window.yt) {yt.player.Application.create(\"player-api\", ytplayer.config);ytplayer.config.loaded = true;}}());";
            return JObject.Parse(extractedJson);
        }

        private static void ThrowYoutubeParseException(Exception innerException, string videoUrl)
        {
            throw new YoutubeParseException("Could not parse the Youtube page for URL " + videoUrl + "\n" +
                                            "This may be due to a change of the Youtube page structure.\n" +
                                            "Please report this bug at www.github.com/flagbug/YoutubeExtractor/issues", innerException);
        }


        private class ExtractionInfo
        {
            public bool RequiresDecryption { get; set; }


            public Uri Uri { get; set; }
        }


    }
}
