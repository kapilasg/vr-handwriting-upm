using System.Collections.Generic;
using UnityEngine;

namespace HandwritingVR
{
    public class DataCollector : MonoBehaviour
    {
        private List<LineRenderer> _drawnLines;

        public DataCollector()
        {
            _drawnLines = new List<LineRenderer>();
        }

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
        public void RemoveAllLines()
        {
            int n = _drawnLines.Count;
            _drawnLines.RemoveRange(0, n);
            if (_drawnLines == null)
            {
                Debug.Log("drawnlines is null");
            }
        }

        public List<LineRenderer> GetDrawnLines()
        {
            return _drawnLines;
        }
    }
}