using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorScript : MonoBehaviour
{
    public GameObject EditorCamera;
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
        EditorCamera.SetActive(true);
        Debug.Log("Editor Camera Active");

        GameObject.Find("CenterEyeAnchor").GetComponent<AudioListener>().enabled = false;
#endif
    }
}
