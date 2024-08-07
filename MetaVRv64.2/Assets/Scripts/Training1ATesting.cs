using System.Collections;
using System.Collections.Generic;
using Oculus.Haptics;
using TMPro;
using UnityEngine;
using Obi;
using System;

public class Training1ATesting : MonoBehaviour
{
    [Header("Scene GameObjects")]
    public GameObject Training;
    public GameObject RobotArm;
    public GameObject MovementEnvelope;
    public GameObject ZeroGravityPGT;
    public GameObject MetaQuestPro;
    public GameObject MetaQuestProControllerMesh;
    public TextMeshPro instructionText;
    public TextMeshPro directInstructionText;
    public TextMeshPro trailCounterText;
    public GameObject IKTarget;
    public GameObject ObiSolver;
    public GameObject ObiRope;
    public GameObject HandMeshL;
    public GameObject HandMeshR;

    public GameObject testCube;

    [Header("Segments")]
    public GameObject WristVisualisation;

    [Header("Finger Collider Manager")]
    public GameObject FingerColliderManagerScript;

    private System.Random random = new System.Random(1952678);
    private int objectsTouched = -1;
    private bool newTrailRequired = true;
    private bool arrivedAtNextTrail = false;
    private Vector3 newTrailPosition;
    private const int MAX_OBJECTS_TOUCHED = 50;
    private bool ropeEnabled = false;
    private bool isVirtual = true;
    private bool moveOutOfEnvelope = true;
    private bool controllerAtNextTrail = false;
    private float timeTillTouched = 0.0f;
    private bool primaryIndexTriggerDown = false;

    private Vector3 lastControllerPos = Vector3.zero;
    private Vector3 initialPGTPos = Vector3.zero;
    private Quaternion initialPGTRot;
    private GameObject cloneRope;

    public void setVirtual(bool v)
    {
        isVirtual = v;
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        Training.SetActive(true);
        resetScene();
        initialPGTPos = ZeroGravityPGT.transform.position;
        initialPGTRot = ZeroGravityPGT.transform.rotation;
        newTrailPosition = initialPGTPos;

        if (isVirtual)
            FingerColliderManagerScript.SetActive(true);
        //FingerColliderManagerScript.GetComponent<FingerColliderManager>().SetTrigger(true);
    }


