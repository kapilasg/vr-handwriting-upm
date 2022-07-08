using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

namespace HandwritingVR
{
    // Class to transform raw data into useful data for feature extraction
    // using Projection, Normalization, Segmentation etc.
    public class DataTransformer : MonoBehaviour
    {
        private List<List<Vector3>> _segments3D;
        private List<List<Vector2>> _segments2D;
        private List<Vector2> _boundBox2D;
        
        private int _numberOfPoints;
        private Vector3 _supportVector;
        private Vector3 _normalVector;
        private Vector3 _directVector1;
        private Vector3 _directVector2;
        
        // Method to convert List of LineRenderer objects into List of Vector3 objects 
        public int SetPoints(List<LineRenderer> drawnLines)
        {
            _segments3D = new List<List<Vector3>>();
            
            for (int i = 0; i < drawnLines.Count-2; i++)
            {
                if (!drawnLines[i]) continue;
                int numberOfPoints = drawnLines[i].positionCount;
                List<Vector3> segmentPoints = new List<Vector3>(numberOfPoints);
                for (int j = 0; j < numberOfPoints; j++)
                {
                    segmentPoints.Add(drawnLines[i].GetPosition(j));
                }
                _segments3D.Add(segmentPoints);
                if (numberOfPoints <= 3)
                {
                    Debug.Log("number of points <= 3 ->" + numberOfPoints);
                }
            }

            // StoreDrawing3D();

            int count = 0;
            foreach (var segment in _segments3D)
            {
                for (int j = 0; j < segment.Count; j++)
                {
                    count++;
                }
            }

            _numberOfPoints = count;
            return count;
        }
        
        // Method to project 3D vectors onto the fitting plane
        public void ProjectSegments2D()
        {
            _segments2D = new List<List<Vector2>>();
            
            FindPlane();
            
            // Project points to Plane in 3D space
            var proj3D = new List<List<Vector3>>();
            for (int i = 0; i < _segments3D.Count; i++)
            {
                List<Vector3> projectedSegment = new List<Vector3>();
                for (int j = 0; j < _segments3D[i].Count; j++)
                {
                    projectedSegment.Add(ProjectToPlane(_segments3D[i][j]));
                }

                proj3D.Add(projectedSegment);
            }

            var boundBox3D = GetBoundingBox3D(proj3D);
            
            // calculate Normalized 2D Projections
            var lowLeft = Get2DFrom3D(boundBox3D[0]);
            var lowRight = Get2DFrom3D(boundBox3D[1]);
            var uppLeft = Get2DFrom3D(boundBox3D[3]);
            
            var dx = Math.Abs(lowRight.x - lowLeft.x);
            var dy = Math.Abs(uppLeft.y - lowLeft.y);
            var d = dx / dy;
            
            foreach (var segment in proj3D)
            {
                List<Vector2> projSeg2D = new List<Vector2>();
                foreach (var point in segment)
                {
                    var newPoint = Get2DFrom3D(point);
                    newPoint.x -= lowLeft.x;
                    newPoint.x *= 1 / dx;
                    newPoint.y -= lowLeft.y;
                    newPoint.y *= 1 / dy;
                    
                    if (d <= 1)
                    {
                        newPoint.x *= d;
                    }
                    else
                    {
                        newPoint.y *= (1 / d);
                    }
                    projSeg2D.Add(newPoint);
                }

                _segments2D.Add(projSeg2D);
            }
        }

        // Main Method to segment lines
        public void SegmentLines()
        {
            // _segments2D = new List<List<Vector2>>(); // segment in 2D;
            var tmpProjSeg = new List<List<Vector2>>(_segments2D);
            _segments2D = new List<List<Vector2>>();
            foreach (var line in tmpProjSeg)
            {
                List<Vector2> segment;
                if (line.Count < 8)
                {
                    // Debug.Log("Segment has less than 8 Points and can therefore not further be divided");
                    segment = new List<Vector2>(line);
                    _segments2D.Add(segment);
                    continue;
                }
                // Finding first segment in line
                var segmInd = FindFirstSegmentIndex(line);
                // Debug.Log("number of points in first segment: "+segmInd.numOfPoints);
                // Debug.Log("is it the last segment?: "+segmInd.lastSegment);
                segment = new List<Vector2>(line.GetRange(0, segmInd.numOfPoints));
                _segments2D.Add(segment);
                
                int bound = 0;
                var restStartIndex = 0;
                while (!segmInd.lastSegment && bound < 5)
                {
                    // Finding next segment in line
                    // Debug.Log("In segmenting while loop!");
                    // starts at end of last segment
                    restStartIndex += segmInd.numOfPoints;
                    // Debug.Log("restStartIndex: "+restStartIndex);
                    // define rest of line as new line without the first segment
                    var restLine = new List<Vector2>(
                        line.GetRange(restStartIndex, line.Count-restStartIndex));
                    // Debug.Log("number of points in restLine: " + restLine.Count);
                    segmInd = FindFirstSegmentIndex(restLine);
                    // Debug.Log("number of points in first segment in restLine: "+segmInd.numOfPoints);
                    var nextSegment = new List<Vector2>(line.GetRange(restStartIndex, segmInd.numOfPoints));
                    _segments2D.Add(nextSegment);
                    bound++;
                }

                if (bound >= 5)
                {
                    Debug.Log("bound exceeded");
                }
            }
        }

