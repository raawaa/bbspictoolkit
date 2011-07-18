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
using System.Threading;
using BBSCore.Event;
using BBSCore;

namespace BBSPicUploader
{
    /// <summary>
    /// Interaction logic for Preview.xaml
    /// </summary>
    public partial class Preview : Window
    {
        private string _board;
        private string _title;
        private string _content;
        private PostWorker _worker;

        public Preview(string board, string title, string content)
        {
            InitializeComponent();

            _board = board;
            _title = title;
            _content = content;
        }

        private void btnPost_Click(object sender, RoutedEventArgs e)
        {
            _board = this.cmbBoard.Text.Trim();
            _title = this.txtTitle.Text.Trim();
            _content = this.txtContent.Text;

            LockUI();

            _worker = new PostWorker(_board, _title, _content);

            var thread = new Thread(new ThreadStart(_worker.Work));

            _worker.OnFinished += new WorkerBase.FinishHanlder(worker_OnFinished);

            thread.IsBackground = true;
            //thread.
            thread.Start();                        
        }

        private void LockUI()
        {
            this.cmbBoard.IsReadOnly = true;
            this.txtTitle.IsEnabled = false;
            this.txtContent.IsEnabled = false;
            this.btnPost.IsEnabled = false;
        }

        private void UnlockUI()
        {
            this.cmbBoard.IsReadOnly = false;
            this.txtTitle.IsEnabled = true;
            this.txtContent.IsEnabled = true;
            this.btnPost.IsEnabled = true;
        }

        void worker_OnFinished(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(new Action(OnPostFinished), null);
        }

        void OnPostFinished()
        {
            var result = MessageBox.Show("发贴成功，是否关闭窗口？", "提示", MessageBoxButton.OKCancel);

            if (result ==  MessageBoxResult.OK)
            {
                this.Close();
            }

            UnlockUI();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            this.Title += " v" + Helper.GetVersion();            

            this.cmbBoard.Text = _board;
            this.txtTitle.Text = _title;
            this.txtContent.Text = _content;

            this.cmbBoard.ItemsSource = ConfigManager.Boards;
        }
    }
}
