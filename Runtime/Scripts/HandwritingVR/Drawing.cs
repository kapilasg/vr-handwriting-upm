using System;
using UnityEngine;
using UnityEngine.Events;

namespace HandwritingVR
{
  public class Drawing : MonoBehaviour
  {
    [Serializable]
    public class TextInputEvent : UnityEvent<string>
    {
    }
   
    public Material lineMaterial;
    public float lineWidth = 0.01f;
    public float maxSegmentDistance = 0.02f;
    public float minCornerAngle = 10;
    public DrawingData collectData;
    public DrawGizmo gizmo;
    public string word;
    public TextInputEvent onLetterDrawn;
    
    private LineRenderer _currentLine;
    private LineRenderer _lineGameObject;
    private float _sqrMaxSegmentDistance;
    private float _startTimer;
    private bool _foundDoubleTrigger;
    
    private void Awake()
    {
      _sqrMaxSegmentDistance = maxSegmentDistance * maxSegmentDistance;
      
      var go = new GameObject("DrawLine", typeof(LineRenderer));
      _lineGameObject = go.GetComponent<LineRenderer>();
      _lineGameObject.material = lineMaterial;
      _lineGameObject.widthMultiplier = lineWidth;
      _lineGameObject.numCapVertices = 4;
      _lineGameObject.numCornerVertices = 4;
      _lineGameObject.tag = "Line";
      _foundDoubleTrigger = false;
      Debug.Log("Drawing: Awake");
    }
    
    private void Update()
    {
      if (!_currentLine) return;
      if (DoubleTriggerDetected())
      {
        OnLetterFinished();
      }
      OnDrawing();
    }

    public void OnLetterFinished()
    {
      if (collectData != null)
      {
        char c = collectData.FinishedLetter();
        word = collectData.GetWord();
        Debug.Log("Word: "+word);
        Action onFinishedDrawing = () => onLetterDrawn.Invoke(word);
        // if (gizmo != null) gizmo.SetCollectData(collectData);
        var clones = GameObject.FindGameObjectsWithTag("Line");
        foreach (var clone in clones)
        {
          if (clone.name.Contains("(Clone)"))
          {
            Destroy(clone);
          }
        }
      }
    }

    private void OnDrawing()
    {
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
      }
    }
    
    private bool DoubleTriggerDetected()
    {
      if (!_foundDoubleTrigger) return false;
      _foundDoubleTrigger = false;
      return true;
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
        if (_startTimer != 0)
        {
          float timeDiff = Time.time - _startTimer;
          if (timeDiff < 0.2f)
          {
            _foundDoubleTrigger = true;
          }
        }
        _currentLine = Instantiate(_lineGameObject);
        _startTimer = Time.time;
        
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