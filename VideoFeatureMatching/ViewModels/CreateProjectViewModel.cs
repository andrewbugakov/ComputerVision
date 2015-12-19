using System;
using System.Drawing;
using System.Windows;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;
using VideoFeatureMatching.Core;
using VideoFeatureMatching.DAL;
using VideoFeatureMatching.Utils;
using Point = System.Drawing.Point;

namespace VideoFeatureMatching.ViewModels
{
    public class CreateProjectViewModel : BaseViewModel, IDisposable
    {
        private Capture _capture;
        private IImage _previewImageSource;
        private Detectors _selectedDetector;
        private Feature2D _detector;
        private Feature2D _descripter;
        private Descripters _selectedDescripter;
        private Matchers _selectedMatcher;
        private string _videoPath;
        private VideoCloudPoints _tempCloudPoints;
        private int _framesCount;
        private int _selectedFrameIndex;
        private FeatureGeneratingStates _generatingStates;

        public CreateProjectViewModel()
        {
            _detector = GetNativeDetector(SelectedDetector);
            _descripter = GetNativeDescripter(SelectedDescripter);
        }

        private Mat _previousDescripters;

        private void CaptureOnImageGrabbed(object sender, EventArgs eventArgs)
        {
            var capture = (Capture) sender;

            var frame = new Mat();
            capture.Retrieve(frame);

            // 1. get key points
            var keyPoints = new VectorOfKeyPoint(_detector.Detect(frame));
            _tempCloudPoints.SetKeyFeatures(_selectedFrameIndex, keyPoints);

            // 2. get descripters
            var descripters = new Mat();
            _descripter.Compute(frame, keyPoints, descripters);

            // draw keypoints
            var imageFrame = new Mat();
            Features2DToolbox.DrawKeypoints(frame, keyPoints, imageFrame, new Bgr(Color.DarkBlue),
                Features2DToolbox.KeypointDrawType.NotDrawSinglePoints);

            if (_selectedFrameIndex != 0)
            {
                var previousKeyPoints = _tempCloudPoints.GetKeyFeatures(_selectedFrameIndex - 1);
                var previousKeyDescripters = _previousDescripters;

                const int k = 2;
                const double uniquenessThreshold = 0.8;

                // 3. compute all matches with previous frame
                var matches = new VectorOfVectorOfDMatch();
                var matcher = GetNativeMatcher(SelectedMatcher);
                matcher.Add(previousKeyDescripters);

                matcher.KnnMatch(descripters, matches, k, null);

                var mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
                mask.SetTo(new MCvScalar(255));
                Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);
                Features2DToolbox.VoteForSizeAndOrientation(previousKeyPoints, keyPoints,
                       matches, mask, 1.5, 20);
                Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(previousKeyPoints,
                            keyPoints, matches, mask, 2);

                var managedMask = mask.GetData();

                // 4. separate good matches
                var currentKeys = keyPoints;

                for (int i = 0; i < matches.Size; i++)
                {
                    var match = matches[i][0];
                    // filter wrong matches
                    if (managedMask[i] == 1)
                    {
                        var previousIndex = match.TrainIdx;
                        var currentIndex = match.QueryIdx;

                        var previousPoint = previousKeyPoints[previousIndex].Point;
                        var currentPoint = currentKeys[currentIndex].Point;

                        _tempCloudPoints.Unite(_selectedFrameIndex - 1, previousIndex,
                            _selectedFrameIndex, currentIndex);

                        CvInvoke.Line(imageFrame,
                            Point.Round(previousPoint),
                            Point.Round(currentPoint),
                            new Bgr(Color.Red).MCvScalar,
                            2);
                    }
                }
            }

            _previousDescripters = descripters;

            PreviewImageSource = imageFrame;

            _selectedFrameIndex++;
            RaisePropertyChanged("Progress");
            RaisePropertyChanged("ProgressText");
            if (_selectedFrameIndex == _framesCount)
            {
                GeneratingStates = FeatureGeneratingStates.Finished;
            }
        }

        public ProjectFile<VideoCloudPoints> GetProjectFile()
        {
            return GeneratingStates == FeatureGeneratingStates.Finished 
                ? new ProjectFile<VideoCloudPoints>(_tempCloudPoints) 
                : null;
        }

        #region Public properties

        #region Video handlers

        public bool IsVideoSelected { get { return !String.IsNullOrEmpty(VideoPath); } }

        public string VideoPath
        {
            get { return _videoPath; }
            set
            {
                if (value == _videoPath) return;
                _videoPath = value;
                RaisePropertyChanged();
                RaisePropertyChanged("IsVideoSelected");
                RaisePropertyChanged("StartCommand");
                RaisePropertyChanged("FinishCommand");
            }
        }

