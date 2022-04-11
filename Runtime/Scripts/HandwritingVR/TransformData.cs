using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

namespace HandwritingVR
{
    public class TransformData
    {
        private List<List<Vector3>> _segments3D;
        private List<List<Vector2>> _segments2D;
        
        private int _numberOfPoints;
        private Vector3 _supportVector;
        private Vector3 _normalVector;
        private Vector3 _directVector1;
        private Vector3 _directVector2;

        public TransformData(List<List<Vector3>> data)
        {
            _segments3D = data;
            GetSegments2D();
        }

        private void GetSegments2D()
        {
            SegmentLines(ProjectSegments2D());
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

        private void SegmentLines(List<List<Vector2>> projectedSegments)
        {
            _segments2D = projectedSegments; // segment in 2D;
            var tmpProjSeg = projectedSegments;
            
            // Find all Segments
            // break continuous sharp angles, keep arcs
            foreach (var continuousLine in tmpProjSeg)
            {
                var count = 0;
                for (int i = 0; i < continuousLine.Count; i++)
                {
                    var startPoint = continuousLine[i];
                    // TODO SEGMENTATION Method!!!
                    if (true)
                    {
                        List<Vector2> segment = new List<Vector2>();
                        _segments2D.Add(segment);
                    }
                }
            }

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

            var dv1 = new Vector3(dirVec1[0], dirVec1[1], dirVec1[2]).normalized;
            var dv2 = new Vector3(dirVec2[0], dirVec2[1], dirVec2[2]).normalized;
            _normalVector = new Vector3(normalVec[0], normalVec[1], normalVec[2]).normalized;

            var x = Vector3.Dot(dv1, Vector3.right);
            var y = Vector3.Dot(dv2, Vector3.up);
            var ux = Vector3.Dot(dv1, Vector3.up);
            var ry = Vector3.Dot(dv2, Vector3.right);
            
            Debug.Log("x = " + x + ", y = " + y + ", ux = " + ux +", ry = " + ry);
            // _dirVec1 soll nach rechts zeigen und _dirVec2 nach oben
            _directVector1 = dv1;
            _directVector2 = dv2;
            if (x >= 0.5)
            {
                // dv1 zeigt nach rechts
                Debug.Log("1");
                if (y > 0.5)
                {
                    Debug.Log("2");
                    // dv2 zeigt nach oben
                    _directVector1 = dv1;
                    _directVector2 = dv2;
                }
                if (y < -0.5)
                {
                    Debug.Log("3");
                    // dv2 zeight nach unten
                    _directVector1 = dv1;
                    _directVector2 = -1 * dv2;
                }
            }
            Debug.Log("4");
            if (x < -0.5)
            {
                Debug.Log("5");
                // dv1 zeigt nach links
                if (y > 0.5)
                {
                    Debug.Log("5");
                    // dv2 zeigt nach oben
                    _directVector2 = dv2;
                    _directVector1 = -1 * dv1;
                }
                if (y < -0.5)
                {
                    Debug.Log("6");
                    // dv2 zeight nach unten
                    _directVector2 = -1 * dv2;
                    _directVector1 = -1 * dv1;
                }
            }
            Debug.Log("7");
            if (ux >= 0.5)
            {
                Debug.Log("8");
                // dv1 zeigt nach oben soll aber nach rechts zeigen
                if (ry > 0.5)
                {
                    Debug.Log("9");
                    // dv2 zeigt nach rechts
                    _directVector1 = dv2;
                    _directVector2 = dv1;
                }
                if (ry < -0.5)
                {
                    Debug.Log("10");
                    // dv2 zeight nach links
                    _directVector1 = -1 * dv2;
                    _directVector2 = dv1;
                }
            }
            Debug.Log("11");
            if (ux <= -0.5)
            {
                Debug.Log("12");
                // dv1 zeigt nach unten soll aber nach rechts zeigen
                if (ry > 0.5)
                {
                    Debug.Log("13");
                    // dv2 zeigt nach rechts
                    _directVector1 = dv2;
                    _directVector2 = -1 * dv1;
                }
                if (ry < -0.5)
                {
                    Debug.Log("14");
                    // dv2 zeight nach links
                    _directVector1 = -1 * dv2;
                    _directVector2 = -1 * dv1;
                }
            }

            if (Camera.main != null)
            {
                var cameraVector = Camera.main.transform.forward;
                Debug.Log("camVec: (x,y,z) (" + cameraVector.x + ", " + cameraVector.y + ", " + cameraVector.z + ")");
                if (Vector3.Dot(_normalVector, cameraVector.normalized) < 0)
                {
                    Debug.Log("change normal direction");
                    _normalVector *= -1;
                }
            }

            Debug.Log("dirVec1: " + _directVector1);
            Debug.Log("dirVec1: (x,y,z) (" + _directVector1.x + ", " + _directVector1.y + ", " + _directVector1.z + ")");
            Debug.Log("dirVec2: (x,y,z) (" + _directVector2.x + ", " + _directVector2.y + ", " + _directVector2.z + ")");
            Debug.Log("normVec: (x,y,z) (" + _normalVector.x + ", " + _normalVector.y + ", " + _normalVector.z + ")");
        }
        
        private Vector3 CalcSupportVector()
        {
            Debug.Log("CalcSupportVector() called");
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
            Debug.Log("calc 3x3 matrix v = " + v.ToString());
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
    }
}