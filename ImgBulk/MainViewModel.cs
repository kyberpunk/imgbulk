using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ImgBulk
{
    class MainViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        #region Properties

        private readonly ImageFiles _images;

        public ObservableCollection<ImageFile> Images
        {
            get { return _images; }
        }

        private bool _canConvert;

        public bool CanConvert
        {
            get
            {
                return _canConvert;
            }
            set
            {
                if (_canConvert == value) return;
                _canConvert = value;
                NotifyPropertyChanged();
            }
        }

        private bool _canClear;

        public bool CanClear
        {
            get
            {
                return _canClear;
            }
            set
            {
                if (_canClear == value) return;
                _canClear = value;
                NotifyPropertyChanged();
            }
        }

        public IEnumerable<ImageFile> SelectedImages { get; set; } 

        public ImageFormat[] ImageTypes { get; private set; }

        private bool _changeFormat;

        public bool ChangeFormat
        {
            get { return _changeFormat; }
            set
            {
                if (_changeFormat == value) return;
                _changeFormat = value;
                NotifyPropertyChanged();
                CheckCanConvert();
                if(!value)
                    SetQualityDefaultIfError();
            }
        }

        private ImageFormat _convertTo;

        public ImageFormat ConverTo
        {
            get { return _convertTo; }
            set
            {
                if (Equals(_convertTo, value)) return;
                _convertTo = value;
                NotifyPropertyChanged();
                if (!value.Equals(ImageFormat.Jpeg))
                {
                    SetQualityDefaultIfError();
                }
            }
        }

        private int _quality = 100;

        public string Quality
        {
            get
            {
                return _quality.ToString();
            }
            set
            {
                try
                {
                    var num = int.Parse(value);
                    if (_quality == num) return;
                    _quality = num;
                    NotifyPropertyChanged();
                    RemoveErrors();
                    if (num < 0 || num > 100)
                    {
                        AddError("Quality must be a number 0 - 100.");
                    }
                }
                catch (FormatException)
                {
                    Quality = "100";
                }
                
            }
        }

        private bool _changeSize;

        public bool ChangeSize
        {
            get { return _changeSize; }
            set
            {
                if (_changeSize == value) return;
                _changeSize = value;
                NotifyPropertyChanged();
                CheckCanConvert();
                if (!value)
                {
                    SetImageSizeDefaultIfError();
                }
            }
        }

        private int _imageSize = 640;

        public string ImageSize
        {
            get { return _imageSize.ToString(); }
            set
            {
                try
                {
                    var num = int.Parse(value);
                    if (_imageSize == num) return;
                    _imageSize = num;
                    NotifyPropertyChanged();
                    RemoveErrors();
                    if (num <= 0)
                    {
                        AddError("Image size must be greater than 0.");
                    }
                }
                catch (FormatException)
                {
                    ImageSize = "640";
                }
            }
        }

        public object[] DefaultSizes { get; set; }

        private object _imageSizeDefault;

        public object ImageSizeDefault
        {
            get
            {
                return _imageSizeDefault;
            }
            set
            {
                if (_imageSizeDefault == value) return;
                _imageSizeDefault = value;
                NotifyPropertyChanged();
                if (value is int)
                {
                    ImageSize = value.ToString();
                }
            }
        }

        private bool _resizeSmaller;

        public bool ResizeSmaller
        {
            get
            {
                return _resizeSmaller;
            }
            set
            {
                if (_resizeSmaller == value) return;
                _resizeSmaller = value;
                NotifyPropertyChanged();
            }
        }

        private string _outputPath;

        public string OutputPath
        {
            get
            {
                return _outputPath;
            }
            set
            {
                if (_outputPath == value) return;
                _outputPath = value;
                NotifyPropertyChanged();
                if (Directory.Exists(value))
                {
                    RemoveErrors();
                }
                else
                {
                    AddError("Output folder does not exist.");
                }
            }
        }

        private bool _overwrite;

        public bool Overwrite
        {
            get
            {
                return _overwrite;
            }
            set
            {
                if (_overwrite == value) return;
                _overwrite = value;
                NotifyPropertyChanged();
            }
        }

        public Dictionary<string, List<String>> Errors
        {
            get { return _errors; }
        }

        #endregion

        #region Commands

        private readonly RelayCommand _addImagesCommand;

        public ICommand AddImagesCommand
        {
            get { return _addImagesCommand; }
        }
        
        private readonly RelayCommand _removeImagesCommand;

        public ICommand RemoveImagesCommand
        {
            get { return _removeImagesCommand; }
        }

        private readonly RelayCommand _changeOutputCommand;

        public ICommand ChangeOutputCommand
        {
            get { return _changeOutputCommand; }
        }

        private readonly RelayCommand _clearCommand;

        public ICommand ClearCommand
        {
            get { return _clearCommand; }
        }

        #endregion

        public MainViewModel()
        {
            _images = new ImageFiles();
            _images.CollectionChanged += (sender, e) =>
            {
                CheckCanConvert();
                CheckCanClear();
            };
            ImageTypes = new[]
                {
                    ImageFormat.Jpeg,
                    ImageFormat.Png, 
                    ImageFormat.Tiff, 
                    ImageFormat.Gif, 
                    ImageFormat.Bmp, 
                };
            DefaultSizes = new object[] {640, 800, 1024, 1280, "Custom"};
            _errors = new Dictionary<string, List<string>>();
            _addImagesCommand = new RelayCommand(param => AddImages());
            _removeImagesCommand = new RelayCommand(param =>
            {
                var selectedItems = ((IEnumerable<object>) param).Cast<ImageFile>().ToList();
                RemoveImages(selectedItems);
            });
            _changeOutputCommand = new RelayCommand(param => ChangeOutput());
            _clearCommand = new RelayCommand(param => Clear());
            _outputPath = Directory.GetCurrentDirectory();
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void AddImages()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Title = "Select Images",
                Filter = "Image files|*.jpg;*.jpeg;*.png;*.tiff;*.gif;*.bmp"
            };
            var dialogResult = dialog.ShowDialog();
            if (dialogResult != true) return;
            AddImages(dialog.FileNames);
        }

        public void AddImages(string[] images)
        {
            if (images == null) throw new ArgumentNullException("images");
            foreach (var image in images
                .Where(path => _images.All(file => file.FilePath != path))
                .Select(fileName => new ImageFile(fileName))
                .Where(image => image.IsSupportedImage))
            {
                _images.Add(image);
            }
        }

        private void RemoveImages(IEnumerable<ImageFile> imageFiles)
        {
            foreach(var imageFile in imageFiles)
            {
                _images.Remove(imageFile);
            }
        }

        private void ChangeOutput()
        {
            var dialog = new CommonOpenFileDialog
            {
                Title = "Choose Folder",
                IsFolderPicker = true,
                InitialDirectory = _outputPath
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                OutputPath = dialog.FileName;
            }
        }

        private void CheckCanConvert()
        {
            CanConvert = (ChangeFormat || ChangeSize) && _images.Count > 0;
        }

        private void CheckCanClear()
        {
            CanClear = _images.Count > 0;
        }

        private readonly Dictionary<string, List<String>> _errors; 

        public IEnumerable GetErrors(string propertyName)
        {
            return _errors.ContainsKey(propertyName) ? _errors[propertyName] : null;
        }

        public bool HasErrors
        {
            get { return _errors.Keys.Count > 0; }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private void AddError(string message, [CallerMemberName] string propertyName = "")
        {
            List<string> errorList;
            _errors.TryGetValue(propertyName, out errorList);
            if (errorList == null)
            {
                errorList = new List<string>();
                _errors.Add(propertyName, errorList);
            }
            errorList.Add(message);
            if (ErrorsChanged != null) ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
        }

        private void RemoveErrors([CallerMemberName] string propertyName = "")
        {
            if (!_errors.Remove(propertyName)) return;
            if (ErrorsChanged != null) ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
        }

        private void SetQualityDefaultIfError()
        {
            if (GetErrors("Quality") != null)
            {
                Quality = "100";
            }
        }

        private void SetImageSizeDefaultIfError()
        {
            if (GetErrors("ImageSize") != null)
            {
                ImageSize = "640";
            }
        }

        private void Clear()
        {
            _images.Clear();
        }

        public ConversionProgress GetProgress()
        {
            return new ConversionProgress(_images, _changeFormat, _convertTo, _quality, _changeSize, _imageSize, _resizeSmaller, _overwrite, _outputPath);
        }
    }
}