        #endregion

        #region Feature settings

        public Detectors SelectedDetector
        {
            get { return _selectedDetector; }
            set
            {
                if (value == _selectedDetector) return;
                _selectedDetector = value;
                _detector = GetNativeDetector(value);
                RaisePropertyChanged();
            }
        }

        public Descripters SelectedDescripter
        {
            get { return _selectedDescripter; }
            set
            {
                if (value == _selectedDescripter) return;
                _selectedDescripter = value;
                _descripter = GetNativeDescripter(value);
                RaisePropertyChanged();
            }
        }

        public Matchers SelectedMatcher
        {
            get { return _selectedMatcher; }
            set
            {
                if (value == _selectedMatcher) return;
                _selectedMatcher = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Generation handlers

        private FeatureGeneratingStates GeneratingStates
        {
            get { return _generatingStates; }
            set
            {
                if (value == _generatingStates) return;
                _generatingStates = value;
                RaisePropertyChanged("StartCommand");
                RaisePropertyChanged("FinishCommand");
                RaisePropertyChanged("ReadyCommand");
            }
        }

        public Command StartCommand
        {
            get
            {
                return new Command(() =>
                {
                    _capture = new Capture(VideoPath);
                    _capture.ImageGrabbed += CaptureOnImageGrabbed;

                    _selectedFrameIndex = 0;
                    _framesCount = (int)_capture.GetCaptureProperty(CapProp.FrameCount);

                    _tempCloudPoints = new VideoCloudPoints(VideoPath, _framesCount);
                    _capture.Start();

                    GeneratingStates = FeatureGeneratingStates.Processing;
                    RaisePropertyChanged("Progress");
                    RaisePropertyChanged("ProgressText");
                }, () => IsVideoSelected && GeneratingStates != FeatureGeneratingStates.Processing);
            }
        }

        public Command FinishCommand
        {
            get
            {
                return new Command(() =>
                {
                    _capture.ImageGrabbed -= CaptureOnImageGrabbed;
                    _capture.Stop();
                    _capture.Dispose();
                    _capture = null;
                    GeneratingStates = FeatureGeneratingStates.Idle;
                }, () => IsVideoSelected && GeneratingStates == FeatureGeneratingStates.Processing);
            }
        }

        #endregion

        #region Image handlers

        public IImage PreviewImageSource
        {
            get { return _previewImageSource; }
            set
            {
                if (Equals(value, _previewImageSource)) return;
                _previewImageSource = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Progress handlers

        public double Progress { get { return _framesCount == 0 ? 0 : (double)_selectedFrameIndex / _framesCount; } }

        public string ProgressText { get { return (int)(Progress*100) + "%"; } }

        #endregion

        #region Window commands

        public Command<Window> ReadyCommand
        {
            get
            {
                return new Command<Window>(window =>
                {
                    window.Close();
                    _capture.Stop();
                    _capture.ImageGrabbed -= CaptureOnImageGrabbed;
                }, () => GeneratingStates == FeatureGeneratingStates.Finished);
            }
        }

        public Command<Window> CloseCommand
        {
            get
            {
                return new Command<Window>(window =>
                {
                    _capture.Stop();
                    _capture.ImageGrabbed -= CaptureOnImageGrabbed;
                    window.Close();
                });
            }
        }

        #endregion

        #endregion

        #region Private

        private Feature2D GetNativeDetector(Detectors detectors)
        {
            switch (detectors)
            {
                case Detectors.Surf:
                    return new SURF(200);
                case Detectors.Fast:
                    return new FastDetector();
                case Detectors.ORB:
                    return new ORBDetector();
                case Detectors.Sift:
                    return new SIFT();
                default:
                    throw new ArgumentException("Don't know type " + detectors);
            }
        }

        private Feature2D GetNativeDescripter(Descripters descripter)
        {
            switch (descripter)
            {
                case Descripters.Surf:
                    return new SURF(200);
                case Descripters.Sift:
                    return new SIFT();
                case Descripters.Brief:
                    return new BriefDescriptorExtractor();
                default:
                    throw new ArgumentException("Don't know type " + descripter);
            }
        }

        private DescriptorMatcher GetNativeMatcher(Matchers matcher)
        {
            switch (matcher)
            {
                case Matchers.BFL2:
                    return new BFMatcher(DistanceType.L2);
                case Matchers.Hamming:
                    return new BFMatcher(DistanceType.Hamming);
                default:
                    throw new ArgumentException("Don't know type " + matcher);
            }
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            // dispose camera
            FinishCommand.Execute();
        }

        #endregion
    }
}