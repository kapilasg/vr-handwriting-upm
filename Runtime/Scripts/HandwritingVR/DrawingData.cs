using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;
using Plane = UnityEngine.Plane;
using Vector3 = UnityEngine.Vector3;

namespace HandwritingVR
{
    public class DrawingData : MonoBehaviour
    {
        private List<LineRenderer> _drawnLines;
        private List<List<Vector3>> _segments;
        private int _numberOfPoints;
        private Vector3 _supportVector;
        private Vector3 _normalVector;
        private Vector3 _directVector1;
        private Vector3 _directVector2;
        private Plane _plane;
        private List<List<Vector3>> _projectedSegments;
        private List<Vector3> _boundingBox;
        private List<List<Vector2>> _normalizedSegments;
        private Boolean letterDone = false;

        public DrawingData()
        {
            _drawnLines = new List<LineRenderer>();
            _segments = new List<List<Vector3>>();
        }

        // Adds LineRenderer Object to the List _drawnLines.
        public void AddLine(LineRenderer line)
        {
            _drawnLines.Add(line);
            Debug.Log("DrawingData: added Line");
        }

        // Updates the last added line in _drawnLines with the new Line.
        public void UpdateLine(LineRenderer line)
        {
            Debug.Log("DrawingData: updated Line");
            int n = _drawnLines.Count;
            _drawnLines[n - 1] = line;
            Debug.Log("DrawingData: updated Line");
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
            Debug.Log("DrawingData: Finished Letter");
            SetPoints();
            SetPlane();
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

        public Boolean GetLetterDone()
        {
            Debug.Log("GetLetterDone "+letterDone);
            return letterDone;
        }

        public void SetLetterDone(Boolean done)
        {
            letterDone = done;
            RemoveAllLines();
        }

        // call this method when the character is finished drawing
        private void SetPoints()
        {
            if (_segments == null)
            {
                Debug.Log("_points is null");
                return;
            }

            foreach (var line in _drawnLines)
            {
                int numberOfPoints = line.positionCount;
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

                    _segments.Add(segmentPoints);

                    Debug.Log("_segments.Count = " + _segments.Count);

                }
                /*
                    _points.Add(line.GetPosition(0));
                    _points.Add(line.GetPosition(numberOfPoints / 2));
                    _points.Add(line.GetPosition(numberOfPoints - 1));
                    */
            }

            int count = 0;
            for (var index = 0; index < _segments.Count; index++)
            {
                var segment = _segments[index];
                for (int j = 0; j < segment.Count; j++)
                {
                    count++;
                }
            }

            _numberOfPoints = count;
            Debug.Log("_numberOfPoints" + _numberOfPoints);
        }

        private void SetPlane()
        {
            int numberOfPoints = _segments.Count;
            if (numberOfPoints < 3)
            {
                return;
            }

            FindPlane();
        }

        private void FindPlane()
        {
            Debug.Log("FindPlane() called");
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

            _directVector1 = new Vector3(dirVec1[0], dirVec1[1], dirVec1[2]).normalized;
            _directVector2 = new Vector3(dirVec2[0], dirVec2[1], dirVec2[2]).normalized;
            _normalVector = new Vector3(normalVec[0], normalVec[1], normalVec[2]).normalized;
            Debug.Log("dirVec1: " + _directVector1);
            Debug.Log("dirVec1: (x,y,z)" + _directVector1.x + ", " + _directVector1.y + ", " + _directVector1.z + ")");
            Debug.Log("dirVec2: (x,y,z)" + _directVector2.x + ", " + _directVector2.y + ", " + _directVector2.z + ")");
            Debug.Log("normVec: (x,y,z)" + _normalVector.x + ", " + _normalVector.y + ", " + _normalVector.z + ")");
            _plane.SetNormalAndPosition(_normalVector, _supportVector);
            Debug.Log("plane: " + _plane);
        }

        private Vector3 ProjectToPlane(Vector3 v)
        {
            Vector3 result;
            var distance = _plane.GetDistanceToPoint(v);
            var factor = Vector3.Dot((v - _supportVector), _normalVector);
            var div = Vector3.Dot(_normalVector, _normalVector);
            result = v - (factor / div) * _normalVector;

            // TODO convert 3D vectors to 2D vectors
            return result;
        }

