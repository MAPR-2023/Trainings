using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerObjectCollider : MonoBehaviour
{
    public bool rotating = false;
    public GameObject underlayingCollider;
    private GameObject col;
    private Rigidbody rb;
    // Start is called before the first frame update
    void Start() {
        col = Instantiate(underlayingCollider);
        col.SetActive(true);
        rb = col.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        //position
        rb.velocity = (transform.position - col.transform.position) / Time.fixedDeltaTime;

        //rotation
        if (rotating)
        {
            Quaternion rotationDifference = transform.rotation * Quaternion.Inverse(col.transform.rotation);
            rotationDifference.ToAngleAxis(out float angleInDegree, out Vector3 rotationAxis);

            Vector3 rotationDifferenceInDegree = angleInDegree * rotationAxis;

            rb.angularVelocity = (rotationDifferenceInDegree * Mathf.Deg2Rad / Time.fixedDeltaTime);
        }
     }

    public void SetTrigger(bool trigger)
    {
        col.GetComponent<SphereCollider>().isTrigger = trigger;
    }
}
