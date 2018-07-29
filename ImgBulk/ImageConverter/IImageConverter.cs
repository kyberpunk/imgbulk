using System.Drawing;

namespace ImgBulk.ImageConverter
{
    interface IImageConverter
    {
        Bitmap Convert(Bitmap image);
    }
}