        private Vector3 CalcSupportVector()
        {
            Debug.Log("CalcSupportVector() called");
            Vector3 center = new Vector3(0, 0, 0);
            foreach (List<Vector3> segment in _segments)
            {
                for (int i = 0; i < segment.Count; i++)
                {
                    center.x += segment[i].x;
                    center.y += segment[i].y;
                    center.z += segment[i].z;
                }
            }

            Debug.Log("numberOfPoints in calcSupportvector = " + _numberOfPoints);
            center /= _numberOfPoints;
            Debug.Log("center (x,y,z): (" + center.x + ", " + center.y + ", " + center.z + ")");
            return center;
        }

        private Matrix<float> CalcDecomp()
        {
            Debug.Log("CalcDecomp() called");
            // make a matrix out of all points which is a vector list
            var A = Matrix<float>.Build;
            float[,] vectorArray = new float[_numberOfPoints, 3];
            Debug.Log("numberOfPoints = " + _numberOfPoints);
            int count = 0;
            for (int i = 0; i < _segments.Count; i++)
            {
                for (int j = 0; j < _segments[i].Count; j++)
                {
                    //Debug.Log("count =" + count + ", i =" + i + ", j =" + j);
                    vectorArray[count, 0] = _segments[i][j].x;
                    vectorArray[count, 1] = _segments[i][j].y;
                    vectorArray[count, 2] = _segments[i][j].z;
                    count++;
                }
            }

            var a = A.DenseOfArray(vectorArray);
            var decomp = a.Svd(true);
            var v = decomp.VT
                .Transpose(); // returns 3x3 matrix where the first 2 colums are "Richtungvektoren" and the 3. is normal vector to plane.
            Debug.Log("calc 3x3 matrix v = " + v.ToString());
            return v;
        }

        // This method returns the supportVector (Focus point).
        public Vector3 GetSupportVector()
        {
            return _supportVector;
        }

        // This method returns the normal vector to create the plane.
        public Vector3 GetNormalVector()
        {
            return _normalVector;
        }

        // This method returns a directional vector (can be used with GetDirectVector2() to span a plane).
        public Vector3 GetDirectVector1()
        {
            return _directVector1;
        }

        // This method returns a directional vector (can be used with GetDirectVector1() to span a plane).
        public Vector3 GetDirectVector2()
        {
            return _directVector2;
        }

        // This method returns a Plane Object with smallest distance to all points.
        public Plane GetPlane()
        {
            return _plane;
        }

        // This method returns the number of points collected so far.
        public int GetNumberOfPoints()
        {
            return _numberOfPoints;
        }

        public List<List<Vector3>> GetProjectedSegments()
        {
            _projectedSegments = new List<List<Vector3>>();
            //Debug.Log("segment count"+_segments.Count);
            for (int i = 0; i < _segments.Count; i++)
            {
                List<Vector3> projectedSegment = new List<Vector3>();
                for (int j = 0; j < _segments[i].Count; j++)
                {
                    projectedSegment.Add(ProjectToPlane(_segments[i][j]));
                }

                _projectedSegments.Add(projectedSegment);
            }
            //Debug.Log("projected segments" + _projectedSegments.ToString());
            return _projectedSegments;
        }

