using Emgu.CV;
using Emgu.CV.Util;

namespace VideoFeatureMatching.Core
{
    public class FeatureVideoModel
    {
        private readonly int _frameCount;
        private readonly VectorOfKeyPoint[] _vectorOfKeyPoints;
        private readonly string _videoPath;

        public FeatureVideoModel(string videoPath, int frameCount)
        {
            _frameCount = frameCount;
            _videoPath = videoPath;
            _vectorOfKeyPoints = new VectorOfKeyPoint[frameCount];
        }

        public string VideoPath { get { return _videoPath; } }

        public void SetKeyFeatures(int frameIndex, VectorOfKeyPoint keyPoints)
        {
            _vectorOfKeyPoints[frameIndex] = keyPoints;
        }

        public VectorOfKeyPoint GetKeyFeatures(int frameIndex)
        {
            return _vectorOfKeyPoints[frameIndex];
        }

        public void Unite(int frameIndexA, int keyIndexA, int frameIndexB, int keyIndexB)
        {
            // TODO unite!!
        }
    }
}