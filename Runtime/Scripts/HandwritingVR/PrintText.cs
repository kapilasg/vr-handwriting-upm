using UnityEngine;
using UnityEngine.UI;

namespace HandwritingVR
{
    public class PrintText : MonoBehaviour
    {
        public Text displayText;
        public Text writtenText;
        public DrawingData collectData;

        private void OnEnable()
        {
            SetWrittenText();
        }

        private void Update()
        {
            SetDisplayText();
            SetWrittenText();
        }

        void SetWrittenText()
        {
            string txt = "";
            string str = collectData.GetWord(); // _drawing.word;
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
            displayText.text = "my watch fell in the water";
        }
    }
}
