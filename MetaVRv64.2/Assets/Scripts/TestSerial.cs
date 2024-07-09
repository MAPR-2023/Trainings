using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using SerialPortUtility;

public class TestSerial : MonoBehaviour
{
    private SerialPortUtilityPro port;
    private int rotation = -180;
    private float timePassed = 0;
    public GameObject textObject;
    private TextMeshProUGUI text;
    // Start is called before the first frame update

    private void Awake()
    {
        port = this.GetComponent<SerialPortUtilityPro>();
        port.BaudRate = 115200;
        port.OpenMethod = SerialPortUtilityPro.OpenSystem.USB;
        
    }

    void Start()
    {
        text = textObject.GetComponent<TextMeshProUGUI>();
        try
        {
            SerialPortUtilityPro.DeviceInfo[] deviceInfos = SerialPortUtilityPro.GetConnectedDeviceList(SerialPortUtilityPro.OpenSystem.USB);
            string devicesText = "";
            //foreach (SerialPortUtilityPro.DeviceInfo deviceInfo in deviceInfos)
            //{
            //devicesText += "port: " + deviceInfos[0].PortName;
            //devicesText += "- product: " + deviceInfos[0].Product;
            //devicesText += "- vendor" + deviceInfos[0].Vendor;
            //devicesText += "- serial" + deviceInfos[0].SerialNumber + "\n";
            //}
            //text.SetText(devicesText);
        } catch (System.Exception e)
        {
            text.SetText(e.Message);
        }

    }

    // Update is called once per frame
    void Update()
    {
        timePassed += Time.deltaTime;
        if (timePassed > 1.0f)
        {
            rotation = rotation + 30;
            rotation = ((180+ rotation) % 360) - 180;
            port.WriteCRLF("S"+rotation);

            timePassed -= 1.0f;
        }
    }

    //Example Read Data : AAA,BBB,CCC,DDD<CR><LF>
    //for List data
    public void ReadComplateList(object data)
    {
        var text = data as List<string>;
        for (int i = 0; i < text.Count; ++i)
            if (text[i] != "-1")
                Debug.Log(text[i]);
    }


}
