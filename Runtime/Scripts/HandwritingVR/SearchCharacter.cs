using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HandwritingVR
{
    // Class to search in training base for most similar character
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
                characterList.Add(c);
            }
            else
            {
                characterList.Add(c);
            }
        }

        public (char c, float accuracy, List<char> bestMatches) RecognitionMode(List<Segment> segments)
        {
            if (characterList.Count == 0)
            {
                return (' ', 0f, new List<char>());
            }
            char bestChar = ' ';
            int bestValue = 0;
            List<char> bestMatches = new List<char>();
            for (int i = 0; i < characterList.Count; i++)
            {
                int currentValue = characterList[i].CompareCharacters(segments);
                if (bestValue <= currentValue)
                {
                    // only small alphabet considered
                    // skip numbers in trainingsbase
                    if (characterList[i].letter >= 48 && characterList[i].letter <= 57)
                    { 
                        Debug.Log("best char: "+bestChar);
                        break;
                    }
                    bestValue = currentValue;
                    bestChar = characterList[i].letter;
                    if (Char.IsUpper(bestChar))
                    {
                        bestChar = Char.ToLower(bestChar);
                    }
                    if(DuplicateExists(bestChar, bestMatches))
                    {
                        bestMatches.Remove(bestChar); // For best char at end of list
                        bestMatches.Add(bestChar);
                    }
                    else
                    {
                        bestMatches.Add(bestChar);
                    }
                    
                    if (bestMatches.Count > 5)
                    {
                        bestMatches.RemoveRange(0, 1); // remove first element if list is full
                    }
                }
            }
            bestMatches.Reverse();
            float accuracy = bestValue / 19f;
            return (bestChar, accuracy, bestMatches);
        }
        
        /*public void RecognitionMode2(List<Segment> segments)
        {
            // Open trainingsfile with same number of segments
            // TODO later proper Min-Max Inference: 
            // IF segment[0] has VL = Very High
            // AND segment[1] has HL = Very High
            // Then look in trainingsfile for similar structure
            // And RETURN their character 
        }*/

        private static bool DuplicateExists(char c, List<char> list)
        {
            foreach (var t in list)
            {
                if (t == c)
                {
                    return true;
                }
            }

            return false;
        }
    }
}