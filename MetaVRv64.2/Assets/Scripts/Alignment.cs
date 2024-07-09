using TMPro;
using UnityEngine;
using UnityEditor;

public class Alignment : MonoBehaviour
{
    // Game Objects
    [Header("Game Objects")]
    public GameObject cameraRig; 
    public TextMeshPro instructionText;
    public GameObject alignmentCursor;
    public GameObject alignmentTarget;
    public GameObject trainingScript;
    public GameObject Training1Scene;

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
        const int trainingStage = 3;
        alignmentStage = Mathf.Min(3, Mathf.Max(0, alignmentStage));
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
            case (trainingStage):
                hideCursor();
                training();
                break;
            default:
                baseHeightAlignment();
                break;
        }

        bool AButtonDown = OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.Space);
        bool BButtonDown = OVRInput.GetDown(OVRInput.Button.Two) || Input.GetKeyDown(KeyCode.Backspace);
        if (AButtonDown) alignmentStage++;
        if (BButtonDown) alignmentStage--;
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
            instructionText.SetText("You are now aligning the height.\nLet go to place the robot arm at that height.\nTo change the location of the robotarm. Hold your head above the robotarm and hold the Oculus Quest Home button.");
            Vector3 acPosition = alignmentCursor.transform.position;

            // offset to make it align
            acPosition.x = robot.transform.position.x;
            acPosition.z = robot.transform.position.z ;
            robot.transform.position = acPosition;
        }
        else
        {
            Training1Scene.transform.position = Vector3.zero;
            instructionText.SetText("You will now align the height of the base of the robotarm.\nPress the trigger to start.\nTo change the location of the robotarm. Hold your head above the robotarm and hold the Oculus Quest Home button.\nPress A to confirm and go to the next alignment setting. (B to go back)");
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
            instructionText.SetText("You are now aligning the rotation of the robotarm using your thumbstick.\nLet go to rotate the arm with the real physical arm.\n"+ thumbstick.x.ToString());
        }
        else
        {
            angle = 0;
            instructionText.SetText("Rotate the robotarm so that it aligns with the real robotarm.\nHold the trigger to rotate the robotarm.\nPress A to confirm and go to the next alignment setting. (B to go back)");
        }

        if (angle > 0)
            robotVisualiser.transform.RotateAround(desk.transform.position, Vector3.up, angle);
        else if (angle < 0)
            robotVisualiser.transform.RotateAround(desk.transform.position, Vector3.down, -angle);
    }

    private void movementTest()
    {
        alignmentTarget.transform.position = alignmentCursor.transform.position;
        instructionText.SetText("This is a movement test for the robotarm and the digital robot arm.\nPress A to confirm and go to the training. (B to go back)");
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        trainingScript.SetActive(false);
    }

    private void training()
    {
        instructionText.SetText("The training has begun. (B to go back to aligning)");
        ovrManager.isInsightPassthroughEnabled = false;
        trainingScript.SetActive(true);
    }
}
