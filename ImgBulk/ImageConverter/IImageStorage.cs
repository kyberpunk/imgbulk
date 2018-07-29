using System.Drawing;

namespace ImgBulk.ImageConverter
{
    interface IImageStorage
    {
        void Save(ImageFile file, Bitmap image);
    }
}
