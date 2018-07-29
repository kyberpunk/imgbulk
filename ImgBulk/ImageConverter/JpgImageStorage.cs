using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace ImgBulk.ImageConverter
{
    sealed class JpgImageStorage : DefaultImageStorage
    {
        private readonly ImageCodecInfo _encoder;
        private readonly EncoderParameters _encoderParameters;

        public JpgImageStorage(string outputFolder, int quality, bool overwrite)
            : base(outputFolder, ImageFormat.Jpeg, overwrite)
        {
            _encoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
            _encoderParameters = new EncoderParameters(1);
            _encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, Convert.ToInt64(quality));
        }

        public override void Save(ImageFile file, Bitmap image)
        {
            image.Save(GetFilePath(file, ImageFormat.Jpeg), _encoder, _encoderParameters);
        }
    }
}
