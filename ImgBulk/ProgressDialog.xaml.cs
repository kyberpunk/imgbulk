using System.Windows;
using System.Windows.Data;

namespace ImgBulk
{
    /// <summary>
    /// Interaction logic for ProgressDialog.xaml
    /// </summary>
    public partial class ProgressDialog
    {
        private readonly ConversionProgress _progress;

        public static readonly DependencyProperty StatusTextProperty = DependencyProperty.Register(
            "StatusText", typeof (string), typeof (FrameworkElement), new FrameworkPropertyMetadata(OnStatusTextChangedCallback));

        private static void OnStatusTextChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var progressDialog = ((ProgressDialog) d);
            progressDialog.StatusBox.Text = e.NewValue.ToString();
            progressDialog.StatusScroll.ScrollToBottom();
        }

        public string StatusText
        {
            get { return (string)GetValue(StatusTextProperty); }
            set { SetValue(StatusTextProperty, value); }
        }
        
        public ProgressDialog(ConversionProgress progress)
        {
            InitializeComponent();
            _progress = progress;
            DataContext = progress;
            SetBinding(StatusTextProperty, new Binding("StatusText") {Source = progress});
            this.Closing += ProgressDialog_Closing;
        }

        void ProgressDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = _progress.CanCancel;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            RunProgress();
        }

        private async void RunProgress()
        {
            await _progress.Run();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
