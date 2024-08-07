using System.Collections;
using System.Collections.Generic;
using Oculus.Haptics;
using TMPro;
using UnityEngine;
using Obi;
using System;

public class Training1AExploring : MonoBehaviour
{
    [Header("PassThrough")]
    public GameObject CenterEyeAnchor;
    public GameObject cameraRig;
    private OVRManager ovrManager;

    [Header("Scene GameObjects")]
    public GameObject Training;
    public GameObject RobotArm;
    public GameObject ZeroGravityPGT;
    public GameObject MetaQuestPro;
    public GameObject MetaQuestProControllerMesh;
    public TextMeshPro instructionText;
    public GameObject IKTarget;
    public GameObject ObiSolver;
    public GameObject ObiRope;
    public GameObject HandMeshL;
    public GameObject HandMeshR;

    public GameObject testCube;

    [Header("Segments")]
    public GameObject ShoulderSegment;
    public GameObject ElbowSegment;
    public GameObject WristSegment;
    public GameObject DeskMountL;
    public GameObject DeskMountR;

    public GameObject ShoulderVisualisation;
    public GameObject ElbowVisualisation;
    public GameObject WristVisualisation;

    [Header("Training Management")]
    public GameObject FingerColliderManagerScript;

    //private variables
    private bool wasIndexDown = false;
    private bool wasHandDown = false;
    private bool primaryIndexTriggerDown = false;
    private bool primaryHandTriggerDown = false;
    private bool obiRopeEnabled = false;
    int training1aStage = 0;
    private Vector3 initialPGTPos = Vector3.zero;
    private Quaternion initialPGTRot;
    private GameObject cloneRope;

    private bool isVirtual = true;

    public void setVirtual(bool v) {
        isVirtual = v;
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        resetScene();
        initialPGTPos = ZeroGravityPGT.transform.position;
        initialPGTRot = ZeroGravityPGT.transform.rotation;

        // turn off controller model and turn on pgt on hand
        //MetaQuestProControllerMesh.SetActive(false);
        //MetaQuestPro.GetComponent<Grabbing>().enabled = true;

        // set first training scene as active
        Training.SetActive(true);

        // enable the fade in of robot arm segments when hands are too close
        enableFadeSegment();
        goodLighting();
        //ovrManager = cameraRig.GetComponent<OVRManager>();
        // todo add button for passthrough toggle

        if (isVirtual)
            FingerColliderManagerScript.SetActive(true);
    }

    public void goodLighting()
    {
        // change skybox settings after disabling passthrough
        Camera centerEyeCam = CenterEyeAnchor.GetComponent<Camera>();
        centerEyeCam.clearFlags = CameraClearFlags.Skybox;
        centerEyeCam.backgroundColor = Color.white;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
    }

    private void OnDisable()
    {
        // disable training scene
        Training.SetActive(false);

        // disable segment fades
        disableFadeSegment();

        // change skybox settings after enable passthrough
        Camera centerEyeCam = CenterEyeAnchor.GetComponent<Camera>();
        centerEyeCam.clearFlags = CameraClearFlags.SolidColor;
        Color blackTransparant = Color.black;
        blackTransparant.a = 0;
        centerEyeCam.backgroundColor = blackTransparant;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

        FingerColliderManagerScript.SetActive(false);
    }

    public void enableFadeSegment()
    {
        // activate the proximity segment fade
        DeskMountL.SetActive(false);
        DeskMountR.SetActive(false);
        ShoulderSegment.GetComponent<MeshRenderer>().enabled = false;
        ElbowSegment.GetComponent<MeshRenderer>().enabled = false;
        WristSegment.GetComponent<MeshRenderer>().enabled = false;

        ShoulderVisualisation.GetComponent<FadeInSegment>().enabledFade = true;
        ElbowVisualisation.GetComponent<FadeInSegment>().enabledFade = true;
        WristVisualisation.GetComponent<FadeInSegment>().enabledFade = true;
    }

