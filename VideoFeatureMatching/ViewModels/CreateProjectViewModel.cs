using System;
using System.Windows;
using System.Windows.Media;
using Emgu.CV;
using VideoFeatureMatching.DAL;
using VideoFeatureMatching.Utils;

namespace VideoFeatureMatching.ViewModels
{
    public class CreateProjectViewModel : BaseViewModel, IDisposable
    {
        private Capture _capture;
        private Mat _previewImageSource;

        public CreateProjectViewModel()
        {
            CvInvoke.UseOpenCL = false;
            _capture = new Capture();
            _capture.ImageGrabbed += CaptureOnImageGrabbed;
        }

        private void CaptureOnImageGrabbed(object sender, EventArgs eventArgs)
        {
            Mat frame = new Mat();
            _capture.Retrieve(frame, 0);

            PreviewImageSource = frame;
        }

        public ProjectFile<object> GetProjectFile()
        {
            // TODO implement
            return null;
        }

        public string VideoPath { get; set; }

        public Command OpenVideoCommand { get; set; }

        public Command StartCommand
        {
            get
            {
                return new Command(() =>
                {
                    _capture.Start();
                });
            }
        }

        public Command FinishCommand
        {
            get
            {
                return new Command(() =>
                {
                    _capture.Pause();
                });
            }
        }

        public Command ReadyCommand { get; set; }

        public Command CloseCommand { get; set; }

        public Mat PreviewImageSource
        {
            get { return _previewImageSource; }
            set
            {
                if (Equals(value, _previewImageSource)) return;
                _previewImageSource = value;
                RaisePropertyChanged();
            }
        }

        public double Progress { get; set; }

        public string ProgressText { get; set; }

        public void Dispose()
        {
            if (_capture != null)
            {
                _capture.Dispose();
                _capture = null;
            }
        }
    }
}