using System.Collections.Generic;
using UnityEngine;

namespace HandwritingVR
{
    // Class DataCollector contains methods to control data collection
    public class DataCollector : MonoBehaviour
    {
        // Instance variable to collect drawn lines
        private List<LineRenderer> _drawnLines;

        public DataCollector()
        {
            _drawnLines = new List<LineRenderer>();
        }

        // Method to collect lines
        public void AddLine(LineRenderer line)
        {
            _drawnLines.Add(line);
        }

        // Method to update the last added line in _drawnLines with a new line.
        public void UpdateLine(LineRenderer line)
        {
            int n = _drawnLines.Count;
            if (n <= 0) return;
            _drawnLines[n - 1] = line;
        }

        // Method to remove all lines in _drawnLines.
        public void RemoveAllLines()
        {
            int n = _drawnLines.Count;
            _drawnLines.RemoveRange(0, n);
            if (_drawnLines == null)
            {
                Debug.Log("drawnlines is null");
            }
        }

        // Method to retrieve instance variable _drawnLines
        public List<LineRenderer> GetDrawnLines()
        {
            return _drawnLines;
        }
    }
}