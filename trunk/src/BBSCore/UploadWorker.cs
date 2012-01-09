using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace BBSCore
{
    public class UploadWorker : WorkerBase, IWorker
    {

        public const int MaxFileSize = 1000 * 1000;

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
            var isGifOrPng = fi.Extension.ToLower() == ".gif" || fi.Extension.ToLower() == ".png";

            var ext = fi.Extension;

            Stream stream = null;
            
            var convert = false;

            //var image = Image.FromFile(Filepath);

            //var propItem = image.GetPropertyItem(20624);

            //if (propItem != null)
            //{ 
            //    propItem = 
            //}



            // 如果超过最大文件尺寸或者是BMP，则进行转换
            if (fi.Length > MaxFileSize || isBitmap)
            {
                convert = true;
            }
            // 如果是gif和PNG，没有超过尺寸，不转换
            else if(isGifOrPng)
            {
                convert = false;
            }
            // jpg，没有超过尺寸，如果没有指定不转换，则默认转换。
            else if (!BBS.Config.UseRawSize)
            {
                convert = true;
            }            

            if (convert)
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
                Url = BBS.Upload(stream, ext, Board, description: Description);
            }

            Finish();
        }
       
    }
}
