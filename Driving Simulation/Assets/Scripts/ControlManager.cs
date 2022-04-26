using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Vehicles.Car;
using UnityEngine.UI;
using System.IO.Ports;
using System.Text;
using UnityEngine.SceneManagement;

public class ControlManager : MonoBehaviour
{
    private float accelerate;
    private float brake;
    // range: -0.418315 (unpressed) to -0.9242979 (fully pressed)
    private float brakeMin = -0.418315f;
    private float brakeMax = -0.9242979f;
    private bool brakePressed = false; 
    
    private float steering;
    private float switchControl; //button for handing back control to the AI
    private bool switchPressed = false;
    
    private CarController controller;
    private CarAIControl AI;
    private Overtaking overtaking;
    private int action;
    Quaternion startRotation;
    public bool resetTracker = false;

    private DataLogger fileWriter;
    private string logText;
    private string file;
    private int sessionID;
    Transform driverlessCar;
    Transform oppCar;
    public GameObject label;
    public GameObject panel;
    UDPSender sender;

    public SerialPort port;
    string amplitude = "64";
    string frequency = "15";
    string currentMotors = "000000";
    AudioSource sound;
    public AudioClip crash;
    bool played = false;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name == "Driving" || SceneManager.GetActiveScene().name == "Training")
        {
            sender = GameObject.FindGameObjectWithTag("Communication").GetComponent<UDPSender>();
            StartCoroutine(WaitForID());
        }
        controller = GetComponent<CarController>();
        AI = GetComponent<CarAIControl>();
        overtaking = GetComponent<Overtaking>();
        sound = GetComponent<AudioSource>();
        port = new SerialPort("COM6", 115200, Parity.None, 8, StopBits.One);
        port.RtsEnable = false;
        port.DtrEnable = false;
        port.ReadTimeout = 5;
        port.Open();
    }

    private void Start()
    {
        startRotation = transform.rotation;
        if (SceneManager.GetActiveScene().name == "Training" || SceneManager.GetActiveScene().name == "Driving")
        {
            //open logfile
            file = (System.DateTime.Now.ToString("yyyyMMdd") + "_Simulation_" + SceneManager.GetActiveScene().name + "_" + sessionID + ".txt");
            fileWriter = new DataLogger();
            fileWriter.Open(file);
        }
    }

    void Update()
    {
        action = overtaking.action;
        brake = CrossPlatformInputManager.GetAxis("Brake");
        brake = Map(brake, brakeMin, brakeMax, 0, 100); //map brake input to percent values
        LogData();
        if (brake >= 20 && !brakePressed) //20%
        {
            brakePressed = true;
            panel.SetActive(true);
            StartCoroutine("PauseSimulation");
        }
        if (action == 2 && oppCar != null && Vector3.Distance(transform.position, oppCar.position) <= 4.5) //crash
        {
            StartCoroutine("PlaySound");
        }


        /*
         * Basis for switching from AI control to manual control 
         * Not used yet
         * The numbers are only dummies and depend on the used input devices
         * 
         * 
        accelerate = CrossPlatformInputManager.GetAxis("Accelerate");
        brake = CrossPlatformInputManager.GetAxis("Brake");
        steering = CrossPlatformInputManager.GetAxis("Horizontal");
        switchControl = CrossPlatformInputManager.GetAxis("Button");
        if (AI.enabled && overtaking.action != 0 && (overtaking.distanceA < 300 || overtaking.distanceB < 300)) //only allow unser interventions when cars are closer than 300
        {
            if (accelerate > 20 || brake > 20) 
            {
                AI.enabled = false;
                LogData();
            }
        }
        if (!AI.enabled) 
        {
            float v = (accelerate > brake)
                                  ? accelerate
                                  : brake;
            controller.Move(steering, v, v, 0);
            //log data
            if (switchControl && !switchPressed) //switch back to AI 
            {
                AI.enabled = true;
                LogData();
            }
        }
        LogData();
         */
    }

    void LogData()
    {
        if (SceneManager.GetActiveScene().name == "Driving" || SceneManager.GetActiveScene().name == "Training")
        {
            action = overtaking.action;
            driverlessCar = overtaking.driverlessCar;
            oppCar = overtaking.oppCar;
            float distA; //distance to lead car
            if (driverlessCar != null)
            {
                distA = Vector3.Distance(transform.position, driverlessCar.position);
            }
            else
            {
                distA = -1000;
            }
            float distB; //distance to opposite car
            if (oppCar != null)
            {
                distB = Vector3.Distance(transform.position, oppCar.position);
            }
            else
            {
                distB = -1000;
            }
            logText = action + "  " + currentMotors + "  " + distA + "  " + distB + "  " + brake;
            fileWriter.writeLine(logText, Time.time.ToString());
            fileWriter.Flush();
        }
    }

    IEnumerator PauseSimulation()
    {
        VestMessage("000000", port);
        if (action == 2)
        {
            label.GetComponent<Text>().text = "Braking detected. \n Well done, you detected you car's failure!";
            sender.StopApp();
        }
        else
        {
            label.GetComponent<Text>().text = "Braking detected, but not necessary here. \n Your car had everything under control.";
        }
        StopCars();
        yield return new WaitForSeconds(5.0f);
        if (action != 2)
        {
            //reset car + tracker
            overtaking.action = 0;
            resetTracker = true;
            transform.position = new Vector3(0, transform.position.y, transform.position.z);
            transform.rotation = startRotation;
            GetComponent<Rigidbody>().isKinematic = false;
            panel.SetActive(false);
            brakePressed = false;
            controller.MaxSpeed = 108;
        }
        else //simulation is over
        {
            Application.Quit();
        }
    }

    IEnumerator DetectAccident()
    {
        sender.StopApp();
        label.GetComponent<Text>().text = "You had an accident.";
        VestMessage("000000", port);
        StopCars();
        yield return new WaitForSeconds(5);
        Application.Quit();
    }

    IEnumerator PlaySound()
    {
        sound.Stop();
        sound.clip = crash;
        sound.loop = false;
        sound.Play();
        yield return new WaitForSeconds(sound.clip.length);
        panel.SetActive(true);
        StartCoroutine("DetectAccident");
    }

    void StopCars()
    {
        if (driverlessCar != null)
        {
            Destroy(driverlessCar.gameObject);
        }
        if (oppCar != null)
        {
            Destroy(oppCar.gameObject);
        }
        GetComponent<Rigidbody>().isKinematic = true;
    }

    public void VestMessage(string motors, SerialPort port)
    {
        if (motors != currentMotors)
        {
            string msg;
            msg = amplitude + ";" + frequency + ";" + motors;
            print(msg);
            currentMotors = motors;
            UpdateVest(msg, port);
        }
    }

    void UpdateVest(string msg, SerialPort port)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(msg);
        port.Write(bytes, 0, bytes.Length);
        port.BaseStream.Flush();
    }

    private void OnApplicationQuit()
    {
        QuitDataCollection();
    }

    float Map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    public void QuitDataCollection()
    {
        VestMessage("000000", port);
        port.Close();
        fileWriter.Close(file);
    }

    IEnumerator WaitForID()
    {
        while (sender.sessionID == -1) yield return true;
        sessionID = sender.sessionID;
    }
}
