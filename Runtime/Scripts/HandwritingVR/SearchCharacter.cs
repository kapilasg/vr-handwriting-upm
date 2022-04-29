using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HandwritingVR
{
    public class SearchCharacter
    {
        public List<Character> characterList;

        public SearchCharacter()
        {
            characterList = new List<Character>();
        }

        public void TrainingsMode(Character c)
        {
            // Read in json file and add drawn letter + character:
            InsertCharacter(c);
        }

        private void InsertCharacter(Character c)
        {
            if (characterList is null)
            {
                characterList = new List<Character>();
                // TODO Insert all possible segment sequences Reihenfolgen
                // n! Moöglichkeiten
                characterList.Add(c);
            }
            else
            {
                characterList.Add(c);
            }
        }
        
        /*public void RecognitionMode2(List<Segment> segments)
        {
            // Open trainingsfile with same number of segments
            // TODO later Min-Max Inference: 
            // IF segment[0] has VL = Very High
            // AND segment[1] has HL = Very High
            // Then look in trainingsfile for similar structure
            // And RETURN their character 

        }*/

        public (char c, float accuracy) RecognitionMode(List<Segment> segments)
        {
            if (characterList.Count == 0)
            {
                return (' ', 0f);
            }
            char bestChar = ' ';
            int bestValue = 0;
            for (int i = 0; i < characterList.Count; i++)
            {
                int v = characterList[i].CompareCharacters(segments);
                if (bestValue < v)
                {
                    bestValue = v;
                    bestChar = characterList[i].letter;
                }
            }
            // bestValue 
            float accuracy = bestValue / 19f;
            return (bestChar, accuracy);
        }
    }
}