    private void disableFadeSegment()
    {
        // activate the proximity segment fade
        DeskMountL.SetActive(true);
        DeskMountR.SetActive(true);
        ShoulderSegment.GetComponent<MeshRenderer>().enabled = true;
        ElbowSegment.GetComponent<MeshRenderer>().enabled = true;
        WristSegment.GetComponent<MeshRenderer>().enabled = true;

        ShoulderVisualisation.GetComponent<FadeInSegment>().enabledFade = false;
        ElbowVisualisation.GetComponent<FadeInSegment>().enabledFade = false;
        WristVisualisation.GetComponent<FadeInSegment>().enabledFade = false;
    }

    private void trackTriggers()
    {
        if (primaryIndexTriggerDown)
            primaryIndexTriggerDown = !OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger);
        else
            primaryIndexTriggerDown = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger);

        if (primaryHandTriggerDown)
            primaryHandTriggerDown = !OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger);
        else
            primaryHandTriggerDown = OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger);
    }

    // Update is called once per frame
    void Update()
    {
        IKTarget.transform.position = ZeroGravityPGT.transform.position;
        int noRope = 0; int withRope = 1;
        changeStage();

        if (training1aStage == noRope)
        {
            if (cloneRope != null)
                cloneRope.SetActive(false);
            NudgeTowardsEnvelope();
            if (isVirtual)
            {
                RobotArm.SetActive(false);
            } else
            {
                RobotArm.SetActive(true);
                NudgeTowardsRotationEnvelope();
            }
            
            //Nudge PGT to physical controller?

            Destroy(cloneRope);
        }
        else if (training1aStage == withRope)
        {
            cloneRope.SetActive(true);
            if (isVirtual)
            {
                RobotArm.SetActive(false);
            }
            else
            {
                RobotArm.SetActive(true);
                NudgeTowardsRotationEnvelope();
            }
        }
//#if !UNITY_EDITOR
//        Haptics();
//#endif
    }

    private void resetScene()
    {
        if (initialPGTPos != Vector3.zero)
        {
            ZeroGravityPGT.GetComponent<Rigidbody>().velocity = Vector3.zero;
            ZeroGravityPGT.transform.position = initialPGTPos;
            ZeroGravityPGT.transform.rotation = initialPGTRot;

            //ObiRope.GetComponent<ObiRope>().ResetParticles();
            cloneRope = Instantiate(ObiRope, ObiSolver.transform);
            cloneRope.GetComponent<ObiRope>().ResetParticles();
        }
    }

    private void changeStage()
    {
        trackTriggers();
        if ((!wasIndexDown && primaryIndexTriggerDown) || Input.GetKeyDown(KeyCode.R)) // for rope
        {
            training1aStage++;
            wasIndexDown = true;

            if (training1aStage == 1)
            {
                resetScene();
            }
        }
        if (!primaryIndexTriggerDown)
        {
            wasIndexDown = false;
        }
        training1aStage = training1aStage % 2; //Mathf.Min(1, Mathf.Max(0, training1aStage));
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

        Vector3 handLeftPosition = testCube.transform.position;//OVRInput.GetLocalControllerPosition(OVRInput.Controller.LHand); 
        Vector3 handRightPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RHand);

        Vector3 toolPosition = ZeroGravityPGT.transform.position;

        float leftDistance = Vector3.Distance(toolPosition, handLeftPosition);
        float rightDistance = Vector3.Distance(toolPosition, handRightPosition);

        float distance = Mathf.Min(leftDistance, rightDistance);


        if (distance <= 0.5f)
        {
            //ZeroGravityPGT.GetComponent<Rigidbody>().freezeRotation = true;
            Quaternion wristSegmentRotation = WristVisualisation.transform.rotation;

            float degrees = MapToDegrees(distance);//0.1f * Mathf.Exp(1 / (distance - 0.1f));
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

    public HapticClip PgtDrillingHapticClip;
    private HapticClipPlayer player;

    private void Awake()
    {
//#if !UNITY_EDITOR
//        player = new HapticClipPlayer(PgtDrillingHapticClip);
//        player.priority = 0;
//        player.isLooping = true;
//#endif
    }

    private void Haptics()
    {
        float triggerPressed = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);

        player.amplitude = triggerPressed;

        if (triggerPressed > 0.01f)
            player.Play(Controller.Right);
        else
            player.Stop();
        
    }
}
