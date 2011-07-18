using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BBSCore
{
    public class UploadWorker : WorkerBase, IWorker
    {

        public const int MaxFileSize = 1000 * 1000 * 1000;

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
            var fi = new FileInfo(Filepath);

            var isBitmap = fi.Extension.ToLower() == ".bmp";
            var isGif = fi.Extension.ToLower() == ".gif";

            var ext = fi.Extension;

            Stream stream = null;

            // 如果不是gif，以及尺寸超过上传上限，或者开启强制转换，或者后缀为bmp 则进行转换
            if (fi.Length > MaxFileSize && !isGif || !BBS.Config.UseRawSize || isBitmap)
            {
                stream = ImageHelper.Resize(fi.FullName, BBS.Config.ThumbWidth, System.Windows.Media.BitmapScalingMode.Fant);

                ext = ".jpg";
            }
            else
            {
                stream = new FileStream(fi.FullName, FileMode.Open);
            }

            using (stream)
            {
                stream.Position = 0;
                Url = BBS.Upload(stream, ext, Board, description: Description);
            }

            Finish();
        }
       
    }
}
