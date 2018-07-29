using System.Linq;
using System.Windows;

namespace ImgBulk
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (MainViewModel)DataContext;
            if ((viewModel.ChangeFormat || viewModel.ChangeSize) && viewModel.Images.Count > 0)
            {
                if (viewModel.Errors.Values.Count > 0)
                {
                    MessageBox.Show(viewModel.Errors.Values.First().First());
                    return;
                }
                var progress = viewModel.GetProgress();
                var dialog = new ProgressDialog(progress) {Owner = this};
                dialog.ShowDialog();
            }
        }

        private void ImageListBox_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
        }

        private void ImageListBox_Drop(object sender, DragEventArgs e)
        {
            var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
            ((MainViewModel)DataContext).AddImages(files);
        }
    }
}
