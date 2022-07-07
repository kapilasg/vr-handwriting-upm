using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace HandwritingVR
{
    public class EvaluationLog : MonoBehaviour
    {
        // public string participantID; // Create folder with this id and multiple file path in awake method 
                                     // Other Classes will have a EvaluationLog Object with access to these files to write to

        private string _dirName;
        private static string _rawDrawingDataFile;
        private static string _phrasesFile; 
        private static string _timeStampFile;
        private static string _recognizedCharFile;

        private StreamWriter _recCharWriter;

        private string _phrases;
        private int _counter; 
        
        private string _dataPath = "Packages/handwriting/Assets/EvaluationData";

        /*
        private string[] _phrases;  // (PrintText.cs)
        private char[] _recognizedChars; // 
        private int _misclassifiedChars;
        private float _time; // For words per minute
        private int _numberOfWords; // get from phrases
        private int _numberOfChars;
        private int _countBackspace; // == misclassified
        private char[][] _bestMatches; // for each letter
        private string _enteredText; // To compare with phrases for final count of how many letters were wrongly recognized
        private string _folderPath;
        */

        private void Awake()
        {
            int participantID = 0;
            _dirName = _dataPath+"/student_" + participantID;
            while (Directory.Exists(_dirName))
            {
                participantID++;
                _dirName = _dataPath+"/student_" + participantID;
            }
            
            Directory.CreateDirectory(_dirName);
            if (Directory.Exists(_dirName))
            {
                Debug.Log("Directory created");
            }
            _rawDrawingDataFile = _dirName + "/rawDrawingData.json";
            File.Create(_rawDrawingDataFile);
            _phrasesFile = _dirName + "/phrases.txt";
            File.Create(_phrasesFile);
            _timeStampFile = _dirName + "/timeStamps.txt";
            File.Create(_timeStampFile);
            _recognizedCharFile = _dirName + "/recChars.txt";
            File.Create(_recognizedCharFile);
            _counter = 0;
        }

        public static void LogDrawingData(SegmentPoints3D sp3d)
        {
            if (new FileInfo(_rawDrawingDataFile).Length == 0 )
            {
                Debug.Log("json file is empty");
                var newList = new List<SegmentPoints3D>();
                newList.Add(sp3d);
                var segmentsList = new SegmentPoints3DList(newList);
                string writeJson = JsonUtility.ToJson(segmentsList, true);
                File.WriteAllText(_rawDrawingDataFile, writeJson);
            }
            else
            {
                var jsonText = File.ReadAllText(_rawDrawingDataFile);
                var list = JsonUtility.FromJson<SegmentPoints3DList>(jsonText);
                list.Add(sp3d);
                string writeJson = JsonUtility.ToJson(list, true);
                File.WriteAllText(_rawDrawingDataFile, writeJson);
            }
        }

        public void LogPhrases(string text)
        {
            _phrases = text;
            // Debug.Log("_phrases in evalLog: "+_phrases);
        }

        public void LogTimeStamps(string text)
        {
            if (_counter == 0)
            {
                File.WriteAllText(_phrasesFile, _phrases);
                _counter++;
            }
            
            string[] lines = text.Split("\n");
            using (StreamWriter sw = File.AppendText(_timeStampFile))
            {
                foreach (var line in lines)
                {
                    sw.WriteLine(line);
                }
            }
            // sw.WriteLine(lines[0]);
            // sw.WriteLine(lines[1]);
        }

        public void LogFoundChars(string str)
        {
            // StreamWriter sw = File.AppendText(_recognizedCharFile);
            string[] lines = str.Split("\n");

            using (StreamWriter w = File.AppendText(_recognizedCharFile))
            {
                foreach (var line in lines)
                {
                    w.WriteLine(line);
                }
            }
        }

    }
}