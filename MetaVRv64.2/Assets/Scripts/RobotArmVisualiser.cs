using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotArmVisualiser : MonoBehaviour
{
    public GameObject shoulder;
    public GameObject elbow;
    public GameObject wrist;

    public GameObject shoulderVisual;
    public GameObject elbowVisual;
    public GameObject wristVisual;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        shoulderVisual.transform.localRotation = shoulder.transform.rotation;
        elbowVisual.transform.localRotation = elbow.transform.localRotation;
        wristVisual.transform.localRotation = wrist.transform.localRotation;
    }
}
