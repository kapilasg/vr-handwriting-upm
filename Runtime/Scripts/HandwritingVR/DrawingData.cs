using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Messaging;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;
using Plane = UnityEngine.Plane;
using Vector3 = UnityEngine.Vector3;

namespace HandwritingVR
{
    public class DrawingData : MonoBehaviour
    {
        private List<LineRenderer> _drawnLines;
        private List<List<Vector3>> _segments3D;
        private List<List<Vector2>> _segments2D;
        
        private int _numberOfPoints;
        private Vector3 _supportVector;
        private Vector3 _normalVector;
        private Vector3 _directVector1;
        private Vector3 _directVector2;
        
        public DrawingData()
        {
            _drawnLines = new List<LineRenderer>();
            _segments3D = new List<List<Vector3>>();
        }

        // Adds LineRenderer Object to the List _drawnLines.
        public void AddLine(LineRenderer line)
        {
            _drawnLines.Add(line);
            //Debug.Log("DrawingData: added Line");
        }

        // Updates the last added line in _drawnLines with the new Line.
        public void UpdateLine(LineRenderer line)
        {
            //Debug.Log("DrawingData: updated Line");
            int n = _drawnLines.Count;
            _drawnLines[n - 1] = line;
            //Debug.Log("DrawingData: updated Line");
        }

        // Removes all lines in _drawnLines.
        public void RemoveAllLines()
        {
            int n = _drawnLines.Count;
            _drawnLines.RemoveRange(0, n);
            if (_drawnLines == null)
            {
                Debug.Log("drawnlines is null");
            }
        }

        public void FinishedLetter() // char c
        {
            SetPoints();
            Debug.Log("DrawingData: Finished Letter");
            Debug.Log("number of _segments in 3D "+ _segments3D.Count);
            _segments2D = ProjectSegments2D();
            Debug.Log(_segments2D);
            Debug.Log("Seg2D count: "+_segments2D.Count);
            SegmentLines();

            /*
            Character letter = new Character('A', points2D.Count, segments);
            string json = JsonUtility.ToJson(letter);
            File.WriteAllText(Application.dataPath + "/fuzzyRules.json", json);
            */

            /*if (c != ' ')
            {
                List<List<Vector2>> points2D = GetNormalizedSegments();
                List<Segment> segments = new List<Segment>();
                for (int i = 0; i < points2D.Count; i++)
                {
                    Segment s = new Segment(i, points2D[i], Get2DBoundingBox());
                    segments.Add(s);
                }
                Character letter = new Character((char)c, points2D.Count, segments);
            }
            //RemoveAllLines();
            letterDone = true;*/
        }

        // call this method when the character is finished drawing
        private void SetPoints()
        {
            if (_segments3D == null)
            {
                Debug.Log("_points is null");
                return;
            }

            foreach (var line in _drawnLines)
            {
                int numberOfPoints = line.positionCount;
                Debug.Log("number of points in line ->" + numberOfPoints);
                if (numberOfPoints <= 2)
                {
                    // temporary solution for end of letter problem
                    Debug.Log("number of points <= 2 ->" + numberOfPoints);
                    /*Vector3[] tmp = new Vector3[numberOfPoints];
                    line.GetPositions(tmp);
                    foreach (var t in tmp)
                    {
                        _points.Add(t);
                    }*/
                }
                else
                {
                    Debug.Log("number of points = " + numberOfPoints);
                    List<Vector3> segmentPoints = new List<Vector3>(numberOfPoints);
                    for (int i = 0; i < numberOfPoints; i++)
                    {
                        segmentPoints.Add(line.GetPosition(i));
                    }

                    _segments3D.Add(segmentPoints);

                    Debug.Log("_segments3D.Count = " + _segments3D.Count);

                }
                /*
                    _points.Add(line.GetPosition(0));
                    _points.Add(line.GetPosition(numberOfPoints / 2));
                    _points.Add(line.GetPosition(numberOfPoints - 1));
                    */
            }

            int count = 0;
            for (var index = 0; index < _segments3D.Count; index++)
            {
                var segment = _segments3D[index];
                for (int j = 0; j < segment.Count; j++)
                {
                    count++;
                }
            }

            _numberOfPoints = count;
            Debug.Log("_numberOfPoints" + _numberOfPoints);
        }
        
        private List<List<Vector2>> ProjectSegments2D()
        {
            var proj2D = new List<List<Vector2>>();
            
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

                proj2D.Add(projSeg2D);
            }
            
            return proj2D;
        }