        // Helper Method to recursively segment lines (divide at corners)
        private (int numOfPoints,bool lastSegment) FindFirstSegmentIndex(List<Vector2> line)
        {
            // Debug.Log("(FindFirstSegment) number of points in line: "+ line.Count);
            if (line.Count < 8)
            {
                // Debug.Log("Too short to segment"+ line.Count);
                return (line.Count, true);
            }
            
            // Just a straight line:
            // var avrAngle = Vector2.Angle(Vector2.right, line[^1] - line[0]);
            var calcAvrAngle = 0f;
            var calcLength = 0f;
            var counter = 0;
            for (int i = 2; i < line.Count-2; i++)
            {
                // calcAvrAngle += Vector2.Angle(Vector2.right, line[i+1] - line[i]);
                calcAvrAngle += Vector2.Angle(line[i+2] - line[i], line[i-2] - line[i]);
                counter++;
            }
            calcAvrAngle /= counter;
            for (int i = 0; i < line.Count - 1; i++)
            {
                calcLength += Vector2.Distance(line[i], line[i + 1]);
            }
            if (165 <= calcAvrAngle && calcAvrAngle <= 195 
                && Math.Abs(Vector2.Distance(line[0], line[^1]) - calcLength) < 0.15)
            {
                // Debug.Log("Just a straight line.");
                return (line.Count, true);
            }

            // var counter = 4; // Each segment has at least 4 points.

            for (int i = 2; i < line.Count - 3; i++) // Dont look at first and last points (Ungenau/Zittrige hand)
            {
                var right = line[i + 2] - line[i];
                var left = line[i - 2] - line[i];
                var angle = Vector2.Angle(right, left);
                //Debug.Log("Angle "+i+" in between: "+ angle); // Is weighting necessary?
                if (angle < 120) // consider < 130 for small drawings!!!
                {
                    if (i + 4 < line.Count)
                    {
                        var nextAngle = Vector2.Angle(line[i + 3] - line[i+1], line[i - 1] - line[i+1]);
                        var nextAngle2 = Vector2.Angle(line[i + 4] - line[i+2], line[i] - line[i+2]);
                        // compare all angles -> smallest angle is breaking point
                        if (angle < nextAngle && angle < nextAngle2)
                        {
                            // breaking point i
                            // check how many points are left
                            var pointsLeft = line.Count - i;
                            if (pointsLeft < 4)
                            {
                                return (line.Count, true);
                            }
                            return (i + 1, false);
                        }

                        if (nextAngle < angle && nextAngle < nextAngle2)
                        {
                            // breaking point i+1
                            var pointsLeft = line.Count - i+1;
                            if (pointsLeft < 4)
                            {
                                return (line.Count, true);
                            }
                            return (i + 2, false);
                        }

                        if (nextAngle2 < angle && nextAngle2 < nextAngle)
                        {
                            // breaking point i+2
                            var pointsLeft = line.Count - i+2;
                            if (pointsLeft < 4)
                            {
                                return (line.Count, true);
                            }
                            return (i + 3, false);
                        }
                    }

                    if (i + 3 < line.Count)
                    {
                        var nextAngle = Vector2.Angle(line[i + 3] - line[i+1], line[i - 1] - line[i+1]);
                        if (angle < nextAngle)
                        {
                            // breaking point i
                            // check how many points are left
                            var pointsLeft = line.Count - i;
                            if (pointsLeft < 4)
                            {
                                return (line.Count, true);
                            }
                            return (i + 1, false);
                        }
                        else
                        {
                            var pointsLeft = line.Count - i+1;
                            if (pointsLeft < 4)
                            {
                                return (line.Count, true);
                            }
                            return (i + 2, false);
                        }

                    }
                    var points = line.Count - i;
                    if (points <= 4) // TODO bug probably in this if...
                    {
                        return (line.Count, true);
                    }
                    return (i + 1, false);
                }
            }
            return (line.Count, true);
        }

