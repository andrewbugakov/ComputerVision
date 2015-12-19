using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Emgu.CV;
using Emgu.CV.CvEnum;
using VideoFeatureMatching.Core;
using VideoFeatureMatching.DAL;
using VideoFeatureMatching.L10n;
using VideoFeatureMatching.Utils;

namespace VideoFeatureMatching.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly VideoCloudPointsFileAccessor _baseFileAccessor = new VideoCloudPointsFileAccessor();

        private ProjectFile<VideoCloudPoints> _projectFile;

        private Capture _capture;
        private string _progressTime;
        private PlayerStates _playerState;
        private double _progress;
        private IImage _videoImageSource;

        public MainViewModel()
        {
            _playerState = PlayerStates.Stopped;
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

        public Command<Window> ExitCommand
        {
            get
            {
                return new Command<Window>(window =>
                {
                    SaveExistingProjectIfNeeded();
                    window.Close();
                });
            }
        }

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

            Progress = 0;
            RaisePropertyChanged("PlayPauseCommand");
            RaisePropertyChanged("StopCommand");
        }

        private void CloseProject()
        {
            _projectFile = null;
            RaisePropertyChanged("IsProjectOpened");
            RaisePropertyChanged("IsProjectSaved");
            
            RaisePropertyChanged("SaveProjectCommand");
            RaisePropertyChanged("SaveAsProjectCommand");
            RaisePropertyChanged("CloseProjectCommand");

            _capture.ImageGrabbed -= CaptureOnImageGrabbed;
            _capture.Dispose();
            _capture = null;

            RaisePropertyChanged("PlayPauseCommand");
            RaisePropertyChanged("StopCommand");
        }

        #endregion

        #region File System Commands

        public Command CreateProjectCommand { get { return new Command(CreateProject); } }
        public Command SaveProjectCommand { get { return new Command(SaveProject, () => IsProjectOpened && !IsProjectSaved); } }
        public Command SaveAsProjectCommand { get { return new Command(SaveAsProject, () => IsProjectOpened); } }
        public Command OpenProjectCommand { get { return new Command(OpenProject); } }
        public Command CloseProjectCommand { get { return new Command(CloseProject, () => IsProjectOpened); } }

        #endregion

        #region File System methods

        private void CreateProject()
        {
            if (SaveExistingProjectIfNeeded() == MessageBoxResult.Cancel)
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

        private void OpenProject()
        {
            try
            {
                if (SaveExistingProjectIfNeeded() == MessageBoxResult.Cancel)
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

        private MessageBoxResult SaveExistingProjectIfNeeded()
        {
            if (IsProjectOpened && !IsProjectSaved)
            {
                var result = MessageBox.Show(GetCurrentWindowDelegate.Invoke(), Strings.SaveProject, Strings.Attention, MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    SaveProject();
                }
                return result;
            }
            return MessageBoxResult.Yes;
        }

        #endregion

        #region Player Properties

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

        #endregion

        #region Player methods

        private void CaptureOnImageGrabbed(object sender, EventArgs eventArgs)
        {
            var capture = (Capture) sender;

            //Show image
            var frame = new Mat();
            _capture.Retrieve(frame);
            VideoImageSource = frame;

            //Show time stamp
            double timeIndex = capture.GetCaptureProperty(CapProp.PosMsec);
            ProgressTime = TimeSpan.FromMilliseconds(timeIndex).ToString("g");

            //show frame number
            double framenumber = capture.GetCaptureProperty(CapProp.PosFrames);
            double totalFrames = capture.GetCaptureProperty(CapProp.FrameCount);
            _progress = framenumber / totalFrames;
            RaisePropertyChanged("Progress");

            /*Note: We can increase or decrease this delay to fastforward of slow down the display rate
             if we want a re-wind function we would have to use _Capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_FRAMES, FrameNumber*);
            //and call the process frame to update the picturebox ProcessFrame(null, null);. This is more complicated.*/

            //Wait to display correct framerate
            var frameRate = capture.GetCaptureProperty(CapProp.Fps);
            Thread.Sleep((int)(1000.0 / frameRate)); //This may result in fast playback if the codec does not tell the truth

            //Lets check to see if we have reached the end of the video
            //If we have lets stop the capture and video as in pause button was pressed
            //and reset the video back to start
            if (framenumber == totalFrames)
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

        #region Helpers

        public Func<Window> GetCurrentWindowDelegate { get; set; }

        #endregion
    }
}