        private void SegmentLines()
        {
            // _segments2D = new List<List<Vector2>>(); // segment in 2D;
            Debug.Log("(Before) Number of 2D Segments: " + _segments2D.Count);
            var tmpProjSeg = new List<List<Vector2>>(_segments2D);
            _segments2D = new List<List<Vector2>>();
            Debug.Log("tmpProjSeg size "+tmpProjSeg.Count);
            List<Vector2> segment = new List<Vector2>();
            foreach (var line in tmpProjSeg)
            {
                if (line.Count < 8)
                {
                    Debug.Log("Segment has less than 8 Points and can therefore not further be divided");
                    segment = new List<Vector2>(line);
                    _segments2D.Add(segment);
                    continue;
                }
                var segmInd = FindFirstSegmentIndex(line);
                Debug.Log("segmInd num of points: "+segmInd.numOfPoints);
                Debug.Log("segmInd is last segment: "+segmInd.lastSegment);
                segment = new List<Vector2>(line.GetRange(0, segmInd.numOfPoints));
                _segments2D.Add(segment);
                var restStartIndex = 0;
                while (!segmInd.lastSegment)
                {
                    Debug.Log("In segmenting while loop!");
                    restStartIndex = segmInd.numOfPoints;
                    var restSegment = new List<Vector2>(
                        line.GetRange(restStartIndex, line.Count-restStartIndex));
                    segmInd = FindFirstSegmentIndex(restSegment);
                    var nextSegment = new List<Vector2>(line.GetRange(restStartIndex, segmInd.numOfPoints));
                    _segments2D.Add(nextSegment);
                }
            }

            Debug.Log("(After) Number of 2D Segments: " + _segments2D.Count);
        }

        private (int numOfPoints,bool lastSegment) FindFirstSegmentIndex(List<Vector2> line)
        {
            Debug.Log("(FindFirstSegment) number of points in line: "+ line.Count);
            if (line.Count < 8)
            {
                Debug.Log("Too short to segment"+ line.Count);
                return (line.Count, true);
            }
            
            // Just a straight line:
            var avrAngle = Vector2.Angle(Vector2.right, line[^1] - line[0]);
            var calcAvrAngle = 0f;
            var calcLength = 0f;
            for (int i = 0; i < line.Count-1; i++)
            {
                calcAvrAngle += Vector2.Angle(Vector2.right, line[i+1] - line[i]);
                calcLength += Vector2.Distance(line[i], line[i + 1]);
            }
            calcAvrAngle /= line.Count;
            Debug.Log("avrAngle: "+ avrAngle);
            Debug.Log("calcAvrAngle: "+ calcAvrAngle);
            Debug.Log("avrLength: "+ Vector2.Distance(line[0], line[^1]));
            Debug.Log("calcLength: "+ calcLength);
            Debug.Log("avrAngle - calcAvrAngle: " + Math.Abs(avrAngle - calcAvrAngle));
            if (Math.Abs(avrAngle - calcAvrAngle) < 25 
                && Math.Abs(Vector2.Distance(line[0], line[^1]) - calcLength) < 0.2)
            {
                Debug.Log("Just a straight line.");
                return (line.Count, true);
            }

            var counter = 4; // Each segment has at least 4 points.
            
            for (int i = 0; i < line.Count-5; i++) // Dont look at first and last points (Ungenau/Zittrige hand)
            {
                // var aa = Vector2.Angle(Vector2.right, line[i+2] - line[i]);
                // var ab = Vector2.Angle(Vector2.right, line[i+4] - line[i+2]);
                var a1 = Vector2.SignedAngle(Vector2.right, line[i+2] - line[i]);
                var a2 = Vector2.SignedAngle(Vector2.right, line[i+4] - line[i+2]);
                if (a1 < 0) a1 += 360;
                if (a2 < 0) a2 += 360;
                
                // Debug.Log("angle1 "+i+": " + (int)(aa-ab) + ", (a1 = "+aa+", a2 = "+ab+")");
                Debug.Log("angle "+i+": " + (int)(a1-a2) + ", (a1 = "+(int)(a1)+", a2 = "+(int)(a2)+")");
                
                if (Math.Abs(a1 - a2) < 140)
                {
                    Debug.Log("Unterbruch!!!"); // Don't compress debugs!!! 
                }
                
                //angles.Add((int)(aa-ab));
            }
            /*Debug.Log("(FindFirstSegment) number of points in line: "+ line.Count);
            Debug.Log("(FindFirstSegment) number of angles: "+ angles.Count);
            for (int i = 0; i < angles.Count; i++)
            {
                Debug.Log("angle "+i+": " + angles[i]);
            }*/
            
            return (line.Count, true);


            /*/Debug.Log("Length of line to segment: "+ line.Count);
            var index = 0;
            var a1 = Vector2.Angle(Vector2.right, line[1] - line[0]);
            var a2 = Vector2.Angle(Vector2.right, line[3] - line[2]);
            var threshold = 40;
            // Found Straight Line beginning Segment
            if (a1 - a2 < threshold)
            {
                Debug.Log("Found straight line at the beginning");
                // check rest of segment if it only consist of same angle
                while (a1 - a2 < threshold && index + 4 < line.Count)
                {
                    a1 = Vector2.Angle(Vector2.right, line[index + 1] - line[index]);
                    a2 = Vector2.Angle(Vector2.right, line[index + 3] - line[index + 2]);
                    index++;
                    Debug.Log("index (straight): "+ index);
                }
                Debug.Log("index (straight) at end: "+ index);
                Debug.Log("index (straight) if cond.: "+ (line.Count - index + 1));
                if (line.Count - index + 1 <= 4)
                {
                    Debug.Log("BUT the last points are not for own segment");
                    Debug.Log("Therefore last segment of straight line was found");
                    Debug.Log("index (straight after found): "+ index);
                    return (line.Count - 1, true);
                }
                else
                {
                    Debug.Log("Found end of straight line but not end of segment index: "+ (index+1));
                    Debug.Log("index (straight after found2): "+ index);
                    return (index + 1, false);
                }
            }
            else
            {
                // Arc was found at the beginning
                Debug.Log("Found Arc at the beginning");
                while (a1 - a2 > threshold && a1 - a2 < 60 && index + 4 < line.Count)
                {
                    // search for end of arc
                    a1 = Vector2.Angle(Vector2.right, line[index + 1] - line[index]);
                    a2 = Vector2.Angle(Vector2.right, line[index + 3] - line[index + 2]);
                    index++;
                    Debug.Log("index (arc): "+ index);
                }
                
                if (line.Count - index + 1 <= 4)
                {
                    Debug.Log("BUT the last points are not for own segment");
                    Debug.Log("Therefore last segment of Arc was found");
                    Debug.Log("index (arc after found): "+ index);
                    return (line.Count - 1, true);
                }
                else
                {
                    Debug.Log("Found end of Arc but not end of segment");
                    Debug.Log("index (straight after found2): "+ index);
                    return (index + 1, false);
                }
            }
            */
        }
        
