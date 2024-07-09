using System.Collections;
using System.Collections.Generic;
using DitzelGames.FastIK;
using UnityEngine;

public class Grabbing : MonoBehaviour
{
    public GameObject handPGT;
    public GameObject physicalPGT;

    public GameObject IK;
    public GameObject drillPointsScript;

    public Vector3 lastKnownPlacedPosition;

    private float progressedTime;
    private bool wasHeld = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        progressedTime += Time.deltaTime;
        if (progressedTime > 10)
        {
            InHandShow();
        }
    }

    private void InHandShow()
    {
        OVRInput.ControllerInHandState inHand = OVRInput.GetControllerIsInHandState(OVRInput.Hand.HandRight);

        bool keyGPressed = false;
        bool inEditor = false;
#if UNITY_EDITOR
        keyGPressed = Input.GetKey(KeyCode.G);
        inEditor = true;
#endif

        if (inHand == OVRInput.ControllerInHandState.ControllerInHand || keyGPressed)
        {
            handPGT.SetActive(true);
            physicalPGT.SetActive(false);

            // turn off IK
            IK.GetComponent<FastIKFabric>().sendingMessages = false;

            Vector3 RTouchCurrentLocation = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);

            // make robot arm go away when controller is away from IK target
            if (Vector3.Distance(lastKnownPlacedPosition, RTouchCurrentLocation) > 0.4)
            {
                IK.GetComponent<BraccioController>().SendSerialMessage("B0 S-90 E0 W50");
            }

            wasHeld = true;

        }
        else // if (inHand == OVRInput.ControllerInHandState.ControllerNotInHand || (!keyGPressed && inEditor))
        {
            lastKnownPlacedPosition = handPGT.transform.position;

            handPGT.SetActive(false);

            if (wasHeld)
            {
                wasHeld = false;
                physicalPGT.transform.position = handPGT.transform.position;
                physicalPGT.GetComponent<Rigidbody>().velocity = handPGT.GetComponent<Rigidbody>().velocity;
                physicalPGT.SetActive(true);
            }


            // make it reset back to a position where turk can reinsert it to robot arm
            // turn on IK
        }
    }
}
