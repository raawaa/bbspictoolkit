using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BBSCore
{
    public class UploadWorker : WorkerBase, IWorker
    {

        public string Board { get; private set; }
        public string Filepath { get; private set; }
        public string Description { get; private set; }
        public string Url { get; private set; }

        public UploadWorker(string board, string filepath, string description)
        {
            Board = board;
            Filepath = filepath;
            Description = description;
        }

        public void Work()
        {
            Url = BBS.Upload(Filepath, Board, description: Description);

            Finish();
        }
    }
}
