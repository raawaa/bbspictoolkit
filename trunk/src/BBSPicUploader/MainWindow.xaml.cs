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
using System.Windows.Shapes;
using BBSCore;
using System.Threading;
using BBSCore.Event;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace BBSPicUploader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Regex _imgRegex = new Regex("[jpg|bmp|png|gif|jpeg]$", RegexOptions.IgnoreCase);

        private List<PicInfo> _pics;
        private Thread _workThread;
        private BBSUploader _worker;

        private string _lastPic;
        private bool _lastAutoPost;

        public MainWindow()
        {
            InitializeComponent();                    
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            SaveDescription();

            var board = this.cmbBoard.Text.Trim();

            if (string.IsNullOrEmpty(board))
            {
                System.Windows.Forms.MessageBox.Show("请填写板面");
                this.cmbBoard.Focus();
                return;
            }

            var title = this.txtTitle.Text.Trim();

            if (string.IsNullOrEmpty(title))
            {
                title = this.txtTitle.Text = "无标题";
            }

            var preface = this.txtPreface.Text.Trim();
            var summery = this.txtSummary.Text.Trim();

            _lastAutoPost = this.chkPost.IsChecked ?? false;

            ConfigManager.Config.AutoPost = _lastAutoPost;            

            _worker = new BBSUploader(board, title, _pics, _lastAutoPost, preface, summery);

            _worker.OnProgressChange += new BBSUploader.ProgressHanlder(bbsuploader_OnProgressChange);
            _worker.OnStateChanged += new BBSUploader.StateHandler(bbsuploader_OnStateChanged);
            _worker.OnFinished += new WorkerBase.FinishHanlder(bbsuploader_OnFinished);

            lockUI();

            _workThread = new Thread(new ThreadStart(_worker.Work));
            _workThread.IsBackground = true;
            _workThread.Start();
        }

        void lockUI()
        {
            this.txtDescription.IsEnabled = false;
            this.txtPreface.IsEnabled = false;
            this.txtSummary.IsEnabled = false;
            this.txtText.IsEnabled = false;
            this.cmbBoard.IsReadOnly = true;
            this.btnAdd.IsEnabled = false;
            this.btnAddFolder.IsEnabled = false;
            this.btnRemove.IsEnabled = false;
            this.btnUpload.IsEnabled = false;
            this.txtTitle.IsEnabled = false;
            this.btnNext.IsEnabled = false;
            this.btnPrev.IsEnabled = false;
            this.btnClear.IsEnabled = false;
            this.listPics.IsEnabled = false;
            this.chkPost.IsEnabled = false;
        }

        void UnlockUI()
        {
            this.txtDescription.IsEnabled = true;
            this.txtPreface.IsEnabled = true;
            this.txtSummary.IsEnabled = true;
            this.txtText.IsEnabled = true;
            this.cmbBoard.IsReadOnly = false;
            this.btnAdd.IsEnabled = true;
            this.btnAddFolder.IsEnabled = true;
            this.btnRemove.IsEnabled = true;
            this.btnUpload.IsEnabled = true;
            this.txtTitle.IsEnabled = true;
            this.btnNext.IsEnabled = true;
            this.btnPrev.IsEnabled = true;
            this.btnClear.IsEnabled = true;
            this.listPics.IsEnabled = true;
            this.chkPost.IsEnabled = true;
        }

        void ClearAll()
        {
            ClearData();
            ClearUI();
        }

        void ClearData()
        {
            _lastPic = string.Empty;
            _pics = new List<PicInfo>();
            _workThread = null;
        }

        void ClearUI()
        {
            this.txtTitle.Text = string.Empty;
            this.txtPreface.Text = string.Empty;
            this.txtSummary.Text = string.Empty;
            this.txtText.Text = string.Empty;
            this.txtDescription.Text = string.Empty;
            this.progressBar1.Value = 0;
            this.lblProgress.Content = string.Empty;
            this.listPics.Items.Clear();

            this.txtDescription.IsEnabled = this.txtText.IsEnabled = false;
        }

        void bbsuploader_OnFinished(object sender, EventArgs e)
        {
            // throw new NotImplementedException();
            this.Dispatcher.Invoke(new Action(OnFinished), null);
        }

        void OnFinished()
        {
            var result = System.Windows.MessageBox.Show("上传完成！", "BBSPicUploader");            
            
            ClearAll();                            

            UnlockUI();

            // 非自动发贴
            if (!_lastAutoPost)
            {
                var previewWindow = new Preview(_worker.Board, _worker.Title, _worker.FullContent);

                previewWindow.Show();
            }
        }



        void bbsuploader_OnStateChanged(object sender, BBSCore.Event.StateEventArgs e)
        {
         //   throw new NotImplementedException();
        }        

        void bbsuploader_OnProgressChange(object sender, ProgressEventArgs e)
        {
            //throw new NotImplementedException();
            this.Dispatcher.Invoke(new Action<ProgressEventArgs>(OnProgressChange), e);
        }

        void OnProgressChange(BBSCore.Event.ProgressEventArgs e)
        {
            this.progressBar1.Value = (int)e.Progress;
            this.lblProgress.Content = _worker.Finished + "/" + _worker.Total;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();

            openFileDialog.Multiselect = true;

            openFileDialog.Filter = "图像文件(*.jpg;*.gif;*.png;*.bmp;*.jpeg)|*.jpg;*.gif;*.png;*.jpeg;*.bmp|所有文件(*.*)|*.*";

            var result = openFileDialog.ShowDialog() ?? false;

            if (!result)
            {
                return;
            }

            var files = openFileDialog.FileNames;

            files.ToList().ForEach(f =>
            {
                AddNewPic(f);
            });
        }        

        private void AddNewPic(string fullpath)
        {
            var picInfo = _pics.FirstOrDefault(p => p.FullFilename.ToLower() == fullpath.ToLower());

            if (picInfo != null) return;

            _pics.Add(new PicInfo { FullFilename = fullpath });

            this.listPics.Items.Add(fullpath);
        }

        private void RemovePic(string fullpath)
        {
            var picInfo = _pics.FirstOrDefault(p => p.FullFilename.ToLower() == fullpath.ToLower());

            if (picInfo == null) return;

            _pics.Remove(picInfo);

            this.listPics.Items.Remove(fullpath);
        }

        private void btnAddFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog = new FolderBrowserDialog();

            var result = folderBrowserDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }

            AddFolder(folderBrowserDialog.SelectedPath);
        }

        private void AddFolder(string path)
        {
            var directoryInfo = new DirectoryInfo(path);

            var folders = directoryInfo.GetDirectories();

            folders.ToList().ForEach(di =>
                {
                    AddFolder(di.FullName);
                });

            var files = directoryInfo.GetFiles();

            files.ToList().ForEach(fi =>
            {
                if (_imgRegex.IsMatch(fi.Extension))
                {
                    AddNewPic(fi.FullName);
                }
            });
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.Forms.MessageBox.Show("确定要清空上传列表？", "提示", MessageBoxButtons.OKCancel);

            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }

            _pics.Clear();
            this.listPics.Items.Clear();
            UpdateImgPreview();
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            var item = this.listPics.SelectedItem;
            if(item == null)
            {
                return;
            }

            var fullpath = item.ToString();

            RemovePic(fullpath);

            UpdateImgPreview();
        }

        private void listPics_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveDescription();
            UpdateImgPreview();
        }

        private void UpdateImgPreview()
        {
            var item = this.listPics.SelectedItem;

            this.txtDescription.IsEnabled = (item != null);
            this.txtText.IsEnabled = (item != null);

            var index = this.listPics.SelectedIndex + 1;
            var total = _pics.Count();

            this.lblIndexAndCount.Content = index + "/" + total;

            if (item == null)
            {
                this.imgPreview.Source = null;
                this.txtDescription.Text = string.Empty;
                this.txtText.Text = string.Empty;
                return;
            }           

            var fullpath = item.ToString();

            var bitmapImage = new BitmapImage();

            var picInfo = GetPicInfo(fullpath);

            try
            {
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(fullpath);
                bitmapImage.EndInit();

                this.imgPreview.Source = bitmapImage;

                this.txtDescription.Text = picInfo.Description;
                this.txtText.Text = picInfo.Text;

                _lastPic = fullpath;                
            }
            catch
            {
            }

            this.txtDescription.Focus();
        }

        private void SaveDescription()
        {
            var description = this.txtDescription.Text;
            var text = this.txtText.Text;

            var picInfo = GetPicInfo(_lastPic);

            if (picInfo == null)
            {
                return;
            }

            picInfo.Description = description;
            picInfo.Text = text;                
        }

        private PicInfo GetPicInfo(string fullpath)
        {
            var picInfo = _pics.FirstOrDefault(p => p.FullFilename.ToLower() == fullpath.ToLower());

            return picInfo;
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            var selectedIndex = this.listPics.SelectedIndex;

            if (selectedIndex == -1 || selectedIndex == listPics.Items.Count - 1) return;

            this.listPics.SelectedIndex++;
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            var selectedIndex = this.listPics.SelectedIndex;

            if (selectedIndex == -1 || selectedIndex == 0) return;

            this.listPics.SelectedIndex--;
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.MessageBox.Show("是否要退出程序？", "退出", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                this.Close();
            }
        }

        private void MenuItem_AutoUpdate_Click(object sender, RoutedEventArgs e)
        {
            Helper.LunchAutoUpdate();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            this.Title += " v" + Helper.GetVersion();

            ClearAll();

            this.chkPost.IsChecked = ConfigManager.Config.AutoPost;

            this.cmbBoard.ItemsSource = ConfigManager.Boards;
        }        
    }
}
