using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace VideoFeatureMatching.DataStructures
{
    // Idea got from http://e-maxx.ru/algo/dsu
    public class DisjointSetUnion
    {
        private readonly Dictionary<Point, Point>[] _sets; 

        public DisjointSetUnion(int layers)
        {
            _sets = new Dictionary<Point, Point>[layers];
        }

        private Point FindSet(Point point)
        {
            var dictionary = _sets[point.X];
            if (dictionary == null || !dictionary.ContainsKey(point))
            {
                return point;
            }
            else
            {
                var set = dictionary[point];
                if (set == point) return point;
                else
                {
                    var newSet = FindSet(set);
                    dictionary[point] = newSet;
                    return newSet;
                }
            }
        }

        private void MakeSet(Point point, Point set)
        {
            var dictionary = _sets[point.X] ?? (_sets[point.X] = new Dictionary<Point, Point>());
            dictionary[point] = set;
        }

        public void Unit(int layerA, int indexA, int layerB, int indexB)
        {
            var pointA = new Point(layerA, indexA);
            var pointB = new Point(layerB, indexB);

            var setA = FindSet(pointA);
            var setB = FindSet(pointB);

            if (setA != setB)
            {
                MakeSet(setB, setB);
                MakeSet(setA, setB);
            }
        }

        /// <summary>
        /// Returns array of lists.
        /// Each list contains Tuple.
        /// First argument is layerIndex
        /// Second argument is pointIndex.
        /// </summary>
        /// <returns></returns>
        public List<Tuple<int, int>>[] GetUnitPoints()
        {
            var dictionary = new Dictionary<Point, List<Point>>();

            foreach (var pointDictionary in _sets)
            {
                foreach (var point in pointDictionary.Keys.ToList())
                {
                    var set = FindSet(point);
                    var list = dictionary.ContainsKey(set) ? dictionary[set] : (dictionary[set] = new List<Point>());

                    list.Add(point);
                }
            }

            return dictionary.Values.Select(lists => lists.Select(point => new Tuple<int, int>(point.X, point.Y)).ToList()).ToArray();
        }

        [DebuggerDisplay("X:{X} Y:{Y}")]
        private class Point
        {
            public int X { get; set; }
            public int Y { get; set; }

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
    }
}