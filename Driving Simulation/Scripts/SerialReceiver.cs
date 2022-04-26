using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;
using UnityEngine.SceneManagement;

public class SerialReceiver : MonoBehaviour
{
    //read GSR sensor values

    SerialPort port = new SerialPort("COM3", 115200);
    DataLogger log;
    string file;
    int sessionID = -1;
    Thread arduino;
    string time;
    public bool sceneChanged;

    void Start()
    {
        port.Open();
        StartCoroutine(WaitForID());
        arduino = new Thread(new ThreadStart(SerialRead));
        arduino.IsBackground = true;
        arduino.Start();
    }

    private void Update()
    {
        time = Time.time.ToString();
        if (sceneChanged)
        {
            log.Close(file);
            file = (System.DateTime.Now.ToString("yyyyMMdd") + "_GSR_Driving_" + sessionID + ".txt");
            log = new DataLogger();
            log.Open(file);
            sceneChanged = false;
        }
    }

    IEnumerator WaitForID()
    {
        while (GetComponent<UDPSender>().sessionID == -1) yield return null;
        sessionID = GetComponent<UDPSender>().sessionID;
        file = (System.DateTime.Now.ToString("yyyyMMdd") + "_GSR_Training_" + sessionID + ".txt");
        log = new DataLogger();
        log.Open(file);
    }

    private void OnApplicationQuit()
    {
        port.Close();
        log.Close(file);
    }

    void SerialRead()
    {
        while (true)
        {
            string value = port.ReadLine();
            log.writeLine(value, time);
            log.Flush();
        }
    }
}
