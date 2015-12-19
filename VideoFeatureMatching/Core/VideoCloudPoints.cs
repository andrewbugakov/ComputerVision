using System;
using System.Collections.Generic;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using VideoFeatureMatching.DataStructures;

namespace VideoFeatureMatching.Core
{
    public class VideoCloudPoints
    {
        private readonly VectorOfKeyPoint[] _vectorOfKeyPoints;
        private readonly DisjointSetUnion _disjointSetUnion;
        private readonly string _videoPath;
        private readonly int _frameCount;

        public VideoCloudPoints(string videoPath, int frameCount)
        {
            _frameCount = frameCount;
            _videoPath = videoPath;
            _vectorOfKeyPoints = new VectorOfKeyPoint[frameCount];
            _disjointSetUnion = new DisjointSetUnion(frameCount);
        }

        public string VideoPath { get { return _videoPath; } }
        public int FrameCount { get { return _frameCount; } }

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
            _disjointSetUnion.Unit(frameIndexA, keyIndexA, frameIndexB, keyIndexB);
        }

        public List<Tuple<MKeyPoint, MKeyPoint>> GetMatches(int frameIndex)
        {
            var currentKeys = GetKeyFeatures(frameIndex);
            var previousKeys = GetKeyFeatures(frameIndex - 1);

            var result = new List<Tuple<MKeyPoint, MKeyPoint>>();

            for (int i = 0; i < currentKeys.Size; i++)
            {
                var chain = _disjointSetUnion.GetChain(frameIndex, i);
                if (chain != null)
                {
                    var pair = chain.FirstOrDefault(t => t.Item1 == frameIndex - 1);
                    if (pair != null)
                    {
                        result.Add(Tuple.Create(currentKeys[i], previousKeys[pair.Item2]));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns unique sequences of matches
        /// </summary>
        /// <returns></returns>
        public List<Tuple<int, int>>[] GetMatchesArrays()
        {
            return _disjointSetUnion.GetUnitPoints();
        }
    }
}