        private void FindPlane()
        {
            //Debug.Log("FindPlane() called");
            _supportVector = CalcSupportVector();

            var v = CalcDecomp();
            var dirVec1 = v.Column(0); // => direction vectors
            var dirVec2 = v.Column(1);
            var normalVec = v.Column(2);

            float i = dirVec1.DotProduct(dirVec2);
            if (i == 0)
            {
                Debug.Log("dirVec1 and dirVec2 are orthogonal to each other!!!");
            }

            var dv1 = new Vector3(dirVec1[0], dirVec1[1], dirVec1[2]).normalized;
            var dv2 = new Vector3(dirVec2[0], dirVec2[1], dirVec2[2]).normalized;
            _normalVector = new Vector3(normalVec[0], normalVec[1], normalVec[2]).normalized;

            var x = Vector3.Dot(dv1, Vector3.right);
            var y = Vector3.Dot(dv2, Vector3.up);
            var ux = Vector3.Dot(dv1, Vector3.up);
            var ry = Vector3.Dot(dv2, Vector3.right);
            
            //Debug.Log("x = " + x + ", y = " + y + ", ux = " + ux +", ry = " + ry);
            // _dirVec1 soll nach rechts zeigen und _dirVec2 nach oben
            _directVector1 = dv1;
            _directVector2 = dv2;
            if (x >= 0.5)
            {
                // dv1 zeigt nach rechts
                //Debug.Log("1");
                if (y > 0.5)
                {
                    //Debug.Log("2");
                    // dv2 zeigt nach oben
                    _directVector1 = dv1;
                    _directVector2 = dv2;
                }
                if (y < -0.5)
                {
                    //Debug.Log("3");
                    // dv2 zeight nach unten
                    _directVector1 = dv1;
                    _directVector2 = -1 * dv2;
                }
            }
            //Debug.Log("4");
            if (x < -0.5)
            {
                //Debug.Log("5");
                // dv1 zeigt nach links
                if (y > 0.5)
                {
                    //Debug.Log("5");
                    // dv2 zeigt nach oben
                    _directVector2 = dv2;
                    _directVector1 = -1 * dv1;
                }
                if (y < -0.5)
                {
                    //Debug.Log("6");
                    // dv2 zeight nach unten
                    _directVector2 = -1 * dv2;
                    _directVector1 = -1 * dv1;
                }
            }
            //Debug.Log("7");
            if (ux >= 0.5)
            {
                //Debug.Log("8");
                // dv1 zeigt nach oben soll aber nach rechts zeigen
                if (ry > 0.5)
                {
                    //Debug.Log("9");
                    // dv2 zeigt nach rechts
                    _directVector1 = dv2;
                    _directVector2 = dv1;
                }
                if (ry < -0.5)
                {
                    //Debug.Log("10");
                    // dv2 zeight nach links
                    _directVector1 = -1 * dv2;
                    _directVector2 = dv1;
                }
            }
            //Debug.Log("11");
            if (ux <= -0.5)
            {
                //Debug.Log("12");
                // dv1 zeigt nach unten soll aber nach rechts zeigen
                if (ry > 0.5)
                {
                    //Debug.Log("13");
                    // dv2 zeigt nach rechts
                    _directVector1 = dv2;
                    _directVector2 = -1 * dv1;
                }
                if (ry < -0.5)
                {
                    //Debug.Log("14");
                    // dv2 zeight nach links
                    _directVector1 = -1 * dv2;
                    _directVector2 = -1 * dv1;
                }
            }

            if (Camera.main != null)
            {
                var cameraVector = Camera.main.transform.forward;
                //Debug.Log("camVec: (x,y,z) (" + cameraVector.x + ", " + cameraVector.y + ", " + cameraVector.z + ")");
                if (Vector3.Dot(_normalVector, cameraVector.normalized) < 0)
                {
                    //Debug.Log("change normal direction");
                    _normalVector *= -1;
                }
            }

            /*
             Debug.Log("dirVec1: " + _directVector1);
            Debug.Log("dirVec1: (x,y,z) (" + _directVector1.x + ", " + _directVector1.y + ", " + _directVector1.z + ")");
            Debug.Log("dirVec2: (x,y,z) (" + _directVector2.x + ", " + _directVector2.y + ", " + _directVector2.z + ")");
            Debug.Log("normVec: (x,y,z) (" + _normalVector.x + ", " + _normalVector.y + ", " + _normalVector.z + ")");
            */
        }
  
