using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using VideoFeatureMatching.Core;

namespace VideoFeatureMatching.DAL
{
    public class VideoCloudPointsFileAccessor : BaseFileAccessor<VideoCloudPoints>
    {
        protected override void SaveTo(VideoCloudPoints model, string path)
        {
            using (var file = File.OpenWrite(path))
            {
                using (var stream = new BinaryWriter(file))
                {
                    stream.Write(model.VideoPath);
                    stream.Write(model.FrameCount);

                    for (int i = 0; i < model.FrameCount; i++)
                    {
                        var featureKeys = model.GetKeyFeatures(i);
                        stream.Write(featureKeys.Size);
                        for (int j = 0; j < featureKeys.Size; j++)
                        {
                            var feature = featureKeys[j];
                            // stream.Write(feature.Angle);
                            // stream.Write(feature.ClassId);
                            // stream.Write(feature.Octave);
                            // stream.Write(feature.Response);
                            // stream.Write(feature.Size);
                            stream.Write(feature.Point.X);
                            stream.Write(feature.Point.Y);
                        }
                    }

                    List<Tuple<int, int>>[] arrayOfMatches = model.GetMatchesArrays();
                    stream.Write(arrayOfMatches.Length);
                    for (int i = 0; i < arrayOfMatches.Length; i++)
                    {
                        var matches = arrayOfMatches[i];
                        stream.Write(matches.Count);
                        for (int j = 0; j < matches.Count; j++)
                        {
                            stream.Write(matches[j].Item1);
                            stream.Write(matches[j].Item2);
                        }
                    }
                }
            }
        }

        protected override VideoCloudPoints ReadFrom(string path)
        {
            using (var file = File.OpenRead(path))
            {
                using (var stream = new BinaryReader(file))
                {
                    var videoPath = stream.ReadString();
                    var frameCounts = stream.ReadInt32();
                    var videCloudPoints = new VideoCloudPoints(videoPath, frameCounts);

                    for (int i = 0; i < frameCounts; i++)
                    {
                        var keysCount = stream.ReadInt32();
                        var array = new MKeyPoint[keysCount];
                        for (int j = 0; j < keysCount; j++)
                        {
                            var keyFeature = new MKeyPoint();
//                            keyFeature.Angle = stream.ReadSingle();
//                            keyFeature.ClassId = stream.ReadInt32();
//                            keyFeature.Octave = stream.ReadInt32();
//                            keyFeature.Response = stream.ReadSingle();
//                            keyFeature.Size = stream.ReadSingle();
                            keyFeature.Point = new PointF(stream.ReadSingle(), stream.ReadSingle());
                            array[j] = keyFeature;
                        }
                        videCloudPoints.SetKeyFeatures(i, new VectorOfKeyPoint(array));
                    }

                    int matchesListCount = stream.ReadInt32();
                    for (int i = 0; i < matchesListCount; i++)
                    {
                        int matchesCount = stream.ReadInt32();
                        int firstFrameIndex = -1;
                        int firstFeatureIndex = -1;

                        for (int j = 0; j < matchesCount; j++)
                        {
                            int frameIndex = stream.ReadInt32();
                            int featureIndex = stream.ReadInt32();
                            if (firstFeatureIndex != -1 && firstFeatureIndex != -1)
                            {
                                videCloudPoints.Unite(firstFrameIndex, firstFeatureIndex, frameIndex, featureIndex);
                            }
                            else
                            {
                                firstFrameIndex = frameIndex;
                                firstFeatureIndex = featureIndex;
                            }
                        }
                    }
                    return videCloudPoints;
                }
            }
        }
    }
}