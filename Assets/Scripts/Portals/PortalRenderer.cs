using UnityEngine;

public class PortalRenderer : MonoBehaviour
{
    [Header("Target Camera (the camera from the other portal)")]
    public Camera targetCamera;

    private void Update()
    {
        // Render the target camera manually, once per frame
        if (targetCamera != null)
            targetCamera.Render();
    }
}
