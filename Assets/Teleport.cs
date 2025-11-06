using UnityEngine;

public class Teleport : MonoBehaviour
{
    public Transform teleportDestination;

    void OnTriggerEnter(Collider other)
    {
        // Check if the object entering has the Player tag
        if (other.CompareTag("Player"))
        {
            other.transform.position = teleportDestination.position;
        }
    }
}
