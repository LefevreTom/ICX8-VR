using UnityEngine;

[RequireComponent(typeof(Camera))]
public class QuickPortalCamera : MonoBehaviour
{
    public MeshRenderer screenRenderer; // The quad showing the portal
    public int textureSize = 512;       // RenderTexture resolution

    private RenderTexture portalTexture;

    void Start()
    {
        // Create a RenderTexture once
        portalTexture = new RenderTexture(textureSize, textureSize, 16);
        portalTexture.name = "PortalRenderTexture_" + GetInstanceID();
        GetComponent<Camera>().targetTexture = portalTexture;

        if (screenRenderer != null)
            screenRenderer.material.mainTexture = portalTexture;
    }
}
