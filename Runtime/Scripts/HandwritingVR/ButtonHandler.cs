using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace HandwritingVR
{
    public class ButtonHandler : MonoBehaviour
    {
        public DrawingData collectData;
    
        public void SpaceOnClick()
        {
            Debug.Log("Space Button clicked!");
            collectData.SetModifiedWord(' ');
        }

        public void BackspaceOnClick()
        {
            Debug.Log("Backspace Button clicked! ");
            collectData.SetModifiedWord('-');
        }
        
    }
}
