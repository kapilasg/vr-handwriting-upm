using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

namespace HandwritingVR
{
    // Class to access raw data from DataCollector class and send it to DataTransformer class
    // and use results to create the text-input and more.
    
    // This class also contains methods relevant for the evaluation
    public class DataManager : MonoBehaviour
    {
        public DataCollector dataCollector;
        public DataTransformer dataTransformer;
        public EvaluationLog evalLog;
        
        private char _foundChar = ' ';
        private List<char> _bestResults = new List<char>();
        private StringBuilder _word;
        
        private float _startTime;
        private bool _timeHandler;
        
        private int _spaceCounter;
        private int _backspaceCounter;
        private int _byClickCounter;
        private int _recognizedCharCounter;
        private int _streamInputCounter;

        private string _dataPath = "Packages/handwriting/Assets/TrainingBase";
        
        public DataManager()
        {
            _word = new StringBuilder();
            _timeHandler = false;
            _spaceCounter = 0;
            _backspaceCounter = 0;
            _byClickCounter = 0;
            _recognizedCharCounter = 0;
            _streamInputCounter = 0;
        }
        
        // Method to get Find Character after drawing it
        // Sets _foundChar and _bestResults
        public char FinishedLetter()
        {
            var numOfPoints = dataTransformer.SetPoints(dataCollector.GetDrawnLines());
            if (numOfPoints == 0)
            {
                return ' ';
            }
            if (numOfPoints > 0 && numOfPoints <= 3)
            {
                // _word.Append('.');
                return '.'; 
            }
            
            dataTransformer.ProjectSegments2D();
            dataTransformer.CalcBoundBox2D();
            dataTransformer.SegmentLines();

            List<Segment> segments = dataTransformer.GetCharSegments();
            
            // string trainingsLetter = " ";
            
            /* Uncomment this to store drawing*/
            // StoreDrawing(trainingsLetter);
            // RecoverDrawing(trainingsLetter);
            
            /* Uncomment this to extend your training base*/
            // Character letter = new Character(trainingsLetter[0], _segments2D.Count, segments);
            // TrainingsMode(letter);
            
            var found = RecognizeCharacter(segments);
            _foundChar = found.c;
            _bestResults = found.bestMatches;
            if (_foundChar != ' ')
            {
                _word.Append(_foundChar.ToString().ToLower());
            }

            _recognizedCharCounter++;
            _streamInputCounter++;

            if (_timeHandler)
            {
                _startTime = Time.time;
                _timeHandler = false;
            }

            string logFoundChar = _word + "\n";
            logFoundChar += "Found: "+_foundChar+" \n";
            logFoundChar += "Top five: " + string.Join(", ", _bestResults) + " \n";
            evalLog.LogFoundChars(logFoundChar);
            
            Debug.Log("Found the following character " + found.c);
            
            ResetVariables();
            return found.c;
        }
        
        // Method to remove all collected data, get ready for next drawn character
        private void ResetVariables()
        {
            dataCollector.RemoveAllLines();
            dataTransformer.ResetVariables();
        }
        
        // Method to store character in JSON format to extend training base
        private void WriteToJson(Character c)
        {
            SearchCharacter sc = ReadFromJson(c.numberOfSegments);
            sc.TrainingsMode(c);
            string writeJson = JsonUtility.ToJson(sc);
            File.WriteAllText(_dataPath + "/trainingBase"+c.numberOfSegments+".json", writeJson);
        }
        
        // Method to read characters from JSON file
        private SearchCharacter ReadFromJson(int fileIndex)
        {
            SearchCharacter sc;
            if (File.Exists(_dataPath + "/trainingBase" + fileIndex + ".json"))
            {
                string readJson = File.ReadAllText(_dataPath + "/trainingBase"+fileIndex+".json");
                sc = JsonUtility.FromJson<SearchCharacter>(readJson);
            }
            else
            {
                // Debug.Log("File with index "+fileIndex+" doesn't exist");
                sc = new SearchCharacter();
            }
            return sc;
        }
        
        // Method to find most similar character 
        private (char c, float accuracy, List<char> bestMatches) RecognizeCharacter(List<Segment> segments)
        {
            int n = segments.Count;
            SearchCharacter sc = ReadFromJson(n);
            var result = sc.RecognitionMode(segments);
            if (result.c == ' ')
            {
                Debug.Log("Character Not Found!!");
            }

            return (result.c, result.accuracy, result.bestMatches);
        }
        
        // Method to activate Training mode
        private void TrainingsMode(Character c)
        {
            WriteToJson(c);
        }
        
