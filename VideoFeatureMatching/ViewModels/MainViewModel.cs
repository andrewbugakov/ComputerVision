using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using VideoFeatureMatching.Core;
using VideoFeatureMatching.DAL;
using VideoFeatureMatching.L10n;
using VideoFeatureMatching.Utils;
using Point = System.Drawing.Point;

namespace VideoFeatureMatching.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // Stopwatch needs to make fps to real
        private readonly Stopwatch _stopwatch;
        private readonly VideoCloudPointsFileAccessor _baseFileAccessor;

        private ProjectFile<VideoCloudPoints> _projectFile;

        private Capture _capture;
        private string _progressTime;
        private PlayerStates _playerState;
        private double _progress;
        private IImage _videoImageSource;
        private string _pointInformation;
        private IImage _originImage;
        private string _videoInformation;

        public MainViewModel()
        {
            CvInvoke.UseOpenCL = false;
            _playerState = PlayerStates.Stopped;
            _stopwatch = new Stopwatch();
            _baseFileAccessor = new VideoCloudPointsFileAccessor();
        }

        #region General Properties

        public string PageTitle
        {
            get
            {
                if (!IsProjectOpened)
                {
                    return Strings.ProgramName;
                }

                return "[" + (_projectFile.Path ?? Strings.Filename_untittled) + (!IsProjectSaved ? "*" : "") + "]";
            }
        }

        public Command OpenAboutCommmand { get { return new Command(() => new About().Show()); } }

        public Command<Window> ExitCommand { get { return new Command<Window>(window => window.Close()); } }

        #endregion

        #region Project handlers

        public bool IsProjectOpened { get { return _projectFile != null; } }
        public bool IsProjectSaved { get { return _projectFile != null && _projectFile.IsSaved; } }

        private void OpenProject(ProjectFile<VideoCloudPoints> projectFile)
        {
            _projectFile = projectFile;

            RaisePropertyChanged("IsProjectOpened");
            RaisePropertyChanged("IsProjectSaved");

            RaisePropertyChanged("SaveProjectCommand");
            RaisePropertyChanged("SaveAsProjectCommand");
            RaisePropertyChanged("CloseProjectCommand");

            _capture = new Capture(projectFile.Model.VideoPath);
            _capture.ImageGrabbed += CaptureOnImageGrabbed;

            VideoInformation = GetVideoInformation(_capture);
            Progress = 0;
            RaisePropertyChanged("PlayPauseCommand");
            RaisePropertyChanged("StopCommand");
        }

        private void CloseProject(Window window)
        {
            if (SaveExistingProjectIfNeeded(window) == MessageBoxResult.Cancel)
                return;

            _projectFile = null;
            RaisePropertyChanged("IsProjectOpened");
            RaisePropertyChanged("IsProjectSaved");
            
            RaisePropertyChanged("SaveProjectCommand");
            RaisePropertyChanged("SaveAsProjectCommand");
            RaisePropertyChanged("CloseProjectCommand");

            Stop();
            _capture.ImageGrabbed -= CaptureOnImageGrabbed;
            _capture = null;

            RaisePropertyChanged("PlayPauseCommand");
            RaisePropertyChanged("StopCommand");

            VideoInformation = String.Empty;
        }

        #endregion

        #region File System commands

        public ICommand CreateProjectCommand { get { return new Command<Window>(CreateProject); } }
        public ICommand SaveProjectCommand { get { return new Command(SaveProject, () => IsProjectOpened && !IsProjectSaved); } }
        public ICommand SaveAsProjectCommand { get { return new Command(SaveAsProject, () => IsProjectOpened); } }
        public ICommand OpenProjectCommand { get { return new Command<Window>(OpenProject); } }
        public ICommand CloseProjectCommand { get { return new Command<Window>(CloseProject, () => IsProjectOpened); } }

        #endregion

        #region File System methods

        private void CreateProject(Window window)
        {
            if (SaveExistingProjectIfNeeded(window) == MessageBoxResult.Cancel)
                return;

            var viewModel = new CreateProjectViewModel();
            var createProjectWindow = new CreateProjectWindow();
            createProjectWindow.DataContext = viewModel;
            createProjectWindow.Closing += (sender, args) =>
            {
                var project = viewModel.GetProjectFile();
                if (project != null)
                {
                    OpenProject(project);
                }
            };
            createProjectWindow.Show();
        }

        private void SaveProject()
        {
            _baseFileAccessor.Save(_projectFile);
        }

        private void SaveAsProject()
        {
            _baseFileAccessor.SaveAs(_projectFile);
        }

        private void OpenProject(Window window)
        {
            try
            {
                if (SaveExistingProjectIfNeeded(window) == MessageBoxResult.Cancel)
                    return;

                var project = _baseFileAccessor.Open();
                if (project != null)
                {
                    OpenProject(project);
                }
            }
            // TODO better handlers
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), Strings.ExceptionMessage_Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public MessageBoxResult SaveExistingProjectIfNeeded(Window window)
        {
            if (IsProjectOpened && !IsProjectSaved)
            {
                var result = MessageBox.Show(window, Strings.SaveProject, Strings.Attention, MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    SaveProject();
                }
                return result;
            }
            return MessageBoxResult.Yes;
        }

        #endregion

        #region Player properties

        public PlayerStates PlayerState
        {
            get { return _playerState; }
            private set
            {
                if (value == _playerState) return;
                _playerState = value;
                RaisePropertyChanged();
            }
        }

        public ICommand PlayPauseCommand { get { return new Command(PlayPause, () => IsProjectOpened); } }
        public ICommand StopCommand { get { return new Command(Stop, () => IsProjectOpened); } }

        public string ProgressTime
        {
            get { return _progressTime; }
            private set
            {
                if (value == _progressTime) return;
                _progressTime = value;
                RaisePropertyChanged();
            }
        }

        public double Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                var frameCount = _capture.GetCaptureProperty(CapProp.FrameCount);
                _capture.SetCaptureProperty(CapProp.PosFrames, value * frameCount);
            }
        }

        public IImage OriginImage
        {
            get { return _originImage; }
            set
            {
                if (Equals(value, _originImage)) return;
                _originImage = value;
                RaisePropertyChanged();
            }
        }

        public IImage VideoImageSource
        {
            get { return _videoImageSource; }
            set
            {
                if (Equals(value, _videoImageSource)) return;
                _videoImageSource = value;
                RaisePropertyChanged();
            }
        }

        public string VideoInformation
        {
            get { return _videoInformation; }
            set
            {
                if (value == _videoInformation) return;
                _videoInformation = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Player methods

        private void CaptureOnImageGrabbed(object sender, EventArgs eventArgs)
        {
            var capture = (Capture) sender;

            //Show time stamp
            double timeIndex = capture.GetCaptureProperty(CapProp.PosMsec);
            ProgressTime = TimeSpan.FromMilliseconds(timeIndex).ToString("g");

            //show frame number
            double frameNumber = capture.GetCaptureProperty(CapProp.PosFrames);
            double totalFrames = capture.GetCaptureProperty(CapProp.FrameCount);
            _progress = frameNumber / totalFrames;
            RaisePropertyChanged("Progress");

            // Show image with keyPoints
            var frame = new Mat();
            _capture.Retrieve(frame);
            var keyFeatures = _projectFile.Model.GetKeyFeatures((int)frameNumber - 1);

            var imageFrame = new Mat();
            Features2DToolbox.DrawKeypoints(frame, keyFeatures, imageFrame, new Bgr(Color.DarkBlue),
                Features2DToolbox.KeypointDrawType.NotDrawSinglePoints);

            if (frameNumber > 1)
            {
                var matches = _projectFile.Model.GetMatches((int) frameNumber - 1);
                foreach (var match in matches)
                {
                    CvInvoke.Line(imageFrame,
                        Point.Round(match.Item1.Point),
                        Point.Round(match.Item2.Point),
                        new Bgr(Color.Red).MCvScalar,
                        2);
                }
            }

            OriginImage = VideoImageSource = imageFrame;

            //Wait to display correct framerate
            var frameRate = capture.GetCaptureProperty(CapProp.Fps);
            var rightElapsedMilliseconds = 1000.0 / frameRate;
            var realElapsedMilliseconds = _stopwatch.ElapsedMilliseconds;
            var waitingMilliseconds = Math.Max(0, rightElapsedMilliseconds - realElapsedMilliseconds);

            Thread.Sleep((int)waitingMilliseconds);
            _stopwatch.Restart();

            if (frameNumber == totalFrames)
            {
                Stop();
            }
        }

        private void PlayPause()
        {
            if (PlayerState == PlayerStates.Playing)
            {
                _capture.Pause();
                PlayerState = PlayerStates.Paused;
            }
            else
            {
                _capture.Start();
                PlayerState = PlayerStates.Playing;
            }
        }

        private void Stop()
        {
            _capture.Stop();
            Progress = 0;
            PlayerState = PlayerStates.Stopped;
        }

        #endregion

        #region Point handlers

        public void ShowPointInformation(double x, double y)
        {
            int frameIndex = (int)_capture.GetCaptureProperty(CapProp.PosFrames) - 1;
            var keyFeaturesVector = _projectFile.Model.GetKeyFeatures(frameIndex);

            var nearestKeyFeature = GetNearestKeyPoint(x, y, keyFeaturesVector);

            var keyIndex = keyFeaturesVector.FirstIndexOf(keyFeature => keyFeature.Point == nearestKeyFeature.Point);

            var chain = _projectFile.Model.GetChain(frameIndex, keyIndex);
            int firstFrame = frameIndex;
            int lastFrame = frameIndex;
            foreach (var pair in chain)
            {
                firstFrame = Math.Min(firstFrame, pair.Item1);
                lastFrame = Math.Max(lastFrame, pair.Item2);
            }

            PointInformation = String.Format(Strings.PointInformationFormat,
                firstFrame,
                frameIndex,
                lastFrame);

            var image = (IImage)OriginImage.Clone();
            CvInvoke.Circle(image, Point.Round(nearestKeyFeature.Point), 5, new Bgr(Color.Yellow).MCvScalar, 2);
            VideoImageSource = image;
        }

        private MKeyPoint GetNearestKeyPoint(double x, double y, VectorOfKeyPoint keyFeaturesVector)
        {
            var width = _capture.GetCaptureProperty(CapProp.FrameWidth);
            var height = _capture.GetCaptureProperty(CapProp.FrameHeight);
            var point = new PointF((float) (x*width), (float) (y*height));

            var result = keyFeaturesVector.Enumerable().First();
            var minDistanse = double.MaxValue;

            foreach (var mKeyPoint in keyFeaturesVector.Enumerable())
            {
                var distance = new LineSegment2DF(mKeyPoint.Point, point).Length;
                if (distance < minDistanse)
                {
                    result = mKeyPoint;
                    minDistanse = distance;
                }
            }
            return result;
        }

        public string PointInformation
        {
            get { return _pointInformation; }
            set
            {
                if (value == _pointInformation) return;
                _pointInformation = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Helpers

        private string GetVideoInformation(Capture capture)
        {
            var height = _capture.GetCaptureProperty(CapProp.FrameHeight); 
            var width = _capture.GetCaptureProperty(CapProp.FrameWidth);
            var frameRate = capture.GetCaptureProperty(CapProp.Fps);
            var totalFrames = capture.GetCaptureProperty(CapProp.FrameCount);
            var codecDouble = capture.GetCaptureProperty(CapProp.FourCC);
            var codec = ConvertToStringCaptureProperty(codecDouble);

            return String.Format("Format: {0}x{1}\nFPS: {2}\nFrames: {3}\nCodec: {4}",
                width,
                height,
                frameRate,
                totalFrames,
                codec);
        }

        private string ConvertToStringCaptureProperty(double value)
        {
            // step by step
            var uInt32 = Convert.ToUInt32(value);
            byte[] bytes = BitConverter.GetBytes(uInt32);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        #endregion
    }
}