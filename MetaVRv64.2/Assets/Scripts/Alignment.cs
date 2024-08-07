using TMPro;
using UnityEngine;
using UnityEditor;

public class Alignment : MonoBehaviour
{
    // Game Objects
    [Header("Game Objects")]
    public GameObject cameraRig; 
    public TextMeshPro instructionText;
    public TextMeshPro isVirtualText;
    public GameObject alignmentCursor;
    public GameObject alignmentTarget;
    public GameObject MovementTestEnvelope;
    public GameObject Training1Scene;

    [Header("Scripts")]
    public GameObject training1AExploringScript;
    public GameObject training1ATestingScript;

    // Materials
    [Header("Materials")]
    public Material active;
    public Material passive;
    public Material hidden;

    // Robot Components
    [Header("Robot Components")]
    public GameObject robot;
    public GameObject robotForAngles;
    public GameObject robotVisualiser;
    public GameObject desk;

    // Private Variables
    private MeshRenderer alignmentCursorMesh;
    private bool primaryIndexTriggerDown = false;
    private bool primaryHandTriggerDown = false;
    private OVRManager ovrManager;
    private bool isVirtual = true;

    private int alignmentStage = 0;

    // Start is called before the first frame update
    void Start()
    {
        ovrManager = cameraRig.GetComponent<OVRManager>();
        alignmentCursorMesh = alignmentCursor.GetComponent<MeshRenderer>();
        ovrManager.isInsightPassthroughEnabled = true;
    }

