using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hammock;
using Hammock.Retries;
using Hammock.Web;
using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;

namespace BBSCore
{
    public static class BBS
    {
        public const int Generation = 3;
        public const int CoreVersion = 0;
        public static int UIVersion { get; private set; }

        public const string Authority = "http://bbs.sjtu.edu.cn/";
        public const string Referer = Authority;
        public const string UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/13.0.782.55 Safari/535.1";
        public const string LoginPath = "bbslogin";
        public const string UploadPath = "bbsdoupload";
        public const string SendPath = "bbssnd";

        static Regex UploadedUrlRegex = new Regex(@"\&data=(http\:\/\/bbs\.sjtu\.edu\.cn\/file\/[\w\.\/]+)");

        static NameValueCollection _cookies;

        static string Signature
        {
            get
            {
                return string.Format("by BBSPicUploader v{0}.{1}.{2}",
                    Generation, CoreVersion, UIVersion);
            }
        }

        public static void Init(int uiVersion)
        {
            UIVersion = uiVersion;
        }

        public static void TestRegex()
        {
            var match = UploadedUrlRegex.Match("FlashVars:\"text=复制&data=http://bbs.sjtu.edu.cn/file/test/1310878958210850.jpg\"};var attributes={id:\"clipboard\",style:\"margin:0 0 -5px 10px;\"};swfobject.embedSWF(\"/file/bbs/clip.swf\"");

            Console.WriteLine(match.Groups[1].Value);
        }

        /// <summary>
        /// 登录BBS，返回是否登录成功。
        /// </summary>
        /// <param name="username">用户名。</param>
        /// <param name="password">密码。</param>
        /// <returns></returns>
        public static bool Login(string username, string password)
        {
            var client = GetNewClient();
            var request = GetNewRequest(LoginPath, WebMethod.Post);
            request.AddHeader("Referer", Referer);

            request.AddField("id", username);
            request.AddField("pw", password);

            var response = client.Request(request);

            var content = response.Content;

            Console.WriteLine(content);

            var cookies = response.Cookies;
            var isOk = false;

            for (int i = 0; i < cookies.Keys.Count; i++)
            {
                var key = cookies.Keys[i];
                var value = cookies[key];

                if (key == "utmpuserid" && value == username)
                {
                    isOk = true;
                }

                Console.WriteLine("name: {0} value: {1}", key, value);
            }          

            if(isOk)
            {
                _cookies = new NameValueCollection(cookies);
            }

            return isOk;
        }

        /// <summary>
        /// 上传文件到指定的板面。
        /// </summary>
        /// <param name="filepath">要上传的文件路径</param>
        /// <param name="board">板面</param>
        /// <param name="star">星标|1～5（默认为1）</param>
        /// <param name="description">描述（默认为空）</param>        
        /// <param name="live">存活天数（默认180天）</param>
        /// <returns></returns>
        public static string Upload(string filepath, string board, int star = 1, string description = "", int live = 180)
        {
            var client = GetNewClient();
            var request = GetNewRequest(UploadPath, WebMethod.Post);
            
            request.AddHeader("Referer", "http://bbs.sjtu.edu.cn/bbsupload?board=" + board);

            var fi = new FileInfo(filepath);

            request.AddFile("up", fi.Name, fi.FullName);

            request.AddField("MAX_FILE_SIZE", "1048577");
            request.AddField("board", board);
            request.AddField("level", (star - 1).ToString());
            request.AddField("live", live.ToString());
            request.AddField("exp", description + " " + Signature);

            var response = client.Request(request);

            var content = response.GetGBContent();

            //var content = response.Content;

          //  Console.WriteLine(content);

            var match = UploadedUrlRegex.Match(content);

            if (match.Groups.Count == 2)
            {
                return match.Groups[1].Value;
            }

            return null;
        }

        /// <summary>
        /// 发表一个帖子。
        /// </summary>
        /// <param name="board">板面</param>
        /// <param name="title">标题</param>
        /// <param name="text">正文</param>
        /// <returns></returns>
        public static string Post(string board, string title, string text)
        {
            var client = GetNewClient();
            var request = GetNewRequest(SendPath, WebMethod.Post);

            text += string.Format("\r\n Posted {0} {1}", Signature, DateTime.Now.ToString());

            request.AddHeader("Referer", "http://bbs.sjtu.edu.cn/bbspst?board=" + board);

            request.AddField("board", board);
            request.AddField("title", title);
            request.AddField("signature", "1");
            request.AddField("autocr", "on");
            request.AddField("text", text);            
           
            var response = client.Request(request);

            var content = response.GetGBContent();            

         //   Console.WriteLine(content);            

            return content;
        }

        static RestClient GetNewClient()
        {
            var client = new RestClient();

            client.Authority = Authority;
            client.Encoding = Encoding.GetEncoding("GB2312");           
            client.Timeout = new TimeSpan(0, 5, 0);            

            return client;
        }

        static RestRequest GetNewRequest(string path, WebMethod method = WebMethod.Get)
        {
            var request = new RestRequest();
            request.Path = path;
            request.Method = method;
            request.Encoding = Encoding.GetEncoding("GB2312"); // ("GBK");          
            request.UserAgent = UserAgent;                       

            if (_cookies != null)
            {
                for (int i = 0; i < _cookies.Keys.Count; i++)
                {
                    var key = _cookies.Keys[i];
                    var value = _cookies[key];

                    request.AddCookie(new Uri(Authority), key, value);
                }
            }

            return request;
        }
    }
}
