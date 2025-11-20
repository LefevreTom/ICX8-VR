using System.Collections;
using UnityEngine;
using Unity.XR.CoreUtils;

[RequireComponent(typeof(Collider))]
public class PortalSimple : MonoBehaviour
{
    [Header("Portal Settings")]
    public Transform targetPortal;   // The portal to teleport to
    public float teleportOffset = 1.5f; // How far in front of the target portal
    public float lockTime = 0.5f;       // Brief lock to avoid instant retrigger

    private bool locked = false;

    private void Reset()
    {
        // Ensure collider is trigger
        Collider col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (locked || targetPortal == null) return;

        // 1) Check if XR Origin
        var xr = other.GetComponentInParent<XROrigin>();
        if (xr != null)
        {
            StartCoroutine(TeleportXROriginCoroutine(xr));
            return;
        }

        // 2) Non-XR player
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null)
        {
            TeleportNonXR(other.transform);
            return;
        }
    }

    private void TeleportNonXR(Transform player)
    {
        // Place player in front of the target portal
        player.position = targetPortal.position + targetPortal.forward * teleportOffset;

        // Lock portals briefly
        StartCoroutine(TemporaryLockBoth());
    }

    private IEnumerator TeleportXROriginCoroutine(XROrigin xr)
    {
        locked = true;

        Transform camera = xr.Camera.transform;

        // Move XR Origin so the camera is in front of the target portal
        Vector3 offset = camera.position - xr.transform.position; // head offset
        xr.transform.position = targetPortal.position - offset + targetPortal.forward * teleportOffset;

        // Stop momentum if Rigidbody present
        var rb = xr.GetComponentInChildren<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = Vector3.zero;

        StartCoroutine(TemporaryLockBoth());

        yield return null;
    }

    private IEnumerator TemporaryLockBoth()
    {
        locked = true;

        // Lock paired portal if present
        var targetPortalComp = targetPortal.GetComponent<PortalSimple>();
        if (targetPortalComp != null)
            targetPortalComp.locked = true;

        yield return new WaitForSeconds(lockTime);

        locked = false;
        if (targetPortalComp != null)
            targetPortalComp.locked = false;
    }
}
