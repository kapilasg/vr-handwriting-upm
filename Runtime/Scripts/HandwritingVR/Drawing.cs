using System.Collections.Generic;
using UnityEngine;

namespace HandwritingVR
{
  public class Drawing : MonoBehaviour
  {
  public Material lineMaterial;
    // public char letter;
    public float lineWidth = 0.01f;
    public float maxSegmentDistance = 0.02f;
    public float minCornerAngle = 10;
    public DrawingData collectData;
    public DrawGizmo gizmo;
    public bool letterDone;

    private LineRenderer _currentLine;
    private LineRenderer _lineGameObject;
    private float _sqrMaxSegmentDistance;

    private void Awake()
    {
      _sqrMaxSegmentDistance = maxSegmentDistance * maxSegmentDistance;
      
      var go = new GameObject("DrawLine", typeof(LineRenderer));
      _lineGameObject = go.GetComponent<LineRenderer>();
      _lineGameObject.material = lineMaterial;
      _lineGameObject.widthMultiplier = lineWidth;
      _lineGameObject.numCapVertices = 4;
      _lineGameObject.numCornerVertices = 4;
      Debug.Log("Drawing: Awake");
    }

    private void Update()
    {
      if (!_currentLine) return;
      if (letterDone)
      {
        Debug.Log("Calling Finished Letter (Drawing)");
        if (collectData != null)
        {
          Debug.Log("Calling Finished Letter (Drawing)");
          collectData.FinishedLetter();
          letterDone = false;
          /*Debug.Log("LETTER "+letter);
          collectData.FinishedLetter(letter);
          letterDone = false;
          letter = ' ';*/
          
          
          Debug.Log("letter done before gizmo");
          if (gizmo != null)
          {
            Debug.Log("Gizmo set collect data");
            gizmo.SetCollectData(collectData);  
          }
          Debug.Log("letter done before return");
          /*if (collectData.GetLetterDone())
          {
            Destroy(_lineGameObject);
            //Awake();
          }*/
          // collectData.SetLetterDone(false);
          return;
        }
      }

      // Update current position
      var numPositions = _currentLine.positionCount;
      var position = transform.position;
      _currentLine.SetPosition(numPositions - 1, position);
      if (collectData != null)
      {
        collectData.UpdateLine(_currentLine);
      }
      // If the last position was far enough away, create new point
      var lastPosition = _currentLine.GetPosition(numPositions - 2);
      if ((lastPosition - position).sqrMagnitude > _sqrMaxSegmentDistance)
      {
        // Check if the last position can be removed
        if (numPositions > 2)
        {
          var rootPosition = _currentLine.GetPosition(numPositions - 3);
          var angle = Vector3.Angle(lastPosition - rootPosition, position - lastPosition);
          if (angle < minCornerAngle)
          {
            _currentLine.SetPosition(numPositions - 2, position);
            if (collectData != null)
            {
              collectData.UpdateLine(_currentLine);
            }
          }
          else
          {
            _currentLine.positionCount++;
            _currentLine.SetPosition(numPositions, position);
            if (collectData != null)
            {
              collectData.UpdateLine(_currentLine);
            }
          }
        }
        else
        {
          _currentLine.positionCount++;
          _currentLine.SetPosition(numPositions, position);
          if (collectData != null)
          {
            collectData.UpdateLine(_currentLine);
          }
        }
        
        // When to call Destroy(_currentLine);
        
      }
    }

    private void OnDisable()
    {
      Debug.Log("Drawing: OnDisable");
      _currentLine = null;
    }

    public void OnInteraction(Transform interactor, bool start)
    {
      if (start)
      {
        _currentLine = Instantiate(_lineGameObject);
        var position = transform.position;
        _currentLine.SetPosition(0, position);
        _currentLine.SetPosition(1, position);
        if (collectData == null)
        {
          Debug.Log("_collectData is NULL");
        }
        else
        {
          collectData.AddLine(_currentLine);
        }

      }
      else
      {
        _currentLine = null;
      }
    }
    
  }
}