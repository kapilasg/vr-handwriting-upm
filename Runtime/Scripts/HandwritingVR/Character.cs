using System;
using System.Collections.Generic;
using UnityEngine;

namespace HandwritingVR
{

    [Serializable]
    public class Character
    {
        public char letter;
        public int numberOfSegments;
        public List<Segment> segments;

        public Character(char c, int numOfSegments, List<Segment> list)
        {
            letter = c;
            numberOfSegments = numOfSegments;
            segments = list;
            Debug.Log("Character "+ c +" created!! With number of segments: " + numOfSegments);
        }
        
        
        public int CompareCharacters(List<Segment> providedSegments)
        {
            int compareValue = 0;

            for (int i = 0; i < segments.Count; i++)
            {
                int s = segments[i].CompareSegments(providedSegments[i]);
                compareValue += s;
            }
            return compareValue;
        }
    }
}