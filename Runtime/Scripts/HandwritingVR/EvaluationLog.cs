using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace HandwritingVR
{
    // Class to log evaluation data in text or JSON files 
    public class EvaluationLog : MonoBehaviour
    {
        private string _dirName;
        private static string _rawDrawingDataFile;
        private static string _phrasesFile; 
        private static string _timeStampFile;
        private static string _recognizedCharFile;

        private StreamWriter _recCharWriter;

        private string _phrases;
        private int _counter; 
        
        private string _dataPath = "Packages/handwriting/Assets/EvaluationData";
        
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

        // Method to log drawing data in JSON file
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

        // Method to set which phrases are copied
        public void LogPhrases(string text)
        {
            _phrases = text;
        }

        // Method to log copied phrases and timestamps
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
        }

        // Method to log which characters were recognized for the drawing in text file
        public void LogFoundChars(string str)
        {
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