using TMPro;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class FadeInSegment : MonoBehaviour
{
    public TextMeshPro instructionText;
    public bool enabledFade = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (enabledFade)
        {
            Vector3 handLeftPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LHand);
            Vector3 handRightPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RHand);

            //instructionText.SetText(handLeftPosition.x + " - " + handLeftPosition.y + " - " + handLeftPosition.z);

            Vector3 segmentPosition = this.transform.position;

            float leftDistance = Vector3.Distance(segmentPosition, handLeftPosition);
            float rightDistance = Vector3.Distance(segmentPosition, handRightPosition);

            float distance = Mathf.Min(leftDistance, rightDistance);

            if (distance < 0.2f)
            {
                MeshRenderer mr = this.GetComponent<MeshRenderer>();
                mr.enabled = true;
                var col = mr.material.color;
                col.a = 1.0f - distance;
                mr.material.color = col;
            } else
            {
                this.GetComponent<MeshRenderer>().enabled = false;
            }
        } else
        {
            GetComponent<MeshRenderer>().enabled = true;
        }
    }
}
