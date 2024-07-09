using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UsbSerialCustom : MonoBehaviour
{

    AndroidJavaClass unityClass;
    AndroidJavaObject unityActivity;
    AndroidJavaObject _pluginInstance;

    public TextMeshPro flippedStateText;

    bool isFlipped = false;

    // Start is called before the first frame update
    void Start()
    {
        InitializePlugin("com.hoho.android.usbserial.util.UnityUsbSerial");
    }

    void InitializePlugin(string pluginName)
    {
        unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
        _pluginInstance = new AndroidJavaObject(pluginName);

        if (_pluginInstance == null)
        {
            Debug.Log("Plugin Instance Error");
        }
        _pluginInstance.Call("receiveUnityActivity", unityActivity);
    }

    public void Write(string line)
    {
        if (_pluginInstance != null)
        {
            _pluginInstance.Call("Write", line);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            WriteFlipped();
        }
    }

    public void WriteFlipped()
    {
        string line = "W" + (isFlipped ? "90" : "-90");
        Write(line);
        isFlipped = !isFlipped;

        Debug.Log(line);

        flippedStateText.SetText(isFlipped ? "true" : "false");
    }
}
