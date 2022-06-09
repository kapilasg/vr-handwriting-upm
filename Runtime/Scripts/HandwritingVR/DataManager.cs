using UnityEngine;

namespace HandwritingVR
{
    public class DataManager : MonoBehaviour
    {
        public DataCollector dataCollector;
        public DataTransformer dataTransformer;
        
        // DataManager accesses raw data from dataCollector 
        // sends data to dataTransformer and then stores result back in dataCollector
        // Transforming includes planeFinding, Projection, Normalization, calcBoundingBox and Segmentation
        
        // The FindCharacter, Store/Retrieve function are in DataManager!!!
        // Here are also all Evaluation relevant functions
        
        public char FinishedLetter()
        {
            SetPoints();
            if (_numberOfPoints == 0)
            {
                return ' ';
            }
            if (_numberOfPoints > 0 && _numberOfPoints <= 3)
            {
                // _word.Append('.');
                return '.'; 
            }
            
            _segments2D = ProjectSegments2D();
            
            _boundBox2D = calcBoundBox2D();
            SegmentLines();

            List<Segment> segments = new List<Segment>();
            for (int i = 0; i < _segments2D.Count; i++)
            {
                if (_boundBox2D.Count == 4)
                {
                    Segment s = new Segment(i, _segments2D[i], _boundBox2D);
                    segments.Add(s);
                }
                else
                {
                    Debug.Log("_boundBox2D has "+ _boundBox2D.Count+" points");
                }
            }
            
            // string trainingsLetter = "h"; // i, j dot preferably as point but circle should work too
            // StoreDrawing(trainingsLetter);
            // RecoverDrawing(trainingsLetter);
            // Character letter = new Character(trainingsLetter[0], _segments2D.Count, segments);
            // TrainingsMode(letter);
            
            var found = RecognizeCharacter(segments);
            _foundCharacter = found.c;
            _bestResults = found.bestMatches;
            if (_foundCharacter != ' ')
            {
                _word.Append(_foundCharacter.ToString().ToLower());
            }

            _recognizedCharCounter++;
            _streamInputCounter++;

            if (_timeHandler)
            {
                _startTime = Time.time;
                _timeHandler = false;
            }

            string logFoundChar = _word + "\n";
            logFoundChar += "Found: "+_foundCharacter+" \n";
            logFoundChar += "Top five: " + string.Join(", ", _bestResults) + " \n";
            evalLog.LogFoundChars(logFoundChar);
            
            Debug.Log("Found the following character " + found.c + " with the accuracy: " +
                      (found.accuracy * 100) / _segments2D.Count + "%");
            
            ResetVariables();
            return found.c;
        }

        private void ResetVariables()
        {
            RemoveAllLines();
            _segments3D = new List<List<Vector3>>();
            _segments2D = new List<List<Vector2>>();
            _boundBox2D = new List<Vector2>();
            _numberOfPoints = 0;
            _supportVector = new Vector3();
            _directVector1 = new Vector3();
            _directVector2 = new Vector3();
            _normalVector = new Vector3();
        }
        private void WriteToJson(Character c)
        {
            SearchCharacter sc = ReadFromJson(c.numberOfSegments);
            sc.TrainingsMode(c);
            string writeJson = JsonUtility.ToJson(sc);
            File.WriteAllText(Application.dataPath + "/trainingBase"+c.numberOfSegments+".json", writeJson);
            Debug.Log("End of WriteToJson");
        }

        private SearchCharacter ReadFromJson(int fileIndex)
        {
            SearchCharacter sc;
            if (File.Exists(Application.dataPath + "/trainingBase" + fileIndex + ".json"))
            {
                string readJson = File.ReadAllText(Application.dataPath + "/trainingBase"+fileIndex+".json");
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
            SegmentPoints3D sp3d = new SegmentPoints3D(_segments3D);
            EvaluationLog.LogDrawingData(sp3d);
        }
        
        private void StoreDrawing(string fileName)
        {
            SegmentPoints sp = new SegmentPoints(_segments2D, _boundBox2D);
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
                _segments2D = sp.GetPoints();
                _boundBox2D = sp.boundBox;
            }
            else
            {
                Debug.Log("storedDrawing: FileNotFound");
            }
        }

        

    }
}