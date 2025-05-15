using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.SubsystemsImplementation;
using UnityEngine.XR.Management;
using TMPro;
using UnityEditor.MPE;

public class DroneFlight : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField]private float speed, rotationAmount;
    [SerializeField]private GameObject player;
    private XRHandSubsystem xrHandSubsystem;
    [SerializeField]private bool droneMoveUsingIndex, rotateRight, rotateLeft;
    public Vector3 fingerForwardDir;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = player.GetComponent<Rigidbody>();
        rb.isKinematic=true;
        xrHandSubsystem = XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRHandSubsystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Timer.moveDrone && droneMoveUsingIndex){
            player.transform.Translate(fingerForwardDir * speed);
            
            //DRONE MOVE CONTINUOUSLY USING FINGER POINT DIRECTION
            if (xrHandSubsystem == null) return;
            //get left hand
            XRHand leftHand = xrHandSubsystem.leftHand;
            if (!leftHand.isTracked) return;
            //get left hand index tip
            XRHandJoint indexTip = leftHand.GetJoint(XRHandJointID.IndexTip);

            if (indexTip.TryGetPose(out Pose pose))
            {
                fingerForwardDir = pose.rotation * Vector3.forward;

                Debug.DrawRay(pose.position, fingerForwardDir * 0.2f, Color.green, 1.0f);

            }
            else
            {
                Debug.Log("Index finger tip not tracked.");
            }

            if(rotateRight){
                player.transform.Rotate(Vector3.up * rotationAmount, Space.World);
            }
            if(rotateLeft){
                player.transform.Rotate(-Vector3.up * rotationAmount, Space.World);
            }
        }

        
    }
    public void PointAt(){
        player.transform.Translate(player.transform.forward * speed);
    }
    public void TriggerIndexDirection()
    {
        droneMoveUsingIndex=true;
    }

    public void ThumbsUp(){
        //rotate drone right, yaw
        rotateRight = true;
    }
    public void Fist(){
        //rotate drone left, yaw
        rotateLeft = true;
    }
    public void RightFalse(){
        rotateRight = false;
    }
    public void LeftFalse(){
        rotateLeft = false;
    }
}
