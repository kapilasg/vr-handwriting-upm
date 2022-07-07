using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

namespace HandwritingVR
{
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
        
        // DataManager accesses raw data from dataCollector 
        // sends data to dataTransformer and then stores result back in dataCollector
        // Transforming includes planeFinding, Projection, Normalization, calcBoundingBox and Segmentation
        
        // The FindCharacter, Store/Retrieve function are in DataManager!!!
        // Here are also all Evaluation relevant functions

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

            // string trainingsLetter = "h"; // i, j dot preferably as point but circle should work too
            // StoreDrawing(trainingsLetter);
            // RecoverDrawing(trainingsLetter);
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
        private void ResetVariables()
        {
            dataCollector.RemoveAllLines();
            dataTransformer.ResetVariables();
        }
        private void WriteToJson(Character c)
        {
            SearchCharacter sc = ReadFromJson(c.numberOfSegments);
            sc.TrainingsMode(c);
            string writeJson = JsonUtility.ToJson(sc);
            File.WriteAllText(_dataPath + "/trainingBase"+c.numberOfSegments+".json", writeJson);
            Debug.Log("End of WriteToJson");
        }
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
                Debug.Log("File with index "+fileIndex+" doesn't exist");
                sc = new SearchCharacter();
            }
            return sc;
        }
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

        private void TrainingsMode(Character c)
        {
            WriteToJson(c);
        }

        private void StoreDrawing3D()
        {
            var seg3D = dataTransformer.Get3DSegments();
            SegmentPoints3D sp3d = new SegmentPoints3D(seg3D);
            EvaluationLog.LogDrawingData(sp3d);
        }
        
        private void StoreDrawing(string fileName)
        {
            var seg2D = dataTransformer.Get2DSegments();
            var box2D = dataTransformer.GetBoundBox2D();
            SegmentPoints sp = new SegmentPoints(seg2D, box2D);
            string json = JsonUtility.ToJson(sp);
            File.WriteAllText(Application.dataPath + "/storedDrawing_"+fileName+".json", json);
            if (File.Exists(Application.dataPath + "/storedDrawing_" + fileName + ".json"))
            {
                Debug.Log("File created!!");
            }
            else
            {
                Debug.Log("File not created!!");
            }
        }

        private void RecoverDrawing(string fileName)
        {
            if (File.Exists(Application.dataPath + "/storedDrawing_"+fileName+".json"))
            {
                string readJson = File.ReadAllText(Application.dataPath + "/storedDrawing_"+fileName+".json");
                SegmentPoints sp = JsonUtility.FromJson<SegmentPoints>(readJson);
                var segments2D = sp.GetPoints();
                var boundBox2D = sp.boundBox;
                // To create a List<Segment> to query for char 
                // use dataTransformer.GetCharSegment(segments2D, boundBox2D)
            }
            else
            {
                Debug.Log("storedDrawing: FileNotFound");
            }
        }
        
        
        public char GetCharacter()
        {
            return _foundChar;
        }

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

        public string GetWord()
        {
            return _word.ToString();
        }

        public void ResetWord()
        {
            _word = new StringBuilder();
        }

        public float GetStartTime()
        {
            return _startTime;
        }

        public void ResetTimer()
        {
            _timeHandler = true;
            _startTime = 0.0f;
        }

        public List<char> GetBestMatches()
        {
            return _bestResults;
        }
        
        public void SpaceOnClick()
        {
            Debug.Log("Space Button clicked!");
            SetModifiedWord(' ');
            _spaceCounter++;
            _streamInputCounter++;
            evalLog.LogFoundChars(_word + "\n" + "(Space)" +" \n");
        }
        
        public void BackspaceOnClick()
        {
            Debug.Log("Backspace Button clicked! ");
            SetModifiedWord('-');
            _backspaceCounter++;
            _streamInputCounter++;
            evalLog.LogFoundChars(_word + "\n" + "(Backspace)" +" \n");
        }

        public void LetterOnClick(Text c)
        {
            SetModifiedWord(c.text[0]);
            _byClickCounter++;
            _streamInputCounter++;
            evalLog.LogFoundChars(_word + "\n" + "(Corrected / byClick: ) " +c.text[0]+" \n");
        }

        public int GetSpaceCounter()
        {
            return _spaceCounter;
        }
        
        public int GetBackSpaceCounter()
        {
            return _backspaceCounter;
        }
        
        public int GetByClickCounter()
        {
            return _byClickCounter;
        }

        public int GetRecCharCounter()
        {
            return _recognizedCharCounter;
        }

        public int GetInputStreamCounter()
        {
            return _streamInputCounter;
        }

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