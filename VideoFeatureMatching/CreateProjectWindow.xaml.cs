using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using VideoFeatureMatching.L10n;
using VideoFeatureMatching.ViewModels;

namespace VideoFeatureMatching
{
    /// <summary>
    /// Логика взаимодействия для CreateProjectWindow.xaml
    /// </summary>
    public partial class CreateProjectWindow : Window
    {
        public CreateProjectWindow()
        {
            InitializeComponent();
        }

        public CreateProjectViewModel ViewModel
        {
            get { return (CreateProjectViewModel) DataContext; }
        }

        private void OpenVideoFile_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = String.Format("{0}|{1}|{2}|{3}",
                Strings.AllVideoExtensions,
                Strings.VideoExtenstion1,
                Strings.VideoExtenstion2,
                Strings.VideoExtenstion3);

            var result = openFileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                ViewModel.VideoPath = openFileDialog.FileName;
            }
        }
    }
}
