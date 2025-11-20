using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PortalCameraURP : MonoBehaviour
{
    public MeshRenderer[] screenRenderers;
    public int textureSize = 1024;

    private RenderTexture portalTexture;

    void Awake()
    {
        Camera cam = GetComponent<Camera>();

        // Create a RenderTexture
        portalTexture = new RenderTexture(textureSize, textureSize, 16, RenderTextureFormat.ARGB32);
        portalTexture.name = "PortalRT_" + GetInstanceID();
        portalTexture.useMipMap = false;
        portalTexture.autoGenerateMips = false;

        cam.targetTexture = portalTexture;

        // Assign the texture to ALL linked screens
        if (screenRenderers != null)
        {
            foreach (var r in screenRenderers)
            {
                if (r != null)
                    r.material.mainTexture = portalTexture;
            }
        }

        // Ensure portal surfaces never render recursively
        int layer = LayerMask.NameToLayer("PortalSurface");
        if (layer >= 0)
            cam.cullingMask &= ~(1 << layer);
    }
}
