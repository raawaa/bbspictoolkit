using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BBSCore.Event;
using System.Threading;
using BBSCore.Enum;
using System.Diagnostics;
//using BBSCore.

namespace BBSCore
{
    public class BBSUploader : WorkerBase, IWorker
    {
        public const int MaxThreads = 4;

        public delegate void ProgressHanlder(object sender, ProgressEventArgs e);
        public delegate void StateHandler(object sender, StateEventArgs e);

        public event ProgressHanlder OnProgressChange;
        public event StateHandler OnStateChanged;

        private UploaderState _state;

        public UploaderState State
        {
            get { return _state; }
            private set
            {
                if (_state == value)
                {
                    return;
                }

                _state = value;

                if (OnStateChanged != null)
                {
                    OnStateChanged.Invoke(this, new StateEventArgs(_state));
                }
            }
        }

        public double Progress
        {
            get
            {
                if (_list == null || _list.Count == 0) return 0;

                return (double)Finished * 100 / _list.Count;
            }
        }

        private List<PicInfo> _list;
        private Queue<PicInfo> _queue;
        public int Finished { get; private set; }
        public int Total
        {
            get
            {
                return _list == null ? 0 : _list.Count;
            }
        }

        private Thread[] threads = new Thread[MaxThreads];

        public string Board { get; private set; }
        public string Title { get; private set; }
        public string Preface { get; private set; }
        public string Summmery { get; private set; }
        public bool AutoPost {get;private set;}

        public string FullContent { get; private set; }
       
        public BBSUploader(string board, string title, IList<PicInfo> pics, bool autoPost,string preface = null, string summery = null)
        {
            _state = UploaderState.Waiting;

            Board = board;
            Title = title;
            Preface = preface;
            Summmery = summery;
            AutoPost = autoPost;

            _list = new List<PicInfo>();
            _queue = new Queue<PicInfo>();

            pics.ToList().ForEach(p =>
            {
                _list.Add(p);
                _queue.Enqueue(p);
            });
        }


        public void Work()
        {
            this.State = UploaderState.Uploading;

            Upload();

            BuildFullContent();

            if (AutoPost)
            {
                this.State = UploaderState.Posting;

                Post();
            }

            this.State = UploaderState.Finished;

            Finish();
        }

        void Upload()
        {
            while (Finished < _list.Count)
            {                
                for (int i = 0; i < threads.Length && _queue.Count > 0 ; i++)
                {
                    var thread = threads[i];

                    var isAvaliable = false;

                    if (thread == null)
                    {
                        isAvaliable = true;
                    }
                    else if (thread.ThreadState == System.Threading.ThreadState.Aborted
                        || thread.ThreadState == System.Threading.ThreadState.Stopped)
                    {
                        threads[i] = null;
                        isAvaliable = true;
                    }

                    if (!isAvaliable)
                    {
                        continue;
                    }

                    var picInfo = _queue.Dequeue();

                    var worker = new UploadWorker(Board, picInfo.FullFilename, picInfo.Description);

                    worker.OnFinished += new FinishHanlder(worker_OnFinished);

                    thread = new Thread(new ThreadStart(worker.Work));

                    thread.Name = "Uploader Work " + picInfo.FullFilename;

                    thread.IsBackground = true;

                    threads[i] = thread;

                    Console.WriteLine("Thread{0} get work path = {1}", i, picInfo.FullFilename);

                    thread.Start();
                }

                Thread.Sleep(1000);
            }
        }

        void worker_OnFinished(object sender, EventArgs e)
        {
            var index = threads.ToList().IndexOf(Thread.CurrentThread);

            UploadWorker worker = sender as UploadWorker;

            var picInfo = _list.FirstOrDefault(p => p.FullFilename == worker.Filepath);

            picInfo.Url = worker.Url;

            lock (this)
            {
                Finished++;
            }

            if (OnProgressChange != null)
            {
                OnProgressChange.Invoke(this, new ProgressEventArgs(this.Progress));
            }

            worker.OnFinished -= worker_OnFinished;

            Console.WriteLine("Thread{0} Finished! url = {1}", index, picInfo.Url);
        }

        void BuildFullContent()
        {
             var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(Preface))
            {
                sb.AppendLine(Preface + "\r\n");
            }

            _list.ForEach(p =>
            {
                sb.AppendLine(p.Url);

                if (p.Text != null && p.Text.Trim() != string.Empty)
                {
                    sb.AppendLine(p.Text + "\r\n");
                }
                else if (p.Description != null && p.Description.Trim() != string.Empty)
                {
                    sb.AppendLine(p.Description + "\r\n");
                }
            });

            if (!string.IsNullOrEmpty(Summmery))
            {
                sb.AppendLine("\r\n" + Summmery);
            }

            FullContent = sb.ToString();
                      
        }

        void Post()
        {
            if (_list.Count > 0)
            {
                Title += " #" + _list.Count + "P";
            }

            BBS.Post(Board, Title, FullContent);
        }        
    }
}
