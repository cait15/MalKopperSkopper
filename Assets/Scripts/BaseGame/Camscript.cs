using UnityEngine;

public class IsometricCameraSetup : MonoBehaviour
{
    [Header("Camera Settings")]
    public float isometricAngle = 45f;
    public float tiltAngle = 35.264f;
    public float distance = 30f;
    public float height = 20f;
    
    [Header("Look At Target")]
    public Vector3 lookAtCenter = Vector3.zero;
    public Vector3 targetOffset = Vector3.zero;
    
    [Header("Camera Movement")]
    public bool allowDragPan = true;
    public float dragSensitivity = 0.05f;
    public float zoomSensitivity = 2f;
    public float minOrthographicSize = 50f;
    public float maxOrthographicSize = 100f;
    
    [Header("Camera Bounds")]
    public bool useCameraBounds = true;
    public float minX = -50f;
    public float maxX = 50f;
    public float minZ = -50f;
    public float maxZ = 50f;
    
    [Header("Smooth Movement")]
    public bool smoothPan = true;
    public float panSmoothSpeed = 8f;
    
    private Camera cam;
    private Vector3 targetLookAtCenter;
    private Vector3 lastMousePos;
    private bool isDragging = false;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = gameObject.AddComponent<Camera>();
        }
        
        cam.orthographic = true;
        cam.orthographicSize = 50f;
        
        targetLookAtCenter = lookAtCenter;
        SetupIsometricView();
    }
    
    void Update()
    {
        HandleMouseInput();
        UpdateCameraPosition();
    }
    
    void HandleMouseInput()
    {
        if (!allowDragPan) return;
        
        // Mouse drag to pan
        if (Input.GetMouseButtonDown(2)) // Middle mouse button
        {
            isDragging = true;
            lastMousePos = Input.mousePosition;
        }
        
        if (Input.GetMouseButtonUp(2))
        {
            isDragging = false;
        }
        
        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            
            // Convert screen space delta to world space
            float worldDeltaX = -delta.x * dragSensitivity;
            float worldDeltaZ = -delta.y * dragSensitivity;
            
            targetLookAtCenter += new Vector3(worldDeltaX, 0, worldDeltaZ);
            lastMousePos = Input.mousePosition;
        }
        
        // Mouse wheel to zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float newSize = cam.orthographicSize - (scroll * zoomSensitivity);
            cam.orthographicSize = Mathf.Clamp(newSize, minOrthographicSize, maxOrthographicSize);
        }
    }
    
    void UpdateCameraPosition()
    {
        // Smooth pan if enabled
        if (smoothPan)
        {
            lookAtCenter = Vector3.Lerp(lookAtCenter, targetLookAtCenter, Time.deltaTime * panSmoothSpeed);
        }
        else
        {
            lookAtCenter = targetLookAtCenter;
        }
        
        // Clamp camera bounds
        if (useCameraBounds)
        {
            lookAtCenter = ClampCameraPosition(lookAtCenter);
            targetLookAtCenter = lookAtCenter;
        }
        
        SetupIsometricView();
    }
    
    Vector3 ClampCameraPosition(Vector3 pos)
    {
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        return pos;
    }
    
    void SetupIsometricView()
    {
        float horizontalDistance = distance * Mathf.Cos(isometricAngle * Mathf.Deg2Rad);
        float verticalDistance = distance * Mathf.Sin(tiltAngle * Mathf.Deg2Rad);
        
        Vector3 cameraPos = lookAtCenter + new Vector3(horizontalDistance, height + verticalDistance, horizontalDistance);
        transform.position = cameraPos;
        
        transform.LookAt(lookAtCenter + targetOffset + new Vector3(0, height * 0.5f, 0));
    }
    
    public void SetOrthographicSize(float size)
    {
        if (cam != null)
        {
            cam.orthographicSize = Mathf.Clamp(size, minOrthographicSize, maxOrthographicSize);
        }
    }
    
    public void SetDistance(float newDistance)
    {
        distance = newDistance;
        SetupIsometricView();
    }
    
    public void SetHeight(float newHeight)
    {
        height = newHeight;
        SetupIsometricView();
    }
    
    public void ResetView()
    {
        lookAtCenter = Vector3.zero;
        targetLookAtCenter = Vector3.zero;
        cam.orthographicSize = 15f;
        SetupIsometricView();
    }
    
    public void SetCameraBounds(float minXBound, float maxXBound, float minZBound, float maxZBound)
    {
        minX = minXBound;
        maxX = maxXBound;
        minZ = minZBound;
        maxZ = maxZBound;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        
        float horizontalDistance = distance * Mathf.Cos(isometricAngle * Mathf.Deg2Rad);
        float verticalDistance = distance * Mathf.Sin(tiltAngle * Mathf.Deg2Rad);
        
        Vector3 cameraPos = lookAtCenter + new Vector3(horizontalDistance, height + verticalDistance, horizontalDistance);
        Vector3 lookAtPos = lookAtCenter + targetOffset + new Vector3(0, height * 0.5f, 0);
        
        Gizmos.DrawLine(cameraPos, lookAtPos);
        Gizmos.DrawWireSphere(cameraPos, 2f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(lookAtPos, 2f);
        
        // Draw bounds
        if (useCameraBounds)
        {
            Gizmos.color = Color.cyan;
            Vector3 boundsCenter = new Vector3((minX + maxX) / 2, 0, (minZ + maxZ) / 2);
            Vector3 boundsSize = new Vector3(maxX - minX, 0.1f, maxZ - minZ);
            DrawBounds(boundsCenter, boundsSize);
        }
    }
    
    void DrawBounds(Vector3 center, Vector3 size)
    {
        Vector3 halfSize = size / 2;
        
        // Draw the four corners
        Vector3[] corners = new Vector3[4]
        {
            center + new Vector3(-halfSize.x, 0, -halfSize.z),
            center + new Vector3(halfSize.x, 0, -halfSize.z),
            center + new Vector3(halfSize.x, 0, halfSize.z),
            center + new Vector3(-halfSize.x, 0, halfSize.z)
        };
        
        // Draw lines connecting corners
        Gizmos.DrawLine(corners[0], corners[1]);
        Gizmos.DrawLine(corners[1], corners[2]);
        Gizmos.DrawLine(corners[2], corners[3]);
        Gizmos.DrawLine(corners[3], corners[0]);
    }
}