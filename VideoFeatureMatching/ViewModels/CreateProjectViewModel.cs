using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;
using Microsoft.Win32;
using VideoFeatureMatching.Core;
using VideoFeatureMatching.DAL;
using VideoFeatureMatching.L10n;
using VideoFeatureMatching.Utils;
using DescriptorMatcher = Emgu.CV.Features2D.DescriptorMatcher;

namespace VideoFeatureMatching.ViewModels
{
    public class CreateProjectViewModel : BaseViewModel, IDisposable
    {
        private Capture _capture;
        private IImage _previewImageSource;
        private Detectors _selectedDetector;
        private Feature2D _detector;
        private Feature2D _descripter;
        private DescriptorMatcher _matcher;
        private Descripters _selectedDescripter;
        private Matchers _selectedMatcher;
        private string _videoPath;
        private FeatureVideoModel _tempModel;
        private int _framesCount;
        private int _selectedFrameIndex;
        private FeatureGeneratingStates _generatingStates;

        public CreateProjectViewModel()
        {
            _detector = GetNativeDetector(SelectedDetector);
            _descripter = GetNativeDescripter(SelectedDescripter);
            _matcher = GetNativeMatcher(SelectedMatcher);
        }

        private void CaptureOnImageGrabbed(object sender, EventArgs eventArgs)
        {
            Mat frame = new Mat();
            _capture.Retrieve(frame, 0);
            
            int k = 2;
            double uniquenessThreshold = 0.8;
            double hessianThresh = 300;

            UMat uModelImage = frame.ToUMat(AccessType.Read);

            //extract features from the object image
            UMat modelDescriptors = new UMat();

            var keys = _detector.Detect(frame);

            var keyPoints = new VectorOfKeyPoint(keys);
            // surf.DetectAndCompute(uModelImage, null, keyPoints, modelDescriptors, true);

            Mat resultFrame = new Mat();
            Features2DToolbox.DrawKeypoints(frame, keyPoints, resultFrame, new Bgr(0, 0, 255), Features2DToolbox.KeypointDrawType.DrawRichKeypoints);


            PreviewImageSource = resultFrame;

//            using ()
//            using (UMat uObservedImage = observedImage.ToUMat(AccessType.Read))
//            {
//
//                watch = Stopwatch.StartNew();
//
//                // extract features from the observed image
//                UMat observedDescriptors = new UMat();
//                surfCPU.DetectAndCompute(uObservedImage, null, observedKeyPoints, observedDescriptors, false);
//                BFMatcher matcher = new BFMatcher(DistanceType.L2);
//                matcher.Add(modelDescriptors);
//
//                matcher.KnnMatch(observedDescriptors, matches, k, null);
//                mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
//                mask.SetTo(new MCvScalar(255));
//                Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);
//
//                int nonZeroCount = CvInvoke.CountNonZero(mask);
//                if (nonZeroCount >= 4)
//                {
//                    nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints,
//                       matches, mask, 1.5, 20);
//                    if (nonZeroCount >= 4)
//                        homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints,
//                           observedKeyPoints, matches, mask, 2);
//                }
//
//                watch.Stop();
//            }
            SelectedFrameIndex++;
            if (SelectedFrameIndex == FramesCount)
            {
                GeneratingStates = FeatureGeneratingStates.Finished;
            }
        }

        public ProjectFile<FeatureVideoModel> GetProjectFile()
        {
            return GeneratingStates == FeatureGeneratingStates.Finished 
                ? new ProjectFile<FeatureVideoModel>(_tempModel) 
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
                _matcher = GetNativeMatcher(value);
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

                    SelectedFrameIndex = 0;
                    FramesCount = (int)_capture.GetCaptureProperty(CapProp.FrameCount);

                    _capture.Start();
                    GeneratingStates = FeatureGeneratingStates.Processing;
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

        public double Progress { get { return FramesCount == 0 ? 0 : (double)SelectedFrameIndex/FramesCount; }}

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
                }, () => GeneratingStates == FeatureGeneratingStates.Finished);
            }
        }

        public Command<Window> CloseCommand
        {
            get
            {
                return new Command<Window>(window =>
                {
                    window.Close();
                });
            }
        }

        #endregion

        #endregion

        #region Private properties

        private int FramesCount
        {
            get { return _framesCount; }
            set
            {
                if (value == _framesCount) return;
                _framesCount = value;
                RaisePropertyChanged("Progress");
                RaisePropertyChanged("ProgressText");
            }
        }

        private int SelectedFrameIndex
        {
            get { return _selectedFrameIndex; }
            set
            {
                if (value == _selectedFrameIndex) return;
                _selectedFrameIndex = value;
                RaisePropertyChanged("Progress");
                RaisePropertyChanged("ProgressText");
            }
        }

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
                case Matchers.BFL2Sqr:
                    return new BFMatcher(DistanceType.L2Sqr);
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