        // Helper method to store drawings (useful for evaluation log)
        private void StoreDrawing3D()
        {
            var seg3D = dataTransformer.Get3DSegments();
            SegmentPoints3D sp3d = new SegmentPoints3D(seg3D);
            EvaluationLog.LogDrawingData(sp3d);
        }
        
        // Helper method to store drawing in JSON file
        private void StoreDrawing(string fileName)
        {
            var seg2D = dataTransformer.Get2DSegments();
            var box2D = dataTransformer.GetBoundBox2D();
            SegmentPoints sp = new SegmentPoints(seg2D, box2D);
            string json = JsonUtility.ToJson(sp);
            Debug.Log(File.Exists(Application.dataPath + "/storedDrawing_" + fileName + ".json")
                ? "File created!!"
                : "File not created!!");
            File.WriteAllText(Application.dataPath + "/storedDrawing_"+fileName+".json", json);

        }
        
        // Helper method to retrieve drawing in JSON file (useful for testing recognition function)
        private void RecoverDrawing(string fileName)
        {
            if (File.Exists(Application.dataPath + "/storedDrawing_"+fileName+".json"))
            {
                string readJson = File.ReadAllText(Application.dataPath + "/storedDrawing_"+fileName+".json");
                SegmentPoints sp = JsonUtility.FromJson<SegmentPoints>(readJson);
                var segments2D = sp.GetPoints();
                var boundBox2D = sp.boundBox;
                // To create a List<Segment> to query for char 
                // dataTransformer.GetCharSegment(segments2D, boundBox2D); // turns list of Vector2(segments2D) to List of Segments
            }
            else
            {
                Debug.Log("storedDrawing: FileNotFound");
            }
        }
        
        // Method to get found character
        public char GetCharacter()
        {
            return _foundChar;
        }
        
        // Method to control Text-input by setting next character or removing last character
        public void SetModifiedWord(char c)
        {
            if (c == '-')
            {
                if (_word.Length >= 1)
                {
                    _word.Remove(_word.Length - 1, 1);
                }
            }
            else
            {
                _word.Append(c);
            }
        }
        
        // Method to get word/text
        public string GetWord()
        {
            return _word.ToString();
        }
        
        // Method to clear written word/text
        public void ResetWord()
        {
            _word = new StringBuilder();
        }
        
        // Method to start timer for evaluation purpose (WPM)
        public float GetStartTime()
        {
            return _startTime;
        }
        
        // Method to restart timer
        public void ResetTimer()
        {
            _timeHandler = true;
            _startTime = 0.0f;
        }
        
        // Method to get 4 closest matches from drawing (improves text-input method by using them as buttons)
        public List<char> GetBestMatches()
        {
            return _bestResults;
        }
        
        // Method to get space in text-input by clicking a button
        public void SpaceOnClick()
        {
            // Debug.Log("Space Button clicked!");
            SetModifiedWord(' ');
            _spaceCounter++;
            _streamInputCounter++;
            evalLog.LogFoundChars(_word + "\n" + "(Space)" +" \n");
        }
        
        // Method to remove last letter in text-input by clicking a button
        public void BackspaceOnClick()
        {
            // Debug.Log("Backspace Button clicked! ");
            SetModifiedWord('-');
            _backspaceCounter++;
            _streamInputCounter++;
            evalLog.LogFoundChars(_word + "\n" + "(Backspace)" +" \n");
        }
        
        // Method to get one of the best matches in text-input by clicking a button
        public void LetterOnClick(Text c)
        {
            SetModifiedWord(c.text[0]);
            _byClickCounter++;
            _streamInputCounter++;
            evalLog.LogFoundChars(_word + "\n" + "(Corrected / byClick: ) " +c.text[0]+" \n");
        }
        
        // Method to get number of space clicks (for evaluation)
        public int GetSpaceCounter()
        {
            return _spaceCounter;
        }
        
        // Method to get number of backspace clicks (for evaluation)
        public int GetBackSpaceCounter()
        {
            return _backspaceCounter;
        }
        
        // Method to get number of letter clicks (for evaluation)
        public int GetByClickCounter()
        {
            return _byClickCounter;
        }
        
        // Method to get number of recognized character (for evaluation)
        public int GetRecCharCounter()
        {
            return _recognizedCharCounter;
        }
        
        // Method to get number of stream inputs
        public int GetInputStreamCounter()
        {
            return _streamInputCounter;
        }
        
        // Method to reset all counters
        public void ResetCounter()
        {
            _spaceCounter = 0;
            _backspaceCounter = 0;
            _byClickCounter = 0;
            _recognizedCharCounter = 0;
            _streamInputCounter = 0;
        }

    }
}