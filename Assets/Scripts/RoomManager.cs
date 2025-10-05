using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    [System.Serializable]
    public class Room
    {
        public string roomName;
        public GameObject roomObject;
        public Transform cameraPosition;
        public List<InteractableObject> Interactables = new();
    }

    [SerializeField] private List<Room> rooms = new();
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float transitionSpeed = 2f;

    private int _currentRoomIndex;
    private bool _isTransitioning;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeRooms();
        SwitchToRoom(0);
    }

    private void Update()
    {
        // Quick room switching with number keys
        if (Keyboard.current[Key.Digit1].wasPressedThisFrame) SwitchToRoom(0);
        if (Keyboard.current[Key.Digit2].wasPressedThisFrame) SwitchToRoom(1);
        if (Keyboard.current[Key.Digit3].wasPressedThisFrame) SwitchToRoom(2);
        
        // Arrow key navigation
        if (Keyboard.current[Key.LeftArrow].wasPressedThisFrame)
            SwitchToRoom((_currentRoomIndex - 1 + rooms.Count) % rooms.Count);
        if (Keyboard.current[Key.RightArrow].wasPressedThisFrame)
            SwitchToRoom((_currentRoomIndex + 1) % rooms.Count);
    }

    private void InitializeRooms()
    {
        // Auto-find interactables in each room
        foreach (var room in rooms)
        {
            if (room.roomObject != null)
            {
                room.Interactables.AddRange(
                    room.roomObject.GetComponentsInChildren<InteractableObject>()
                );
            }
        }
    }

    public void SwitchToRoom(int index)
    {
        if (index < 0 || index >= rooms.Count || _isTransitioning) return;
        if (index == _currentRoomIndex) return;

        _currentRoomIndex = index;
        StartCoroutine(TransitionToRoom(rooms[_currentRoomIndex]));
    }

    private System.Collections.IEnumerator TransitionToRoom(Room targetRoom)
    {
        _isTransitioning = true;

        // Disable all rooms
        foreach (var room in rooms)
        {
            if (room.roomObject) room.roomObject.SetActive(false);
        }

        // Enable target room
        if (targetRoom.roomObject)
            targetRoom.roomObject.SetActive(true);

        // Move camera
        if (mainCamera && targetRoom.cameraPosition)
        {
            Vector3 startPos = mainCamera.transform.position;
            Quaternion startRot = mainCamera.transform.rotation;
            Vector3 targetPos = targetRoom.cameraPosition.position;
            Quaternion targetRot = targetRoom.cameraPosition.rotation;

            float elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * transitionSpeed;
                mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, elapsed);
                mainCamera.transform.rotation = Quaternion.Lerp(startRot, targetRot, elapsed);
                yield return null;
            }
        }

        _isTransitioning = false;
    }

    public Room GetCurrentRoom()
    {
        return rooms[_currentRoomIndex];
    }

    public List<InteractableObject> GetAllInteractables()
    {
        List<InteractableObject> all = new List<InteractableObject>();
        foreach (var room in rooms)
        {
            all.AddRange(room.Interactables);
        }
        return all;
    }
}