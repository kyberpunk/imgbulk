using System.Drawing;
using System.Drawing.Drawing2D;

namespace ImgBulk.ImageConverter
{
    class ImageSizeConverter : IImageConverter
    {
        private readonly int _size;
        private readonly bool _resizeSmaller;

        public ImageSizeConverter(int size, bool resizeSmaller)
        {
            _size = size;
            _resizeSmaller = resizeSmaller;
        }

        public Bitmap Convert(Bitmap image)
        {
            var sourceWidth = image.Width;
            var sourceHeight = image.Height;

            if (!_resizeSmaller && sourceWidth <= _size && sourceHeight <= _size)
            {
                return image;
            }

            int destWidth = 0;
            int destHeight = 0;

            if (sourceHeight > sourceWidth)
            {
                float ratio = (float) sourceHeight/(float) _size;
                destHeight = _size;
                destWidth = (int)((float)sourceWidth/ratio);
            }
            else
            {
                float ratio = (float)sourceWidth / (float)_size;
                destWidth = _size;
                destHeight = (int)((float)sourceHeight / ratio);
            }

            var b = new Bitmap(destWidth, destHeight);
            var g = Graphics.FromImage((Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(image, 0, 0, destWidth, destHeight);
            g.Dispose();

            return b;
        }
    }
}
