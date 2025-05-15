using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Rendering;
using System.Collections; // Make sure System.Collections is included for Coroutines

public class Checkpointloader : MonoBehaviour
{
    public parse parsed; // Make sure the 'parse' script exists and works correctly
    public GameObject checkpointObj;
    public float checkpointRadius;
    public Transform drone; // Assign your drone/player object here
    public float fontSize;
    public AudioClip checkpointReachedSound;
    public AudioClip checkpointBeaconSound;
    public float lineWidthMultipler;
    public static Vector3 NextCheckpointPosition; // Consider if this should be static
    public TextMeshProUGUI countdownText;
    public GameObject timerCanvas; // drag the Canvas

    private Timer raceTimer;
    private List<Vector3> checkpoints;
    private List<GameObject> checkpointmarker = new List<GameObject>();
    private int currCheckpointIndex = 0;
    private bool isResetting = false;
    private float resetDelay = 3f;
    // private float resetTimer = 0f; // This variable isn't used

    // New variables for deferred loading
    [SerializeField] private int checkpointsPerFrame = 2; // How many checkpoints to create per frame. Adjust as needed.

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Parse the file first - this is still synchronous, consider optimizing 'parsed.ParseFile()' if it's slow
        checkpoints = parsed.ParseFile();

        // Check if parsing was successful and we have checkpoints
        if (checkpoints == null || checkpoints.Count == 0)
        {
            Debug.LogError("Checkpoint file parsing failed or returned no checkpoints!");
            return; // Stop initialization if no checkpoints
        }

        // Start the coroutine to create checkpoints gradually
        StartCoroutine(CreateCheckpointsGradually());

        Invoke("SpawnPlayer", 0.05f); // Spawn player after checkpoint creation

        // Link all checkpoints in order
        CreatePathLine();

