using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HandwritingVR
{
    public class RelatedLetters : MonoBehaviour
    {
        public Text letter1;
        public Text letter2;
        public Text letter3;
        public Text letter4;
        public DrawingData data;

        private void Update()
        {
            SetRelatedLetters();
        }

        void SetRelatedLetters()
        {
            List<char> matches = data.GetBestMatches();
            if (matches.Count == 0)
            {
                return;
            }

            if (matches.Count >= 1)
            {
                if (matches[0] != ' ')
                {
                    letter1.text = matches[0].ToString();
                    letter2.text = "";
                    letter3.text = "";
                    letter4.text = "";
                }
            }

            if (matches.Count >= 2)
            {
                if (matches[1] != ' ')
                {
                    letter1.text = matches[0].ToString();
                    letter2.text = matches[1].ToString();
                    letter3.text = "";
                    letter4.text = "";
                }
            }

            if (matches.Count >= 3)
            {
                if (matches[2] != ' ')
                {
                    letter1.text = matches[0].ToString();
                    letter2.text = matches[1].ToString();
                    letter3.text = matches[2].ToString();
                    letter4.text = "";
                }
            }

            if (matches.Count >= 4)
            {
                if (matches[3] != ' ')
                {
                    letter1.text = matches[0].ToString();
                    letter2.text = matches[1].ToString();
                    letter3.text = matches[2].ToString();
                    letter4.text = matches[3].ToString();
                }
            }
        }
    }
}