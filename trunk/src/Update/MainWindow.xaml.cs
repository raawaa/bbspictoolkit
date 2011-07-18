using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Threading;
using System.ComponentModel;
using System.Diagnostics;

namespace BBSPicUploader.Update
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WebClient _webClient;
        private Thread _thread;
        private string _updateFilename;
        private bool _isForece;

        public MainWindow()
        {
            InitializeComponent();       
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            CheckUpdate();
        }

        private void CheckUpdate()
        {
            this.lblBytes.Content = "正在检查新版本...";

            DownloadUpdateInfo();            
        }

        private void DownloadUpdateInfo()
        {
            CleanThread();

            _thread = new Thread(new ThreadStart(new Action(delegate()
            {
                _webClient = new WebClient();
                _webClient.Proxy = WebRequest.DefaultWebProxy;
                _webClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                _webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(_webClient_DownloadUpdateInfoCompleted);
                _webClient.DownloadStringAsync(new Uri(Global.UpdateUrl));
            })));

            _thread.IsBackground = true;
            _thread.Start();
        }

        private void CheckVersion(string xml)
        {
            var updateInfo = ParseUpdateInfo(xml);


            if (updateInfo.Version != Global.AppVer)
            {
                var result = MessageBox.Show("发现新版本" + updateInfo.Version + "，是否下载？", "自动更新", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    DownloadUpdateFile(updateInfo.UpdateFileUrl);
                }
                else
                {
                    this.Close();
                }

            }
            else
            {
                MessageBox.Show("当前程序为最新版本" + updateInfo.Version, "自动更新");
                this.Close();
            }
        }

        private void DownloadUpdateFile(string updateFileUrl)
        {            
            var tempDir = AppDomain.CurrentDomain.BaseDirectory + "temp";

            var directoryInfp = new DirectoryInfo(tempDir);

            if (!directoryInfp.Exists)
            {
                directoryInfp.Create();
            }

            this.lblBytes.Content = "正在下载更新文件...";

            _updateFilename = directoryInfp.FullName + "\\" + updateFileUrl.Substring(updateFileUrl.LastIndexOf("/") + 1);

            CleanThread();

            _thread = new Thread(new ThreadStart(new Action(delegate()
            {
                _webClient = new WebClient();
                _webClient.Proxy = WebRequest.DefaultWebProxy;

                _webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
                _webClient.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(webClient_DownloadFileCompleted);
                _webClient.DownloadFileAsync(new Uri(updateFileUrl), _updateFilename);            
            })));

            _thread.IsBackground = true;
            _thread.Start();
        }

        void webClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action<AsyncCompletedEventArgs>(OnUpdateFileDownloaded), e);
        }

        void OnUpdateFileDownloaded(AsyncCompletedEventArgs e)
        {
            if (e.Cancelled == true || e.Error != null)
            {
                MessageBox.Show("下载失败！", "自动更新");

                _webClient.Dispose();

                this.Close();
            }
            else
            {
                _webClient.Dispose();
                _webClient = null;

                UpdateFile();
            }
        }        

        private void UpdateFile()
        {
            if (!CloseMainApp()) return;

            this.lblBytes.Content = "正在更新文件...";

            UnZip(_updateFilename, AppDomain.CurrentDomain.BaseDirectory);

            MessageBox.Show("更新已完成！", "自动更新");

            Process.Start("BBSPicUploader.exe");

            Close();
        }

        static void UnZip(string zipFile, string destFolder)
        {
            Shell32.ShellClass sc = new Shell32.ShellClass();
            Shell32.Folder SrcFolder = sc.NameSpace(zipFile);
            Shell32.Folder DestFolder = sc.NameSpace(destFolder);
            Shell32.FolderItems items = SrcFolder.Items();
            DestFolder.CopyHere(items, 20);
        }

        private bool CloseMainApp()
        {
            System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName(Global.MainAppName);

            if (processes.Length > 0)
            {
                var result = MessageBox.Show("检测到主程序正在运行，是否关闭主程序？", "自动更新", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    foreach (var process in processes)
                    {
                        process.Kill();
                    }

                    return true;
                }
                else
                {                    
                    this.Close();
                    return false;
                }
            }

            return true;
        }

        void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action<DownloadProgressChangedEventArgs>(updateDownloadProgress), e);
        }

        void updateDownloadProgress(DownloadProgressChangedEventArgs e)
        {
            this.progressBar1.Value = e.ProgressPercentage;
            this.lblBytes.Content = string.Format("{0}KB/{1}KB", (e.BytesReceived / 1024.0).ToString("0.0"), (e.TotalBytesToReceive / 1024.0).ToString("0.0"));
        }



        private void CleanThread()
        {
            if (_thread != null)
            {
                _thread.Abort();
                _thread = null;
            }
        }

        void _webClient_DownloadUpdateInfoCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            _webClient.Dispose();
            _webClient = null;

            this.Dispatcher.Invoke(new Action<string>(CheckVersion), e.Result);            
        }

        private UpdateInfo ParseUpdateInfo(string xml)
        {

            UpdateInfo updateInfo = null;

            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(xml)))
            {

                var serializer = new XmlSerializer(typeof(UpdateInfo));

                updateInfo = (UpdateInfo)serializer.Deserialize(ms);                
            }

            return updateInfo;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_webClient == null)
            {
                this.Close();
            }

            try
            {
                _webClient.Dispose();
                _webClient = null;
            }
            catch
            {

            }

            this.Close();
        }

        

        
    }
}
