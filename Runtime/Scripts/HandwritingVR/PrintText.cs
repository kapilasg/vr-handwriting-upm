using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace HandwritingVR
{
    public class PrintText : MonoBehaviour
    {
        public EvaluationLog evalLog;
        public Text displayText;
        public Text writtenText;
        public DrawingData data;
        public int numberOfPhrases = 5; // 5
        
        private string[] _displayPhrases;
        private string _selectedPhrases;
        private int _counter;
        private float _startTime;
        private float _totalTime;
        private string _logTimer;

        // "Press Start and next Phrase when you are ready! (Practice round)"
        // "hello world"
        // "Start Evaluation"
        // "Phrase 1"
        // "pause"
        // "Phrase 2"
        // "pause"
        // "Phrase 3"
        // "pause"
        // "Phrase 4"
        // "pause"
        // "Phrase 5"
        // "The end"
        
        private void Awake()
        {
            string[] allPhrases = File.ReadAllLines(Application.dataPath + "/phrases.txt");
            _selectedPhrases = "";
            _displayPhrases = new string[numberOfPhrases*2 + 3];
            Random rand = new Random();
            List<int> selectedLineNumbers = new List<int>();
            for (int i = 0; i < numberOfPhrases; i++)
            {
                int r = rand.Next(0, 500);
                while (selectedLineNumbers.Contains(r))
                {
                    r = rand.Next(0, 500);
                }
                selectedLineNumbers.Add(r);
                // Debug.Log("random phrase: "+allPhrases[r]);
                _selectedPhrases += allPhrases[r] + "\n";
            }
            evalLog.LogPhrases(_selectedPhrases);
            // Debug.Log(_selectedPhrases);
            _displayPhrases[0] = "Press Start for Practice round";
            _displayPhrases[1] = "hello world";
            _displayPhrases[2] = "Start Evaluation";
            int c = 0;
            for (int i = 3; i < _displayPhrases.Length; i++)
            {
                if (i % 2 == 0)
                {
                    _displayPhrases[i] = "Pause";
                    if (i == _displayPhrases.Length - 1)
                    {
                        _displayPhrases[i] = "The End";
                    }
                    continue;
                }
                _displayPhrases[i] = allPhrases[selectedLineNumbers[c]].ToLower();
                c++;
            }

            foreach (var phrase in _displayPhrases)
            {
                Debug.Log(phrase);
            }
            
            _counter = 0;
        }
        
        private void Update()
        {
            SetDisplayText();
            SetWrittenText();
            if (writtenText.text.Length > 5 && !writtenText.text.Equals("No Text"))
            {
                var wt = writtenText.text[Range.StartAt(5)];
                var dt = displayText.text;
                Debug.Log("Written text: "+wt);
                Debug.Log("Displayed text: "+dt);
                if (wt.Equals(dt))
                {
                    Debug.Log("Phrases copied!");
                    DisplayNextPhrase();
                }
            }


        }

        void SetWrittenText()
        {
            string txt = "";
            string str = data.GetWord(); // _drawing.word;
            if (str.Equals(""))
            {
                txt = "No Text";
            }
            else
            {
                txt = "Text:" + str;
            }
            writtenText.text = txt;
        }

        void SetDisplayText()
        {
            if (_counter >= _displayPhrases.Length)
            {
                displayText.text = _displayPhrases[^1];
            }
            displayText.text = _displayPhrases[_counter];  //"my watch fell in the water";
        }

        public void DisplayNextPhrase()
        {
            if (_counter >= _displayPhrases.Length-1)
            {
                return;
            }

            data.SetWord();
            writtenText.text = "";

            if (_startTime != 0)
            {
                var endTime = Time.time;
                _logTimer = _displayPhrases[_counter] + " "+ _startTime +" "+ endTime +" "+ (endTime - _startTime)+"\n";
                _logTimer += writtenText.text + " " + 
                             "correct: " + _displayPhrases[_counter].Equals(writtenText.text[Range.StartAt(5)]) + "\n";
                Debug.Log("LogTimeStamp called");
                // phrase start end phraseTime
                // writtenPhrase correct: true/false
                if (_counter == _displayPhrases.Length - 1)
                {
                    _logTimer += "Total time: " + _totalTime + "\n";
                }
                evalLog.LogTimeStamps(_logTimer);
            }
            var phraseTime = Time.time - _startTime;
            if (!_displayPhrases[_counter].Equals("Pause")
                && !_displayPhrases[_counter].Equals("The End")
                && !_displayPhrases[_counter].Equals("Start Evaluation")
                && !_displayPhrases[_counter].Equals("hello world"))
            {
                _totalTime += phraseTime;
            }
            
            _counter++;
            _startTime = Time.time;
        }

        public void StartTimer()
        {
            _startTime = Time.time;
        }
    }
}
