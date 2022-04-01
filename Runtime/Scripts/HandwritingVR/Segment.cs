using System;
using System.Collections.Generic;
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

        /*public String EX;
        public String EY;*/
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

        /*public String VC;
        public String HC;*/
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

        private void SetFuzzyValues()
        {
            SetHP();
            SetVP();
            SetHLEN();
            SetVLEN();
            SetSLEN();
            SetSX();
            SetSY();
            SetMSTRandMARC();
            SetVL();
            SetHL();
            SetPS();
            SetNS();
            SetAL();
            SetDL();
            SetCL();
            SetUL();
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
            var hp = (xCenter - _segBoundBox[0].x) / (_segBoundBox[1].x - _segBoundBox[0].x);
            HP = MembershipFunction(hp);
        }

        private void SetVP()
        {
            float yCenter = (_segBoundBox[1].y + _segBoundBox[2].y) / 2;
            var vp = (yCenter - _segBoundBox[1].y) / (_segBoundBox[2].y - _segBoundBox[1].y);
            VP = MembershipFunction(vp);
        }

        private void SetHLEN()
        {
            float d = (_points[0] - _points[^1]).magnitude;
            float width = _charBoundBox[1].x - _charBoundBox[0].x;
            var hlen = d / width;
            HLEN = MembershipFunction(hlen);
        }

        private void SetVLEN()
        {
            float d = (_points[0] - _points[^1]).magnitude;
            float height = _charBoundBox[2].y - _charBoundBox[1].y;
            var vlen = d / height;
            VLEN = MembershipFunction(vlen);
        }

        private void SetSLEN()
        {
            float d = (_points[0] - _points[^1]).magnitude;
            float slant = (_charBoundBox[0] - _charBoundBox[2]).magnitude;
            var slen = d / slant;
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
            // TODO verify correctness
            float d = (_points[0] - _points[^1]).magnitude;
            float sum = 0.0f;
            for (int i = 0; i < _points.Count - 1; i++)
            {
                float dl = (_points[i] - _points[i + 1]).magnitude;
                sum += dl;
            }

            var mstr = d / sum;
            var marc = 1 - mstr;
            MSTR = MembershipFunction(mstr);
            MARC = MembershipFunction(marc);
        }

        private float Delta(float x, float b, float c)
        {
            var res = 0.0f;
            var i1 = c - (b / 2);
            var i2 = c + (b / 2);
            if (i1 <= x && x <= i2)
            {
                res = 1 - 2 * Math.Abs((x - c) / 2);
            }

            return res;
        }

        private void SetVL()
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
        }

        private void SetHL()
        {
            Vector2 x = new Vector2(1, 0);
            Vector2 a = _points[^1] - _points[0];
            var angle = Vector2.SignedAngle(x, a);
            if (angle < 0)
            {
                angle += 360;
            }

            var hl = Math.Max(Delta(angle, 90, 0),
                Math.Max(Delta(angle, 90, 180), Delta(angle, 90, 360)));
            HL = MembershipFunction(hl);
        }

        private void SetPS()
        {
            Vector2 x = new Vector2(1, 0);
            Vector2 a = _points[^1] - _points[0];
            var angle = Vector2.SignedAngle(x, a);
            if (angle < 0)
            {
                angle += 360;
            }

            var ps = Math.Max(Delta(angle, 90, 45), Delta(angle, 90, 225));
            PS = MembershipFunction(ps);
        }

        private void SetNS()
        {
            Vector2 x = new Vector2(1, 0);
            Vector2 a = _points[^1] - _points[0];
            var angle = Vector2.SignedAngle(x, a);
            if (angle < 0)
            {
                angle += 360;
            }

            var ns = Math.Max(Delta(angle, 90, 135), Delta(angle, 90, 315));
            NS = MembershipFunction(ns);
        }

        private void SetAL()
        {
            var ys = _segBoundBox[1].y;
            var ye = _segBoundBox[2].y;
            var asum = 0;
            foreach (var point in _points)
            {
                if (point.y > (ys + ye) / 2)
                {
                    asum += 1;
                }
            }

            var al = Math.Min(1, asum / _points.Count);
            AL = MembershipFunction(al);
        }

        private void SetDL()
        {
            var xs = _segBoundBox[0].x;
            var xe = _segBoundBox[1].x;
            var dsum = 0;
            foreach (var point in _points)
            {
                if (point.x > (xs + xe) / 2)
                {
                    dsum += 1;
                }
            }

            var dl = Math.Min(1, dsum / _points.Count);
            DL = MembershipFunction(dl);
        }

        private void SetCL()
        {
            var xs = _segBoundBox[0].x;
            var xe = _segBoundBox[1].x;
            var csum = 0;
            foreach (var point in _points)
            {
                if (point.x < (xs + xe) / 2)
                {
                    csum += 1;
                }
            }

            var cl = Math.Min(1, csum / _points.Count);
            CL = MembershipFunction(cl);
        }

        private void SetUL()
        {
            var ys = _segBoundBox[1].y;
            var ye = _segBoundBox[2].y;
            var usum = 0;
            foreach (var point in _points)
            {
                if (point.y < (ys + ye) / 2)
                {
                    usum += 1;
                }
            }

            var al = Math.Min(1, usum / _points.Count);
            AL = MembershipFunction(al);
        }

        private void SetOL()
        {
            float xCenter = (_segBoundBox[0].x + _segBoundBox[1].x) / 2;
            float yCenter = (_segBoundBox[1].y + _segBoundBox[2].y) / 2;
            Vector2 center = new Vector2(xCenter, yCenter);
            float rExp = ((_segBoundBox[1].x - _segBoundBox[0].x) + (_segBoundBox[2].y - _segBoundBox[1].y)) / 4;
            float rAct = 0;
            var pExp = 2 * Math.PI * rExp;
            float pAct = 0;
            for (int i = 0; i < _points.Count - 1; i++)
            {
                float dl = (_points[i] - _points[i + 1]).magnitude;
                rAct += (_points[i] - center).magnitude;
                pAct += dl;
            }

            rAct += (_points[^1] - center).magnitude;
            rAct = rAct / _points.Count;
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
        }

        // TODO Hockey stick "Jj"(right/left)"l", Walking stick "f"(right/left)"1"

        private string MembershipFunction(float f)
        {
            if (0<= f && f <= 0.0835f)
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