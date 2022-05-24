using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;

namespace HandwritingVR
{
    public class DrawingData : MonoBehaviour
    {
        public EvaluationLog evalLog;
        
        private List<LineRenderer> _drawnLines;
        private List<List<Vector3>> _segments3D;
        private List<List<Vector2>> _segments2D;
        private List<Vector2> _boundBox2D;
        
        private int _numberOfPoints;
        private Vector3 _supportVector;
        private Vector3 _normalVector;
        private Vector3 _directVector1;
        private Vector3 _directVector2;

        private char _foundCharacter = ' ';
        private List<char> _bestResults = new List<char>();
        private StringBuilder _word;

        private int _spaceCounter;
        private int _backspaceCounter;
        private int _byClickCounter;

        public DrawingData()
        {
            _drawnLines = new List<LineRenderer>();
            _segments3D = new List<List<Vector3>>();
            _word = new StringBuilder();
        }

        // Adds LineRenderer Object to the List _drawnLines.
        public void AddLine(LineRenderer line)
        {
            // Debug.Log("Line added!!! with "+line.positionCount+" points");
            _drawnLines.Add(line);
            //Debug.Log("DrawingData: added Line");
        }

        // Updates the last added line in _drawnLines with the new Line.
        public void UpdateLine(LineRenderer line)
        {
            //Debug.Log("DrawingData: updated Line");
            int n = _drawnLines.Count;
            if (n <= 0) return;
            _drawnLines[n - 1] = line;

            //Debug.Log("DrawingData: updated Line");
        }

        // Removes all lines in _drawnLines.
        private void RemoveAllLines()
        {
            int n = _drawnLines.Count;
            _drawnLines.RemoveRange(0, n);
            if (_drawnLines == null)
            {
                Debug.Log("drawnlines is null");
            }
        }

        public char FinishedLetter()
        {
            SetPoints();
            if (_numberOfPoints == 0)
            {
                return ' ';
            }
            if (_numberOfPoints > 0 && _numberOfPoints <= 3)
            {
                // _word.Append('.');
                return '.'; 
            }
            
            _segments2D = ProjectSegments2D();
            
            _boundBox2D = calcBoundBox2D();
            SegmentLines();

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
            
            // string trainingsLetter = "y"; // i, j dot preferably as point but circle should work too
            // StoreDrawing(trainingsLetter);
            // RecoverDrawing(trainingsLetter);
            // Character letter = new Character(trainingsLetter[0], _segments2D.Count, segments);
            // TrainingsMode(letter);
            
            var found = RecognizeCharacter(segments);
            _foundCharacter = found.c;
            _bestResults = found.bestMatches;
            if (_foundCharacter != ' ')
            {
                _word.Append(_foundCharacter.ToString().ToLower());
            }

            string logFoundChar = _word + "\n";
            logFoundChar += "Found: "+_foundCharacter+" \n";
            logFoundChar += "Top five: " + string.Join(", ", _bestResults) + " \n";
            evalLog.LogFoundChars(logFoundChar);
            
            Debug.Log("Found the following character " + found.c + " with the accuracy: " +
                      (found.accuracy * 100) / _segments2D.Count + "%");
            
            ResetVariables();
            return found.c;
        }

        private void ResetVariables()
        {
            RemoveAllLines();
            _segments3D = new List<List<Vector3>>();
            _segments2D = new List<List<Vector2>>();
            _boundBox2D = new List<Vector2>();
            _numberOfPoints = 0;
            _supportVector = new Vector3();
            _directVector1 = new Vector3();
            _directVector2 = new Vector3();
            _normalVector = new Vector3();
        }
        
        private void SetPoints()
        {
            // call this method when the character is finished drawing
            if (_segments3D == null)
            {
                Debug.Log("_segments3D is null");
                return;
            }

            // Debug.Log("number of _drawLines: "+_drawnLines.Count);
            // Debug.Log("number of relevant _drawLines: "+ (_drawnLines.Count - 2));
            for (int i = 0; i < _drawnLines.Count-2; i++)
            {
                if (!_drawnLines[i]) continue;
                int numberOfPoints = _drawnLines[i].positionCount;
                // Debug.Log("number of points in line ->" + numberOfPoints);
                List<Vector3> segmentPoints = new List<Vector3>(numberOfPoints);
                for (int j = 0; j < numberOfPoints; j++)
                {
                    segmentPoints.Add(_drawnLines[i].GetPosition(j));
                }
                _segments3D.Add(segmentPoints);
                if (numberOfPoints <= 3)
                {
                    Debug.Log("number of points <= 3 ->" + numberOfPoints);
                }
            }

            StoreDrawing3D();

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
            // Debug.Log("_segments3D.Count = " + _segments3D.Count);
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
            // Debug.Log("(Before) Number of 2D Segments: " + _segments2D.Count);
            var tmpProjSeg = new List<List<Vector2>>(_segments2D);
            _segments2D = new List<List<Vector2>>();
            // Debug.Log("tmpProjSeg size "+tmpProjSeg.Count);
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

            // Debug.Log("Number of 2D Segments: " + _segments2D.Count);
        }

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
            // Debug.Log("(CalcDecomp) numberOfPoints = " + _numberOfPoints);
            // Debug.Log("(CalcDecomp) _segment3D.Count = " + _segments3D.Count);
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
            var v = decomp.VT.Transpose(); 
            // returns 3x3 matrix where the first 2 colums are "Richtungvektoren" and the 3. is normal vector to plane.
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

