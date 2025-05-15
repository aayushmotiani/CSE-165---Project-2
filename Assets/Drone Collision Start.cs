using UnityEngine;

public class DroneTriggerHandler : MonoBehaviour
{
    public Checkpointloader checkpointManager;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered by: " + other.name);

        if (other.CompareTag("Ground") && checkpointManager != null)
        {
            Debug.Log("Hit the ground");
            checkpointManager.StartCoroutine("ResetToLastCheckpoint");
        }
    }
}