using System;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace ImgBulk
{
    public class ImageFile : INotifyPropertyChanged
    {
        private readonly string _filePath;
        private readonly string _fileName;
        private readonly string _fileNameWithoutExtension;

        public string FileName
        {
            get { return _fileName; }
        }

        public string FilePath
        {
            get { return _filePath; }
        }

        public string FileNameWithoutExtension
        {
            get { return _fileNameWithoutExtension; }
        }

        private ImageFormat _imageType;

        public ImageFormat ImageType
        {
            get
            {
                return _imageType;
            }
            set
            {
                _imageType = value;
                IsSupportedImage = value != null;
            }
        }

        public bool IsSupportedImage { get; private set; }

        public int PixelWidth { get; private set; }

        public int PixelHeight { get; private set; }

        public ImageFile(string filePath)
        {
            _filePath = filePath;
            _fileName = Path.GetFileName(filePath);
            _fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);
            if (extension == null) return;
            switch (extension.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    ImageType = ImageFormat.Jpeg;
                    break;
                case ".png":
                    ImageType = ImageFormat.Png;
                    break;
                case ".tiff":
                    ImageType = ImageFormat.Tiff;
                    break;
                case ".gif":
                    ImageType = ImageFormat.Gif;
                    break;
                case ".bmp":
                    ImageType = ImageFormat.Bmp;
                    break;
            }
            try
            {
                var bitmapFrame = BitmapFrame.Create(new Uri(filePath), BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
                PixelWidth = bitmapFrame.PixelWidth;
                PixelHeight = bitmapFrame.PixelHeight;
            }
            catch (Exception)
            {
                IsSupportedImage = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public override bool Equals(object obj)
        {
            var file = obj as ImageFile;
            if (file != null)
            {
                return file.FilePath == FilePath;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return FilePath.GetHashCode();
        }
    }
}
