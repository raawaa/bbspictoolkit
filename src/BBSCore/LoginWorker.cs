using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BBSCore
{
    public class LoginWorker : WorkerBase, IWorker
    {        
        private string _username;
        private string _password;

        public bool LoginResult { get; private set; }

        public LoginWorker(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public void Work()
        {
            LoginResult = BBS.Login(_username, _password);

            Finish();
        }
    }
}
