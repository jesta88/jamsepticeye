using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractableObject : MonoBehaviour
{
    [Header("Object Settings")]
    [SerializeField] private int pointsValue = 1;
    [SerializeField] private float snapDistance = 0.1f;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color wrongPositionColor = Color.red;
    
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private bool _isInCorrectPosition = true;
    [SerializeField] private bool _isDragging;
    
    private List<Renderer> _objRenderers = new();
    private Color[] _originalColors;
    private Material _outlineMaterial;

    private void Awake()
    {
        GetComponentsInChildren(true, _objRenderers);
        _originalColors = new Color[_objRenderers.Count];
        for (int i = 0; i < _objRenderers.Count; i++)
        {
            _originalColors[i] = _objRenderers[i].material.color;
        }

        // Store original transform
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
    }

    private void OnMouseEnter()
    {
        if (!GameManager.Instance.IsGameActive()) return;
        if (!_isInCorrectPosition && _objRenderers.Count > 0)
        {
            foreach (Renderer objRenderer in _objRenderers)
            {
                objRenderer.material.color = highlightColor;
            }
        }
    }

    private void OnMouseExit()
    {
        if (!_isDragging)
        {
            UpdateVisualState();
        }
    }

    private void OnMouseDown()
    {
        if (!GameManager.Instance.IsGameActive()) return;
        if (!_isInCorrectPosition)
        {
            _isDragging = true;
        }
    }

    private void OnMouseUp()
    {
        if (!_isDragging) return;
        
        _isDragging = false;
        
        // Check if placed correctly
        float distance = Vector3.Distance(transform.position, _originalPosition);
        
        if (distance <= snapDistance)
        {
            // Snap to correct position
            transform.position = _originalPosition;
            transform.rotation = _originalRotation;
            _isInCorrectPosition = true;
            
            GameManager.Instance.AddScore(pointsValue);
            UpdateVisualState();
            
            Debug.Log($"Fixed {gameObject.name}!");
        }
        else
        {
            UpdateVisualState();
        }
    }

    private void OnMouseDrag()
    {
        if (!_isDragging) return;

        if (Camera.main == null) return;
        
        // Simple drag along camera plane
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane dragPlane = new(Camera.main.transform.forward, transform.position);
        
        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 newPos = ray.GetPoint(distance);
            transform.position = newPos;
        }
    }

    public void MoveToRandomPosition(Vector3 offset)
    {
        if (_isInCorrectPosition)
        {
            _isInCorrectPosition = false;
            transform.position = _originalPosition + offset;
            
            // Optional: add slight rotation
            transform.rotation = _originalRotation * Quaternion.Euler(
                Random.Range(-15f, 15f),
                Random.Range(-15f, 15f),
                Random.Range(-15f, 15f)
            );
            
            UpdateVisualState();
            Debug.Log($"Ghost moved {gameObject.name}!");
        }
    }

    private void UpdateVisualState()
    {
        if (_objRenderers.Count == 0) return;
        if (_isInCorrectPosition)
        {
            for (int i = 0; i < _objRenderers.Count; i++)
            {
                _objRenderers[i].material.color = _originalColors[i];
            }
        }
        else
        {
            for (int i = 0; i < _objRenderers.Count; i++)
            {
                _objRenderers[i].material.color = wrongPositionColor;
            }
        }
    }

    public bool IsInCorrectPosition() => _isInCorrectPosition;
}