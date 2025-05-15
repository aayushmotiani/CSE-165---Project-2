using System;
using UnityEngine;

public class CockpitView : MonoBehaviour
{
    public GameObject droneModel;
    
    //private XRHandSubsystem handSubsystem;
    private Boolean isVisible = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        droneModel.SetActive(isVisible);
    }

    // Update is called once per frame
    void Update()
    {
        //Place holder for finger tracking to activate or deactivate
        
    }

    public void SwitchView()
    {
        if (droneModel != null)
        {
            isVisible = !isVisible;
            droneModel.SetActive(isVisible);
        }

    }
}

