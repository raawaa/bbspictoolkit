﻿using System;
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
using BBSCore.Extensions;

namespace BBSPicUploader
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        Thread _loginThread;
        LoginWorker _loginWorker;
        string _username;
        string _password;

        public LoginWindow()
        {           
            InitializeComponent();

            this.Title += " v" + Helper.GetVersion();

            this.Loaded += new RoutedEventHandler(LoginWindow_Loaded);                      
        }

        void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.cmbUsername.Focus();
            this.cmbUsername.Text = ConfigManager.Config.Username;
            this.txtPassword.Password = ConfigManager.Config.Password.Decode();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            TryLogin();            
        }

        private void TryLogin()
        {
            _username = this.cmbUsername.Text.Trim();
            _password = this.txtPassword.Password;

            if (string.IsNullOrEmpty(_username))
            {
                MessageBox.Show("请输入帐号");
                this.cmbUsername.Focus();
                return;
            }
            if (string.IsNullOrEmpty(_password))
            {
                MessageBox.Show("请输入密码");
                this.txtPassword.Focus();
                return;
            }
            

            if (_loginThread != null)
            {
                _loginThread.Abort();
                _loginThread = null;
            }            

            _loginWorker = new LoginWorker(_username, _password);
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

                ConfigManager.Config.Username = _username;
                ConfigManager.Config.Password = _password.Encode();

                ConfigManager.SaveConfig();

                var mainWindow = new MainWindow();

                mainWindow.Show();

                this.Close();
            }
            else
            {
                this.cmbUsername.IsReadOnly = false;
                this.txtPassword.IsEnabled = true;
                this.btnLogin.IsEnabled = true;
                this.btnLogin.Content = "登录";
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

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                var mainWindow = new MainWindow();

                mainWindow.Show();

                this.Close();
            }
        }
    }
}
