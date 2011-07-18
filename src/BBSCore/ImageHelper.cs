using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace BBSCore
{
    public class ImageHelper
    {
        private static object lockObject = new object();

        public static Stream Resize(string photoPath, int maxWidth, BitmapScalingMode scalingMode)
        {
            lock (lockObject)
            {
                var s = DateTime.Now;

                var fileInfo = new FileInfo(photoPath);

                var photoDecoder = BitmapDecoder.Create(new Uri(photoPath), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);
                var photo = photoDecoder.Frames[0];

                double width = photo.PixelWidth;
                double height = photo.PixelHeight;

                if (photo.PixelWidth > maxWidth)
                {
                    var a = (double)maxWidth / photo.PixelWidth;

                    width = maxWidth;

                    height *= a;
                }

                var group = new DrawingGroup();
                RenderOptions.SetBitmapScalingMode(
                    group, scalingMode);
                group.Children.Add(
                    new ImageDrawing(photo, new System.Windows.Rect(0, 0, (int)width, (int)height)));
                var targetVisual = new DrawingVisual();
                var targetContext = targetVisual.RenderOpen();
                targetContext.DrawDrawing(group);
                var target = new RenderTargetBitmap(
                    (int)width, (int)height, 96, 96, PixelFormats.Default);
                targetContext.Close();
                target.Render(targetVisual);
                var targetFrame = BitmapFrame.Create(target);

                var memoryStream = new MemoryStream();

                var targetEncoder = new JpegBitmapEncoder();
                targetEncoder.Frames.Add(targetFrame);
                targetEncoder.Save(memoryStream);

                Console.WriteLine("path: {0} org: {1}*{2} target: {3}*{4} cost:{5}ms size:{6}KB->{7}KB",
                    photoPath, photo.PixelWidth, photo.PixelHeight, width, height, (DateTime.Now - s).TotalMilliseconds.ToString("0.00"),
                    (int)fileInfo.Length / 1024, (int)memoryStream.Length / 1024);

                return memoryStream;
            }
        }

        private void Test()
        {            
        }
    }
}