    private void OnDisable()
    {
        Training.SetActive(false);

        //FingerColliderManagerScript.GetComponent<FingerColliderManager>().SetTrigger(false);
        FingerColliderManagerScript.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        trackTriggers();
        IKTarget.transform.position = ZeroGravityPGT.transform.position;

        if (newTrailRequired)
        {
            setParametersForTrail();
            setInitialSettingsForTrail();
        }
        updateForParameters();

        if (Input.GetKeyDown(KeyCode.T) || OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickDown))
        { // for trail
            moveOutOfEnvelope = true;
            directInstructionText.text = "Move away from the Area";
            //newTrailRequired = true;
            //arrivedAtNextTrail = false;
        }

    }

    /// <summary>
    /// This function randomly sets the parameters for the current trail
    /// * ropeEnabled
    /// * tracking
    /// * moving
    /// </summary>
    private void setParametersForTrail()
    {
        //if (objectsTouched >= MAX_OBJECTS_TOUCHED)
        //    return;
        ropeEnabled = random.Next(0, 2) == 1;
        //moving = random.Next(0, 2) == 1;
        //if (moving) // never need to track an object if it is not moving
        //    tracking = random.Next(0, 2) == 1;
        //else
        //    tracking = false;
        objectsTouched++;
        newTrailRequired = false;

        trailCounterText.text = MAX_OBJECTS_TOUCHED - objectsTouched + " touches remaining.";
    }

    private bool insideEnvelope(Vector3 point, float radius = 0.45f, float height = 0.15f)
    {
        // inside envelope
        return Vector3.Distance(
            MovementEnvelope.transform.position,
            point
            ) < radius
                &&
            point.y > height + RobotArm.transform.position.y
            ;
    }

    /// <summary>
    /// This function sets the initial settings for the given parameters
    /// </summary>
    private void setInitialSettingsForTrail()
    {
        //resetScene();

        // initial settings for rope
        //if (ropeEnabled)
        //{
        //    //ObiRope.GetComponent<ObiRope>().ResetParticles();
        //    cloneRope = Instantiate(ObiRope, ObiSolver.transform);
        //    cloneRope.GetComponent<ObiRope>().ResetParticles();
        //}
        // initial new location
        Vector3 point = new Vector3(-100, -100, -100);
        while (!insideEnvelope(point))
        {
            Vector3 centerOfMovementEnvelope = MovementEnvelope.transform.position;
            double x = centerOfMovementEnvelope.x + (random.NextDouble() - 0.5) * 1.2;
            double y = centerOfMovementEnvelope.y + (random.NextDouble() - 0.5) * 1.2;
            double z = centerOfMovementEnvelope.z + (random.NextDouble() - 0.5) * 1.2;
            point.Set((float)x, (float)y, (float)z);
        }

        newTrailPosition = point;
        //newTrailRotation = Quaternion.Euler((float)random.Next() * 360, (float)random.Next() * 360, (float)random.Next() * 360);
        //ZeroGravityPGT.GetComponent<Rigidbody>().rotation = newTrailRotation;
        ZeroGravityPGT.GetComponent<Rigidbody>().freezeRotation = true;
        ZeroGravityPGT.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        ZeroGravityPGT.GetComponent<Rigidbody>().freezeRotation = false;

    }

    private void trackTriggers()
    {
        if (primaryIndexTriggerDown)
            primaryIndexTriggerDown = !OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger);
        else
            primaryIndexTriggerDown = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger);
    }

    private void updateForParameters()
    {
        // do update for rope
        //if (!ropeEnabled)
        //{
        ObiRope.SetActive(false);

        //    //Nudge PGT to physical controller?

        //    Destroy(cloneRope);
        //}
        //else
        //{
        //    cloneRope.SetActive(true);
        //}

        // do update for moving

        // dit you touch it?

        if (isVirtual) {
            if (arrivedAtNextTrail)
            {
                NudgeTowardsEnvelope();
            } else
            {
                if (Vector3.Distance(ZeroGravityPGT.transform.position, newTrailPosition) < 0.02)
                {
                    arrivedAtNextTrail = true;
                    ZeroGravityPGT.GetComponent<Rigidbody>().velocity = Vector3.zero;
                }
            }
        
            didYouTouchItVirtual();
        } else
        {
            if (arrivedAtNextTrail)
            {
                NudgeTowardsEnvelope();
            }
            else
            {
                if (Vector3.Distance(ZeroGravityPGT.transform.position, newTrailPosition) < 0.02)
                {
                    arrivedAtNextTrail = true;
                    ZeroGravityPGT.GetComponent<Rigidbody>().velocity = Vector3.zero;
                }
            }
            NudgeTowardsRotationEnvelope();
            //StopRigidBodyWhenClose();

            didYouTouchItPhysical();
        }
    }

    private void StopRigidBodyWhenClose()
    {
        Vector3 handLeftPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LHand);
        Vector3 handRightPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RHand);

        Vector3 toolPosition = ZeroGravityPGT.transform.position;

        float leftDistance = Vector3.Distance(toolPosition, handLeftPosition);
        float rightDistance = Vector3.Distance(toolPosition, handRightPosition);

        float distance = Mathf.Min(leftDistance, rightDistance);

        if (distance <= 0.3f && timeTillTouched > 0.5)//secs
        {
            ZeroGravityPGT.GetComponent<Rigidbody>().velocity = Vector3.zero;
            BoxCollider[] colliders = ZeroGravityPGT.GetComponents<BoxCollider>();
            foreach (BoxCollider collider in colliders)
            {
                collider.enabled = false;
            }
        } else
        {
            BoxCollider[] colliders = ZeroGravityPGT.GetComponents<BoxCollider>();
            foreach (BoxCollider collider in colliders)
            {
                collider.enabled = true;
            }
        }
    }

    private void didYouTouchItVirtual()
    {
        if (ZeroGravityPGT.GetComponent<ZeroGravityPGTScript>().TouchedByFinger())
        {
            moveOutOfEnvelope = true;
            directInstructionText.text = "Move away from the Area";
        }

        Vector3 handLeftPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LHand); //testCube.transform.position;
        Vector3 handRightPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RHand);
        if (moveOutOfEnvelope && !insideEnvelope(handLeftPosition, 0.65f, -10f) && !insideEnvelope(handRightPosition, 0.65f, -10f) )
        {
            directInstructionText.text = "Touch the tool";
            newTrailRequired = true;
            arrivedAtNextTrail = false;
            moveOutOfEnvelope = false;
        }

    }

    private void didYouTouchItPhysical()
    {
        //lastControllerPos = MetaQuestPro.transform.position;
        //if (Vector3.Distance(lastControllerPos, MetaQuestPro.transform.position) < 0.001)
        //{
        //    controllerAtNextTrail = true;
        //}

        //if (controllerAtNextTrail && Vector3.Distance(lastControllerPos, MetaQuestPro.transform.position) < 0.001)
        //{
        //    directInstructionText.text = "Move away from the Area";
        //    controllerAtNextTrail = false;
        //    moveOutOfEnvelope = true;
        //} else
        //{
        //    timeTillTouched += Time.deltaTime;
        //}

        //Vector3 handLeftPosition = testCube.transform.position; //OVRInput.GetLocalControllerPosition(OVRInput.Controller.LHand);
        //Vector3 handRightPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RHand);
        //if (moveOutOfEnvelope && !insideEnvelope(handLeftPosition) && !insideEnvelope(handRightPosition))
        //{
        //    directInstructionText.text = "Touch the tool";
        //    newTrailRequired = true;
        //    arrivedAtNextTrail = false;
        //    moveOutOfEnvelope = false;
        //}

        if (primaryIndexTriggerDown || Input.GetKeyDown(KeyCode.R)) // as in tRigger
        {
            moveOutOfEnvelope = true;
            directInstructionText.text = "Move away from the Area";
        }

        Vector3 handLeftPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LHand); //testCube.transform.position;
        Vector3 handRightPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RHand);
        if (moveOutOfEnvelope && !insideEnvelope(handLeftPosition, 0.65f, -10f) && !insideEnvelope(handRightPosition, 0.65f, -10f) & !primaryIndexTriggerDown)
        {
            directInstructionText.text = "Touch the tool";
            newTrailRequired = true;
            arrivedAtNextTrail = false;
            moveOutOfEnvelope = false;
        }
    }

    private void FixedUpdate()
    {
        Rigidbody rb = ZeroGravityPGT.GetComponent<Rigidbody>();
        if (!arrivedAtNextTrail)
        {
            //position
            rb.velocity = (newTrailPosition - ZeroGravityPGT.transform.position) / Time.fixedDeltaTime;
        }
        //if (!arrivedAtNextRotationTrail)
        //{
        //    //rotation
        //    Quaternion rotationDifference = newTrailRotation * Quaternion.Inverse(ZeroGravityPGT.transform.rotation);
        //    rotationDifference.ToAngleAxis(out float angleInDegree, out Vector3 rotationAxis);

        //    Vector3 rotationDifferenceInDegree = angleInDegree * rotationAxis;

        //    rb.angularVelocity = (rotationDifferenceInDegree * Mathf.Deg2Rad / Time.fixedDeltaTime);
        //}
    }

    private void resetScene()
    {
        if (initialPGTPos != Vector3.zero)
        {
            ZeroGravityPGT.GetComponent<Rigidbody>().velocity = Vector3.zero;
            ZeroGravityPGT.transform.position = initialPGTPos;
            ZeroGravityPGT.transform.rotation = initialPGTRot;
        }
    }

    private void NudgeTowardsEnvelope()
    {
        float distance = Vector3.Distance(ZeroGravityPGT.transform.position, RobotArm.transform.position);
        Vector3 TowardsOrigin = -(ZeroGravityPGT.transform.position - RobotArm.transform.position) * 2;

        if (distance >= 0.5f)
        {
            ZeroGravityPGT.GetComponent<Rigidbody>().AddForce(TowardsOrigin, ForceMode.Acceleration);
        }

    }

    private float MapToDegrees(float x)
    {
        return (x >= 0.0f && x <= 0.2f) ? 360.0f - (x * 900.0f) :
               (x > 0.2f && x <= 0.5f) ? 180.0f - ((x - 0.2f) * 600.0f) :
               throw new ArgumentOutOfRangeException("x", "Value must be between 0.0 and 0.5.");
    }

    private void NudgeTowardsRotationEnvelope()
    {

        Vector3 handLeftPosition = testCube.transform.position; //OVRInput.GetLocalControllerPosition(OVRInput.Controller.LHand); //OVRInput.GetLocalControllerPosition(OVRInput.Controller.LHand);
        Vector3 handRightPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RHand);

        Vector3 toolPosition = ZeroGravityPGT.transform.position;

        float leftDistance = Vector3.Distance(toolPosition, handLeftPosition);
        float rightDistance = Vector3.Distance(toolPosition, handRightPosition);

        float distance = Mathf.Min(leftDistance, rightDistance);


        if (distance <= 0.5f)
        {
            //ZeroGravityPGT.GetComponent<Rigidbody>().freezeRotation = true;
            Quaternion wristSegmentRotation = WristVisualisation.transform.rotation;

            float degrees = MapToDegrees(distance); //0.6f * Mathf.Exp(1 / (distance - 0.01f));
            ZeroGravityPGT.transform.localRotation = Quaternion.RotateTowards(ZeroGravityPGT.transform.rotation, wristSegmentRotation, degrees);

            instructionText.SetText("distance: " + distance + " degrees: " + degrees);

            HandMeshL.SetActive(false);
            HandMeshR.SetActive(false);
        }
        else
        {
            HandMeshL.SetActive(true);
            HandMeshR.SetActive(true);
            //ZeroGravityPGT.GetComponent<Rigidbody>().freezeRotation = false;
        }

    }

}
