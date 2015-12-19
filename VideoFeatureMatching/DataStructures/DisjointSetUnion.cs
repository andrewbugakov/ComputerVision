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
        private readonly Dictionary<Point, HashSet<Point>> _values; 

        public DisjointSetUnion(int layers)
        {
            _sets = new Dictionary<Point, Point>[layers];
            _values = new Dictionary<Point, HashSet<Point>>();
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
                if (Equals(set, point)) return point;
                else
                {
                    var newSet = FindSet(set);
                    dictionary[point] = newSet;
                    return newSet;
                }
            }
        }

        private void MakeSet(Point setA, Point setB)
        {
            setA = FindSet(setA);
            setB = FindSet(setB);

            if (Equals(setA, setB))
                return;

            var dictionary = _sets[setA.X] ?? (_sets[setA.X] = new Dictionary<Point, Point>());
            dictionary[setA] = setB;

            var newValues = _values.ContainsKey(setB) ? _values[setB] : (_values[setB] = new HashSet<Point>());
            newValues.Add(setA);
            newValues.Add(setB);

            if (_values.ContainsKey(setA))
            {
                foreach (var value in _values[setA])
                {
                    newValues.Add(value);
                }
                _values.Remove(setA);
            }
        }

        public void Unit(int layerA, int indexA, int layerB, int indexB)
        {
            var pointA = new Point(layerA, indexA);
            var pointB = new Point(layerB, indexB);

            MakeSet(pointB, pointA);
        }

        public IEnumerable<Tuple<int, int>> GetChain(int layer, int index)
        {
            var pointA = new Point(layer, index);
            var set = FindSet(pointA);

            if (_values.ContainsKey(set))
            {
                return _values[set].Select(point => Tuple.Create(point.X, point.Y));
            }
            return null;
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
            return
                _values.Values
                .Select(value => value
                    .Select(point => new Tuple<int, int>(point.X, point.Y))
                    .ToList())
                .ToArray();
        }

        [DebuggerDisplay("X:{X} Y:{Y}")]
        private class Point
        {
            public int X { get; private set; }
            public int Y { get; private set; }

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }

            public override bool Equals(System.Object obj)
            {
                // If parameter is null return false.
                if (obj == null)
                {
                    return false;
                }

                // If parameter cannot be cast to Point return false.
                var p = obj as Point;
                if ((System.Object)p == null)
                {
                    return false;
                }

                // Return true if the fields match:
                return (X == p.X) && (Y == p.Y);
            }

            public bool Equals(Point p)
            {
                // If parameter is null return false:
                if ((object)p == null)
                {
                    return false;
                }

                // Return true if the fields match:
                return (X == p.X) && (Y == p.Y);
            }

            public override int GetHashCode()
            {
                return X ^ Y;
            }
        }
    }
}