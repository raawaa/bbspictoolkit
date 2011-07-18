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

namespace BBSPicUploader.Update
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WebClient _webClient;

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

        private void CheckVersion(string xml)
        {
            var updateInfo = ParseUpdateInfo(xml);


            if (updateInfo.Version != Global.AppVer)
            {
                var result = MessageBox.Show("发现新版本，是否下载？", "自动更新", MessageBoxButton.OKCancel);

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
                this.Close();
            }
        }

        private void DownloadUpdateFile(string updateFileUrl)
        {
            _webClient = new WebClient();
            _webClient.Proxy = WebRequest.DefaultWebProxy; 

            var tempDir = AppDomain.CurrentDomain.BaseDirectory + "temp";

            var directoryInfp = new DirectoryInfo(tempDir);

            if (!directoryInfp.Exists)
            {
                directoryInfp.Create();
            }

            var filename = directoryInfp.FullName + "\\" + updateFileUrl.Substring(updateFileUrl.LastIndexOf("/") + 1);

            _webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
            _webClient.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(webClient_DownloadFileCompleted);
            _webClient.DownloadFileAsync(new Uri(updateFileUrl), filename);            
        }

        void webClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            UpdateFile();
        }

        private void UpdateFile()
        {
            if (!CloseMainApp()) return;
        }

        private bool CloseMainApp()
        {
            return true;
        }

        void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {           
            this.progressBar1.Value = e.ProgressPercentage;
            this.lblBytes.Content = string.Format("{0}KB/{1}KB", (e.BytesReceived / 1024.0).ToString("0.0"), (e.TotalBytesToReceive / 1024.0).ToString("0.0"));
        }

        private void DownloadUpdateInfo()
        {
            _webClient = new WebClient();
            _webClient.Proxy = GlobalProxySelection.GetEmptyWebProxy();

            _webClient.DownloadStringAsync(new Uri(Global.UpdateUrl));
            _webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(_webClient_DownloadUpdateInfoCompleted);

            //var xml = webClient.DownloadString(Global.UpdateUrl);

            //return xml;

            
        }

        void _webClient_DownloadUpdateInfoCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            CheckVersion(e.Result);
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

            _webClient.Dispose();

            this.Close();
        }

        

        
    }
}