        private List<Vector2> calcBoundBox2D()
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

            var boundingBox = new List<Vector2>
            {
                // upper left corner
                new (minX, minY),
                new (maxX, minY),
                new (maxX, maxY),
                new (minX, maxY)
            };
            return boundingBox;
        }
        
        public List<Vector2> GetBoundBox2D()
        {
            return _boundBox2D;
        }

        private void WriteToJson(Character c)
        {
            SearchCharacter sc = ReadFromJson(c.numberOfSegments);
            sc.TrainingsMode(c);
            string writeJson = JsonUtility.ToJson(sc);
            File.WriteAllText(Application.dataPath + "/trainingBase"+c.numberOfSegments+".json", writeJson);
            Debug.Log("End of WriteToJson");
        }

        private SearchCharacter ReadFromJson(int fileIndex)
        {
            SearchCharacter sc;
            if (File.Exists(Application.dataPath + "/trainingBase" + fileIndex + ".json"))
            {
                string readJson = File.ReadAllText(Application.dataPath + "/trainingBase"+fileIndex+".json");
                sc = JsonUtility.FromJson<SearchCharacter>(readJson);
            }
            else
            {
                Debug.Log("File with index "+fileIndex+" doesn't exist");
                sc = new SearchCharacter();
            }
            return sc;
        }

        private (char c, float accuracy, List<char> bestMatches) RecognizeCharacter(List<Segment> segments)
        {
            int n = segments.Count;
            SearchCharacter sc = ReadFromJson(n);
            var result = sc.RecognitionMode(segments);
            if (result.c == ' ')
            {
                Debug.Log("Character Not Found!!");
            }

            return (result.c, result.accuracy, result.bestMatches);
        }

        private void TrainingsMode(Character c)
        {
            WriteToJson(c);
        }

        private void StoreDrawing3D()
        {
            SegmentPoints3D sp3d = new SegmentPoints3D(_segments3D);
            EvaluationLog.LogDrawingData(sp3d);
        }
        
        private void StoreDrawing(string fileName)
        {
            SegmentPoints sp = new SegmentPoints(_segments2D, _boundBox2D);
            string json = JsonUtility.ToJson(sp);
            File.WriteAllText(Application.dataPath + "/storedDrawing_"+fileName+".json", json);
            if (File.Exists(Application.dataPath + "/storedDrawing_" + fileName + ".json"))
            {
                Debug.Log("File created!!");
            }
            else
            {
                Debug.Log("File not created!!");
            }
        }

        private void RecoverDrawing(string fileName)
        {
            if (File.Exists(Application.dataPath + "/storedDrawing_"+fileName+".json"))
            {
                string readJson = File.ReadAllText(Application.dataPath + "/storedDrawing_"+fileName+".json");
                SegmentPoints sp = JsonUtility.FromJson<SegmentPoints>(readJson);
                _segments2D = sp.GetPoints();
                _boundBox2D = sp.boundBox;
            }
            else
            {
                Debug.Log("storedDrawing: FileNotFound");
            }
        }

        public char GetCharacter()
        {
            return _foundCharacter;
        }

        public void SetModifiedWord(char c)
        {
            if (c == '-')
            {
                if (_word.Length >= 1)
                {
                    _word.Remove(_word.Length - 1, 1);
                }
            }
            else
            {
                _word.Append(c);
            }
        }

        public string GetWord()
        {
            return _word.ToString();
        }

        public void SetWord()
        {
            _word = new StringBuilder();
        }

        public List<char> GetBestMatches()
        {
            return _bestResults;
        }
        public void SpaceOnClick()
        {
            Debug.Log("Space Button clicked!");
            SetModifiedWord(' ');
            evalLog.LogFoundChars(_word + "\n" + "(Space)" +" \n");
        }
        
        public void BackspaceOnClick()
        {
            Debug.Log("Backspace Button clicked! ");
            SetModifiedWord('-');
            evalLog.LogFoundChars(_word + "\n" + "(Backspace)" +" \n");
        }

        public void LetterOnClick(Text c)
        {
            SetModifiedWord(c.text[0]);
            evalLog.LogFoundChars(_word + "\n" + "(Corrected / byClick: ) " +c.text[0]+" \n");
        }
    }
}