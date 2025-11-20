using UnityEngine;

public class PortalCamera : MonoBehaviour
{
    public MeshRenderer screenRenderer; 
    public int textureSize = 512;

    private RenderTexture portalTexture;

    void Start()
    {
        portalTexture = new RenderTexture(textureSize, textureSize, 16);
        portalTexture.name = "PortalTexture_" + GetInstanceID();
        GetComponent<Camera>().targetTexture = portalTexture;

        if (screenRenderer != null)
            screenRenderer.material.mainTexture = portalTexture;
    }
}