        public List<List<Vector2>> GetNormalizedSegments()
        {
            var proj2D = new List<List<Vector2>>();
            if (_boundingBox == null)
            {
                var list = GetBoundingBox();
            }

            var lowLeftCorner = Get2DFrom3D(_boundingBox[0]);
            var lowrightCorner = Get2DFrom3D(_boundingBox[1]);
            var uppLeftCorner = Get2DFrom3D(_boundingBox[3]);
            var llx = lowLeftCorner.x;
            var lly = lowLeftCorner.y;
            var dx = lowrightCorner.x - lowLeftCorner.x;
            var dy = uppLeftCorner.y - lowLeftCorner.y;

            var matrixBuilder = Matrix<float>.Build;
            var vectorBuilder = Vector<float>.Build;
            float[,] rotArray = {{0, 1}, {-1, 0}};
            float[,] reflexArray = {{1, 0}, {0, -1}};
            var rotMatrix = matrixBuilder.DenseOfArray(rotArray);
            var reflexMatrix = matrixBuilder.DenseOfArray(reflexArray);
            var transMatrix = rotMatrix; // rotMatrix.Multiply(reflexMatrix);
            foreach (var segment in _projectedSegments)
            {
                List<Vector2> projSeg = new List<Vector2>();
                foreach (var point in segment)
                {
                    var newPoint = Get2DFrom3D(point);
                    newPoint.x -= llx;
                    newPoint.x *= 1 / dx;
                    newPoint.y -= lly;
                    newPoint.y *= 1 / dy;

                    float[] varr = {newPoint.x, newPoint.y};
                    var v = vectorBuilder.Dense(varr);

                    var np = transMatrix.Multiply(v);
                    projSeg.Add(new Vector2(np.At(0), np.At(1)));
                }

                proj2D.Add(projSeg);
            }

            /* Debug with bounded box projection:
             List<Vector2> box2D = new List<Vector2>();
             foreach (var corner in _boundingBox)
            {
                var corner2D = Get2DFrom3D(corner);
                // shift and scale box 
                corner2D.x -= llx;
                corner2D.x *= 1 / dx;
                corner2D.y -= lly;
                corner2D.y *= 1 / dy;
                float[] varr = new float[2];
                varr[0] = corner2D.x;
                varr[1] = corner2D.y;
                var v = vectorBuilder.Dense(varr);
                var np = transMatrix.Multiply(v);
                Debug.Log("Box corners"+new Vector2(np.At(0), np.At(1)));

                box2D.Add(new Vector2(np.At(0)+1, np.At(1)+1));
            }*/

            return proj2D;
        }

        public List<Vector3> GetBoundingBox()
        {
            if (_projectedSegments == null)
            {
                GetProjectedSegments();
            }

            // Debug.Log("projectedSegment " + _projectedSegments.Count);
            var v = Vector3.Dot(_directVector1, _projectedSegments[0][0]);
            var w = Vector3.Dot(_directVector2, _projectedSegments[0][0]);

            float minX = v;
            float maxX = v;
            float minY = w;
            float maxY = w;

            for (int i = 0; i < _projectedSegments.Count; i++)
            {
                for (int j = 0; j < _projectedSegments[i].Count; j++)
                {
                    Vector3 point = _projectedSegments[i][j];
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

            _boundingBox = new List<Vector3>
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

            return _boundingBox;
        }

        public List<Vector2> Get2DBoundingBox()
        {

            _normalizedSegments = GetNormalizedSegments();

            Vector2 v = _normalizedSegments[0][0];
            float minX = v.x;
            float maxX = v.x;
            float minY = v.y;
            float maxY = v.y;
            for (int i = 0; i < _normalizedSegments.Count; i++)
            {
                for (int j = 0; j < _normalizedSegments[i].Count; j++)
                {
                    Vector3 point = _normalizedSegments[i][j];
                    if (point.x < minX)
                    {
                        minX = point.x;
                    }

                    if (point.x > maxX)
                    {
                        maxX = point.x;
                    }

                    if (point.y < minY)
                    {
                        minY = point.y;
                    }

                    if (point.y > maxY)
                    {
                        maxY = point.y;
                    }

                }
            }

            Vector2 vx = new Vector2(1, 0);
            Vector2 vy = new Vector2(0, 1);
            List<Vector2> box = new List<Vector2>
            {
                // lower left corner
                vx * minX + vy * minY,
                // lower right corner
                vx * maxX + vy * minY,
                // upper right corner
                vx * maxX + vy * maxY,
                // upper left corner
                vx * minX + vy * maxY
            };
            return box;
        }

        private Vector2 Get2DFrom3D(Vector3 v)
        {
            var x = Vector3.Dot(_directVector1, v);
            var y = Vector3.Dot(_directVector2, v);

            return new Vector2(x, y);
        }
    }
}