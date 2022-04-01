using System;
using System.Collections.Generic;
using UnityEngine;

namespace HandwritingVR
{

    [Serializable]
    class Character
    {
        public char letter;
        public int numberOfSegments;
        public List<Segment> segments;

        public Character(char c, int numOfSegments, List<Segment> list)
        {
            letter = c;
            numberOfSegments = numOfSegments;
            segments = list;
            Debug.Log("Character created!!" + c + "number of segments" + numOfSegments);
        }

        // This method is only called in trainings mode
        public void WriteFuzzyRuleBase()
        {

        }
    }
}