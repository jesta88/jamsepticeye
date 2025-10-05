using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class InteractableObject : MonoBehaviour
{
    [Header("Object Settings")]
    [SerializeField] private int pointsValue = 1;
    [SerializeField] private float snapDistance = 0.5f;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color wrongPositionColor = Color.red;
    
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isInCorrectPosition = true;
    private bool isDragging = false;
    
    private List<Renderer> objRenderers = new List<Renderer>();
    private Color[] originalColors;

    void Awake()
    {
        GetComponentsInChildren(true, objRenderers);
        originalColors = new Color[objRenderers.Count];
        for (int i = 0; i < objRenderers.Count; i++)
        {
            originalColors[i] = objRenderers[i].material.color;
        }
        
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        
        // Verify setup
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"{gameObject.name} missing Collider!");
        }
        else if (col.isTrigger)
        {
            Debug.LogWarning($"{gameObject.name} collider is Trigger - should be solid!");
        }
    }

    // Called by MouseInteractionManager
    public void OnHoverEnter()
    {
        if (!isInCorrectPosition && objRenderers.Count > 0)
        {
            foreach (Renderer objRenderer in objRenderers)
            {
                objRenderer.material.color = highlightColor;
            }
        }
    }

    public void OnHoverExit()
    {
        if (!isDragging)
        {
            UpdateVisualState();
        }
    }

    public void OnDragStart()
    {
        if (!isInCorrectPosition)
        {
            isDragging = true;
        }
    }

    public void OnDragUpdate(Ray ray)
    {
        if (!isDragging) return;

        Plane dragPlane = new Plane(Camera.main.transform.forward, transform.position);
        
        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 newPos = ray.GetPoint(distance);
            transform.position = newPos;
        }
    }

    public void OnDragEnd()
    {
        if (!isDragging) return;
        
        isDragging = false;
        
        float distance = Vector3.Distance(transform.position, originalPosition);
        
        if (distance <= snapDistance)
        {
            transform.position = originalPosition;
            transform.rotation = originalRotation;
            isInCorrectPosition = true;
            
            GameManager.Instance.AddScore(pointsValue);
            UpdateVisualState();
            
            Debug.Log($"✓ Fixed {gameObject.name}!");
        }
        else
        {
            UpdateVisualState();
        }
    }

    public void MoveToRandomPosition(Vector3 offset)
    {
        if (isInCorrectPosition)
        {
            isInCorrectPosition = false;
            transform.position = originalPosition + offset;
            
            transform.rotation = originalRotation * Quaternion.Euler(
                Random.Range(-15f, 15f),
                Random.Range(-15f, 15f),
                Random.Range(-15f, 15f)
            );
            
            UpdateVisualState();
            Debug.Log($"👻 Ghost moved {gameObject.name}!");
        }
    }

    private void UpdateVisualState()
    {
        if (objRenderers.Count == 0) return;
        
        if (isInCorrectPosition)
        {
            for (int i = 0; i < objRenderers.Count; i++)
            {
                objRenderers[i].material.color = originalColors[i];
            }
        }
        else
        {
            foreach (Renderer objRenderer in objRenderers)
            {
                objRenderer.material.color = wrongPositionColor;
            }
        }
    }

    public bool IsInCorrectPosition() => isInCorrectPosition;
}