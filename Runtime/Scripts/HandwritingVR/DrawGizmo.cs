using System.Collections.Generic;
using UnityEngine;

namespace HandwritingVR
{
    public class DrawGizmo : MonoBehaviour
    {
        public Color lineColor;

        private DrawingData _collectData;
        private List<Vector3> _boundingBox;
        private List<Vector2> _bound2D;
        private List<List<Vector2>> _proj2D;

        public void SetCollectData(DrawingData data)
        {
            _collectData = data;
            Debug.Log("DrawGizmo set collect data");
        }

        private void OnDrawGizmos()
        {
            if (_collectData)
            {
                /*Gizmos.DrawSphere(_collectData.GetSupportVector(), 0.01f);
                // Gizmos.DrawLine(_collectData.GetSupportVector(), _collectData.GetNormalVector());
                // Gizmos.DrawLine(new Vector3(0.0f, 0.0f, 0.0f), _collectData.GetNormalVector());
                // Gizmos.color = Color.yellow;
                // Gizmos.DrawLine(_collectData.GetSupportVector(), _collectData.GetDirectVector1());
                // Gizmos.DrawLine(_collectData.GetSupportVector(), _collectData.GetDirectVector2());
                Gizmos.color = Color.green;
                // Gizmos.DrawLine(new Vector3(0.0f, 0.0f, 0.0f), _collectData.GetNormalVector());
                Gizmos.DrawLine(new Vector3(0.0f, 0.0f, 0.0f), _collectData.GetNormalVector());
                Gizmos.DrawLine(_collectData.GetSupportVector(), _collectData.GetSupportVector() + _collectData.GetNormalVector().normalized);
                Gizmos.color = Color.blue;
                // Gizmos.DrawLine(new Vector3(0.0f, 0.0f, 0.0f), _collectData.GetDirectVector1());
                Gizmos.DrawLine(new Vector3(0.0f, 0.0f, 0.0f), _collectData.GetDirectVector1());
                Gizmos.DrawLine(_collectData.GetSupportVector(), _collectData.GetSupportVector() + _collectData.GetDirectVector1().normalized);
                Gizmos.color = Color.red;
                // Gizmos.DrawLine(new Vector3(0.0f, 0.0f, 0.0f), _collectData.GetDirectVector2());
                Gizmos.DrawLine(new Vector3(0.0f, 0.0f, 0.0f), _collectData.GetDirectVector2());
                Gizmos.DrawLine(_collectData.GetSupportVector(), _collectData.GetSupportVector() + _collectData.GetDirectVector2().normalized);
                // Gizmos.color = Color.yellow;
                // Gizmos.DrawLine(new Vector3(0.0f, 0.0f, 0.0f), _collectData.GetSupportVector());
                //Gizmos.DrawLine(new Vector3(0.0f, 0.0f, 0.0f), _collectData.GetSupportVector().normalized);
                
                if (_boundingBox == null || _bound2D == null)
                {
                    _boundingBox = _collectData.GetBoundingBox();
                    _bound2D = _collectData.Get2DBoundingBox();
                }
                else
                {
                    _bound2D = _collectData.Get2DBoundingBox();
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(_boundingBox[0], _boundingBox[1]);
                    Gizmos.DrawLine(_bound2D[0], _bound2D[1]);

                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(_boundingBox[1], _boundingBox[2]);
                    Gizmos.DrawLine(_bound2D[1], _bound2D[2]);

                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(_boundingBox[2], _boundingBox[3]);
                    Gizmos.DrawLine(_bound2D[2], _bound2D[3]);

                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(_boundingBox[3], _boundingBox[0]);
                    Gizmos.DrawLine(_bound2D[3], _bound2D[0]);
                }
                
                for (int i = 0; i < boundingBox.Count-1; i++)
                {
                    Gizmos.DrawLine(boundingBox[i], boundingBox[i+1]);
                    if (i == boundingBox.Count - 2)
                    {
                        Gizmos.DrawLine(boundingBox[i+1], boundingBox[0]);
                    }
                }*/
				
                Gizmos.color = Color.cyan;
                _proj2D = _collectData.Get2DSegments();
                if (_proj2D is null) return;
                foreach (var segment in _proj2D)
                {
                    for (int i = 0; i < segment.Count-1; i++)
                    {
                        Gizmos.DrawLine(segment[i],segment[i+1]);
                    }
                }
            }
        }
    }
}
