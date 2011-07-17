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
using BBSCore;
using System.Threading;

namespace BBSPicUploader
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        Thread _loginThread;
        LoginWorker _loginWorker;

        public LoginWindow()
        {           
            InitializeComponent();


            this.Loaded += new RoutedEventHandler(LoginWindow_Loaded);

            //BBSPicUploader.BBS.Init(1);

            BBSCore.BBS.Init(1);

          //  this.cmbUsername.Focus();
        }

        void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.cmbUsername.Focus();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            TryLogin();            
        }

        private void TryLogin()
        {
            if (_loginThread != null)
            {
                _loginThread.Abort();
                _loginThread = null;
            }

            var username = this.cmbUsername.Text;
            var password = this.txtPassword.Password;

            _loginWorker = new LoginWorker(username, password);
            _loginWorker.OnFinished += new LoginWorker.FinishHanlder(_loginWorker_FinishEvent);

            _loginThread = new Thread(new ThreadStart(_loginWorker.Work));

            _loginThread.IsBackground = true;

            _loginThread.Start();

            this.cmbUsername.IsReadOnly = true;
            this.txtPassword.IsEnabled = false;
            this.btnLogin.IsEnabled = false;

            this.btnLogin.Content = "登录中";        
        }

        void _loginWorker_FinishEvent(object sender, EventArgs e)
        {
            this.btnLogin.Dispatcher.Invoke(new Action(
               delegate()
               {
                   updateLoginButton(_loginWorker.LoginResult);
               }), null);
        }

        void updateLoginButton(bool isLogined)
        {
            if (isLogined)
            {
               this.btnLogin.Content = "已登录";

               var mainWindow = new MainWindow();

               mainWindow.Show();

               this.Close();
            }
            else
            {
                this.cmbUsername.IsReadOnly = false;
                this.txtPassword.IsEnabled = true;
               this.btnLogin.IsEnabled = true;
               MessageBox.Show("登录失败！");
            }
        }

        private void txtPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TryLogin();
            }
        }
    }
}
