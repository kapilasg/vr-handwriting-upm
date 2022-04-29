using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

namespace HandwritingVR
{

    [Serializable]
    public class Segment
    {
        public int segmentNumber;
        private List<Vector2> _points;
        private List<Vector2> _charBoundBox;
        private List<Vector2> _segBoundBox;
        private float _segmentLength;

        // Fuzzy values of segment
        // HP Horizontal Position
        public String HP;

        // Vertical Position
        public String VP;

        // HLEN Horizontal Length
        public String HLEN;

        // VLEN Vertical Length
        public String VLEN;

        // SLEN Slant Length
        public String SLEN;

        // SX Center X
        public String SX;

        // SY Center Y
        public String SY;
        
        // Is a Dot like on i
        public Boolean isDot;
        
        // MSTR Straightness
        public String MSTR;

        // MARC Arc-ness
        public String MARC;

        // VL Vertical line
        public String VL;

        // HL Horizontal line
        public String HL;

        // Positive slant
        public String PS;

        // Negative slant
        public String NS;
        
        // AL A-Like
        public String AL;

        // DL D-Like
        public String DL;

        // CL C-Like
        public String CL;

        // UL U-Like
        public String UL;

        // OL O-Like
        public String OL;


        public Segment(int number, List<Vector2> segmentPoints, List<Vector2> box)
        {
            segmentNumber = number;
            _points = segmentPoints;
            _charBoundBox = box;
            SetSegmentBox();
            SetFuzzyValues();
        }

        private void SetLength()
        {
            var l = 0f;
            for (int i = 0; i < _points.Count-1; i++)
            {
                l += Vector2.Distance(_points[i], _points[i + 1]);
            }
            _segmentLength = l;
        }
        private void SetFuzzyValues()
        {
            SetHP();
            SetVP();
            SetSX();
            SetSY();
            IsDot();
            SetHLEN();
            SetVLEN();
            SetSLEN();
            SetMSTRandMARC();
            AngleCalculations();
            ArcCalculations();
            SetOL();
        }

