using System.IO;
using UnityEngine;

namespace HandwritingVR
{
    public class EvaluationLog : MonoBehaviour
    {
        public string participantID; // Create folder with this id and multiple file path in awake method 
                                     // Other Classes will have a EvaluationLog Object with access to these files to write to
        private string[] _phrases;  // (PrintText.cs)
        private char[] _recognizedChars; // 
        private int _misclassifiedChars;
        private float _time; // For words per minute
        private int _numberOfWords; // get from phrases
        private int _numberOfChars;
        private int _countBackspace; // == misclassified
        private char[][] _bestmatches; // for each letter
        private string _enteredText; // To compare with phrases for final count of how many letters were wrongly recognized
        private string _folderPath;
        

        // Folder student1
        // --> File with raw drawing data
        // --> File Testphrases (Use this to calculate Words-Per-Minute, count letters)
        // --> File with timestamps per phrase + total time
        // --> File with recognized character 
        //             + Top five best matches
        // Backspace counter?
        // my watch fell in the water
        // m
        // m, w, n, 
        // x
        // y, g, v, l,


        // or

        // Expected: m
        // Best Match: m
        // Top five: m, w, ...

        // Expected: y 
        // Best Match: g
        // Top five: g, y, x, q, f
        // Corrected: yes

        // Expected: " "
        // Found: " "

        // Expected: w
        // Best Match: m
        // Top five: m, x, y, z, k
        // Corrected: Not possible
        // Try again: (if found:)
        // Best Match: m
        // Top five: m, w, x, ...
        // Corrected: yes

        // Try again: (if not found:)
        // Best Match: m
        // Top five: m, y, x, ...
        // Corrected: not possible Go to next letter

    }
}