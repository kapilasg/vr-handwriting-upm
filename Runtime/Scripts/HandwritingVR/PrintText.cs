using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
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
        public string[] testPhrases;
        public int counter;

        private float _startTime;
        private float _totalTime;

        private void Awake()
        {
            string[] allPhrases = File.ReadAllLines(Application.dataPath + "/phrases.txt");
            testPhrases = new string[7];
            Random rand = new Random();
            List<int> selectedLineNumbers = new List<int>();
            for (int i = 1; i < testPhrases.Length-1; i++)
            {
                int r = rand.Next(0, 500);
                while (selectedLineNumbers.Contains(r))
                {
                    r = rand.Next(0, 500);
                }
                testPhrases[i] = allPhrases[r].ToLower();
                selectedLineNumbers.Add(r);
            }

            testPhrases[0] = "Press Start and next Phrase when you are ready!";
            testPhrases[6] = "The End";

            counter = 0;
        }
        
        private void Update()
        {
            SetDisplayText();
            SetWrittenText();
        }

        void SetWrittenText()
        {
            string txt = "";
            string str = data.GetWord(); // _drawing.word;
            if (str.Equals(""))
            {
                txt = "No character was found";
            }
            else
            {
                txt = "Text: " + str;
            }
            writtenText.text = txt;
        }

        void SetDisplayText()
        {
            if (counter >= testPhrases.Length)
            {
                displayText.text = testPhrases[^1];
            }
            displayText.text = testPhrases[counter];  //"my watch fell in the water";
        }

        public void DisplayNextPhrase()
        {
            if (counter >= testPhrases.Length-1)
            {
                return;
            }
            var phraseTime = Time.time - _startTime;
            _totalTime += phraseTime;
            Debug.Log("Total time for phrase: "+ _totalTime);
            Debug.Log("Phrase number: "+ counter);
            Debug.Log("Phrase text: " + testPhrases[counter]);
            Debug.Log("Phrase time: "+ phraseTime);
            counter++;
            // _startTime = Time.time; // for no pause between phrases
        }

        public void StartTimer()
        {
            _startTime = Time.time;
        }
    }
}