        // Initialize timmer
        raceTimer = timerCanvas.GetComponent<Timer>();
    }

    // Coroutine to create checkpoints over several frames
    IEnumerator CreateCheckpointsGradually()
    {
        checkpointmarker.Clear(); // Ensure the list is clear before adding

        for (int i = 0; i < checkpoints.Count; i++)
        {
            Vector3 pos = checkpoints[i];
            // Ensure checkpointObj prefab is assigned in the Inspector
            if (checkpointObj == null)
            {
                 Debug.LogError("Checkpoint Prefab (checkpointObj) is not assigned in the Inspector!");
                 yield break; // Stop the coroutine if prefab is missing
            }

            GameObject marker = Instantiate(checkpointObj, pos, Quaternion.identity);
            marker.transform.localScale = Vector3.one * checkpointRadius * 2;

            // Ensure Renderer and Material exist before trying to set color
            Renderer markerRenderer = marker.GetComponent<Renderer>();
            if(markerRenderer != null && markerRenderer.material != null)
            {
                 markerRenderer.material.color = new Color(0.5f, 0.5f, 1f, 0.3f); // turns blue
            } else {
                 Debug.LogWarning("Marker Renderer or Material not found on instantiated checkpointObj!");
            }

            checkpointmarker.Add(marker);

            // add beacon sound to each marker
            // Ensure checkpointBeaconSound clip is assigned
            if (checkpointBeaconSound != null)
            {
                 AudioSource beacon = marker.AddComponent<AudioSource>();
                 beacon.clip = checkpointBeaconSound;
                 beacon.dopplerLevel = 0f;
                 beacon.loop = true;
                 beacon.playOnAwake = false;
                 beacon.rolloffMode = AudioRolloffMode.Linear;
                 beacon.spatialBlend = 1f;
                 beacon.minDistance = 10f;
                 beacon.maxDistance = 100f;
                 beacon.volume = 1.0f;

                 if (i == 1) beacon.Play(); // Still play the second beacon
            } else {
                 Debug.LogWarning("Checkpoint Beacon Sound clip is not assigned!");
            }


            // starting point color (already blue above, this seems redundant unless it's a different shade)
            if (i == 0 && markerRenderer != null && markerRenderer.material != null)
            {
                markerRenderer.material.color = new Color(0.5f, 0.5f, 1f, 0.3f); // light blue
            }

            // Create and setup label
            GameObject labelGO = new GameObject("CheckpointLabel_" + (i + 1));
            labelGO.transform.SetParent(marker.transform);
            labelGO.transform.localPosition = Vector3.zero;
            //labelGO.transform.localScale = Vector3.one * 3f; // Ensure localScale is appropriate if needed

            TextMeshPro text = labelGO.AddComponent<TextMeshPro>();
            text.text = (i).ToString();
            text.fontSize = fontSize; // Make sure fontSize is assigned
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.black;
            text.enableAutoSizing = false;
            

            // number facing the camera - ensure FaceCamera script exists
            if (labelGO.GetComponent<FaceCamera>() == null) // Prevent adding multiple times if script is already on prefab
            {
                 labelGO.AddComponent<FaceCamera>();
            }


            // --- Defer instantiation/setup to the next frame after processing a batch ---
            // This yields control back to Unity for a frame, distributing the work.
            if ((i + 1) % checkpointsPerFrame == 0)
            {
                 yield return null; // Wait for the next frame
            }
        }

        // All checkpoints created
        Debug.Log("Finished creating all checkpoints.");

        // List of NextCheckpointPosition after all checkpoint creation
        if (checkpoints.Count > 1)
        {
            NextCheckpointPosition = checkpoints[currCheckpointIndex]; // Sets it to checkpoint 0 initially if currCheckpointIndex is 0
        } else if (checkpoints.Count == 1)
        {
             NextCheckpointPosition = checkpoints[0]; // Only one checkpoint
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (currCheckpointIndex >= checkpoints.Count) return;

        // Ensure checkpoints list is not null and the index is valid
        if (checkpoints == null || currCheckpointIndex < 0 || currCheckpointIndex >= checkpoints.Count)
        {
             Debug.LogError("Invalid checkpoint index in Update. Checkpoints list might be empty or null.");
             return;
        }


        Vector3 currentTarget = checkpoints[currCheckpointIndex];
        // Ensure drone transform is assigned
        if (drone == null)
        {
             Debug.LogError("Drone transform is not assigned in Checkpointloader!");
             return; // Cannot calculate distance without drone
        }
        float distance = Vector3.Distance(drone.position, currentTarget);

        if (distance < checkpointRadius)
        {
            // Ensure checkpointmarker list is valid and index is valid
            if (checkpointmarker == null || currCheckpointIndex < 0 || currCheckpointIndex >= checkpointmarker.Count || checkpointmarker[currCheckpointIndex] == null)
            {
                 Debug.LogWarning("Checkpoint marker object not found at index " + currCheckpointIndex + ". Skipping checkpoint logic.");
                 // Attempt to just increment the index if the marker is somehow missing
                 currCheckpointIndex++;
                 // Consider if you need to handle this missing marker case more robustly
                 if (currCheckpointIndex < checkpoints.Count)
                 {
                      // Try to set the next checkpoint position even if the marker was missing
                      NextCheckpointPosition = checkpoints[currCheckpointIndex];
                 }
                 return; // Exit Update loop for this frame after incrementing index
            }

            // stop beacon sound at current
            AudioSource currentBeacon = checkpointmarker[currCheckpointIndex].GetComponent<AudioSource>();
            if (currentBeacon != null) currentBeacon.Stop();


            // play checkpoint reached sound at drone position
            if (checkpointReachedSound != null && drone != null)
            {
                AudioSource.PlayClipAtPoint(checkpointReachedSound, drone.position);
            }


            checkpointmarker[currCheckpointIndex].SetActive(false);
            currCheckpointIndex++;


            if (currCheckpointIndex < checkpoints.Count)
            {
                NextCheckpointPosition = checkpoints[currCheckpointIndex];
                // turn on beacon sound for the next checkpoint
                if (checkpointmarker != null && currCheckpointIndex < checkpointmarker.Count && checkpointmarker[currCheckpointIndex] != null)
                {
                     AudioSource nextBeacon = checkpointmarker[currCheckpointIndex].GetComponent<AudioSource>();
                     if (nextBeacon != null) nextBeacon.Play();
                } else {
                     Debug.LogWarning("Next checkpoint marker object not found at index " + currCheckpointIndex + ". Cannot play beacon sound.");
                }


                HighlightCurrentCheckpoint(); // This will now highlight the *new* current checkpoint
            }
            else
            {
                Debug.Log(" All checkpoints completed!");
                if (raceTimer != null)
                {
                    raceTimer.StopTimer();
                    Debug.Log(" Timer Stopped");
                }
            }
        }
    }

    void HighlightCurrentCheckpoint()
    {
        // Ensure checkpointmarker list is valid and index is valid
        if (checkpointmarker != null && currCheckpointIndex >= 0 && currCheckpointIndex < checkpointmarker.Count && checkpointmarker[currCheckpointIndex] != null)
        {
             var marker = checkpointmarker[currCheckpointIndex];
             Renderer markerRenderer = marker.GetComponent<Renderer>();
             if(markerRenderer != null && markerRenderer.material != null)
             {
                 var mat = markerRenderer.material;
                 mat.color = new Color(1f, 1f, 0f, 0.4f); // yellow
             } else {
                 Debug.LogWarning("Marker Renderer or Material not found on current checkpoint marker for highlighting.");
             }
        } else {
             Debug.LogWarning("Current checkpoint marker object not found at index " + currCheckpointIndex + ". Cannot highlight.");
        }
    }

    void CreatePathLine()
    {
        // Ensure checkpoints list is not null and has enough points for a line
        if (checkpoints == null || checkpoints.Count < 2)
        {
             Debug.LogWarning("Not enough checkpoints to create a path line.");
             return;
        }

        GameObject pathLineObj = new GameObject("CheckpointPath");
        LineRenderer line = pathLineObj.AddComponent<LineRenderer>();

        line.positionCount = checkpoints.Count;
        line.SetPositions(checkpoints.ToArray());

        line.startWidth = 1; // Consider adjusting this based on world scale
        line.endWidth = 1; // Consider adjusting this based on world scale
        line.widthMultiplier = lineWidthMultipler; // Make sure lineWidthMultiplier is assigned
        line.material = new Material(Shader.Find("Sprites/Default")); // Sprites/Default might not be ideal for 3D/VR. Consider a standard or URP/HDRP compatible shader.
        line.startColor = new Color(1f, 1f, 0f, 0.6f);  // yellow
        line.endColor = new Color(1f, 0.1f, 0f, 0.6f);   // orange
        line.widthCurve = AnimationCurve.Linear(0, 0.2f, 1, 0.2f); // Width curve applied *after* widthMultiplier

        line.useWorldSpace = true;
    }

    void SpawnPlayer()
    {
        // Ensure checkpoints list is valid and has at least one point
        if (checkpoints == null || checkpoints.Count == 0)
        {
             Debug.LogError("Checkpoints list is empty or null. Cannot spawn player.");
             return;
        }
        // Ensure drone transform is assigned
        if (drone == null)
        {
             Debug.LogError("Drone transform is not assigned in Checkpointloader!");
             return;
        }

        drone.position = checkpoints[0];

        // Face the drone towards the second checkpoint if it exists
        if (checkpoints.Count > 1)
        {
            Vector3 nextCheckpointDirection = checkpoints[1] - checkpoints[0];
            //nextCheckpointDirection.y = 0f; // Uncomment if you only want to rotate on the Y axis
            if (nextCheckpointDirection != Vector3.zero)
                drone.rotation = Quaternion.LookRotation(nextCheckpointDirection);
        } else {
             // If only one checkpoint, maybe face a default direction or handle differently
             Debug.LogWarning("Only one checkpoint available. Drone cannot be oriented towards a next checkpoint.");
        }
    }


    IEnumerator ResetToLastCheckpoint()
    {
        isResetting = true;

        var movement = drone.GetComponentInChildren<DroneFlight>();
        if (movement != null) movement.enabled = false;

        //move to last checkpoint
        int lastCheckpoint = Mathf.Max(currCheckpointIndex - 1, 0);
        drone.position = checkpoints[lastCheckpoint];

        //drone facing next checkpoint
        if (currCheckpointIndex < checkpoints.Count)
        {
            Vector3 dir = checkpoints[currCheckpointIndex] - checkpoints[lastCheckpoint];
            dir.y = 0;
            if (dir != Vector3.zero)
                drone.rotation = Quaternion.LookRotation(dir);
        }

        //show countdown UI
        if (countdownText != null)
            countdownText.gameObject.SetActive(true);

        float timer = resetDelay;
        while (timer > 0)
        {
            //Mathf.Ceil return smallest interger greater or equal to (timer) 
            countdownText.text = "Respawning in " + Mathf.Ceil(timer).ToString();
            yield return null;
            timer -= Time.deltaTime;
        }

        // reset UI, hide UI and re-enable movement
        countdownText.text = "";
        countdownText.gameObject.SetActive(false);
        movement.enabled = true;
        isResetting = false;


    }

   
}
