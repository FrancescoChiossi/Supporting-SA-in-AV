using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPSender : MonoBehaviour
{
    //used to send session ID and session end to the tablet app
    //is run in background thread for performance reasons

    int port = 9900;
    Thread appThread;
    UdpClient client;
    IPEndPoint ip;

    public TextAsset usedIds; //TextAsset used because this way the file can be assigned in the editor
    string idFile = "IDs.txt"; //other than that, these two are the same

    public int sessionID = -1;
    string IDs;
    DataLogger idLog;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        appThread = new Thread(new ThreadStart(SendData));
        appThread.IsBackground = true;
        appThread.Start();
        IDs = usedIds.text;
        StartCoroutine(CalculateID());
    }

    IEnumerator shortPause() {
        yield return new WaitForSeconds(0.2f);
    }

    IEnumerator CalculateID()
    {
        string id = "";
        while (sessionID == -1) //no ID assigned yet
        {
            id = UnityEngine.Random.Range(1, 100).ToString();
            if (!IDs.Contains(id))
            {
                int.TryParse(id, out sessionID);
                idLog = new DataLogger();
                idLog.Open(idFile);
                idLog.writeLine(sessionID.ToString());
                idLog.Flush();
                StartCoroutine(shortPause()); //ensures that data is written correctly before closing
                idLog.Close(idFile);
            }
            else yield return null;
        }
        
    }

    void SendData()
    {
        client = new UdpClient(port);
        client.Client.Blocking = false;
        while (true)
        {
            ip = new IPEndPoint(IPAddress.Broadcast, port);
            string msg = sessionID.ToString();
            byte[] data = Encoding.UTF8.GetBytes(msg);
            client.Send(data, data.Length, ip);
        }
    }

    public void StopApp()
    {
        byte[] data = Encoding.UTF8.GetBytes("Session end");
        client.Send(data, data.Length, ip);
    }

    private void OnApplicationQuit()
    {
        StopThread();
    }

    void StopThread()
    {
        if (appThread.IsAlive) appThread.Abort();
        client.Close();
    }
}