        private void SetSegmentBox()
        {
            if (_points == null)
            {
                return;
            }

            float minX = _points[0].x;
            float maxX = _points[0].x;
            float minY = _points[0].y;
            float maxY = _points[0].y;

            foreach (var point in _points)
            {
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

            Vector2 vx = new Vector2(1, 0);
            Vector2 vy = new Vector2(0, 1);
            _segBoundBox = new List<Vector2>
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

        }

        private void SetHP()
        {
            float xCenter = (_segBoundBox[0].x + _segBoundBox[1].x) / 2;
            var hp = (xCenter - _charBoundBox[0].x) / (_charBoundBox[1].x - _charBoundBox[0].x);
            // Debug.Log("relativ Horizontal position: "+hp);
            HP = MembershipFunction(hp);
        }

        private void SetVP()
        {
            float yCenter = (_segBoundBox[1].y + _segBoundBox[2].y) / 2;
            var vp = (yCenter - _charBoundBox[1].y) / (_charBoundBox[2].y - _charBoundBox[1].y);
            // Debug.Log("relativ vertical position: "+vp);
            VP = MembershipFunction(vp);
        }

        private void SetHLEN()
        {
            float d = _segBoundBox[1].x - _segBoundBox[0].x; // Vector2.Distance(_points[0], _points[^1]);
            float width = _charBoundBox[1].x - _charBoundBox[0].x;
            var hlen = d / width;
            if (hlen > 1)
            {
                hlen = 1f;
            }
            // Debug.Log("HLEN value: "+hlen);
            HLEN = MembershipFunction(hlen);
        }

        private void SetVLEN()
        {
            float d = _segBoundBox[2].y - _segBoundBox[1].y; // Vector2.Distance(_points[0], _points[^1]);
            float height = _charBoundBox[2].y - _charBoundBox[1].y;
            var vlen = d / height;
            if (vlen > 1)
            {
                vlen = 1f;
            }
            // Debug.Log("VLEN value: "+vlen);
            VLEN = MembershipFunction(vlen);
        }

        private void SetSLEN()
        {
            float d = Vector2.Distance(_segBoundBox[0], _segBoundBox[2]); // Vector2.Distance(_points[0], _points[^1]);
            float slant = Vector2.Distance(_charBoundBox[0], _charBoundBox[2]);
            var slen = d / slant;
            if (slen > 1)
            {
                slen = 1f;
            }
            // Debug.Log("SLEN value: "+slen);
            SLEN = MembershipFunction(slen);
        }

        private void SetSX()
        {
            float xCenter = (_segBoundBox[0].x + _segBoundBox[1].x) / 2;
            SX = MembershipFunction(xCenter);
        }

        private void SetSY()
        {
            float yCenter = (_segBoundBox[1].y + _segBoundBox[2].y) / 2;
            SY = MembershipFunction(yCenter);
        }

        private void SetMSTRandMARC()
        {
            float d = Vector2.Distance(_points[0], _points[^1]);
            float sum = 0.0f;
            for (int i = 0; i < _points.Count - 1; i++)
            {
                float dl = Vector2.Distance(_points[i], _points[i + 1]);
                sum += dl;
            }

            var mstr = d / sum;
            var marc = 1 - mstr;
            MSTR = MembershipFunction(mstr);
            MARC = MembershipFunction(marc);
            // Debug.Log("SegmentNumber "+segmentNumber+", Straightness: "+MSTR);
            // Debug.Log("SegmentNumber "+segmentNumber+", Arcness: "+MARC);
        }

        private float Delta(float x, float b, float c)
        {
            var res = 0.0f;
            var i1 = c - (b / 2);
            var i2 = c + (b / 2);
            if (i1 <= x && x <= i2)
            {
                res = 1 - 2 * Math.Abs((x - c) / b);
            }

            return res;
        }
        
        private void AngleCalculations()
        {
            Vector2 x = new Vector2(1, 0);
            Vector2 a = _points[^1] - _points[0];
            var angle = Vector2.SignedAngle(x, a);
            if (angle < 0)
            {
                angle += 360;
            }
            
            var vl = Math.Max(Delta(angle, 90, 90), Delta(angle, 90, 270));
            VL = MembershipFunction(vl);
            var hl = Math.Max(Delta(angle, 90, 0),
                Math.Max(Delta(angle, 90, 180), Delta(angle, 90, 360)));
            HL = MembershipFunction(hl);
            var ps = Math.Max(Delta(angle, 90, 45), Delta(angle, 90, 225));
            PS = MembershipFunction(ps);
            var ns = Math.Max(Delta(angle, 90, 135), Delta(angle, 90, 315));
            NS = MembershipFunction(ns);
        }

        private void ArcCalculations()
        {
            var xs = _segBoundBox[0].x;
            var xe = _segBoundBox[1].x;
            var ys = _segBoundBox[1].y;
            var ye = _segBoundBox[2].y;
            var aSum = 0f;
            var uSum = 0f;
            var dSum = 0f;
            var cSum = 0f;
            
            foreach (var point in _points)
            {
                if (point.y > (ys + ye) / 2)
                {
                    aSum += 1;
                }
                if (point.y < (ys + ye) / 2)
                {
                    uSum += 1;
                }
                if (point.x > (xs + xe) / 2)
                {
                    dSum += 1;
                }
                if (point.x < (xs + xe) / 2)
                {
                    cSum += 1;
                }
            }
            var al = Math.Min(1f, aSum / _points.Count);
            var ul = Math.Min(1f, uSum / _points.Count);
            var dl = Math.Min(1f, dSum / _points.Count);
            var cl = Math.Min(1f, cSum / _points.Count);
            CL = MembershipFunction(cl);
            DL = MembershipFunction(dl);
            UL = MembershipFunction(ul);
            AL = MembershipFunction(al);
            
            /*var aLen = 0f;
            var uLen = 0f;
            var dLen = 0f;
            var cLen = 0f;
            for (int i = 0; i < _points.Count-1; i++)
            {
                var p0 = _points[i];
                var p1 = _points[i + 1];
                if (p0.y > (ys + ye) / 2)
                {
                    if (p1.y > (ys + ye) / 2)
                    {
                        aLen += Vector2.Distance(p0, p1);
                    }
                    else
                    {
                        var x = ((p1.x - p0.x) / (p1.y - p0.y)) * (ys + ye) / 2;
                        aLen += Vector2.Distance(p0, new Vector2(x, (ys + ye) / 2));
                    }
                }
                
                if (p0.y < (ys + ye) / 2)
                {
                    if (p1.y < (ys + ye) / 2)
                    {
                        uLen += Vector2.Distance(p0, p1);
                    }
                    else
                    {
                        var x = ((p1.x - p0.x) / (p1.y - p0.y)) * (ys + ye) / 2;
                        uLen += Vector2.Distance(p0, new Vector2(x, (ys + ye) / 2));
                    }
                }
                if (p0.x > (xs + xe) / 2)
                {
                    if (p1.x > (xs + xe) / 2)
                    {
                        dLen += Vector2.Distance(p0, p1);
                    }
                    else
                    {
                        var y = ((p1.x - p0.x) / (p1.y - p0.y)) * (xs + xe) / 2;
                        dLen += Vector2.Distance(p0, new Vector2((xs + xe) / 2,y));
                    }
                }
                if (p0.x < (xs + xe) / 2 && p1.x < (xs + xe) / 2)
                {
                    if (p1.x < (xs + xe) / 2)
                    {
                        cLen += Vector2.Distance(p0, p1);
                    }
                    else
                    {
                        var y = ((p1.x - p0.x) / (p1.y - p0.y)) * (xs + xe) / 2;
                        cLen += Vector2.Distance(p0, new Vector2((xs + xe) / 2,y));
                    }
                    
                }
                
            }
            */
            /*
            Debug.Log("number of points: "+_points.Count);
            Debug.Log("C-Like: "+CL+", value: "+cl+" with sum: "+cSum); //+",  length: "+ cLen_segmentLength);
            Debug.Log("D-Like: "+DL+", value: "+dl+" with sum: "+dSum); //+",  length: "+ dLen/_segmentLength);
            Debug.Log("U-Like: "+UL+", value: "+ul+" with sum: "+uSum); //+",  length: "+ uLen_segmentLength);
            Debug.Log("A-Like: "+AL+", value: "+al+" with sum: "+aSum); //+",  length: "+ aLen_segmentLength);
            */
        }

        private void SetOL()
        {
            float xCenter = (_segBoundBox[0].x + _segBoundBox[1].x) / 2;
            float yCenter = (_segBoundBox[1].y + _segBoundBox[2].y) / 2;
            Vector2 center = new Vector2(xCenter, yCenter);
            // compare radius
            float rExp = ((_segBoundBox[1].x - _segBoundBox[0].x) + (_segBoundBox[2].y - _segBoundBox[1].y)) / 4;
            float rAct = 0;
            // compare perimeter
            var pExp = 2 * Math.PI * rExp; 
            float pAct = 0;
            for (int i = 0; i < _points.Count - 1; i++)
            {
                float dl = (_points[i] - _points[i + 1]).magnitude;
                rAct += (_points[i] - center).magnitude;
                pAct += dl;
            }

            rAct += (_points[^1] - center).magnitude;
            rAct /= _points.Count;
            float f = (float) (pAct / pExp);
            var g = rAct / rExp;
            var ol1 = 0f;
            var ol2 = 0f;
            if (f <= 1)
            {
                ol1 = f;
            }
            else
            {
                ol1 = 1 / f;
            }

            if (g <= 1)
            {
                ol2 = g;
            }
            else
            {
                ol2 = 1 / g;
            }

            var ol = Math.Min(ol1, ol2);
            OL = MembershipFunction(ol);
            // Debug.Log("O-like: "+OL+", with perimeter ol1: "+ol1+", with radius ol2: "+ol2);
        }

        private void IsDot()
        {
            if (_points.Count <= 3)
            {
                isDot = true;
            }
            else
            {
                isDot = false;
            }
        }
        // Hockey stick "Jj"(right/left)"l", Walking stick "f"(right/left)"1"

        private string MembershipFunction(float f)
        {
            if (0 <= f && f <= 0.0835f)
            {
                return "VS"; // Very Small
            }

            if (0.0835f < f && f <= 0.2505f)
            {
                return "S"; // Small
            }

            if (0.2505f < f && f <= 0.4175f)
            {
                return "SM"; // Small Medium
            }

            if (0.4175f < f && f <= 0.5845f)
            {
                return "M"; // Medium
            }

            if (0.5845f < f && f <= 0.7515f)
            {
                return "LM"; // Large Medium
            }

            if (0.7515f < f && f <= 0.9165f)
            {
                return "L"; // Large
            }

            if (0.9165f < f && f <= 1f)
            {
                return "VL"; // Very Large
            }
            else
            {
                return "ERROR";
            }
        }
    }
}