using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace HandwritingVR
{
    [Serializable]
    public class SegmentPoints3D
    {
        public List<PointsSet3D> listOfSets;

        public SegmentPoints3D(List<List<Vector3>> points)
        {
            listOfSets = new List<PointsSet3D>();
            for (int i = 0; i < points.Count; i++)
            {
                PointsSet3D ps = new PointsSet3D(points[i]);
                listOfSets.Add(ps);
            }
        }

        public List<List<Vector3>> Get3DPoints()
        {
            List<List<Vector3>> points = new List<List<Vector3>>();
            foreach (var set in listOfSets)
            {
                points.Add(set.GetSet());
            }

            return points;
        }
    }

    [Serializable]
    public class PointsSet3D
    {
        public List<Vector3> set;

        public PointsSet3D(List<Vector3> ps)
        {
            set = ps;
        }

        public List<Vector3> GetSet()
        {
            return set;
        }
    }
    
    [Serializable]
    public class SegmentPoints3DList
    {
        public List<SegmentPoints3D> listOfSegments;

        public SegmentPoints3DList(List<SegmentPoints3D> list)
        {
            listOfSegments = new List<SegmentPoints3D>();
            foreach (var segmentPoints3D in list)
            {
                listOfSegments.Add(segmentPoints3D);
            }
        }

        public void Add(SegmentPoints3D sp3d)
        {
            listOfSegments.Add(sp3d);
        }
    }
}