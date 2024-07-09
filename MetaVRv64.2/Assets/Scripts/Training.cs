using System.Collections;
using System.Collections.Generic;
using Oculus.Haptics;
using TMPro;
using UnityEngine;

public class Training : MonoBehaviour
{
    [Header("PassThrough")]
    public GameObject CenterEyeAnchor;

    [Header("Scene GameObjects")]
    public GameObject Training1;
    public GameObject RobotArm;
    public GameObject ZeroGravityPGT;
    public GameObject MetaQuestPro;
    public GameObject MetaQuestProControllerMesh;
    public TextMeshPro instructionText;
    public GameObject IKTarget;

    [Header("Segments")]
    public GameObject ShoulderSegment;
    public GameObject ElbowSegment;
    public GameObject WristSegment;

    public GameObject ShoulderVisualisation;
    public GameObject ElbowVisualisation;
    public GameObject WristVisualisation;

    //[Header("Materials")]
    //public Material hidden;

    //[Header("Rotation Nudging")]
    //public GameObject pgtOrigin;
    //public GameObject pgtHandle;
    //public GameObject endEffectorOrigin;
    //public GameObject endEffector;

    // Start is called before the first frame update
    void OnEnable()
    {
        // turn off controller model and turn on pgt on hand
        MetaQuestProControllerMesh.SetActive(false);
        MetaQuestPro.GetComponent<Grabbing>().enabled = true;

        // set first training scene as active
        Training1.SetActive(true);
        
        // enable the fade in of robot arm segments when hands are too close
        enableFadeSegment();

        // change skybox settings after disabling passthrough
        Camera centerEyeCam = CenterEyeAnchor.GetComponent<Camera>();
        centerEyeCam.clearFlags = CameraClearFlags.Skybox;
        centerEyeCam.backgroundColor = Color.white;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
    }


    private void OnDisable()
    {
        // disable training scene
        Training1.SetActive(false);

        // disable segment fades
        disableFadeSegment();

        // change skybox settings after enable passthrough
        Camera centerEyeCam = CenterEyeAnchor.GetComponent<Camera>();
        centerEyeCam.clearFlags = CameraClearFlags.SolidColor;
        Color blackTransparant = Color.black;
        blackTransparant.a = 0;
        centerEyeCam.backgroundColor = blackTransparant;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
    }

    private void enableFadeSegment()
    {
        // activate the proximity segment fade
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
        ShoulderSegment.GetComponent<MeshRenderer>().enabled = true;
        ElbowSegment.GetComponent<MeshRenderer>().enabled = true;
        WristSegment.GetComponent<MeshRenderer>().enabled = true;

        ShoulderVisualisation.GetComponent<FadeInSegment>().enabledFade = false;
        ElbowVisualisation.GetComponent<FadeInSegment>().enabledFade = false;
        WristVisualisation.GetComponent<FadeInSegment>().enabledFade = false;
    }

    // Update is called once per frame
    void Update()
    {
        IKTarget.transform.position = ZeroGravityPGT.transform.position;
        NudgeTowardsEnvelope();
        NudgeTowardsRotationEnvelope();
#if !UNITY_EDITOR
        Haptics();
#endif
    }

    private void NudgeTowardsEnvelope()
    {
        float distance = Vector3.Distance(ZeroGravityPGT.transform.position, RobotArm.transform.position);
        Vector3 TowardsOrigin = -(ZeroGravityPGT.transform.position - RobotArm.transform.position);

        if (distance >= 1.0f)
        {
            ZeroGravityPGT.GetComponent<Rigidbody>().AddForce(TowardsOrigin, ForceMode.Acceleration);
        }

    }

    private void NudgeTowardsRotationEnvelope()
    {

        Vector3 handLeftPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LHand);
        Vector3 handRightPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RHand);

        Vector3 toolPosition = ZeroGravityPGT.transform.position;

        float leftDistance = Vector3.Distance(toolPosition, handLeftPosition);
        float rightDistance = Vector3.Distance(toolPosition, handRightPosition);

        float distance = Mathf.Min(leftDistance, rightDistance);


        if (distance <= 0.5f)
        {
            Quaternion wristSegmentRotation = WristVisualisation.transform.rotation;

            float degrees = 0.1f * Mathf.Exp(1/distance);
            ZeroGravityPGT.transform.localRotation = Quaternion.RotateTowards(ZeroGravityPGT.transform.rotation, wristSegmentRotation, degrees);

            instructionText.SetText("distance: "+distance+" degrees: "+degrees);
        }

    }

    public HapticClip PgtDrillingHapticClip;
    private HapticClipPlayer player;

    private void Awake()
    {
#if !UNITY_EDITOR
        player = new HapticClipPlayer(PgtDrillingHapticClip);
        player.priority = 0;
        player.isLooping = true;
#endif
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
