using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerColliderManager : MonoBehaviour
{
    public GameObject index1;
    public GameObject index2;
    public GameObject index3;
    public GameObject middle1;
    public GameObject middle2;
    public GameObject middle3;
    public GameObject ring1;
    public GameObject ring2;
    public GameObject ring3;
    public GameObject pink1;
    public GameObject pink2;
    public GameObject pink3;
    public GameObject thumb1;
    public GameObject thumb2;
    public GameObject thumb3;
    public GameObject palm;

    // Start is called before the first frame update
    void OnEnable()
    {
        index1.GetComponent<FingerObjectCollider>().enabled = true;
        index2.GetComponent<FingerObjectCollider>().enabled = true;
        index3.GetComponent<FingerObjectCollider>().enabled = true;
        middle1.GetComponent<FingerObjectCollider>().enabled = true;
        middle2.GetComponent<FingerObjectCollider>().enabled = true;
        middle3.GetComponent<FingerObjectCollider>().enabled = true;
        ring1.GetComponent<FingerObjectCollider>().enabled = true;
        ring2.GetComponent<FingerObjectCollider>().enabled = true;
        ring3.GetComponent<FingerObjectCollider>().enabled = true;
        pink1.GetComponent<FingerObjectCollider>().enabled = true;
        pink2.GetComponent<FingerObjectCollider>().enabled = true;
        pink3.GetComponent<FingerObjectCollider>().enabled = true;
        thumb1.GetComponent<FingerObjectCollider>().enabled = true;
        thumb2.GetComponent<FingerObjectCollider>().enabled = true;
        thumb3.GetComponent<FingerObjectCollider>().enabled = true;
        palm.GetComponent<FingerObjectCollider>().enabled = true;
    }

    private void OnDisable()
    {
        index1.GetComponent<FingerObjectCollider>().enabled = false;
        index2.GetComponent<FingerObjectCollider>().enabled = false;
        index3.GetComponent<FingerObjectCollider>().enabled = false;
        middle1.GetComponent<FingerObjectCollider>().enabled = false;
        middle2.GetComponent<FingerObjectCollider>().enabled = false;
        middle3.GetComponent<FingerObjectCollider>().enabled = false;
        ring1.GetComponent<FingerObjectCollider>().enabled = false;
        ring2.GetComponent<FingerObjectCollider>().enabled = false;
        ring3.GetComponent<FingerObjectCollider>().enabled = false;
        pink1.GetComponent<FingerObjectCollider>().enabled = false;
        pink2.GetComponent<FingerObjectCollider>().enabled = false;
        pink3.GetComponent<FingerObjectCollider>().enabled = false;
        thumb1.GetComponent<FingerObjectCollider>().enabled = false;
        thumb2.GetComponent<FingerObjectCollider>().enabled = false;
        thumb3.GetComponent<FingerObjectCollider>().enabled = false;
        palm.GetComponent<FingerObjectCollider>().enabled = false;
    }

    public void SetTrigger(bool trigger)
    {
        index1.GetComponent<FingerObjectCollider>().SetTrigger(trigger);
        index2.GetComponent<FingerObjectCollider>().SetTrigger(trigger);
        index3.GetComponent<FingerObjectCollider>().SetTrigger(trigger);
        middle1.GetComponent<FingerObjectCollider>().SetTrigger(trigger);
        middle2.GetComponent<FingerObjectCollider>().SetTrigger(trigger);
        middle3.GetComponent<FingerObjectCollider>().SetTrigger(trigger);
        ring1.GetComponent<FingerObjectCollider>().SetTrigger(trigger);
        ring2.GetComponent<FingerObjectCollider>().SetTrigger(trigger);
        ring3.GetComponent<FingerObjectCollider>().SetTrigger(trigger);
        pink1.GetComponent<FingerObjectCollider>().SetTrigger(trigger);
        pink2.GetComponent<FingerObjectCollider>().SetTrigger(trigger);
        pink3.GetComponent<FingerObjectCollider>().SetTrigger(trigger);
        thumb1.GetComponent<FingerObjectCollider>().SetTrigger(trigger);
        thumb2.GetComponent<FingerObjectCollider>().SetTrigger(trigger);
        thumb3.GetComponent<FingerObjectCollider>().SetTrigger(trigger);
    }
}