    void Update()
    {
        const int baseAlignmentStage = 0;
        const int rotationAlignmentStage = 1;
        const int movementTestStage = 2;
        const int addControllerToMountStage = 3;
        const int training1AExploringStage = 4;
        const int training1ATestStage = 5;
        alignmentStage = Mathf.Min(5, Mathf.Max(0, alignmentStage));
        switch (alignmentStage)
        {
            case (baseAlignmentStage):
                cursorColor();
                baseHeightAlignment();
                break;
            case (rotationAlignmentStage):
                hideCursor();
                rotationAlignment();
                break;
            case (movementTestStage):
                cursorColor();
                movementTest();
                break;
            case (addControllerToMountStage):
                instructionText.SetText(
                "Please insert the controller at the back of the Robot Arm"
                );
                hideCursor();
                break;
            case (training1AExploringStage):
                hideCursor();
                training1AExploring();
                break;
            case (training1ATestStage):
                training1ATest();
                break;
            default:
                cursorColor();
                baseHeightAlignment();
                break;
        }

        bool AButtonDown = OVRInput.GetDown(OVRInput.RawButton.B) || OVRInput.GetDown(OVRInput.RawButton.Y) || Input.GetKeyDown(KeyCode.Space);
        bool BButtonDown = Input.GetKeyDown(KeyCode.Backspace);
        if (AButtonDown) alignmentStage++;
        if (BButtonDown) alignmentStage--;

        bool thumbDown = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick) || Input.GetKeyDown(KeyCode.V); // for virtual
        if (thumbDown) { 
            isVirtual = !isVirtual;
            isVirtualText.text = !isVirtual ? "Using the Robot Arm" : "NOT using the Robot Arm";
            training1AExploringScript.GetComponent<Training1AExploring>().setVirtual(isVirtual);
            training1ATestingScript.GetComponent<Training1ATesting>().setVirtual(isVirtual);
        }
    }

    private void cursorColor()
    {
        if (primaryIndexTriggerDown || primaryHandTriggerDown || Input.GetKeyDown(KeyCode.T))
        {
            alignmentCursorMesh.material = active;
            alignmentCursor.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }
        else
        {
            alignmentCursorMesh.material = passive;
            alignmentCursor.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        }

        trackTriggers();

    }

    private void trackTriggers()
    {
        if (primaryIndexTriggerDown)
            primaryIndexTriggerDown = !OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger);
        else
            primaryIndexTriggerDown = OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger);

        if (primaryHandTriggerDown)
            primaryHandTriggerDown = !OVRInput.GetUp(OVRInput.Button.SecondaryHandTrigger);
        else
            primaryHandTriggerDown = OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger);
    }

    private void hideCursor()
    {
        alignmentCursorMesh.material = hidden;
        trackTriggers();
    }

    private void baseHeightAlignment()
    {
        //Vector3 targetPos = alignmentTarget.transform.localPosition;
        //targetPos.x = 0f;
        //targetPos.y = 100f;
        //targetPos.z = 0f;
        //alignmentTarget.transform.localPosition = targetPos;

        if (primaryIndexTriggerDown)
        {
            instructionText.SetText(
                "You are now aligning the height.\n" +
                "Let go of the RIGHT TRIGGER to place the robot arm at that height.\n" +
                "To change the location of the robotarm. " +
                "Hold your head above the robotarm and hold the HOME BUTTON."
                );
            Vector3 acPosition = alignmentCursor.transform.position;

            // offset to make it align
            acPosition.x = robot.transform.position.x;
            acPosition.z = robot.transform.position.z ;
            robot.transform.position = acPosition;
        }
        else
        {
            Training1Scene.transform.position = Vector3.zero;
            instructionText.SetText(
                "You are will now be aligning the height of the Robot Arm.\n" +
                "hold the RIGHT TRIGGER to place the robot arm at the correct height.\n" +
                "To change the location of the robotarm. " +
                "Hold your head above the robotarm and hold the HOME BUTTON." +
                "Press A to confirm and go to the next alignment setting. (B to go back)"
                );
        }
    }

    private void rotationAlignment()
    {
        Vector3 robotPos = robot.transform.position;
        Training1Scene.transform.localPosition = robotPos;

        Vector3 targetPos = alignmentTarget.transform.localPosition;
        targetPos.y = 0f;
        targetPos.z = 1f;
        alignmentTarget.transform.localPosition = targetPos;
        robotForAngles.SetActive(true);


        float angle;
        if (primaryIndexTriggerDown)
        {
            Vector2 thumbstick = OVRInput.Get(OVRInput.Axis2D.Any);
            angle = thumbstick.x;
            instructionText.SetText(
                "You are now rotating the physical Robot Arm using the TRIGGER.\n" +
                "Use the JOYSTICK to change the angle.\n" +
                thumbstick.x.ToString()
                );
        }
        else
        {
            angle = 0;
            instructionText.SetText(
                "You will now be aligning the rotation of the Robot Arm.\n" +
                "Hold the HOME BUTTON to align the virtual Robot Arm with the physical Robot Arm.\n"+
                "(OPTIONALLY) Hold the TRIGGER and use the JOYSTICK to rotate the physical Robot Arm.\n" +
                "Press A to confirm and go to the next alignment setting. (B to go back)");
        }

        if (angle > 0)
            robotVisualiser.transform.RotateAround(desk.transform.position, Vector3.up, angle);
        else if (angle < 0)
            robotVisualiser.transform.RotateAround(desk.transform.position, Vector3.down, -angle);
    }

    private void movementTest()
    {
        Vector3 alignmentCursorPos = alignmentCursor.transform.position;
        if (insideEnvelope(alignmentCursorPos))
        {
            alignmentTarget.transform.position = alignmentCursor.transform.position;
        }
        instructionText.SetText(
            "This is a movement test for the physical Robot Arm and the virtual Robot Arm.\n" +
            "The Robot Arm should now be aligned and should follow the Cursor above your RIGHT CONTROLLER.\n" +
            "Press A to confirm and go to the training. (B to go back)"
            );
        //RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        training1AExploringScript.SetActive(false);
    }

    private bool insideEnvelope(Vector3 alignmentCursorPos)
    {
        // inside envelope
        return Vector3.Distance(
            MovementTestEnvelope.transform.position,
            alignmentCursorPos
            ) < 0.6
                &&
            alignmentCursorPos.y > 0.05 + robot.transform.position.y
            ;
    }

    private void training1AExploring()
    {
        instructionText.SetText("Training 1A Exploration has begun.\n(press A to begin the test, B to go back to aligning)");
        ovrManager.isInsightPassthroughEnabled = false;
        training1AExploringScript.SetActive(true);
        training1ATestingScript.SetActive(false);
    }

    private void training1ATest()
    {
        instructionText.SetText("Training 1A Testing has begun.\n(press A to begin the test, B to go back to exploring)");
        training1AExploringScript.SetActive(false);
        Training1AExploring t1ae = training1AExploringScript.GetComponent<Training1AExploring>();
        t1ae.goodLighting();
        t1ae.enableFadeSegment();

        training1ATestingScript.SetActive(true);
    }
}
