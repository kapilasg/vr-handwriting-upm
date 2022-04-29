using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace HandwritingVR
{
    [Serializable]
    public class SegmentPoints
    {
        public List<PointsSet> listOfSets;
        public List<Vector2> boundBox;

        public SegmentPoints(List<List<Vector2>> points, List<Vector2> box)
        {
            listOfSets = new List<PointsSet>();
            for (int i = 0; i < points.Count; i++)
            {
                PointsSet ps = new PointsSet(points[i]);
                listOfSets.Add(ps);
            }
            boundBox = box;
        }

        public List<List<Vector2>> GetPoints()
        {
            List<List<Vector2>> points = new List<List<Vector2>>();
            foreach (var set in listOfSets)
            {
                points.Add(set.GetSet());
            }

            return points;
        }
    }

    [Serializable]
    public class PointsSet
    {
        public List<Vector2> set;

        public PointsSet(List<Vector2> ps)
        {
            set = ps;
        }

        public List<Vector2> GetSet()
        {
            return set;
        }
    }
}