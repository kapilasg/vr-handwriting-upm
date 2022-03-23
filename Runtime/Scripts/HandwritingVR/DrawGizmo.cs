using UnityEngine;

namespace HandwritingVR
{
    public class DrawGizmo : MonoBehaviour
    {
        public Color lineColor;

        private DrawingData _collectData;

        public void SetCollectData(DrawingData data)
        {
            _collectData = data;
            Debug.Log("DrawGizmo set collect data");
        }

        private void OnDrawGizmos()
        {
            if (_collectData)
            {
                Gizmos.DrawSphere(_collectData.GetSupportVector(), 0.01f);
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

                Gizmos.color = Color.cyan;
                var projectedPoints = _collectData.GetProjectedPoints();
                for (int i = 0; i < projectedPoints.Count-1; i++)
                {
                    Gizmos.DrawLine(projectedPoints[i], projectedPoints[i+1]);
                }

                Gizmos.color = Color.magenta;
                var boundingBox = _collectData.GetBoundingBox();
                for (int i = 0; i < boundingBox.Count-1; i++)
                {
                    Gizmos.DrawLine(boundingBox[i], boundingBox[i+1]);
                    if (i == boundingBox.Count - 2)
                    {
                        Gizmos.DrawLine(boundingBox[i+1], boundingBox[0]);
                    }
                }
				
                var normProjPoints = _collectData.GetNormalizedProjectedPoints();
                for (int i = 0; i < normProjPoints.Count-1; i++)
                {
                    Gizmos.DrawLine(normProjPoints[i],normProjPoints[i+1]);
                }
            }
        }
    }
}
