using System;
using System.Collections.Generic;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace VideoFeatureMatching.Utils
{
    public static class VectorOfKeyPointExtenstions
    {
        public static IEnumerable<MKeyPoint> Enumerable(this VectorOfKeyPoint vector)
        {
            for (int i = 0; i < vector.Size; i++)
            {
                yield return vector[i];
            }
        }

        public static int FirstIndexOf(this VectorOfKeyPoint vector, Func<MKeyPoint, bool> predicate)
        {
            for (int i = 0; i < vector.Size; i++)
            {
                if (predicate(vector[i]))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}