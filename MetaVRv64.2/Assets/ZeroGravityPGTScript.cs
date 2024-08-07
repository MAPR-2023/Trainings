using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeroGravityPGTScript : MonoBehaviour
{
    private bool touchedByFinger = false;
    public bool TouchedByFinger()
    {
        if (touchedByFinger)
        {
            touchedByFinger = false;
            return true;
        }
        return false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.collider.gameObject.tag == "FingerTouched")
        {
            touchedByFinger = true;
        }
    }

    private void Update()
    {
        // dampener
        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 velocity = rb.velocity;
        if (velocity.magnitude >= 1.0)
        {
            rb.velocity = Vector3.zero;
        }
    }
}