        private Vector3 CalcSupportVector()
        {
            //Debug.Log("CalcSupportVector() called");
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

            //Debug.Log("numberOfPoints in calcSupportvector = " + _numberOfPoints);
            center /= _numberOfPoints;
            //Debug.Log("center (x,y,z): (" + center.x + ", " + center.y + ", " + center.z + ")");
            return center;
        }

        private Matrix<float> CalcDecomp()
        {
            //Debug.Log("CalcDecomp() called");
            // make a matrix out of all points which is a vector list
            var A = Matrix<float>.Build;
            float[,] vectorArray = new float[_numberOfPoints, 3];
            //Debug.Log("numberOfPoints = " + _numberOfPoints);
            int count = 0;
            for (int i = 0; i < _segments3D.Count; i++)
            {
                for (int j = 0; j < _segments3D[i].Count; j++)
                {
                    //Debug.Log("count =" + count + ", i =" + i + ", j =" + j);
                    vectorArray[count, 0] = _segments3D[i][j].x;
                    vectorArray[count, 1] = _segments3D[i][j].y;
                    vectorArray[count, 2] = _segments3D[i][j].z;
                    count++;
                }
            }

            var a = A.DenseOfArray(vectorArray);
            var decomp = a.Svd(true);
            var v = decomp.VT
                .Transpose(); // returns 3x3 matrix where the first 2 colums are "Richtungvektoren" and the 3. is normal vector to plane.
            //Debug.Log("calc 3x3 matrix v = " + v.ToString());
            return v;
        }

        private Vector3 ProjectToPlane(Vector3 v)
        {
            Vector3 result;
            var factor = Vector3.Dot((v - _supportVector), _normalVector);
            var div = Vector3.Dot(_normalVector, _normalVector);
            result = v - (factor / div) * _normalVector;

            return result;
        }
        
        private List<Vector3> GetBoundingBox3D(List<List<Vector3>> projSeg3D)
        {

            // Debug.Log("projectedSegment2 " + _projectedSegments.Count);
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

            /*
            Debug.Log("Bounding Box "+ _boundingBox[0]);
            Debug.Log("Bounding Box "+ _boundingBox[1]);
            Debug.Log("Bounding Box "+ _boundingBox[2]);
            Debug.Log("Bounding Box "+ _boundingBox[3]);
            */

            return boundingBox;
        }

        private Vector2 Get2DFrom3D(Vector3 v)
        {
            var x = Vector3.Dot(_directVector1, v);
            var y = Vector3.Dot(_directVector2, v);

            return new Vector2(x, y);
        }

        public List<List<Vector2>> Get2DSegments()
        {
            return _segments2D ?? null;
        }

    }
}