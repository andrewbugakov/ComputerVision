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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Emgu.CV;
using Emgu.CV.CvEnum;
using VideoFeatureMatching.Utils;
using VideoFeatureMatching.ViewModels;

namespace VideoFeatureMatching
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            CvInvoke.UseOpenCL = false;
            ViewModel = new MainViewModel();

            InitializeComponent();

//            Closing += (sender, args) =>
//            {
//                ViewModel.ExitCommmad.Execute();
//            };
        }

        public MainViewModel ViewModel
        {
            get { return (MainViewModel) DataContext; }
            set { DataContext = value; }
        }
    }
}