        // Method to create List of Segment objects from instance variables
        public List<Segment> GetCharSegments()
        {
            List<Segment> segments = new List<Segment>();
            for (int i = 0; i < _segments2D.Count; i++)
            {
                if (_boundBox2D.Count == 4)
                {
                    Segment s = new Segment(i, _segments2D[i], _boundBox2D);
                    segments.Add(s);
                }
                else
                {
                    Debug.Log("_boundBox2D has "+ _boundBox2D.Count+" points");
                }
            }

            return segments;
        }

        // Method to create List of Segment objects from extern list of Vector2's
        public List<Segment> GetCharSegment(List<List<Vector2>> seg2D, List<Vector2> box)
        {
            List<Segment> segments = new List<Segment>();
            for (int i = 0; i < seg2D.Count; i++)
            {
                if (box.Count == 4)
                {
                    Segment s = new Segment(i, seg2D[i], box);
                    segments.Add(s);
                }
            }

            return segments;
        }

        // Method to find plane fitting best through all points from drawing
        private void FindPlane()
        {
            _supportVector = CalcSupportVector();

            var v = CalcDecomp();
            var dirVec1 = v.Column(0); // => direction vectors
            var dirVec2 = v.Column(1);
            var normalVec = v.Column(2);

            float i = dirVec1.DotProduct(dirVec2);

            var dv1 = new Vector3(dirVec1[0], dirVec1[1], dirVec1[2]).normalized;
            var dv2 = new Vector3(dirVec2[0], dirVec2[1], dirVec2[2]).normalized;
            _normalVector = new Vector3(normalVec[0], normalVec[1], normalVec[2]).normalized;

            var x = Vector3.Dot(dv1, Vector3.right);
            var y = Vector3.Dot(dv2, Vector3.up);
            var ux = Vector3.Dot(dv1, Vector3.up);
            var ry = Vector3.Dot(dv2, Vector3.right);
            
            // _dirVec1 soll nach rechts zeigen und _dirVec2 nach oben
            _directVector1 = dv1;
            _directVector2 = dv2;
            if (x >= 0.5)
            {
                // dv1 zeigt nach rechts
                if (y > 0.5)
                {
                    // dv2 zeigt nach oben
                    _directVector1 = dv1;
                    _directVector2 = dv2;
                }
                if (y < -0.5)
                {
                    // dv2 zeight nach unten
                    _directVector1 = dv1;
                    _directVector2 = -1 * dv2;
                }
            }
            if (x < -0.5)
            {
                // dv1 zeigt nach links
                if (y > 0.5)
                {
                    // dv2 zeigt nach oben
                    _directVector2 = dv2;
                    _directVector1 = -1 * dv1;
                }
                if (y < -0.5)
                {
                    // dv2 zeight nach unten
                    _directVector2 = -1 * dv2;
                    _directVector1 = -1 * dv1;
                }
            }
            if (ux >= 0.5)
            {
                // dv1 zeigt nach oben soll aber nach rechts zeigen
                if (ry > 0.5)
                {
                    // dv2 zeigt nach rechts
                    _directVector1 = dv2;
                    _directVector2 = dv1;
                }
                if (ry < -0.5)
                {
                    // dv2 zeight nach links
                    _directVector1 = -1 * dv2;
                    _directVector2 = dv1;
                }
            }
            if (ux <= -0.5)
            {
                // dv1 zeigt nach unten soll aber nach rechts zeigen
                if (ry > 0.5)
                {
                    // dv2 zeigt nach rechts
                    _directVector1 = dv2;
                    _directVector2 = -1 * dv1;
                }
                if (ry < -0.5)
                {
                    // dv2 zeight nach links
                    _directVector1 = -1 * dv2;
                    _directVector2 = -1 * dv1;
                }
            }

            if (Camera.main != null)
            {
                var cameraVector = Camera.main.transform.forward;
                if (Vector3.Dot(_normalVector, cameraVector.normalized) < 0)
                {
                    _normalVector *= -1;
                }
            }

        }
  
        // Method to find center of drawing
        private Vector3 CalcSupportVector()
        {
            Vector3 center = new Vector3(0, 0, 0);
            _numberOfPoints = 0;
            foreach (List<Vector3> segment in _segments3D)
            {
                for (int i = 0; i < segment.Count; i++)
                {
                    center.x += segment[i].x;
                    center.y += segment[i].y;
                    center.z += segment[i].z;
                    _numberOfPoints++;
                }
            }
            center /= _numberOfPoints;
            return center;
        }

