using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace ImgBulk.ImageConverter
{
    class DefaultImageStorage : IImageStorage
    {
        private readonly ImageFormat _outputFormat;
        private readonly bool _overwrite;
        private readonly string _outputFolder;

        public DefaultImageStorage(string outputFolder, ImageFormat outputFormat, bool overwrite)
        {
            if (outputFolder == null) throw new ArgumentNullException("outputFolder");
            _outputFormat = outputFormat;
            _overwrite = overwrite;
            _outputFolder = outputFolder;
        }

        public virtual void Save(ImageFile file, Bitmap image)
        {
            var format = _outputFormat ?? file.ImageType;
            image.Save(GetFilePath(file, format), format);
        }

        protected string GetFilePath(ImageFile file, ImageFormat convertTo)
        {
            var fileName = file.FileNameWithoutExtension + GetImageExtension(convertTo);
            var filePath = _outputFolder + "\\" + fileName;
            while (File.Exists(filePath))
            {
                if (_overwrite)
                {
                    File.Delete(filePath);
                    break;
                }
                fileName = "copy_" + fileName;
                filePath = _outputFolder + "\\" + fileName;
            }
            return filePath;
        }

        private static string GetImageExtension(ImageFormat convertTo)
        {
            return ImageCodecInfo.GetImageEncoders()
                .First(c => c.FormatID == convertTo.Guid)
                .FilenameExtension
                .Split(';')
                .First()
                .TrimStart('*');
        }
    }
}
