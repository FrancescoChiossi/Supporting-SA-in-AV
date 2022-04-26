using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DataLogger : MonoBehaviour
{
    //Logs data to file

    private StreamWriter writer;
    string file;

    public bool Open(string fileName)
    {
        file = fileName;
        string timeStamp = System.DateTime.Now.ToString("HH:mm:ss:fff"); //results in hour:minute:second:millisecond with leading zeros in 24h format
        string filePath = Application.dataPath + "/" + fileName;
        try
        {
            writer = new StreamWriter(filePath, true);
            if (!fileName.Contains("ID"))
            {
                writer.WriteLine("SessionStart: " + timeStamp);
                if (fileName.Contains("GSR")) writer.WriteLine(timeStamp + "  " + "time passed" +"  " + "GSR");
                else writer.WriteLine(timeStamp + "  " + "time passed" + "  " + "action" + "  " + "motors" + "  " + "distA" + "  " + "distB" + "  " + "brake");
            }
        }
        catch (System.Exception e)
        {
            print(e.ToString());
            return false;
        }
        return true;
    }

    public void Close(string file)
    {
        if (writer != null)
        {
            if (!file.Contains("ID"))
            {
                string timeStamp = System.DateTime.Now.ToString("HH:mm:ss:fff");
                writer.WriteLine("SessionEnd: " + timeStamp);
            }
            writer.Close();
            writer = null;
        }
    }

    public void Flush()
    {
        writer.Flush();
    }

    public void writeLine(string text, string time)
    {
        string timeStamp = System.DateTime.Now.ToString("HH:mm:ss:fff");
        writer.WriteLine(timeStamp + "  " + time + "  " + text);
    }

    public void writeLine(string text)
    {
        writer.WriteLine(text);
    }

    private void OnApplicationQuit()
    {
        Close(file);   
    }
}