        // Method to calculate singular vector decomposition and get vectors to span fitting plane
        private Matrix<float> CalcDecomp()
        {
            // make a matrix out of all points which is a vector list
            var A = Matrix<float>.Build;
            float[,] vectorArray = new float[_numberOfPoints, 3];
            int count = 0;
            for (int i = 0; i < _segments3D.Count; i++)
            {
                for (int j = 0; j < _segments3D[i].Count; j++)
                {
                    vectorArray[count, 0] = _segments3D[i][j].x;
                    vectorArray[count, 1] = _segments3D[i][j].y;
                    vectorArray[count, 2] = _segments3D[i][j].z;
                    count++;
                }
            }

            var a = A.DenseOfArray(vectorArray);
            var decomp = a.Svd(true);
            var v = decomp.VT.Transpose(); 
            // returns 3x3 matrix where the first 2 colums are "Richtungvektoren" and the 3. is normal vector to plane.
            return v;
        }

        // Method to get projected Vector3
        private Vector3 ProjectToPlane(Vector3 v)
        {
            Vector3 result;
            var factor = Vector3.Dot((v - _supportVector), _normalVector);
            var div = Vector3.Dot(_normalVector, _normalVector);
            result = v - (factor / div) * _normalVector;

            return result;
        }
        
        // Method to get 3D bounding box of drawn letter
        private List<Vector3> GetBoundingBox3D(List<List<Vector3>> projSeg3D)
        {
            var v = Vector3.Dot(_directVector1, projSeg3D[0][0]);
            var w = Vector3.Dot(_directVector2, projSeg3D[0][0]);

            float minX = v;
            float maxX = v;
            float minY = w;
            float maxY = w;

            for (int i = 0; i < projSeg3D.Count; i++)
            {
                for (int j = 0; j < projSeg3D[i].Count; j++)
                {
                    Vector3 point = projSeg3D[i][j];
                    var vi = Vector3.Dot(_directVector1, point);
                    var vj = Vector3.Dot(_directVector2, point);

                    if (vi < minX)
                    {
                        minX = vi;
                    }

                    if (vi > maxX)
                    {
                        maxX = vi;
                    }

                    if (vj < minY)
                    {
                        minY = vj;
                    }

                    if (vj > maxY)
                    {
                        maxY = vj;
                    }
                }
            }

            var boundingBox = new List<Vector3>
            {
                // upper left corner
                _directVector1 * minX + _directVector2 * minY,
                // lower left corner
                _directVector1 * maxX + _directVector2 * minY,
                // lower right corner
                _directVector1 * maxX + _directVector2 * maxY,
                // upper right corner
                _directVector1 * minX + _directVector2 * maxY
            };
            
            return boundingBox;
        }

        // Method to convert Vector3 to Vector2
        private Vector2 Get2DFrom3D(Vector3 v)
        {
            var x = Vector3.Dot(_directVector1, v);
            var y = Vector3.Dot(_directVector2, v);

            return new Vector2(x, y);
        }

        // Method to get list of 2D segments
        public List<List<Vector2>> Get2DSegments()
        {
            return _segments2D ?? null;
        }

        // Method to get list of 3D segments
        public List<List<Vector3>> Get3DSegments()
        {
            return _segments3D;
        }

        // Method to calculate 2D bounding box
        public void CalcBoundBox2D()
        {
            float minX = _segments2D[0][0].x;
            float maxX = _segments2D[0][0].x;
            float minY = _segments2D[0][0].y;
            float maxY = _segments2D[0][0].y;

            for (int i = 0; i < _segments2D.Count; i++)
            {
                for (int j = 0; j < _segments2D[i].Count; j++)
                {
                    var vi = _segments2D[i][j].x;
                    var vj = _segments2D[i][j].y;

                    if (vi < minX)
                    {
                        minX = vi;
                    }

                    if (vi > maxX)
                    {
                        maxX = vi;
                    }

                    if (vj < minY)
                    {
                        minY = vj;
                    }

                    if (vj > maxY)
                    {
                        maxY = vj;
                    }
                }
            }

            _boundBox2D = new List<Vector2>
            {
                // upper left corner
                new (minX, minY),
                new (maxX, minY),
                new (maxX, maxY),
                new (minX, maxY)
            };
        }
        
        // Method to get 2D bounding box of drawn and normalized letter
        public List<Vector2> GetBoundBox2D()
        {
            return _boundBox2D;
        }

        // Method to reset all instance variables
        public void ResetVariables()
        {
            _segments3D = new List<List<Vector3>>();
            _segments2D = new List<List<Vector2>>();
            _boundBox2D = new List<Vector2>();
            _numberOfPoints = 0;
            _supportVector = new Vector3();
            _directVector1 = new Vector3();
            _directVector2 = new Vector3();
            _normalVector = new Vector3();
        }

    }
}