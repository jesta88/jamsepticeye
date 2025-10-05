using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AIHauntingManager : MonoBehaviour
{
    [Header("Haunting Settings")]
    [SerializeField] private float initialHauntInterval = 5f;
    [SerializeField] private float minHauntInterval = 2f;
    [SerializeField] private float intervalDecreaseRate = 0.1f;
    [SerializeField] private Vector3 minOffset = new(-2f, 0f, -2f);
    [SerializeField] private Vector3 maxOffset = new(2f, 1f, 2f);
    
    [Header("Difficulty Scaling")]
    [SerializeField] private bool increaseDifficulty = true;
    [SerializeField] private int maxSimultaneousHaunts = 3;

    private float _currentHauntInterval;
    private float _hauntTimer;
    private List<InteractableObject> _availableObjects = new();

    private void Start()
    {
        _currentHauntInterval = initialHauntInterval;
        _hauntTimer = _currentHauntInterval;
        
        // Wait a frame for RoomManager to initialize
        Invoke(nameof(InitializeObjects), 0.1f);
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGameActive()) return;

        _hauntTimer -= Time.deltaTime;

        if (_hauntTimer <= 0)
        {
            HauntRandomObject();
            ResetTimer();
        }
    }

    private void InitializeObjects()
    {
        if (RoomManager.Instance != null)
        {
            _availableObjects = RoomManager.Instance.GetAllInteractables();
            Debug.Log($"AI found {_availableObjects.Count} interactable objects");
        }
    }

    private void HauntRandomObject()
    {
        if (_availableObjects.Count == 0) return;

        // Get objects in correct position
        var correctObjects = _availableObjects
            .Where(obj => obj != null && obj.IsInCorrectPosition())
            .ToList();

        if (correctObjects.Count == 0)
        {
            Debug.Log("No objects to haunt - all are already moved!");
            return;
        }

        // Limit simultaneous haunts
        var incorrectCount = _availableObjects.Count(obj => !obj.IsInCorrectPosition());
        if (incorrectCount >= maxSimultaneousHaunts)
        {
            Debug.Log("Max simultaneous haunts reached");
            return;
        }

        // Pick random object
        InteractableObject targetObject = correctObjects[Random.Range(0, correctObjects.Count)];
        
        // Generate random offset
        Vector3 offset = new(
            Random.Range(minOffset.x, maxOffset.x),
            Random.Range(minOffset.y, maxOffset.y),
            Random.Range(minOffset.z, maxOffset.z)
        );

        targetObject.MoveToRandomPosition(offset);
    }

    private void ResetTimer()
    {
        _hauntTimer = _currentHauntInterval;
        
        // Gradually increase difficulty
        if (increaseDifficulty && _currentHauntInterval > minHauntInterval)
        {
            _currentHauntInterval = Mathf.Max(
                minHauntInterval,
                _currentHauntInterval - intervalDecreaseRate
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