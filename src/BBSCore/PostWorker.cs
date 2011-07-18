using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BBSCore
{
    public class PostWorker:WorkerBase, IWorker
    {
        private string _board;
        private string _title;
        private string _content;

        public PostWorker(string board, string title, string content)
        {
            _board = board;
            _title = title;
            _content = content;
        }

        public void Work()
        {
            BBS.Post(_board, _title, _content);

            Finish();
        }
    }
}
