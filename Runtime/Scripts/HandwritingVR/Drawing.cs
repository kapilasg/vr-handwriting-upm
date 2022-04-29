using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
// using UnityEngine.InputSystem;

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
    //public InputAction letterDoneAction;
    
    private float _timeLeft;
    private LineRenderer _currentLine;
    private LineRenderer _lineGameObject;
    private float _sqrMaxSegmentDistance;
    // private List<LineRenderer> _lines;
    // private List<GameObject> _lineObjects;

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
      _timeLeft = 2.0f;
      // letterDoneAction.performed += _ => OnLetterFinished();
      Debug.Log("Drawing: Awake");
      /*if (collectData is not null)
      {
        Debug.Log("collectData is not null");
        collectData.FinishedLetter();
      }*/
    }
    
    private void Update()
    {
      if (!_currentLine) return;
      _timeLeft -= Time.deltaTime;
      if (_timeLeft <= 0) Debug.Log("time ended!");
      if (letterDone)
      {
        OnLetterFinished();
      }
      OnDrawing();
    }
    
    private void OnLetterFinished()
    {
      Debug.Log("OnLetterFinished() called");
      if (collectData != null)
      {
        collectData.FinishedLetter();
        // if (gizmo != null) gizmo.SetCollectData(collectData);
        _timeLeft = 2.0f;
        var clones = GameObject.FindGameObjectsWithTag("Line");
        foreach (var clone in clones)
        {
          if (clone.name.Contains("(Clone)"))
          {
            Destroy(clone);
          }
        }
        letterDone = false;
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

    private void OnDoubleTrigger()
    {
      // TODO erase lines not recognize last drawing
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