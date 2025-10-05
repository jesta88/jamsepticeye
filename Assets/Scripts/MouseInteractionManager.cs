using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Alternative to OnMouse events - more reliable raycasting approach
/// Attach to Main Camera or a persistent GameObject
/// </summary>
public class MouseInteractionManager : MonoBehaviour
{
    [SerializeField] private Camera mainCam;
    [SerializeField] private LayerMask interactableLayer = -1; // Everything by default
    
    private InteractableObject _currentHovered;
    private InteractableObject _currentDragging;

    void Start()
    {
        if (mainCam == null)
            mainCam = Camera.main;
    }

    void Update()
    {
        if (!GameManager.Instance.IsGameActive()) return;

        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        
        // Handle hover
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactableLayer))
        {
            InteractableObject obj = hit.collider.GetComponent<InteractableObject>();
            
            if (obj != _currentHovered)
            {
                // Exit previous
                if (_currentHovered != null)
                    _currentHovered.OnHoverExit();
                
                // Enter new
                _currentHovered = obj;
                if (_currentHovered != null)
                    _currentHovered.OnHoverEnter();
            }
        }
        else
        {
            // Exit hover if nothing hit
            if (_currentHovered != null)
            {
                _currentHovered.OnHoverExit();
                _currentHovered = null;
            }
        }

        // Handle mouse down
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (_currentHovered != null)
            {
                _currentDragging = _currentHovered;
                _currentDragging.OnDragStart();
            }
        }

        // Handle dragging
        if (Mouse.current.leftButton.isPressed && _currentDragging != null)
        {
            _currentDragging.OnDragUpdate(ray);
        }

        // Handle mouse up
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (_currentDragging != null)
            {
                _currentDragging.OnDragEnd();
                _currentDragging = null;
            }
        }
    }
}