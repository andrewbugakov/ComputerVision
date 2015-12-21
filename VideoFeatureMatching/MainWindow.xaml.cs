using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using VideoFeatureMatching.ViewModels;

namespace VideoFeatureMatching
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            ViewModel = new MainViewModel();

            InitializeComponent();
        }

        public MainViewModel ViewModel
        {
            get { return (MainViewModel) DataContext; }
            set { DataContext = value; }
        }

        private void VideoPlayer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var frameworkElement = (FrameworkElement) sender;
            var coordinates = e.GetPosition(frameworkElement);

            var x = coordinates.X/frameworkElement.ActualWidth;
            var y = coordinates.Y/frameworkElement.ActualHeight;

            ViewModel.ShowPointInformation(x, y);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (ViewModel.SaveExistingProjectIfNeeded(window) == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            base.OnClosing(e);
        }
    }
}
