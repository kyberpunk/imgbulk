using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ImgBulk.ImageConverter;
using ImgBulk.Logging;

namespace ImgBulk
{
    public class ConversionProgress : INotifyPropertyChanged
    {
        private readonly ImageFiles _images;
        private readonly string _outputPath;
        private readonly IImageStorage _storage;
        private readonly IImageConverter _sizeConverter;
        private readonly ILog _log;
        private bool _hasErrors;

        #region Properties

        private readonly StringBuilder _statusText;

        public string StatusText
        {
            get { return _statusText.ToString(); }
            set { _statusText.AppendLine(value); }
        }

        private int _progress;

        public int Progress
        {
            get 
            {
                return _progress;
            }
            set
            {
                if (_progress == value) return;
                _progress = value;
                NotifyPropertyChanged("Progress");
            }
        }

        private int _progressCount = 100;

        public int ProgressCount
        {
            get
            {
                return _progressCount;
            }
            set
            {
                if (_progressCount == value) return;
                _progressCount = value;
                NotifyPropertyChanged("ProgressCount");
            }
        }

        private bool _isRunning;

        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                if (_isRunning == value) return;
                _isRunning = value;
                NotifyPropertyChanged("IsRunning");
            }
        }

        private bool _cancel;

        public bool CanCancel
        {
            get { return !_cancel; }
            set { _cancel = value; }
        }

        #endregion

        #region Commands

        private readonly RelayCommand _cancelCommand;

        public ICommand CancelCommand
        {
            get { return _cancelCommand; }
        }

        private readonly RelayCommand _openFolderCommand;

        public ICommand OpenFolderCommand
        {
            get { return _openFolderCommand; }
        }

        #endregion

        public void AddStatus(string status)
        {
            _statusText.AppendLine(status);
            NotifyPropertyChanged("StatusText");
            if (status != string.Empty)
                _log.Log(status, LogInfo.Info);
        }

        public void AddErrorStatus(Exception ex)
        {
            _statusText.AppendLine("Error occured: " + ex.Message);
            NotifyPropertyChanged("StatusText");
            _log.Log(ex, LogInfo.Error);
        }

        public ConversionProgress(ImageFiles images, bool changeFormat, ImageFormat convertTo, int quality, bool changeSize, int size, bool resizeSmaller, bool overwrite, string outputPath)
        {
            _images = images;
            _outputPath = outputPath;
            _statusText = new StringBuilder();
            _cancelCommand = new RelayCommand(param => Cancel());
            _openFolderCommand = new RelayCommand(param => OpenFolder());
            _log = new FileLog();
            if (changeSize)
            {
                _sizeConverter = new ImageSizeConverter(size, resizeSmaller);
            }
            if (changeFormat && convertTo.Equals(ImageFormat.Jpeg))
            {
                _storage = new JpgImageStorage(outputPath, quality, overwrite);
            }
            else
            {
                _storage = new DefaultImageStorage(outputPath, changeFormat ? convertTo : null, overwrite);
            }

        }

        public void Cancel()
        {
            _cancel = true;
            NotifyPropertyChanged("CanCancel");
        }

        public Task Run()
        {
            return Task.Run(() =>
            {
                ProgressCount = _images.Count;
                Progress = 0;
                _cancel = false;
                NotifyPropertyChanged("CanCancel");
                IsRunning = true;
                var count = _images.Count;
                AddStatus("Converting " + count + " images started");
                foreach (var image in _images.ToList())
                {
                    if (_cancel)
                    {
                        AddStatus("Converting canceled");
                        break;
                    }
                    try
                    {
                        AddStatus("Loading image: " + image.FileName);
                        var bytes = File.ReadAllBytes(image.FilePath);
                        using (var ms = new MemoryStream(bytes))
                        {
                            var bitmap = new Bitmap(ms);
                            AddStatus("Image loaded");
                            if (_sizeConverter != null)
                            {
                                AddStatus("Resizing image");
                                bitmap = _sizeConverter.Convert(bitmap);
                            }
                            AddStatus("Saving image");
                            _storage.Save(image, bitmap);
                            AddStatus("Image Saved");
                        }
                        Progress++;
                    }
                    catch (Exception ex)
                    {
                        AddErrorStatus(ex);
                        _hasErrors = true;
                    }
                    AddStatus(string.Empty);
                }
                AddStatus("Converting images ended");
                AddStatus(Progress + " images succesfully converted");
                AddStatus(count - Progress + " images failed");
                if (_hasErrors)
                {
                    AddStatus("Some errors occured! Please check log for more info");
                }
                IsRunning = false;
                _cancel = true;
                _hasErrors = false;
                NotifyPropertyChanged("CanCancel");
                _log.Save();
            });
        }

        private void OpenFolder()
        {
            if (Directory.Exists(_outputPath))
                Process.Start(_outputPath);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
