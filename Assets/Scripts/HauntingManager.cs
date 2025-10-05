using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HauntingManager : MonoBehaviour
{
    [Header("Haunting Settings")]
    [SerializeField] private float initialHauntInterval = 5f;
    [SerializeField] private float minHauntInterval = 2f;
    [SerializeField] private float intervalDecreaseRate = 0.1f;
    [SerializeField] private Vector3 minOffset = new Vector3(-2f, 0f, -2f);
    [SerializeField] private Vector3 maxOffset = new Vector3(2f, 1f, 2f);
    
    [Header("Visibility Constraints")]
    [SerializeField] private bool keepInCameraView = true;
    [SerializeField] private float screenMargin = 0.1f; // 10% margin from screen edges
    [SerializeField] private int maxRepositionAttempts = 10;
    [SerializeField] private bool checkOcclusion = true;
    [SerializeField] private LayerMask occlusionLayers = -1; // Check against everything by default
    [SerializeField] private bool constrainToDepthPlane = true; // Keep objects at same camera depth
    
    [Header("Difficulty Scaling")]
    [SerializeField] private bool increaseDifficulty = true;
    [SerializeField] private int maxSimultaneousHaunts = 3;

    private float currentHauntInterval;
    private float hauntTimer;
    private List<InteractableObject> availableObjects = new List<InteractableObject>();

    void Start()
    {
        currentHauntInterval = initialHauntInterval;
        hauntTimer = currentHauntInterval;
        
        // Wait a frame for RoomManager to initialize
        Invoke(nameof(InitializeObjects), 0.1f);
    }

    void Update()
    {
        if (!GameManager.Instance.IsGameActive()) return;

        hauntTimer -= Time.deltaTime;

        if (hauntTimer <= 0)
        {
            HauntRandomObject();
            ResetTimer();
        }
    }

    private void InitializeObjects()
    {
        if (RoomManager.Instance != null)
        {
            availableObjects = RoomManager.Instance.GetAllInteractables();
            Debug.Log($"AI found {availableObjects.Count} interactable objects");
        }
    }

    private void HauntRandomObject()
    {
        if (availableObjects.Count == 0) return;

        // Get objects in correct position
        var correctObjects = availableObjects
            .Where(obj => obj != null && obj.IsInCorrectPosition())
            .ToList();

        if (correctObjects.Count == 0)
        {
            Debug.Log("No objects to haunt - all are already moved!");
            return;
        }

        // Limit simultaneous haunts
        var incorrectCount = availableObjects.Count(obj => !obj.IsInCorrectPosition());
        if (incorrectCount >= maxSimultaneousHaunts)
        {
            Debug.Log("Max simultaneous haunts reached");
            return;
        }

        // Pick random object
        InteractableObject targetObject = correctObjects[Random.Range(0, correctObjects.Count)];
        
        // Try to find a valid position
        Vector3 validOffset = Vector3.zero;
        bool foundValidPosition = false;
        
        for (int attempt = 0; attempt < maxRepositionAttempts; attempt++)
        {
            // Generate random offset
            Vector3 testOffset = new Vector3(
                Random.Range(minOffset.x, maxOffset.x),
                Random.Range(minOffset.y, maxOffset.y),
                Random.Range(minOffset.z, maxOffset.z)
            );
            
            Vector3 testPosition = targetObject.transform.position + testOffset;
            
            // Check if position is visible
            if (!keepInCameraView || IsPositionVisible(testPosition))
            {
                validOffset = testOffset;
                foundValidPosition = true;
                break;
            }
        }
        
        if (foundValidPosition)
        {
            targetObject.MoveToRandomPosition(validOffset);
        }
        else
        {
            Debug.LogWarning($"Couldn't find valid position for {targetObject.name} after {maxRepositionAttempts} attempts");
        }
    }
    
    private bool IsPositionVisible(Vector3 worldPosition)
    {
        Camera cam = Camera.main;
        if (cam == null) return true; // Fail-safe
        
        Vector3 viewportPos = cam.WorldToViewportPoint(worldPosition);
        
        // Check if in front of camera and within screen bounds (with margin)
        bool inViewport = viewportPos.z > 0 && 
                          viewportPos.x > screenMargin && 
                          viewportPos.x < (1f - screenMargin) &&
                          viewportPos.y > screenMargin && 
                          viewportPos.y < (1f - screenMargin);
        
        if (!inViewport) return false;
        
        // Check occlusion with raycast
        if (checkOcclusion)
        {
            Vector3 directionToObject = worldPosition - cam.transform.position;
            float distanceToObject = directionToObject.magnitude;
            
            if (Physics.Raycast(cam.transform.position, directionToObject.normalized, 
                out RaycastHit hit, distanceToObject, occlusionLayers))
            {
                // Something is blocking the view
                return false;
            }
        }
        
        return true;
    }

    private void ResetTimer()
    {
        hauntTimer = currentHauntInterval;
        
        // Gradually increase difficulty
        if (increaseDifficulty && currentHauntInterval > minHauntInterval)
        {
            currentHauntInterval = Mathf.Max(
                minHauntInterval,
                currentHauntInterval - intervalDecreaseRate
            );
        }
    }

    // Manual trigger for testing
    [ContextMenu("Trigger Haunt Now")]
    public void TriggerHauntNow()
    {
        HauntRandomObject();
    }
}